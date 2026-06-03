using WhatsAppAssistant.Core.Interfaces;

namespace WhatsAppAssistant.Application.Services;

public class MessageService : IMessageService
{
    private readonly IAssistantService _assistantService;

    public MessageService(IAssistantService assistantService)
    {
        _assistantService = assistantService;
    }

    public async Task<string> ProcessMessageAsync(string from, string message)
    {
        return await _assistantService.GetResponseAsync(message, from);
    }
}