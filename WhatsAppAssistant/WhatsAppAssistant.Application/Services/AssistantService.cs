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

        var fechaActual = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy");
        var horaActual = DateTime.Now.ToString("HH:mm");

        _chatHistory.AddSystemMessage(
            $"Eres un asistente personal inteligente que ayuda a gestionar " +
            $"el calendario, correos y tareas. Respondes siempre en espanol, " +
            $"de forma concisa y amigable.\n\n" +
            $"Hoy es: {fechaActual} hora actual: {horaActual} (Colombia UTC-5)\n\n" +
            "Cuando el usuario quiera crear un evento responde UNICAMENTE con este JSON:\n" +
            "{\"accion\":\"crear_evento\",\"titulo\":\"TITULO\",\"inicio\":\"YYYY-MM-DDTHH:MM:00\",\"fin\":\"YYYY-MM-DDTHH:MM:00\"}\n\n" +
            "Cuando el usuario quiera ver sus eventos responde UNICAMENTE con:\n" +
            "{\"accion\":\"ver_eventos\"}\n\n" +
            "Cuando el usuario quiera eliminar un evento responde UNICAMENTE con:\n" +
            "{\"accion\":\"listar_para_eliminar\"}\n\n" +
            "Cuando el usuario diga cual evento eliminar por numero (ej: 'elimina el 1', 'el 2', 'el primero') " +
            "responde UNICAMENTE con este JSON donde numero es el numero entero del evento:\n" +
            "{\"accion\":\"eliminar_evento\",\"numero\":1}\n\n" +
            "No agregues explicaciones al JSON. Para otras consultas responde normalmente."
        );
    }

    public async Task<string> GetResponseAsync(string userMessage, string numeroWhatsapp)
    {
        _chatHistory.AddUserMessage(userMessage);
        var response = await _chatCompletion.GetChatMessageContentAsync(_chatHistory);
        var reply = response.Content ?? "No pude procesar tu mensaje.";

        var jsonStart = reply.IndexOf('{');
        var jsonEnd = reply.LastIndexOf('}');

        if (jsonStart >= 0 && jsonEnd > jsonStart)
        {
            var jsonStr = reply.Substring(jsonStart, jsonEnd - jsonStart + 1);
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(jsonStr);
                var accion = json.RootElement.GetProperty("accion").GetString();

                var token = await _usuarioRepository.GetGoogleTokenAsync(numeroWhatsapp);
                var refreshToken = await _usuarioRepository.GetGoogleRefreshTokenAsync(numeroWhatsapp);

                if (string.IsNullOrEmpty(token))
                {
                    reply = $"Para gestionar tu calendario necesitas autorizar el acceso primero. Entra aqui:\nhttps://shifting-pancake-superjet.ngrok-free.dev/api/Auth/google?numero={Uri.EscapeDataString(numeroWhatsapp)}";
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
                else if (accion == "listar_para_eliminar")
                {
                    var eventos = await _calendarService.GetUpcomingEventsWithIdAsync(token, refreshToken!);
                    if (eventos.Count == 0 || eventos[0].StartsWith("No tienes"))
                        reply = "No tienes eventos proximos para eliminar.";
                    else
                    {
                        // Guardar la lista en el historial para que Groq la recuerde
                        var listaTexto = "Estos son tus eventos para eliminar:\n" + string.Join("\n", eventos);
                        _chatHistory.AddAssistantMessage(listaTexto);
                        reply = listaTexto + "\n\nResponde con el numero del evento que deseas eliminar.";
                        // Retornar directamente sin pasar por AddAssistantMessage de nuevo
                        return reply;
                    }
                }
                else if (accion == "eliminar_evento")
                {
                    // Ignorar el eventId que manda Groq (puede estar desactualizado)
                    // Siempre consultar la lista en tiempo real y eliminar por posicion
                    var eventos = await _calendarService.GetUpcomingEventsWithIdAsync(token, refreshToken!);

                    // Intentar obtener el numero de posicion del JSON
                    int posicion = 1;
                    try
                    {
                        if (json.RootElement.TryGetProperty("numero", out var numProp))
                            posicion = numProp.GetInt32();
                        else if (json.RootElement.TryGetProperty("eventId", out var idProp))
                            int.TryParse(idProp.GetString(), out posicion);
                    }
                    catch { }

                    if (posicion < 1 || posicion > eventos.Count)
                    {
                        reply = "Numero invalido. Escribe 'quiero eliminar un evento' para ver la lista actualizada.";
                    }
                    else
                    {
                        var lineaEvento = eventos[posicion - 1];
                        var idStart = lineaEvento.IndexOf("[ID:") + 4;
                        var idEnd = lineaEvento.IndexOf("]", idStart);
                        var idReal = lineaEvento.Substring(idStart, idEnd - idStart);
                        reply = await _calendarService.DeleteEventAsync(token, refreshToken!, idReal);
                    }
                }
            }
            catch (Exception ex)
            {
                reply = $"Error: {ex.Message} - JSON: {jsonStr}";
            }
        }

        _chatHistory.AddAssistantMessage(reply);
        return reply;
    }

    public Task<string> GetResponseAsync(string userMessage) => GetResponseAsync(userMessage, "");
}