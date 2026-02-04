using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using our_group.LocationDomain.Core.Entities;
using our_group.LocationDomain.Core.Interfaces;
using our_group.LocationDomain.Core.DTOs;
using our_group.LocationDomain.Infrastructure.Data;


namespace our_group.LocationDomain.Core.Interfaces
{

    public interface ILocationService
    {
        public LocationDto ToLocationDto(Location location);
        public Task<LocationDto> ImportFromGooglePlacesAsync(string googlePlaceId);





        public Task<IEnumerable<ReviewDto>> GetLocationReviewsAsync(int locationId);


        public Task<LocationDto> GetLocationAsync(int id);
       

        public  Task<LocationDetailsDto> GetLocationWithReviewsAsync(int id);


        public Task<List<LocationDto>> GetRandomLocationBatchAsync(int count,GameCriteria criteria);


        public Task<LocationDto> GetRandomLocationForGameAsync(GameCriteria criteria);
       


        public  Task<GuessValidationResult> ValidateGuessAsync(GuessValidationResult playerGuess);

        
    








    }

}

