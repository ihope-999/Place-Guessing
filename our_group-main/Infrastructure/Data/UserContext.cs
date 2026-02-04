using Microsoft.EntityFrameworkCore;
using our_group.Core.Domain.User;
using our_group.Core.Domain.Admin;

public class UserContext : DbContext
{
    public UserContext(DbContextOptions<UserContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Admin> Admins => Set<Admin>();
    //public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<Friendship> Friendships => Set<Friendship>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("user");
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("Users");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).IsRequired();
            b.Property(x => x.PlayerId)
                .ValueGeneratedOnAdd();
            b.Property(x => x.UserName).IsRequired();
            b.Property(x => x.NormalizedUserName).IsRequired();
            b.Property(x => x.Email).IsRequired();
            b.Property(x => x.NormalizedEmail).IsRequired();
            b.Property(x => x.PasswordHash).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.Avatar).HasMaxLength(256);
            b.Property(x => x.FavoriteRegions).HasMaxLength(512);
            b.Property(x => x.Rank).HasDefaultValue(0).IsRequired();

            b.HasIndex(x => x.PlayerId).IsUnique();
            b.HasIndex(x => x.NormalizedUserName).IsUnique();
            b.HasIndex(x => x.NormalizedEmail).IsUnique();
            b.HasIndex(x => x.Rank);
        });

        modelBuilder.Entity<Admin>(b =>
        {
            b.ToTable("Admins");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).IsRequired();
            b.Property(x => x.UserName).IsRequired();
            b.Property(x => x.NormalizedUserName).IsRequired();
            b.Property(x => x.PasswordHash).IsRequired();
            b.Property(x => x.MustChangePassword).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();

            b.HasIndex(x => x.NormalizedUserName).IsUnique();
        });

        // follow entity
       /* modelBuilder.Entity<Follow>(b =>
        {
            b.ToTable("Follows");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).IsRequired();
            b.Property(x => x.FollowerId).IsRequired();
            b.Property(x => x.FolloweeId).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();

            b.HasIndex(x => new { x.FollowerId, x.FolloweeId }).IsUnique();
        });*/

        modelBuilder.Entity<Friendship>(b =>
        {
            b.ToTable("Friendships");
            b.HasKey(f => f.Id);
            b.Property(f => f.RequesterId).IsRequired();
            b.Property(f => f.ReceiverId).IsRequired();

            b.HasIndex(f => new { f.RequesterId, f.ReceiverId }).IsUnique();
        });
    }
}
