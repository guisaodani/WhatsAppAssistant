namespace WhatsAppAssistant.Core.Interfaces;

public interface ICalendarService
{
    Task<string> CreateEventAsync(string title, DateTime start, DateTime end, string? description = null);

    Task<List<string>> GetUpcomingEventsAsync(int maxResults = 5);

    Task<string> CreateEventForUserAsync(string accessToken, string refreshToken, string title, DateTime start, DateTime end, string? description = null);

    Task<List<string>> GetUpcomingEventsForUserAsync(string accessToken, string refreshToken, int maxResults = 5);
}