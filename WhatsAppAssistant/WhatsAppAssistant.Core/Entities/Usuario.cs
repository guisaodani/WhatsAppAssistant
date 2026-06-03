namespace WhatsAppAssistant.Core.Entities;

public class Usuario
{
    public string NumeroWhatsapp { get; set; } = string.Empty;
    public string? GoogleToken { get; set; }
    public string? GoogleRefreshToken { get; set; }
    public string? EmailGoogle { get; set; }
    public DateTime FechaRegistro { get; set; }
}