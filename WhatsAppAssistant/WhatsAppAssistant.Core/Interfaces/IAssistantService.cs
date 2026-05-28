using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsAppAssistant.Core.Interfaces
{
    public interface IAssistantService
    {
        Task<string> GetResponseAsync(string userMessage);
    }
}