using Microsoft.EntityFrameworkCore;
using DengueAlertLK.Models.Entities;
namespace DengueAlertLK.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<DengueCase> DengueCases => Set<DengueCase>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AppUser>().HasIndex(x => x.Username).IsUnique();
        b.Entity<Area>().HasIndex(x => x.Name).IsUnique();
        b.Entity<Area>().HasMany(a => a.Cases).WithOne(c => c.Area)
            .HasForeignKey(c => c.AreaId).OnDelete(DeleteBehavior.Restrict);
    }
}