namespace RaceDayAPI.Dtos
{
    public class Dtos
    {
        public record RegisterDto(string FullName, string Email, string Password, string? Phone);
        public record LoginDto(string Email, string Password);
        public record EventDto(string EventName, string? Description, DateTime EventDate,
                               string Location, decimal Distance, string EventType, string? RouteInfo);
        public record CategoryDto(int EventId, string CategoryName, string CategoryType,
                                  decimal? DistanceKm, int? MinAge, int? MaxAge, decimal Price, int? MaxParticipants);
        public record EnrolDto(int CategoryId);
        public record ResultDto(int EnrolmentId, TimeSpan? FinishTime, int? Position, string Status);
        public record ProfileDto(string FullName, string? PhoneNumber);
    }
}
