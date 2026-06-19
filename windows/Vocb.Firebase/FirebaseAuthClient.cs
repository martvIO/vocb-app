using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Vocb.Firebase;

/// <summary>The credentials a signed-in session holds.</summary>
public sealed record AuthSession(string Uid, string IdToken, string RefreshToken, DateTimeOffset ExpiresAt)
{
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt.AddMinutes(-2);
}

/// <summary>
/// Email/password auth + token refresh via the Identity Toolkit REST API.
/// (Sign in with Apple is used on the Apple clients; Windows uses email/password.)
/// </summary>
public sealed class FirebaseAuthClient
{
    private readonly HttpClient _http;
    private readonly FirebaseConfig _config;

    public FirebaseAuthClient(HttpClient http, FirebaseConfig config)
    {
        _http = http;
        _config = config;
    }

    public Task<AuthSession> SignInAsync(string email, string password, CancellationToken ct = default)
        => PostPasswordAsync("accounts:signInWithPassword", email, password, ct);

    public Task<AuthSession> SignUpAsync(string email, string password, CancellationToken ct = default)
        => PostPasswordAsync("accounts:signUp", email, password, ct);

    /// <summary>Send a password-reset email to the given address.</summary>
    public async Task SendPasswordResetAsync(string email, CancellationToken ct = default)
    {
        var url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_config.ApiKey}";
        var resp = await _http.PostAsJsonAsync(url,
            new { requestType = "PASSWORD_RESET", email }, ct);
        if (!resp.IsSuccessStatusCode)
            throw await ReadAuthErrorAsync(resp, ct);
    }

    private async Task<AuthSession> PostPasswordAsync(string method, string email, string password, CancellationToken ct)
    {
        var url = $"https://identitytoolkit.googleapis.com/v1/{method}?key={_config.ApiKey}";
        var resp = await _http.PostAsJsonAsync(url,
            new { email, password, returnSecureToken = true }, ct);
        if (!resp.IsSuccessStatusCode)
            throw await ReadAuthErrorAsync(resp, ct);
        var body = await resp.Content.ReadFromJsonAsync<PasswordResponse>(cancellationToken: ct)
                   ?? throw new InvalidOperationException("Empty auth response.");
        return new AuthSession(
            body.LocalId, body.IdToken, body.RefreshToken,
            DateTimeOffset.UtcNow.AddSeconds(ParseInt(body.ExpiresIn)));
    }

    /// <summary>
    /// Turn a failed Identity Toolkit response into a <see cref="FirebaseAuthException"/>
    /// with a user-friendly message (e.g. EMAIL_NOT_FOUND → "No account found…").
    /// </summary>
    private static async Task<FirebaseAuthException> ReadAuthErrorAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        string? code = null;
        try
        {
            var body = await resp.Content.ReadFromJsonAsync<ErrorEnvelope>(cancellationToken: ct);
            code = body?.Error?.Message;
        }
        catch { /* non-JSON or empty body — fall back to a generic message */ }
        return new FirebaseAuthException(FirebaseAuthError.Friendly(code), code);
    }

    /// <summary>Exchange a refresh token for a fresh id token.</summary>
    public async Task<AuthSession> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var url = $"https://securetoken.googleapis.com/v1/token?key={_config.ApiKey}";
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
        });
        var resp = await _http.PostAsync(url, form, ct);
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<RefreshResponse>(cancellationToken: ct)
                   ?? throw new InvalidOperationException("Empty refresh response.");
        return new AuthSession(
            body.UserId, body.IdToken, body.RefreshToken,
            DateTimeOffset.UtcNow.AddSeconds(ParseInt(body.ExpiresIn)));
    }

    private static int ParseInt(string? s) => int.TryParse(s, out var v) ? v : 3600;

    private sealed class PasswordResponse
    {
        [JsonPropertyName("localId")] public string LocalId { get; set; } = "";
        [JsonPropertyName("idToken")] public string IdToken { get; set; } = "";
        [JsonPropertyName("refreshToken")] public string RefreshToken { get; set; } = "";
        [JsonPropertyName("expiresIn")] public string ExpiresIn { get; set; } = "3600";
    }

    private sealed class RefreshResponse
    {
        [JsonPropertyName("user_id")] public string UserId { get; set; } = "";
        [JsonPropertyName("id_token")] public string IdToken { get; set; } = "";
        [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = "";
        [JsonPropertyName("expires_in")] public string ExpiresIn { get; set; } = "3600";
    }

    private sealed class ErrorEnvelope
    {
        [JsonPropertyName("error")] public ErrorBody? Error { get; set; }

        public sealed class ErrorBody
        {
            [JsonPropertyName("code")] public int Code { get; set; }
            [JsonPropertyName("message")] public string? Message { get; set; }
        }
    }
}
