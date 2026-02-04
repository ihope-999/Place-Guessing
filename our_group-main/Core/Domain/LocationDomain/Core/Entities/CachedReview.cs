using System;
using System.ComponentModel.DataAnnotations;
using our_group.LocationDomain.Core.DTOs;
using our_group.LocationDomain.Core.Entities;


namespace our_group.LocationDomain.Core.Entities
{

	public class CachedReview
	{
		public int Id { get; set; }
		public int LocationId { get; set; }
		[Range(1, 5)]
		public int StarRating { get; set; }
		public string? Text { get; set; } = string.Empty;
		public string? AuthorName { get; set; } = string.Empty;
		public bool Flagged { get; set; } = false;
		

		public DateTime? ReviewDate { get; set; }
		public DateTime CachedAt { get; set; } = DateTime.UtcNow;
		public DateTime LastUpdated { get; set; }





	}

}
