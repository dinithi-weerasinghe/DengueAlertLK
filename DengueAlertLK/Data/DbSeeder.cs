using Microsoft.AspNetCore.Identity;
using DengueAlertLK.Models.Entities;
namespace DengueAlertLK.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext db, IPasswordHasher<AppUser> hasher)
    {
        // Default admin: admin / Admin@123
        if (!db.Users.Any())
        {
            var admin = new AppUser { Username = "admin", FullName = "Admin User", Role = "Admin" };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");
            db.Users.Add(admin);
            db.SaveChanges();
        }

        // Sample areas (population values are approximate sample data)
        if (!db.Areas.Any())
        {
            db.Areas.AddRange(
                new Area { Name = "Negombo", District = "Gampaha", Latitude = 7.2008, Longitude = 79.8737, Population = 142000 },
                new Area { Name = "Colombo", District = "Colombo", Latitude = 6.9271, Longitude = 79.8612, Population = 750000 },
                new Area { Name = "Gampaha", District = "Gampaha", Latitude = 7.0873, Longitude = 79.9925, Population = 65000 },
                new Area { Name = "Kalutara", District = "Kalutara", Latitude = 6.5854, Longitude = 79.9607, Population = 40000 },
                new Area { Name = "Kandy", District = "Kandy", Latitude = 7.2906, Longitude = 80.6337, Population = 125000 },
                new Area { Name = "Kurunegala", District = "Kurunegala", Latitude = 7.4863, Longitude = 80.3647, Population = 30000 },
                new Area { Name = "Galle", District = "Galle", Latitude = 6.0535, Longitude = 80.2210, Population = 100000 },
                new Area { Name = "Matara", District = "Matara", Latitude = 5.9549, Longitude = 80.5550, Population = 70000 },
                new Area { Name = "Ratnapura", District = "Ratnapura", Latitude = 6.6828, Longitude = 80.3992, Population = 50000 },
                new Area { Name = "Jaffna", District = "Jaffna", Latitude = 9.6615, Longitude = 80.0255, Population = 88000 },
                new Area { Name = "Batticaloa", District = "Batticaloa", Latitude = 7.7310, Longitude = 81.6747, Population = 93000 },
                new Area { Name = "Anuradhapura", District = "Anuradhapura", Latitude = 8.3114, Longitude = 80.4037, Population = 65000 });
            db.SaveChanges();
        }

        // Sample cases for the last 75 days (so the dashboard is not empty)
        if (!db.DengueCases.Any())
        {
            var rnd = new Random(42);
            var weights = new Dictionary<string, int>
            {
                ["Negombo"] = 6,
                ["Colombo"] = 5,
                ["Gampaha"] = 3,
                ["Kalutara"] = 2,
                ["Kandy"] = 2,
                ["Kurunegala"] = 2
            };
            var areas = db.Areas.ToList();

            for (int ago = 74; ago >= 0; ago--)
            {
                var date = DateTime.Today.AddDays(-ago);
                double factor = 0.6 + 0.4 * (74 - ago) / 74.0;      // cases slowly increase
                foreach (var a in areas)
                {
                    int b = weights.GetValueOrDefault(a.Name, 1);
                    int cases = (int)Math.Round(rnd.Next(0, 2 * b + 1) * factor);
                    if (cases == 0) continue;
                    db.DengueCases.Add(new DengueCase
                    {
                        AreaId = a.Id,
                        ReportDate = date,
                        Cases = cases,
                        Recovered = (int)Math.Round(cases * (0.5 + rnd.NextDouble() * 0.4)),
                        Notes = "Sample data"
                    });
                }
            }
            db.SaveChanges();
        }
    }
}