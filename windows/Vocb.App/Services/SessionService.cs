using Vocb.Core;
using Vocb.Firebase;

namespace Vocb.App.Services;

/// <summary>
/// Holds the signed-in session and the Firebase REST clients, and hands out a
/// valid (auto-refreshed) id token. Configure once from Settings with the
/// Firebase project's API key + project id.
/// </summary>
public sealed class SessionService
{
    private readonly HttpClient _http = new();
    private FirebaseConfig? _config;
    private FirebaseAuthClient? _auth;
    private AuthSession? _session;

    public FirestoreClient? Firestore { get; private set; }
    public LookupClient? Lookup { get; private set; }

    public bool IsConfigured => _config is not null;
    public bool IsSignedIn => _session is not null;
    public string? Uid => _session?.Uid;

    public void Configure(FirebaseConfig config)
    {
        _config = config;
        _auth = new FirebaseAuthClient(_http, config);
        Firestore = new FirestoreClient(_http, config);
        Lookup = new LookupClient(_http, config);
    }

    public async Task SignInAsync(string email, string password, CancellationToken ct = default)
    {
        if (_auth is null) throw new InvalidOperationException("Configure the Firebase project first.");
        _session = await _auth.SignInAsync(email, password, ct);
    }

    /// <summary>Return a valid id token, refreshing it if it's near expiry.</summary>
    public async Task<string> GetIdTokenAsync(CancellationToken ct = default)
    {
        if (_session is null || _auth is null) throw new InvalidOperationException("Not signed in.");
        if (_session.IsExpired)
            _session = await _auth.RefreshAsync(_session.RefreshToken, ct);
        return _session.IdToken;
    }

    /// <summary>Convenience: look up a selected word/phrase end-to-end.</summary>
    public async Task<LookupResponse> LookupAsync(string text, CancellationToken ct = default)
    {
        if (Lookup is null) throw new InvalidOperationException("Not configured.");
        var token = await GetIdTokenAsync(ct);
        return await Lookup.LookupAsync(text, token, ct);
    }
}
