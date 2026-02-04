using System;  
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using our_group.LocationDomain.Core.Enums;
using our_group.LocationDomain.Core.DTOs;
using our_group.LocationDomain.Core.Interfaces;

using Microsoft.AspNetCore.Mvc.TagHelpers;
using System.Reflection.Metadata.Ecma335;


namespace our_group.LocationDomain.Core.Entities
{


	public class Location
	{

		private Location() { } // EF
							   //Basic info
		public int Id { get; set; }
		public string? GooglePlaceId { get; set; } = string.Empty; // This is for API Id!!!
		public string Name { get; set; } = string.Empty;
		public string? Address { get; set; } = string.Empty;
		public Coordinate? Coordinates { get; set; } // defined in another file
		public double AverageRating { get; set; }
		public bool isActive { get; set; } = true; // this value can be set false if you dont want other players to get a certain location


		

		//Geographic values 
		public string? Region { get; set; }
		public string? Country { get; set; }
		public string? City { get; set; }

		//type,difficulty
		public LocationType Type { get; set; }
		public DifficultyLevel Difficulty { get; set; }


		// creationtime and lastcreationtime
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime LastCached { get; set; } = DateTime.UtcNow;

		public List<CachedReview> Reviews
		{
			get => _reviews;
			private set => _reviews = value;
		}

		// 2 different same list that has the same memory address used for different purposes
		/*public*/private List<CachedReview> _reviews = new(); // later can be changed to a private one after we address other issues!! 




		public static Location CreateFromGooglePlaces(GooglePlaceDetails googlePlace)
		{
			if (string.IsNullOrEmpty(googlePlace.PlaceId)) throw new ArgumentException("Google Place ID is required");
			if (string.IsNullOrEmpty(googlePlace.PlaceName)) throw new ArgumentException("Location name is required");

			var location = new Location
			{
				GooglePlaceId = googlePlace.PlaceId,
				Name = googlePlace.PlaceName,
				Address = googlePlace.Address,
				Coordinates = new Coordinate(googlePlace.Location.Lat, googlePlace.Location.Lng),
				Type = DetermineLocationType(googlePlace.Types),


				// getting city country and region is a bit more complicated than others
				City = GetAddressComponent(googlePlace.AddressComponents, "locality"),
				Country = GetAddressComponent(googlePlace.AddressComponents, "country"),
				Region = GetAddressComponent(googlePlace.AddressComponents, "administrative_area_level_1")

				//
			};

			location.Difficulty = DetermineInitialDifficulty(googlePlace);
			location.CreatedAt = DateTime.UtcNow;
			location.LastCached = DateTime.UtcNow;

            return location;
        }

        private static string? GetAddressComponent(List<AddressComponent>? components, string type)
		{
			return components?
				.FirstOrDefault(c => c.Types.Contains(type))?
				.LongName;
		}

		public double CalculateDistanceTo(Coordinate other)
		{
			return other.CalculateDistance(
				Coordinates?.Latitude ?? 0,
				Coordinates?.Longitude ?? 0,
				other?.Latitude ?? 0,
				other?.Longitude ?? 0);
		}


		// this part can be configured later since I had a hard time doing this without using API and see the results
		

		public bool IsGuessValid(Coordinate guess, double pointOF)
		{
			var distance = CalculateDistanceTo(guess);
			return distance <= pointOF;
		}

		public void UpdateDifficultyBasedOnReviews()
		{
			if (!_reviews.Any()) return;

			AverageRating = _reviews.Average(r => r.StarRating);
			var reviewCount = _reviews.Count;
			var difficultyNumber = AverageRating * reviewCount / 100;

			Difficulty = difficultyNumber switch
			{
				> 2.0 => DifficultyLevel.SoEasy_Difficulty,
				> 1.5 => DifficultyLevel.Easy_Difficulty,
				> 1.0 => DifficultyLevel.Medium_Difficulty,
				> 0.5 => DifficultyLevel.Hard_Difficulty,
				_ => DifficultyLevel.VeryHard_Difficulty,

			};
		}

		public void AddReview(IEnumerable<CachedReview> reviews) {

			foreach (var review in reviews)
			{
				_reviews.Add(review);

			}
			LastCached = DateTime.UtcNow;

		}

        public static LocationType DetermineLocationType(List<string> googlePlaceTypes)
        {
            if (googlePlaceTypes == null || !googlePlaceTypes.Any())
                return LocationType.Landmark;

            var types = googlePlaceTypes.Select(t => t.ToLowerInvariant()).ToList();

            foreach (var type in types)
            {
                switch (type)
                {
                    // Museum types
                    case "museum":
                    case "art_gallery":
                        return LocationType.Museum;

                    // Restaurant types
                    case "restaurant":
                    case "cafe":
                    case "bakery":
                    case "bar":
                    case "food":
                    case "meal_takeaway":
                    case "meal_delivery":
                        return LocationType.Restaurant;

                    // Park types
                    case "park":
                    case "amusement_park":
                    case "national_park":
                    case "theme_park":
                        return LocationType.Park;

                    // Historical sites
                    case "historic_site":
                    case "monument":
                    case "memorial":
                        return LocationType.HistoricalSite;

                    // Natural wonders
                    case "natural_feature":
                    case "campground":
                    case "zoo":
                    case "aquarium":
                        return LocationType.NaturalWonder;

                    // Religious sites
                    case "church":
                    case "mosque":
                    case "synagogue":
                    case "hindu_temple":
                    case "place_of_worship":
                        return LocationType.ReligiousSite;

                    // Shopping
                    case "shopping_mall":
                    case "department_store":
                    case "clothing_store":
                    case "jewelry_store":
                    case "shoe_store":
                    case "home_goods_store":
                    case "electronics_store":
                    case "book_store":
                        return LocationType.Shopping;

                    // Entertainment
                    case "movie_theater":
                    case "stadium":
                    case "casino":
                    case "bowling_alley":
                    case "night_club":
                    case "tourist_attraction":
                        return LocationType.Entertainment;

                    // Hotel
                    case "hotel":
                    case "lodging":
                    case "motel":
                    case "bed_and_breakfast":
                        return LocationType.Hotel;

                    // City
                    case "locality":
                    case "administrative_area_level_1":
                    case "administrative_area_level_2":
                        return LocationType.City;

                    // Country
                    case "country":
                    case "administrative_area_level_0":
                        return LocationType.Country;

                    // Landmark
                    case "point_of_interest":
                    case "establishment":
                        return LocationType.Landmark;
                }
            }
			//default
            return LocationType.Landmark;
        }

        public static DifficultyLevel DetermineInitialDifficulty(GooglePlaceDetails googlePlace)
		{

			var reviewCount = googlePlace.UserRatingsTotal;


			if (reviewCount > 100000) return DifficultyLevel.SoEasy_Difficulty;
			if (reviewCount > 50000 ) return DifficultyLevel.Easy_Difficulty;
			if (reviewCount > 10000 ) return DifficultyLevel.Medium_Difficulty;
			if (reviewCount > 1000) return DifficultyLevel.Hard_Difficulty;
			if (reviewCount >= 0) return DifficultyLevel.VeryHard_Difficulty;
            return DifficultyLevel.VeryHard_Difficulty;

        }




	}






}



