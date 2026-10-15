namespace RaceDayAPI.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; } = null!;
        public string CategoryName { get; set; } = "";
        public string CategoryType { get; set; } = "";  // Age, Distance
        public decimal? DistanceKm { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public decimal Price { get; set; }
        public int? MaxParticipants { get; set; }
    }
}
