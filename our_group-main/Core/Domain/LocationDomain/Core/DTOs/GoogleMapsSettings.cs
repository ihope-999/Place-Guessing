using System;

namespace our_group.LocationDomain.Core.DTOs
{


    public class GoogleMapsSettings
    {
        public string ApiKey { get; set; } = string.Empty;

        public string PlacesBaseUrl { get; set; } = "https://maps.googleapis.com/maps/api/place/";
        public string GeocodingBaseUrl{ get; set; } = string.Empty;
        public int CacheDurationHours{ get; set; }
        public int RateLimitPerMinute { get; set; }
        
    }
}
