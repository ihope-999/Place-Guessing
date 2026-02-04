using Microsoft.EntityFrameworkCore;
using our_group.LocationDomain.Core.Entities;
using our_group.LocationDomain.Core.Enums;
using our_group.LocationDomain.Core.DTOs;
using our_group.LocationDomain.Core.Interfaces;
using System;



namespace our_group.LocationDomain.Infrastructure.Data
{

public class LocationDbContext : DbContext
{

        public LocationDbContext(DbContextOptions<LocationDbContext> options) : base(options) { }


    public DbSet<Location> Locations => Set<Location>();
    public DbSet<CachedReview> CachedReviews => Set<CachedReview>();



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // All LocationDbContext tables live under the 'location' schema
        modelBuilder.HasDefaultSchema("location");

        // Coordinate is a value object owned by Location (no separate PK)
        modelBuilder.Entity<Location>().OwnsOne(l => l.Coordinates);
        
        modelBuilder.Entity<Location>()
            .HasMany(l => l.Reviews)
            .WithOne()
            .HasForeignKey(r => r.LocationId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}



        // If we need to do create tables manually we can use onmodelcreating so this is for later if we want to use it! 
 }



