using RaceDayAPI.Models;

namespace RaceDayAPI.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            db.Database.EnsureCreated();          // also applies the Roles HasData
            if (db.Users.Any()) return;

            const string pw = "Password123!";     // demo only
            string Hash() => BCrypt.Net.BCrypt.HashPassword(pw);

            db.Users.AddRange(
                // Organisers (RoleId 1)
                new User { RoleId = 1, FullName = "Tlhologelo Sekoto", Email = "tr.otokes23@raceday.co.za", PhoneNumber = "0821234567", PasswordHash = Hash() },
                new User { RoleId = 1, FullName = "Thandi Mathe", Email = "sebotse.mm21@raceday.co.za", PhoneNumber = "0837654321", PasswordHash = Hash() },
                // Participants (RoleId 2)
                new User { RoleId = 2, FullName = "Shonisani Thavhiwa", Email = "shoni.sp12@raceday.co.za", PhoneNumber = "0841112222", PasswordHash = Hash() },
                new User { RoleId = 2, FullName = "Rearabecwe Tlatsi", Email = "tlatsi.otokes04@raceday.co.za", PhoneNumber = "0829998888", PasswordHash = Hash() }
            );
            db.SaveChanges();
        }
    }
}
