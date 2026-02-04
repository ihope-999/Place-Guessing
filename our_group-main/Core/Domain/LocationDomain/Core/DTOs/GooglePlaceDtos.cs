using System.Collections.Generic;
using System.Runtime;
using System.Text.Json.Serialization;

namespace our_group.LocationDomain.Core.DTOs
{
    public class GoogleLocation
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }
        [JsonPropertyName("lng")]
        public double Lng { get; set; }

    }

    public record GooglePlaceReview(
        string? AuthorName,
        int Rating,
        string? Text,
        long? Time
    );

    public class AddressComponent
    {
        [JsonPropertyName("long_name")]
        public string LongName { get; set; } = string.Empty;
        [JsonPropertyName("short_name")]
        public string ShortName { get; set; } = string.Empty;
        [JsonPropertyName("types")]
        public List<string> Types { get; set; } = new();
    }

    public record GooglePlaceDetails(
        string? PlaceId,
        string? PlaceName,
        string? Address,
        GoogleLocation Location,
        double Rating,
        int UserRatingsTotal,
        List<string> Types,
        List<GooglePlaceReview> Reviews,
        List<AddressComponent>? AddressComponents = null
    );


    public class GooglePlaceResult
    {

        [JsonPropertyName("place_id")]
        public string PlaceId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("geometry")]
        public GoogleGeometry Geometry { get; set; } = new();

        [JsonPropertyName("formatted_address")]
        public string FormattedAddress { get; set; } = string.Empty;

        [JsonPropertyName("types")]
        public List<string> Types { get; set; } = new();

        [JsonPropertyName("rating")]
        public double? Rating { get; set; }

        [JsonPropertyName("user_ratings_total")]
        public int? UserRatingsTotal { get; set; }

        [JsonPropertyName("reviews")]
        public List<GooglePlaceReview>? Reviews { get; set; }

        [JsonPropertyName("address_components")]
        public List<AddressComponent> AdddressComponents { get; set; } = new();

        [JsonPropertyName("photos")]
        public List<GooglePhoto>? Photos { get; set; }







    }




    public class GooglePlaceDetailsResponse
    {
        [JsonPropertyName("result")]
        public GooglePlaceResult? Result { get; set; }

    }

    public class GoogleGeometry
    {
        [JsonPropertyName("location")]
        public GoogleLocation Location { get; set; } = new();


    }
    public class GooglePhoto
    {
        [JsonPropertyName("photo_reference")]
        public string PhotoReference { get; set; } = string.Empty;

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }
    }



}