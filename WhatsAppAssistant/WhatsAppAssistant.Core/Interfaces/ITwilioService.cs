using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsAppAssistant.Core.Interfaces
{
    public interface ITwilioService
    {
        Task SendMessageAsync(string to, string message);
    }
}