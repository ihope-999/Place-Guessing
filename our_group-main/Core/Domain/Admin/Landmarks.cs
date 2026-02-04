using System;
// landmar dto for the admin
namespace our_group.Core.Domain.Admin.Landmarks
{
    public class LandmarkDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }


    }
}



// ReviewDto: Id, StarRating, Text, AuthorName, CachedAt 
namespace our_group.Core.Domain.Admin.Landmarks
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public int StarRating { get; set; }
        public string Text { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CachedAt { get; set; }
    }
}
