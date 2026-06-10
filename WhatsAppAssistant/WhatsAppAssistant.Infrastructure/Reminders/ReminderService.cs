using WhatsAppAssistant.Core.Interfaces;

namespace WhatsAppAssistant.Infrastructure.Reminders;

public class ReminderService : IReminderService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ICalendarService _calendarService;
    private readonly ITwilioService _twilioService;

    public ReminderService(
        IUsuarioRepository usuarioRepository,
        ICalendarService calendarService,
        ITwilioService twilioService)
    {
        _usuarioRepository = usuarioRepository;
        _calendarService = calendarService;
        _twilioService = twilioService;
    }

    public async Task CheckAndSendRemindersAsync()
    {
        Console.WriteLine($"[Hangfire] Revisando recordatorios: {DateTime.Now:HH:mm:ss}");

        var usuarios = await _usuarioRepository.GetAllUsersAsync();
        Console.WriteLine($"[Hangfire] Usuarios encontrados: {usuarios.Count}");

        foreach (var usuario in usuarios)
        {
            Console.WriteLine($"[Hangfire] Usuario: {usuario.NumeroWhatsapp}");
            Console.WriteLine($"[Hangfire] Token vacio: {string.IsNullOrEmpty(usuario.GoogleToken)}");
            Console.WriteLine($"[Hangfire] RefreshToken vacio: {string.IsNullOrEmpty(usuario.GoogleRefreshToken)}");

            if (string.IsNullOrEmpty(usuario.GoogleToken)) continue;

            try
            {
                Console.WriteLine($"[Hangfire] Consultando calendario para {usuario.NumeroWhatsapp}...");
                var eventos = await _calendarService.GetUpcomingEventsForReminderAsync(
                    usuario.GoogleToken,
                    usuario.GoogleRefreshToken ?? "",
                    minutosAntes: 30
                );
                Console.WriteLine($"[Hangfire] Eventos encontrados: {eventos.Count}");

                foreach (var evento in eventos)
                {
                    Console.WriteLine($"[Hangfire] Enviando recordatorio: {evento}");
                    await _twilioService.SendMessageAsync(
                        usuario.NumeroWhatsapp,
                        $"Recordatorio: tienes '{evento}' en 30 minutos."
                    );
                    Console.WriteLine($"[Hangfire] Recordatorio enviado a {usuario.NumeroWhatsapp}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Hangfire] Error detallado: {ex.GetType().Name} - {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[Hangfire] Inner exception: {ex.InnerException.Message}");
            }
        }
    }
}