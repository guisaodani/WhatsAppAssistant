using Microsoft.SemanticKernel.ChatCompletion;
using WhatsAppAssistant.Core.Interfaces;

namespace WhatsAppAssistant.Application.Services;

public class AssistantService : IAssistantService
{
    private readonly IChatCompletionService _chatCompletion;
    private readonly ICalendarService _calendarService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ChatHistory _chatHistory;

    public AssistantService(IChatCompletionService chatCompletion, ICalendarService calendarService, IUsuarioRepository usuarioRepository)
    {
        _chatCompletion = chatCompletion;
        _calendarService = calendarService;
        _usuarioRepository = usuarioRepository;
        _chatHistory = new ChatHistory();

        _chatHistory.AddSystemMessage("""
            Eres un asistente personal inteligente que ayuda a gestionar
            el calendario, correos y tareas. Respondes siempre en español,
            de forma concisa y amigable.

            Cuando el usuario quiera crear un evento, extrae titulo, fecha y hora.
            Cuando el usuario quiera ver sus eventos, indícalo.

            Si necesitas crear o consultar eventos responde EXACTAMENTE en JSON:
            {"accion": "crear_evento", "titulo": "...", "inicio": "2026-06-03T15:00:00", "fin": "2026-06-03T16:00:00"}
            {"accion": "ver_eventos"}
            {"accion": "solicitar_autorizacion"}
            """);
    }

    public async Task<string> GetResponseAsync(string userMessage, string numeroWhatsapp)
    {
        _chatHistory.AddUserMessage(userMessage);

        var response = await _chatCompletion.GetChatMessageContentAsync(_chatHistory);
        var reply = response.Content ?? "No pude procesar tu mensaje.";

        if (reply.TrimStart().StartsWith("{"))
        {
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(reply);
                var accion = json.RootElement.GetProperty("accion").GetString();

                if (accion == "solicitar_autorizacion" || accion == "crear_evento" || accion == "ver_eventos")
                {
                    var token = await _usuarioRepository.GetGoogleTokenAsync(numeroWhatsapp);
                    var refreshToken = await _usuarioRepository.GetGoogleRefreshTokenAsync(numeroWhatsapp);

                    if (string.IsNullOrEmpty(token))
                    {
                        reply = $"Para gestionar tu calendario necesitas autorizar el acceso. Entra a este link:\nhttps://shifting-pancake-superjet.ngrok-free.dev/api/Auth/google?numero={numeroWhatsapp}";
                    }
                    else if (accion == "crear_evento")
                    {
                        var titulo = json.RootElement.GetProperty("titulo").GetString() ?? "Evento";
                        var inicio = json.RootElement.GetProperty("inicio").GetDateTime();
                        var fin = json.RootElement.GetProperty("fin").GetDateTime();
                        reply = await _calendarService.CreateEventForUserAsync(token, refreshToken!, titulo, inicio, fin);
                    }
                    else if (accion == "ver_eventos")
                    {
                        var eventos = await _calendarService.GetUpcomingEventsForUserAsync(token, refreshToken!);
                        reply = "Tus proximos eventos:\n" + string.Join("\n", eventos);
                    }
                }
            }
            catch { }
        }

        _chatHistory.AddAssistantMessage(reply);
        return reply;
    }

    public Task<string> GetResponseAsync(string userMessage) => GetResponseAsync(userMessage, "");
}