using backend_application.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_application.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Building>()
            .HasMany(b => b.Users)
            .WithMany(u => u.Buildings)
            .UsingEntity<BuildingUser>(j => j.ToTable("BuildingUser"));
        
        modelBuilder.Entity<Building>()
            .HasMany(b => b.Users)
            .WithMany(u => u.Buildings)
            .UsingEntity(j => j.ToTable("BuildingUser"));
        
        modelBuilder.Entity<Room>()
            .HasOne(r => r.Building)
            .WithMany(b => b.Rooms)
            .HasForeignKey(r => r.BuildingId);
        
        modelBuilder.Entity<Device>()
            .HasOne(d => d.Room)
            .WithMany(r => r.Devices)
            .HasForeignKey(d => d.RoomId);
    }

    public DbSet<BuildingUser> BuildingUser { get; set; }
    public DbSet<Building> Buildings { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<User> Users { get; set; }
}