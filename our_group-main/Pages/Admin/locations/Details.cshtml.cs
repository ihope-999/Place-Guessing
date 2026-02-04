using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using our_group.LocationDomain.Core.Interfaces;
using our_group.LocationDomain.Infrastructure.Data;
using our_group.LocationDomain.Core.DTOs;
using our_group.LocationDomain.Core.Entities;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;

namespace our_group.Pages.Admin.Locations
{
    public record LandmarkAdminState(
        bool IsActive = true,
        bool IsBroken = false,
        bool IsInappropriate = false,
        int? ManualDifficulty = null,
        string? CategoryOverride = null
    );

    public class DetailsModel : PageModel
    {
        private readonly ILocationService _locationService;
        private readonly LocationDbContext _db;
        private readonly IHttpClientFactory _httpFactory;
        private readonly GoogleMapsSettings _gmaps;

        // In-memory admin state for flags/quality not modeled in DB
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, LandmarkAdminState> _admin = new();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, (int? MinStars, int? MinLength)> _quality = new();

        public DetailsModel(ILocationService locationService, LocationDbContext db, IHttpClientFactory httpFactory, IOptions<GoogleMapsSettings> gmaps)
        {
            _locationService = locationService;
            _db = db;
            _httpFactory = httpFactory;
            _gmaps = gmaps.Value;
        }

        [FromRoute]
        public int Id { get; set; }

        public LocationDetailsDto? Item { get; set; }
        public List<ReviewDto> Reviews { get; set; } = new();
        public LandmarkAdminState AdminState { get; set; }
        public HashSet<int> FlaggedReviewIds { get; set; } = new();

        public (int? MinStars, int? MinLength) QualityThresholds { get; set; }

        [BindProperty]
        public AdminInputModel AdminInput { get; set; } = new();

        [BindProperty]
        public QualityInputModel QualityInput { get; set; } = new();

        public class AdminInputModel
        {
            public bool IsActive { get; set; }
            public bool IsBroken { get; set; }
            public bool IsInappropriate { get; set; }
            public int? ManualDifficulty { get; set; }
            public string? CategoryOverride { get; set; }
        }

        public class QualityInputModel
        {
            public int? MinStars { get; set; }
            public int? MinLength { get; set; }
        }

        public Dictionary<int, int> StarBuckets { get; set; } = new();
        public int? SelectedRating { get; set; }

        public async Task<IActionResult> OnGetAsync(int id, int? rating)
        {
            // Ensure the PageModel Id property is set for form posts
            this.Id = id;
            // Avoid LocationService.Include on a field. Load directly via DbContext.
            var loc = await _db.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
            if (loc is null) return NotFound();

            var reviews = await _db.CachedReviews.AsNoTracking()
                .Where(r => r.LocationId == id)
                .ToListAsync();

            var reviewDtos = reviews.Select(r => new ReviewDto(
                    Id: r.Id,
                    StarRating: r.StarRating,
                    Text: r.Text,
                    AuthorName: r.AuthorName,
                    LastCachedAt: r.CachedAt
                )).ToList();

            // Build star buckets 1..5
            StarBuckets = Enumerable.Range(1, 5).ToDictionary(s => s, s => reviewDtos.Count(rv => rv.StarRating == s));
            if (rating is >= 1 and <= 5)
            {
                SelectedRating = rating;
                reviewDtos = reviewDtos.Where(r => r.StarRating == rating.Value).ToList();
            }

            Item = new LocationDetailsDto(
                Id: loc.Id,
                Name: loc.Name,
                Address: loc.Address,
                Coordinates: loc.Coordinates ?? new Coordinate(0, 0),
                Region: loc.Region ?? string.Empty,
                Country: loc.Country ?? string.Empty,
                City: loc.City ?? string.Empty,
                Category: loc.Type.ToString(),
                Difficulty: (int)loc.Difficulty,
                AverageRating: loc.AverageRating,
                LastCached: loc.LastCached,
                Reviews: reviewDtos
            );
            Reviews = Item.Reviews ?? new List<ReviewDto>();
            AdminState = _admin.GetValueOrDefault(id, new LandmarkAdminState());
            // Load flagged review ids from DB
            FlaggedReviewIds = (await _db.CachedReviews.AsNoTracking()
                .Where(r => r.LocationId == id && r.Flagged)
                .Select(r => r.Id)
                .ToListAsync()).ToHashSet();
            QualityThresholds = _quality.GetValueOrDefault(id, (null, null));
            AdminInput = new AdminInputModel
            {
                IsActive = AdminState.IsActive,
                IsBroken = AdminState.IsBroken,
                IsInappropriate = AdminState.IsInappropriate,
                ManualDifficulty = AdminState.ManualDifficulty,
                CategoryOverride = AdminState.CategoryOverride
            };
            QualityInput = new QualityInputModel
            {
                MinStars = QualityThresholds.MinStars,
                MinLength = QualityThresholds.MinLength
            };
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id)
        {
            // Persist to in-memory admin state for non-DB flags
            var current = _admin.GetValueOrDefault(id, new LandmarkAdminState());
            _admin[id] = current with {
                IsActive = AdminInput.IsActive,
                IsBroken = AdminInput.IsBroken,
                IsInappropriate = AdminInput.IsInappropriate,
                ManualDifficulty = AdminInput.ManualDifficulty,
                CategoryOverride = string.IsNullOrWhiteSpace(AdminInput.CategoryOverride) ? null : AdminInput.CategoryOverride
            };

            // Reflect DB-backed fields directly
            var loc = await _db.Locations.FirstOrDefaultAsync(l => l.Id == id);
            if (loc is not null)
            {
                loc.isActive = AdminInput.IsActive;
                if (AdminInput.ManualDifficulty is >= 1 and <= 5)
                {
                    loc.Difficulty = (our_group.LocationDomain.Core.Enums.DifficultyLevel)AdminInput.ManualDifficulty.Value;
                }
                if (!string.IsNullOrWhiteSpace(AdminInput.CategoryOverride) &&
                    System.Enum.TryParse<our_group.LocationDomain.Core.Enums.LocationType>(AdminInput.CategoryOverride, true, out var cat))
                {
                    loc.Type = cat;
                }
                await _db.SaveChangesAsync();
            }
            return RedirectToPage("/Admin/locations/Details", new { id });
        }

        public async Task<IActionResult> OnPostFlagAsync(int id, int reviewId, bool flag)
        {
            var review = await _db.CachedReviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.LocationId == id);
            if (review is not null)
            {
                review.Flagged = flag;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage("/Admin/locations/Details", new { id });
        }

        public async Task<IActionResult> OnPostQualityAsync(int id)
        {
            // Normalize invalid values
            int? minStars = QualityInput.MinStars is >= 1 and <= 5 ? QualityInput.MinStars : null;
            int? minLength = QualityInput.MinLength is > 0 and <= 10000 ? QualityInput.MinLength : null;
            _quality[id] = (minStars, minLength);
            return RedirectToPage("/Admin/locations/Details", new { id });
        }

        public async Task<IActionResult> OnPostRefreshAsync(int id)
        {
            var loc = await _db.Locations.FirstOrDefaultAsync(l => l.Id == id);
            if (loc is null) return NotFound();
            if (string.IsNullOrWhiteSpace(loc.GooglePlaceId))
            {
                return RedirectToPage("/Admin/locations/Details", new { id });
            }

            try
            {
                var client = _httpFactory.CreateClient();
                var url = $"{_gmaps.PlacesBaseUrl}details/json?place_id={Uri.EscapeDataString(loc.GooglePlaceId)}&key={_gmaps.ApiKey}&fields=reviews,rating,user_ratings_total";
                var res = await client.GetAsync(url);
                res.EnsureSuccessStatusCode();
                var json = await res.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<RefreshDetailsResponse>(json);
                var reviews = data?.Result?.Reviews ?? new List<RefreshReview>();

                var existing = await _db.CachedReviews.Where(r => r.LocationId == id).ToListAsync();
                // Build a map using a stable content-based key to preserve flags
                static string Key(int star, string? text, string? author) => $"{star}\u0001{(text ?? string.Empty).Trim()}\u0001{(author ?? string.Empty).Trim()}";
                var flagMap = existing.ToDictionary(
                    r => Key(r.StarRating, r.Text, r.AuthorName),
                    r => r.Flagged
                );
                if (reviews.Count > 0)
                {
                    // Be strict: keep at most one review per exact star rating 1..5
                    var byStar = reviews
                        .Where(rv => (rv.Rating ?? 0) >= 1 && (rv.Rating ?? 0) <= 5)
                        .GroupBy(rv => rv.Rating!.Value)
                        .ToDictionary(g => g.Key, g => g.First());

                    var now = DateTime.UtcNow;
                    var newRows = Enumerable.Range(1, 5)
                        .Where(s => byStar.ContainsKey(s))
                        .Select(s => byStar[s])
                        .Select(rv => new CachedReview
                        {
                            LocationId = id,
                            StarRating = rv.Rating ?? 0,
                            Text = rv.Text,
                            AuthorName = rv.AuthorName,
                            CachedAt = now,
                            LastUpdated = now,
                            Flagged = flagMap.GetValueOrDefault(Key(rv.Rating ?? 0, rv.Text, rv.AuthorName), false)
                        }).ToList();

                    if (newRows.Count > 0)
                    {
                        // Replace the set atomically: remove old then add new, preserving flags based on content
                        if (existing.Count > 0)
                        {
                            _db.CachedReviews.RemoveRange(existing);
                        }
                        await _db.CachedReviews.AddRangeAsync(newRows);
                    }

                    // Option A: require ≥3 distinct star buckets and at least one low (≤2★) and one high (≥4★)
                    var starSet = byStar.Keys.ToHashSet();
                    bool hasLow = starSet.Any(s => s <= 2);
                    bool hasHigh = starSet.Any(s => s >= 4);
                    bool qualityOk = starSet.Count >= 3 && hasLow && hasHigh;
                    if (!qualityOk)
                    {
                        loc.isActive = false;
                    }
                }
                // Use Google’s aggregated rating if provided
                if (data?.Result?.Rating is not null)
                {
                    loc.AverageRating = data.Result.Rating.Value;
                }
                loc.LastCached = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            catch
            {
                // ignore errors for now
            }
            return RedirectToPage("/Admin/locations/Details", new { id });
        }

        private sealed class RefreshDetailsResponse
        {
            [JsonPropertyName("result")] public RefreshDetailsResult? Result { get; set; }
        }
        private sealed class RefreshDetailsResult
        {
            [JsonPropertyName("reviews")] public List<RefreshReview>? Reviews { get; set; }
            [JsonPropertyName("rating")] public double? Rating { get; set; }
        }
        private sealed class RefreshReview
        {
            [JsonPropertyName("author_name")] public string? AuthorName { get; set; }
            [JsonPropertyName("rating")] public int? Rating { get; set; }
            [JsonPropertyName("text")] public string? Text { get; set; }
        }
    }
}
