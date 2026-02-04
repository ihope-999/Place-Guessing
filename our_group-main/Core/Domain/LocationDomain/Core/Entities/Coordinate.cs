using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace our_group.LocationDomain.Core.Entities
{


    public class Coordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; } 


        public Coordinate(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90)
            {
                throw new ArgumentException("Latitude must be -90 to 90");
            }
            if (longitude < -180 || longitude > 180)
            {

                throw new ArgumentException("Longitude must be -180 to 180!");
            }
            Latitude = latitude;
            Longitude = longitude;
        }
        public override string ToString()
        {
            return $"{Latitude}, {Longitude}";
        }

        public override bool Equals(object? obj)
        {
            return obj is Coordinate other &&
                Latitude == other.Latitude &&
                Longitude == other.Longitude;
        }

        public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);




        public static double ToRadians(double degrees) => degrees * Math.PI / 180;

        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // earth radius km to calculate stuff
            
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }



    }
}