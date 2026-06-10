namespace WhatsAppAssistant.Core.Interfaces;

public interface IUsuarioRepository
{
    Task<string?> GetGoogleTokenAsync(string numeroWhatsapp);

    Task<string?> GetGoogleRefreshTokenAsync(string numeroWhatsapp);

    Task SaveTokensAsync(string numeroWhatsapp, string token, string refreshToken, string email);

    Task<bool> UsuarioExisteAsync(string numeroWhatsapp);

    Task<List<Core.Entities.Usuario>> GetAllUsersAsync();
}