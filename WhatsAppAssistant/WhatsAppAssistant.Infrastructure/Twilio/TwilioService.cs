using Microsoft.Extensions.Options;
using Twilio.Rest.Api.V2010.Account;
using WhatsAppAssistant.Core.Entities;
using WhatsAppAssistant.Core.Interfaces;
using TwilioClient = Twilio.TwilioClient;
using PhoneNumber = Twilio.Types.PhoneNumber;
using IOptions = Microsoft.Extensions.Options.IOptions<WhatsAppAssistant.Core.Entities.TwilioSettings>;

namespace WhatsAppAssistant.Infrastructure.Twilio;

public class TwilioService : ITwilioService
{
    private readonly TwilioSettings _settings;

    public TwilioService(IOptions settings)
    {
        _settings = settings.Value;
        Console.WriteLine($"[Twilio] AccountSid vacio: {string.IsNullOrEmpty(_settings.AccountSid)}");
        Console.WriteLine($"[Twilio] AuthToken vacio: {string.IsNullOrEmpty(_settings.AuthToken)}");
        TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
    }

    public async Task SendMessageAsync(string to, string message)
    {
        Console.WriteLine($"[Twilio] Enviando a: {to}");
        Console.WriteLine($"[Twilio] Desde: {_settings.WhatsAppNumber}");

        await MessageResource.CreateAsync(
            from: new PhoneNumber(_settings.WhatsAppNumber),
            to: new PhoneNumber(to),
            body: message
        );
    }
}