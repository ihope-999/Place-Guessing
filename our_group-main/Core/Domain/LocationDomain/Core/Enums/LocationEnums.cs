

namespace our_group.LocationDomain.Core.Enums
{

	public enum LocationType
	{
		Landmark = 1,
		Museum = 2,
		Restaurant = 3,
		Park = 4,
		HistoricalSite =5,
		NaturalWonder = 6,
		ReligiousSite = 7,
		Shopping = 8,
		Entertainment = 9,
		Hotel = 10,
		City = 11,
		Country = 12

	}

	public enum DifficultyLevel
	{
		SoEasy_Difficulty = 1,
		Easy_Difficulty = 2,
		Medium_Difficulty = 3,
		Hard_Difficulty = 4,
		VeryHard_Difficulty = 5
	}

	public static class LocationConstants
	{
		public const int MIN_REVIEWS_FOR_GAME = 5;
		public const int CACHE_DURATION_DAYS = 30;

		//random thresholds i came up with, we can update it later to a more accurate version
		public const double COUNTRY_THRESHOLD_KM = 100;
		public const double LANDMARK_GUESS_THRESHOLD_KM = 5;
		public const double MUSEUM_GUESS_THRESHOLD = 2;
        public const double PARK_GUESS_THRESHOLD= 30;
        public const double RELIGIOUS_SITE_THRESHOLD = 60;
        public const double SHOPPNIG_SITE_THRESHOLD = 70;
        public const double ENTERTAINMENT_THRESHOLD = 30;
        public const double HOTEL_THRESHOLD = 80;
        public const double NATURAL_WONDER_THRESHOLD = 110;
        public const double HISTORICAL_SITE_THRESHOLD = 34;
        public const double SHOPPING_SITE_THRESHOLD = 33;
        public const double RESTAURANT_GUESS_THRESHOLD = 100.0;
        public const double CITY_THRESHOLD = 100;




    }
}