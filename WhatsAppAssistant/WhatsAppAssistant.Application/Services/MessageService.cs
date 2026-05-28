using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatsAppAssistant.Core.Interfaces;

namespace WhatsAppAssistant.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IAssistantService _aiService;

        public MessageService(IAssistantService aiService)
        {
            _aiService = aiService;
        }

        public async Task<string> ProcessMessageAsync(string from, string message)
        {
            // Aquí irá la lógica de IA, calendario.

            return await _aiService.GetResponseAsync(message);
        }
    }
}