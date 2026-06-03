namespace WhatsAppAssistant.Core.Interfaces;

public interface IAssistantService
{
    Task<string> GetResponseAsync(string userMessage);

    Task<string> GetResponseAsync(string userMessage, string numeroWhatsapp);
}