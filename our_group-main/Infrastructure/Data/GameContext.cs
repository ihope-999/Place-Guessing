using MediatR;
using Microsoft.EntityFrameworkCore;
using our_group.Core.Domain.Game;

namespace our_group.Infrastructure.Data;

public class GameContext : DbContext
{
    // private readonly IMediator _mediator; 
    public GameContext(DbContextOptions<GameContext> options) : base(options) { }

    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<Round> Rounds { get; set; } = null!;
    public DbSet<Location> Locations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("game");

        modelBuilder.Entity<Round>(r =>
        {
            // Owned collection: Answers
            r.OwnsMany(x => x.Answers, answers =>
            {
                answers.ToTable("PlayerGuess", "game");

                // EF needs shadow key for owned collection elements
                answers.Property<int>("Id");
                answers.HasKey("RoundId", "Id");

                // Foreign key back to Round
                answers.WithOwner().HasForeignKey("RoundId");

                // Map value properties
                answers.Property(a => a.UserId).IsRequired();
                answers.Property(a => a.Lat).IsRequired();
                answers.Property(a => a.Lng).IsRequired();
            });

            // WinnerId and WinnerName are lists => stored as arrays in PostgreSQL
            r.Property(x => x.WinnerId).HasColumnType("integer[]");
            r.Property(x => x.WinnerName).HasColumnType("text[]");
        });

        modelBuilder.Entity<Location>()
            .OwnsMany(x => x.Reviews);
        modelBuilder.Entity<Location>().ToTable("Locations", "game");
        modelBuilder.Entity<Location>().HasKey(l => l.Id);

       /*
        modelBuilder.Entity<Game>()
            .HasMany(g => g.Players)
            .WithOne(p => p.Game)
            .HasForeignKey(p => p.GameId)
            .OnDelete(DeleteBehavior.Cascade);
        */
       modelBuilder.Entity<Player>()
           .HasKey(p => p.Id); 
       
        modelBuilder.Entity<Player>()
            .HasOne(p => p.Game)
            .WithMany(g => g.Players)
            .HasForeignKey(p => p.GameId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Player>().ToTable("Players", "game");
        modelBuilder.Ignore<our_group.Core.Domain.User.User>();

        base.OnModelCreating(modelBuilder);
    }
}
