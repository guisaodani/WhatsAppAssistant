using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using WhatsAppAssistant.Core.Entities;
using WhatsAppAssistant.Core.Interfaces;

namespace WhatsAppAssistant.Infrastructure.Calendar;

public class GoogleCalendarService : ICalendarService
{
    private readonly string _clientId;
    private readonly string _clientSecret;

    public GoogleCalendarService(string clientId, string clientSecret)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
    }

    public CalendarService GetCalendarService(string accessToken, string refreshToken)
    {
        var token = new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret
            }
        });

        var credential = new UserCredential(flow, "user", token);

        return new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "WhatsAppAssistant"
        });
    }

    public async Task<string> CreateEventAsync(string title, DateTime start, DateTime end, string? description = null)
    {
        throw new NotImplementedException("Usar CreateEventForUserAsync");
    }

    public async Task<List<string>> GetUpcomingEventsAsync(int maxResults = 5)
    {
        throw new NotImplementedException("Usar GetUpcomingEventsForUserAsync");
    }

    public async Task<string> CreateEventForUserAsync(string accessToken, string refreshToken, string title, DateTime start, DateTime end, string? description = null)
    {
        var service = GetCalendarService(accessToken, refreshToken);

        var evento = new Event
        {
            Summary = title,
            Description = description,
            Start = new EventDateTime { DateTimeDateTimeOffset = new DateTimeOffset(start, TimeSpan.FromHours(-5)), TimeZone = "America/Bogota" },
            End = new EventDateTime { DateTimeDateTimeOffset = new DateTimeOffset(end, TimeSpan.FromHours(-5)), TimeZone = "America/Bogota" }
        };

        var request = service.Events.Insert(evento, "primary");
        var resultado = await request.ExecuteAsync();

        return $"Evento creado: {resultado.Summary} el {start:dddd dd 'de' MMMM 'a las' HH:mm}";
    }

    public async Task<List<string>> GetUpcomingEventsForUserAsync(string accessToken, string refreshToken, int maxResults = 5)
    {
        var service = GetCalendarService(accessToken, refreshToken);

        var request = service.Events.List("primary");
        request.TimeMinDateTimeOffset = DateTimeOffset.UtcNow;
        request.ShowDeleted = false;
        request.SingleEvents = true;
        request.MaxResults = maxResults;
        request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

        var events = await request.ExecuteAsync();
        var lista = new List<string>();

        if (events.Items == null || !events.Items.Any())
        {
            lista.Add("No tienes eventos proximos.");
            return lista;
        }

        foreach (var e in events.Items)
        {
            var fecha = e.Start.DateTimeDateTimeOffset ?? DateTimeOffset.Parse(e.Start.Date);
            lista.Add($"- {e.Summary} el {fecha:dddd dd 'de' MMMM 'a las' HH:mm}");
        }

        return lista;
    }
}