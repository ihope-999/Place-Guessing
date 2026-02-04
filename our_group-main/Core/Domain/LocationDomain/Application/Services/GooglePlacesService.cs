using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using our_group.LocationDomain.Core.Entities;
using our_group.LocationDomain.Core.DTOs;
using our_group.LocationDomain.Core.Interfaces;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace our_group.LocationDomain.Application.Services
{

    public class GooglePlacesService : IGooglePlacesService
    {


        
        private readonly HttpClient _httpClient; // to make requests or getting responses from the API
        private readonly GoogleMapsSettings _settings; // API key and URLs of the site is here




        // injection
        public GooglePlacesService(HttpClient httpClient, IOptions<GoogleMapsSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }



        

        public async Task<GooglePlaceDetails> GetPlaceDetailsAsync(string placeId)
        {


           
                //getting the location url according to the placeId
                var url = $"{_settings.PlacesBaseUrl}details/json?place_id={placeId}&key={_settings.ApiKey}&fields=name,geometry,formatted_address,types,rating,user_ratings_total,photos,reviews,address_components";

                
                var response = await _httpClient.GetAsync(url); // get the data
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(); // get the content that is useful for us from the data 

                var googleResponse = JsonSerializer.Deserialize<GooglePlaceDetailsResponse>(content); // change it to a readable nice format


                if (googleResponse?.Result == null)
                {
                    throw new InvalidOperationException("Invalid API response");

                }

                return MapToGooglePlaceDetails(googleResponse.Result); // change it to a format that we created so that we get the data we want specifically in the way we know.



         }
       

            public async Task<List<GooglePlaceReview>> GetPlaceReviewsAsync(string PlaceId)
            {


          
            
                var url = $"{_settings.PlacesBaseUrl}details/json?place_id={PlaceId}&key={_settings.ApiKey}&fields=reviews";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var googleResponse = JsonSerializer.Deserialize<GooglePlaceDetailsResponse>(content);

            
                var Location = MapToGooglePlaceDetails(googleResponse.Result!);

                return Location.Reviews;


            }
            
        



        private GooglePlaceDetails MapToGooglePlaceDetails(GooglePlaceResult result)
        {

            return new GooglePlaceDetails(
                  PlaceId: result.PlaceId,
                  PlaceName: result.Name,
                  Address: result.FormattedAddress,
                  Location: new GoogleLocation{Lat =result.Geometry.Location.Lat, Lng =result.Geometry.Location.Lng},
                  Rating: result.Rating ?? 0,
                  UserRatingsTotal: result.UserRatingsTotal ?? 0,
                  Types: result.Types ?? new List<string>(),
                  Reviews: result.Reviews


                );

        }



    }
}





   


