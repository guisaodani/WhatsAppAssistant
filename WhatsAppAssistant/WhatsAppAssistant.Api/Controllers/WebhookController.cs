using Microsoft.AspNetCore.Mvc;
using WhatsAppAssistant.Core.Interfaces;

namespace WhatsAppAssistant.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public WebhookController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost("whatsapp")]
        public async Task<IActionResult> ReceiveMessage([FromForm] string From, [FromForm] string Body)
        {
            var reply = await _messageService.ProcessMessageAsync(From, Body);

            var twiml = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <Response>
                <Message>{reply}</Message>
            </Response>
            """;

            return Content(twiml, "application/xml");
        }
    }
}