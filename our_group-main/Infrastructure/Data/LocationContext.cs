using Microsoft.EntityFrameworkCore;

public class LocationContext : DbContext{
    public LocationContext(DbContextOptions<LocationContext> options) : base(options){}

    protected override void OnModelCreating(ModelBuilder modelBuilder){
        modelBuilder.HasDefaultSchema("location"); 
        base.OnModelCreating(modelBuilder);
    }
}