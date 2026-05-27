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
        public async Task<string> ProcessMessageAsync(string from, string message)
        {
            // Por ahora retornamos un eco para verificar que el webhook funciona
            // Aquí irá la lógica de IA, calendario, etc.

            return await Task.FromResult($"Hola! Recibi tu mensaje: {message}");
        }
    }
}