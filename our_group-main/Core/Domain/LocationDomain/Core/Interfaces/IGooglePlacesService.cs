using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using our_group.LocationDomain.Core.Entities;
using our_group.LocationDomain.Core.DTOs;
using our_group.LocationDomain.Infrastructure.Data;



namespace our_group.LocationDomain.Core.Interfaces
{


    public interface IGooglePlacesService
    {

        Task<GooglePlaceDetails> GetPlaceDetailsAsync(string placeId);
        Task<List<GooglePlaceReview>> GetPlaceReviewsAsync(string placeId);


    }

}