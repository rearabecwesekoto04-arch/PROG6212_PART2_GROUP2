namespace RaceDayAPI.Models
{
    public class User
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
