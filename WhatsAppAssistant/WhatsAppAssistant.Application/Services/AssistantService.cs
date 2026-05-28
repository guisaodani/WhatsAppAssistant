using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatsAppAssistant.Core.Interfaces;

namespace WhatsAppAssistant.Application.Services
{
    public class AssistantService : IAssistantService
    {
        private readonly IChatCompletionService _chatCompletion;
        private readonly ChatHistory _chatHistory;

        public AssistantService(IChatCompletionService chatCompletion)
        {
            _chatCompletion = chatCompletion;
            _chatHistory = new ChatHistory();

            _chatHistory.AddSystemMessage("""
            Eres un asistente personal inteligente que ayuda a gestionar
            el calendario, correos y tareas. Respondes siempre en español,
            de forma concisa y amigable. Cuando el usuario quiera agendar
            algo, pide los detalles necesarios como fecha y hora.
            """);
        }

        public async Task<string> GetResponseAsync(string userMessage)
        {
            _chatHistory.AddUserMessage(userMessage);

            var response = await _chatCompletion.GetChatMessageContentAsync(_chatHistory);
            var reply = response.Content ?? "No pude procesar tu mensaje.";

            _chatHistory.AddAssistantMessage(reply);

            return reply;
        }
    }
}