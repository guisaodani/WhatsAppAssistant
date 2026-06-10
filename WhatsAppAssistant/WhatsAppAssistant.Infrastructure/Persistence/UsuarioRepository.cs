using Dapper;
using Npgsql;
using WhatsAppAssistant.Core.Interfaces;

namespace WhatsAppAssistant.Infrastructure.Persistence;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly string _connectionString;

    public UsuarioRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private NpgsqlConnection GetConnection() => new NpgsqlConnection(_connectionString);

    public async Task<bool> UsuarioExisteAsync(string numeroWhatsapp)
    {
        using var conn = GetConnection();
        var result = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM usuarios WHERE numero_whatsapp = @numero",
            new { numero = numeroWhatsapp });
        return result > 0;
    }

    public async Task<string?> GetGoogleTokenAsync(string numeroWhatsapp)
    {
        using var conn = GetConnection();
        return await conn.ExecuteScalarAsync<string>(
            "SELECT google_token FROM usuarios WHERE numero_whatsapp = @numero",
            new { numero = numeroWhatsapp });
    }

    public async Task<string?> GetGoogleRefreshTokenAsync(string numeroWhatsapp)
    {
        using var conn = GetConnection();
        return await conn.ExecuteScalarAsync<string>(
            "SELECT google_refresh_token FROM usuarios WHERE numero_whatsapp = @numero",
            new { numero = numeroWhatsapp });
    }

    public async Task SaveTokensAsync(string numeroWhatsapp, string token, string refreshToken, string email)
    {
        using var conn = GetConnection();
        await conn.ExecuteAsync("""
            INSERT INTO usuarios (numero_whatsapp, google_token, google_refresh_token, email_google)
            VALUES (@numero, @token, @refreshToken, @email)
            ON CONFLICT (numero_whatsapp)
            DO UPDATE SET
                google_token = @token,
                google_refresh_token = @refreshToken,
                email_google = @email
            """,
            new { numero = numeroWhatsapp, token, refreshToken, email });
    }

    public async Task<List<Core.Entities.Usuario>> GetAllUsersAsync()
    {
        using var conn = GetConnection();
        var result = await conn.QueryAsync<Core.Entities.Usuario>(
            "SELECT numero_whatsapp AS NumeroWhatsapp, google_token AS GoogleToken, " +
            "google_refresh_token AS GoogleRefreshToken, email_google AS EmailGoogle " +
            "FROM usuarios"
        );
        return result.ToList();
    }
}