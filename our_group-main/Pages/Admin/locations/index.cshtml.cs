using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using our_group.LocationDomain.Core.Interfaces;
using our_group.LocationDomain.Infrastructure.Data;
using Microsoft.Extensions.Options;
using our_group.LocationDomain.Core.DTOs;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using our_group.LocationDomain.Core.Entities;

namespace our_group.Pages.Admin.Locations
{
    public class IndexModel : PageModel
    {
        private readonly ILocationService _locationService;
        private readonly LocationDbContext _db;
        private readonly IHttpClientFactory _httpFactory;
        private readonly GoogleMapsSettings _gmaps;
        public IndexModel(ILocationService locationService, LocationDbContext db, IHttpClientFactory httpFactory, IOptions<GoogleMapsSettings> gmaps)
        {
            _locationService = locationService;
            _db = db;
            _httpFactory = httpFactory;
            _gmaps = gmaps.Value;
        }

        public List<LocationDto> Items { get; set; } = new();
        public Dictionary<int, bool> ActiveMap { get; set; } = new();
        [BindProperty]
        public string? ImportPlaceId { get; set; }
        [BindProperty]
        public string? SearchQuery { get; set; }
        public List<SearchPlaceResult> SearchResults { get; set; } = new();
        // Structured filters
        [BindProperty]
        public string? FilterCategory { get; set; }
        [BindProperty]
        public string? FilterCountry { get; set; }
        [BindProperty]
        public string? FilterRegion { get; set; }
        [BindProperty]
        public string? FilterCity { get; set; }
        [BindProperty]
        public int? FilterLimit { get; set; }
        [TempData]
        public string? StatusMessage { get; set; }

        public async Task OnGetAsync() => await LoadItemsAsync();

        private async Task LoadItemsAsync()
        {
            var entities = await _db.Locations.AsNoTracking().ToListAsync();
            Items = entities.Select(e => _locationService.ToLocationDto(e)).ToList();
            ActiveMap = entities.ToDictionary(e => e.Id, e => e.isActive);
        }

        public async Task<IActionResult> OnPostImportAsync()
        {
            if (string.IsNullOrWhiteSpace(ImportPlaceId))
            {
                StatusMessage = "Please provide a Google Place ID.";
                return RedirectToPage();
            }

            try
            {
                var id = ImportPlaceId.Trim();

                // If already imported, just go to details
                var existing = await _db.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.GooglePlaceId == id);
                if (existing is not null)
                {
                    StatusMessage = $"Already imported: '{existing.Name}' (Id {existing.Id}).";
                    return RedirectToPage("/Admin/locations/Details", new { id = existing.Id });
                }

                // Import directly using Details API including place_id in fields
                var importedId = await ImportByPlaceIdAsync(id);
                StatusMessage = $"Imported place (Id {importedId}).";
                return RedirectToPage("/Admin/locations/Details", new { id = importedId });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Import failed: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            // Build query from either free text or structured filters
            var query = (SearchQuery ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                var categoryPhrase = MapCategoryToQuery(FilterCategory);
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(categoryPhrase)) parts.Add(categoryPhrase);
                if (!string.IsNullOrWhiteSpace(FilterCity)) parts.Add(FilterCity!);
                if (!string.IsNullOrWhiteSpace(FilterRegion)) parts.Add(FilterRegion!);
                if (!string.IsNullOrWhiteSpace(FilterCountry)) parts.Add(FilterCountry!);
                query = string.Join(" ", parts);
            }
            if (string.IsNullOrWhiteSpace(query))
            {
                StatusMessage = "Please enter a search query or choose filters.";
                await LoadItemsAsync();
                return Page();
            }

            try
            {
                var client = _httpFactory.CreateClient();
                var url = $"{_gmaps.PlacesBaseUrl}textsearch/json?query={Uri.EscapeDataString(query)}&key={_gmaps.ApiKey}";
                var res = await client.GetAsync(url);
                res.EnsureSuccessStatusCode();
                var json = await res.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<TextSearchResponse>(json);
                var results = (data?.Results ?? new List<TextSearchResult>())
                    .Select(r => new SearchPlaceResult(r.PlaceId, r.Name, r.FormattedAddress, r.Geometry?.Location?.Lat ?? 0, r.Geometry?.Location?.Lng ?? 0))
                    .ToList();
                var limit = (FilterLimit.HasValue && FilterLimit.Value > 0 && FilterLimit.Value <= 20) ? FilterLimit.Value : 10;
                SearchResults = results.Take(limit).ToList();
                await LoadItemsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Search failed: {ex.Message}";
                await LoadItemsAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int id, bool active)
        {
            var loc = await _db.Locations.FirstOrDefaultAsync(l => l.Id == id);
            if (loc is null)
            {
                StatusMessage = $"Location {id} not found.";
                return RedirectToPage();
            }
            loc.isActive = active;
            await _db.SaveChangesAsync();
            StatusMessage = active ? "Location added to rotation." : "Location removed from rotation.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var loc = await _db.Locations.FirstOrDefaultAsync(l => l.Id == id);
            if (loc is null)
            {
                StatusMessage = $"Location {id} not found.";
                return RedirectToPage();
            }
            // Remove reviews first (no FK cascade configured)
            var reviews = await _db.CachedReviews.Where(r => r.LocationId == id).ToListAsync();
            if (reviews.Count > 0)
            {
                _db.CachedReviews.RemoveRange(reviews);
            }
            _db.Locations.Remove(loc);
            await _db.SaveChangesAsync();
            StatusMessage = $"Deleted location {id}.";
            return RedirectToPage();
        }

        public record SearchPlaceResult(string PlaceId, string Name, string Address, double Lat, double Lng);

        private sealed class TextSearchResponse
        {
            [JsonPropertyName("results")] public List<TextSearchResult> Results { get; set; } = new();
        }
        private sealed class TextSearchResult
        {
            [JsonPropertyName("place_id")] public string PlaceId { get; set; } = string.Empty;
            [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
            [JsonPropertyName("formatted_address")] public string FormattedAddress { get; set; } = string.Empty;
            [JsonPropertyName("geometry")] public TextSearchGeometry Geometry { get; set; } = new();
        }
        private sealed class TextSearchGeometry
        {
            [JsonPropertyName("location")] public TextSearchLocation Location { get; set; } = new();
        }
        private sealed class TextSearchLocation
        {
            [JsonPropertyName("lat")] public double Lat { get; set; }
            [JsonPropertyName("lng")] public double Lng { get; set; }
        }

        private static string? MapCategoryToQuery(string? category)
        {
            if (string.IsNullOrWhiteSpace(category)) return null;
            switch (category.Trim())
            {
                case "Landmark": return "tourist attraction";
                case "Museum": return "museum";
                case "Restaurant": return "restaurant";
                case "Park": return "park";
                case "HistoricalSite": return "historic site monument";
                case "NaturalWonder": return "natural feature";
                case "ReligiousSite": return "church mosque temple synagogue";
                case "Shopping": return "shopping mall";
                case "Entertainment": return "movie theater stadium casino";
                case "Hotel": return "hotel";
                case "City": return "city";
                case "Country": return "country";
                default: return category;
            }
        }

        private async Task<int> ImportByPlaceIdAsync(string placeId)
        {
            var client = _httpFactory.CreateClient();
            var url = $"{_gmaps.PlacesBaseUrl}details/json?place_id={Uri.EscapeDataString(placeId)}&key={_gmaps.ApiKey}&fields=place_id,name,geometry,formatted_address,types,rating,user_ratings_total,reviews,address_components";
            var res = await client.GetAsync(url);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();

            var details = JsonSerializer.Deserialize<RawDetailsResponse>(json);
            if (details?.Result is null) throw new InvalidOperationException("Invalid API response");

            var r = details.Result;
            var gdetails = new GooglePlaceDetails(
                PlaceId: r.PlaceId,
                PlaceName: r.Name,
                Address: r.FormattedAddress,
                Location: new GoogleLocation { Lat = r.Geometry?.Location?.Lat ?? 0, Lng = r.Geometry?.Location?.Lng ?? 0 },
                Rating: r.Rating ?? 0,
                UserRatingsTotal: r.UserRatingsTotal ?? 0,
                Types: r.Types ?? new List<string>(),
                Reviews: (r.Reviews ?? new List<RawReview>()).Select(rv => new GooglePlaceReview(rv.AuthorName, rv.Rating ?? 0, rv.Text, rv.Time)).ToList(),
                AddressComponents: (r.AddressComponents ?? new List<RawAddressComponent>()).Select(ac => new AddressComponent { LongName = ac.LongName ?? string.Empty, ShortName = ac.ShortName ?? string.Empty, Types = ac.Types ?? new List<string>() }).ToList()
            );

            // Quality gate (Option A): require at least 3 distinct star buckets among 1..5
            // and require at least one low (<=2★) and one high (>=4★)
            /*var starSet = (gdetails.Reviews ?? new List<GooglePlaceReview>())
                .Where(rv => rv.Rating >= 1 && rv.Rating <= 5)
                .Select(rv => rv.Rating)
                .Distinct()
                .ToHashSet();
            bool hasLow = starSet.Any(s => s <= 2);
            bool hasHigh = starSet.Any(s => s >= 4);
            bool qualityOk = starSet.Count >= 3 && hasLow && hasHigh;
            if (!qualityOk)
            {
                throw new InvalidOperationException($"Place does not meet quality threshold: need ≥3 distinct star levels including one ≤2★ and one ≥4★.");
            }*/
            
            // Minimal sanity check: require at least 1 review
            if (gdetails.Reviews == null || gdetails.Reviews.Count == 0)
            {
                throw new InvalidOperationException("Place has no reviews to import.");
            }

            if (string.IsNullOrWhiteSpace(gdetails.PlaceId))
                throw new InvalidOperationException("Place details missing place_id");

            var location = Location.CreateFromGooglePlaces(gdetails);
            await _db.Locations.AddAsync(location);
            await _db.SaveChangesAsync();

            // Persist cached reviews explicitly (no EF navigation configured)
           /* if (gdetails.Reviews is not null && gdetails.Reviews.Count > 0)
            {
                // Be strict: keep at most one review per exact star rating 1..5
                var byStar = gdetails.Reviews
                    .Where(rv => rv.Rating >= 1 && rv.Rating <= 5)
                    .GroupBy(rv => rv.Rating)
                    .ToDictionary(g => g.Key, g => g.First());

                var now = DateTime.UtcNow;
                var cachedReviews = Enumerable.Range(1, 5)
                    .Where(s => byStar.ContainsKey(s))
                    .Select(s => byStar[s])
                    .Select(rv => new CachedReview
                    {
                        LocationId = location.Id,
                        StarRating = rv.Rating,
                        Text = rv.Text,
                        AuthorName = rv.AuthorName,
                        CachedAt = now,
                        LastUpdated = now
                    }).ToList();

                if (cachedReviews.Count > 0)
                {
                    _db.CachedReviews.AddRange(cachedReviews);
                }
                // Use Google’s aggregated rating when available
                location.AverageRating = gdetails.Rating;
                await _db.SaveChangesAsync();
            }*/
           
           if (gdetails.Reviews is not null && gdetails.Reviews.Count > 0)
           {
               var now = DateTime.UtcNow;

               var cachedReviews = gdetails.Reviews
                   .Where(rv => rv.Rating >= 1 && rv.Rating <= 5)
                   .Select(rv => new CachedReview
                   {
                       LocationId = location.Id,
                       StarRating = rv.Rating,
                       Text = rv.Text,
                       AuthorName = rv.AuthorName,
                       CachedAt = now,
                       LastUpdated = now
                   }).ToList();

               if (cachedReviews.Count > 0)
               {
                   _db.CachedReviews.AddRange(cachedReviews);
               }

               location.AverageRating = gdetails.Rating;
               await _db.SaveChangesAsync();
           }

            return location.Id;
        }

        private sealed class RawDetailsResponse
        {
            [JsonPropertyName("result")] public RawDetailsResult? Result { get; set; }
        }
        private sealed class RawDetailsResult
        {
            [JsonPropertyName("place_id")] public string? PlaceId { get; set; }
            [JsonPropertyName("name")] public string? Name { get; set; }
            [JsonPropertyName("formatted_address")] public string? FormattedAddress { get; set; }
            [JsonPropertyName("types")] public List<string>? Types { get; set; }
            [JsonPropertyName("rating")] public double? Rating { get; set; }
            [JsonPropertyName("user_ratings_total")] public int? UserRatingsTotal { get; set; }
            [JsonPropertyName("reviews")] public List<RawReview>? Reviews { get; set; }
            [JsonPropertyName("address_components")] public List<RawAddressComponent>? AddressComponents { get; set; }
            [JsonPropertyName("geometry")] public TextSearchGeometry? Geometry { get; set; }
        }
        private sealed class RawReview
        {
            [JsonPropertyName("author_name")] public string? AuthorName { get; set; }
            [JsonPropertyName("rating")] public int? Rating { get; set; }
            [JsonPropertyName("text")] public string? Text { get; set; }
            [JsonPropertyName("time")] public long? Time { get; set; }
        }
        private sealed class RawAddressComponent
        {
            [JsonPropertyName("long_name")] public string? LongName { get; set; }
            [JsonPropertyName("short_name")] public string? ShortName { get; set; }
            [JsonPropertyName("types")] public List<string>? Types { get; set; }
        }
    }
}
