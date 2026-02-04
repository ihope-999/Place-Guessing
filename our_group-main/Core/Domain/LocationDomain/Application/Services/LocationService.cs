using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using our_group.LocationDomain.Core.Entities;
using our_group.LocationDomain.Core.Enums;
using our_group.LocationDomain.Core.Interfaces;
using our_group.LocationDomain.Core.DTOs;
using our_group.LocationDomain.Infrastructure.Data;



namespace our_group.LocationDomain.Application.Services
{
    public class LocationService : ILocationService
    {

        private readonly LocationDbContext _context;
        private readonly IGooglePlacesService _googlePlacesService;
        public LocationService(LocationDbContext context, IGooglePlacesService googlePlacesService)
        {
            _context = context;
            _googlePlacesService = googlePlacesService;
        }




        //math
        public int CalculatePointsGainedFromGuess(double distance, double threshold)
        {


            var accuracy_of_the_player_guess = 1 - (distance / threshold);
            int pointsGained = (int)(accuracy_of_the_player_guess * 100);

            return pointsGained;

        }
        public double GetDistanceThreshold(LocationType type)
        {
            return type switch
            {

                (LocationType)1 => LocationConstants.LANDMARK_GUESS_THRESHOLD_KM,
                (LocationType)2 => LocationConstants.MUSEUM_GUESS_THRESHOLD,
                (LocationType)3 => LocationConstants.RESTAURANT_GUESS_THRESHOLD,
                (LocationType)4 => LocationConstants.PARK_GUESS_THRESHOLD,
                (LocationType)5 => LocationConstants.HISTORICAL_SITE_THRESHOLD,
                (LocationType)6 => LocationConstants.NATURAL_WONDER_THRESHOLD,
                (LocationType)7 => LocationConstants.RELIGIOUS_SITE_THRESHOLD,
                (LocationType)8 => LocationConstants.SHOPPNIG_SITE_THRESHOLD,
                (LocationType)9 => LocationConstants.ENTERTAINMENT_THRESHOLD,
                (LocationType)10 => LocationConstants.HOTEL_THRESHOLD,
                (LocationType)11 => LocationConstants.CITY_THRESHOLD,
                (LocationType)12 => LocationConstants.COUNTRY_THRESHOLD_KM,
                _ => 1000

            };
        }

        //changing to a locationdto format function
        public LocationDto ToLocationDto(Location location) => new(


                Id: location.Id,
                Name: location.Name,
                Country: location.Country,
                City: location.City,
                Latitude: location.Coordinates?.Latitude ?? 0.1,
                Longitude: location.Coordinates?.Longitude ?? 0.1,
                Category: location.Type.ToString(),
                Difficulty: (int)location.Difficulty


    );



        //getting the googleAPI data to our database
        public async Task<LocationDto> ImportFromGooglePlacesAsync(string googlePlaceId)
        {


            // try to look if it exists in the DB
            var existingLocation = await _context.Locations.FirstOrDefaultAsync(l => l.GooglePlaceId == googlePlaceId);

            if (existingLocation != null)
            {
                return ToLocationDto(existingLocation); // if exists just get the data
            }

            //else find the data and add to our DB
            var googlePlaceDetails = await _googlePlacesService.GetPlaceDetailsAsync(googlePlaceId);
            var location = Location.CreateFromGooglePlaces(googlePlaceDetails);
            var reviews = await _googlePlacesService.GetPlaceReviewsAsync(googlePlaceId);

            var cachedReviews = reviews.Select(r => new CachedReview
            {
                StarRating = r.Rating,
                Text = r.Text,
                AuthorName = r.AuthorName,
                CachedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            }).ToList();
            location.AddReview(cachedReviews);
            await _context.Locations.AddAsync(location);
            await _context.SaveChangesAsync();  // save changes

            return ToLocationDto(location);


        }


        

        
   
        
        public async Task<IEnumerable<ReviewDto>> GetLocationReviewsAsync(int locationId)
        {
            var location = await _context.Locations.Include(r => r.Reviews).FirstOrDefaultAsync(l => l.Id == locationId);

            if (location == null) throw new ArgumentException($"{locationId} is not found!!");

            return location.Reviews.Select(r => new ReviewDto(

                Id: r.Id,
                StarRating: r.StarRating,
                Text: r.Text,
                AuthorName: r.AuthorName,
                LastCachedAt: r.CachedAt
                )).ToList();
        }

        public async Task<LocationDto> GetLocationAsync(int id)
        {
            var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == id);

            if (location == null) throw new ArgumentException($"The place with the id {id} cannot be found! ");


            var locationDTO = ToLocationDto(location);
            return locationDTO;

        }

        public async Task<LocationDetailsDto> GetLocationWithReviewsAsync(int id)
        {
            var location = await _context.Locations.Include(l => l.Reviews).FirstOrDefaultAsync(l => l.Id == id);



            return new LocationDetailsDto(
                Id: location.Id,
                Name: location.Name,
                Region: location.Region,
                Country: location.Country,
                Address: location.Address,
                City: location.City,
                Coordinates: location.Coordinates,
                Category: location.Type.ToString(),
                Difficulty: (int)location.Difficulty,
                AverageRating: location.AverageRating,
                LastCached: location.LastCached,
                Reviews: location.Reviews.Select(r => new ReviewDto(


                    Id: r.Id,
                    Text: r.Text,
                    AuthorName: r.AuthorName,
                    LastCachedAt: r.CachedAt,
                    StarRating: r.StarRating


                    )).ToList()

                 ?? new List<ReviewDto>());
        }




        public async Task<LocationDto> GetRandomLocationForGameAsync(GameCriteria criteria)
        {

            var randomLocations = _context.Locations.Where(l => l.isActive);


            var count = await randomLocations.CountAsync();
            if (count == 0) throw new InvalidOperationException("No location is currently active ");

            var randomFunc = new Random();
            var randomLocationFound = await randomLocations.Skip(randomFunc.Next(0, count)).FirstOrDefaultAsync();

            var randomLocationFoundDTO = ToLocationDto(randomLocationFound!);

            return randomLocationFoundDTO;
        }


        public async Task<GuessValidationResult> ValidateGuessAsync(GuessValidationResult playerGuess)
        {
            var location = await _context.Locations.FindAsync(playerGuess.LocationId);
            if (location == null) throw new ArgumentException("Location not found! Please try again!");

            var distance = location.CalculateDistanceTo
                (new Coordinate(playerGuess.GuessLatitude, playerGuess.GuessLongitude));

            var threshold = GetDistanceThreshold(location.Type);
            bool isCorrect = distance <= threshold;

            var pointsGained = isCorrect ? CalculatePointsGainedFromGuess(distance, threshold) : 0;

            return new GuessValidationResult(
                LocationId: location.Id,
                isCorrect: isCorrect,
                pointsGained: pointsGained,
                threshold: threshold,
                distanceKM: distance,
                GuessLatitude: playerGuess.GuessLatitude,
                GuessLongitude: playerGuess.GuessLongitude,

                Message: isCorrect ?
                            $"You guessed it correct!, It is {distance}km away" :
                            $"You guessed it incorrectly... distance is {distance} but the threshold was {threshold}."


                );


        }

        public async Task<List<LocationDto>> GetRandomLocationBatchAsync(int count, GameCriteria criteria)
        {

            //check for activeness(true by default)
            var listOfActiveGames = _context.Locations.Where(l => l.isActive);


            //check for region and categories to filter locations
            if (!string.IsNullOrEmpty(criteria.Region))
            {
                listOfActiveGames = listOfActiveGames.Where(l => l.Region == criteria.Region);
            }

            if (!string.IsNullOrEmpty(criteria.Category))
            {
                listOfActiveGames = listOfActiveGames.Where(l => l.Type.ToString() == criteria.Category);
            }

            //checking for difficulty levels, mindifficulty<=wanted_location_difficulty<=max_difficulty
            if (criteria.MinDifficulty > 0)
            {
                listOfActiveGames = listOfActiveGames.Where(l => (int)l.Difficulty >= criteria.MinDifficulty);

            }
            if (criteria.MaxDifficulty > 0)
            {
                listOfActiveGames = listOfActiveGames.Where(l => (int)l.Difficulty <= criteria.MaxDifficulty);


            }

            //filter more with review count
            listOfActiveGames = listOfActiveGames.Where(l => l.Reviews.Count >= LocationConstants.MIN_REVIEWS_FOR_GAME);

            var listofActiveGames_length = await listOfActiveGames.CountAsync();

            if (listofActiveGames_length == 0)
            {
                throw new InvalidOperationException("No locations with the criterias set by the user/player, PlEASE TRY AGAIN");

            }

            if (listofActiveGames_length <= count)
            {
                var randomGamesFound = await listOfActiveGames.ToListAsync();

                return randomGamesFound.Select(ToLocationDto).ToList(); // select each game and do tolocationdto function make them a list
            }


            var random = new Random();
            var randomLocations = new List<Location>();

            //randomizing to filter left locations
            for (int i = 0; i < count && i < listofActiveGames_length; i++)
            {
                var skip = random.Next(0, listofActiveGames_length - i);
                var location = await listOfActiveGames.Skip(skip).FirstOrDefaultAsync();

                if (location != null)
                {
                    randomLocations.Add(location);
                    listOfActiveGames = listOfActiveGames.Where(l => l.Id != location.Id);
                    listofActiveGames_length--;
                }
            }

            return randomLocations.Select(ToLocationDto).ToList();
        }


   











    }
    


    


}