using Vocb.Core;
using Vocb.Firebase;

namespace Vocb.App.Services;

/// <summary>
/// Holds the signed-in session and the Firebase REST clients, hands out a valid
/// (auto-refreshed) id token, and persists the session so the app remembers you
/// across restarts. The Firebase project is baked in (see <see cref="FirebaseDefaults"/>),
/// so the only thing a user enters is their email + password.
/// </summary>
public sealed class SessionService
{
    private readonly HttpClient _http = new();
    private readonly SessionStore _store = new();
    private FirebaseConfig? _config;
    private FirebaseAuthClient? _auth;
    private AuthSession? _session;
    private string? _email;

    public FirestoreClient? Firestore { get; private set; }
    public LookupClient? Lookup { get; private set; }

    public bool IsConfigured => _config is not null;
    public bool IsSignedIn => _session is not null;
    public string? Uid => _session?.Uid;
    public string? Email => _email;

    /// <summary>Raised whenever the signed-in state changes (sign in/up, restore, sign out).</summary>
    public event EventHandler? SignedInChanged;

    public void Configure(FirebaseConfig config)
    {
        _config = config;
        _auth = new FirebaseAuthClient(_http, config);
        Firestore = new FirestoreClient(_http, config);
        Lookup = new LookupClient(_http, config);
    }

    public async Task SignInAsync(string email, string password, CancellationToken ct = default)
    {
        var auth = RequireAuth();
        _session = await auth.SignInAsync(email, password, ct);
        _email = email;
        Persist();
        RaiseSignedInChanged();
    }

    public async Task SignUpAsync(string email, string password, CancellationToken ct = default)
    {
        var auth = RequireAuth();
        _session = await auth.SignUpAsync(email, password, ct);
        _email = email;
        Persist();
        RaiseSignedInChanged();
    }

    /// <summary>Send a password-reset email. Does not change the signed-in state.</summary>
    public Task SendPasswordResetAsync(string email, CancellationToken ct = default)
        => RequireAuth().SendPasswordResetAsync(email, ct);

    /// <summary>
    /// Restore a previously-saved session by exchanging the stored refresh token for a
    /// fresh id token. Returns true if a session was restored. Safe to call at startup.
    /// </summary>
    public async Task<bool> TryRestoreAsync(CancellationToken ct = default)
    {
        var auth = RequireAuth();
        var saved = _store.Load();
        if (saved is null) return false;
        try
        {
            _session = await auth.RefreshAsync(saved.RefreshToken, ct);
            _email = saved.Email;
            Persist(); // the refresh token can rotate — keep the latest
            RaiseSignedInChanged();
            return true;
        }
        catch
        {
            // Refresh token revoked/expired or offline — drop it and start signed-out.
            _store.Clear();
            _session = null;
            _email = null;
            return false;
        }
    }

    /// <summary>Sign out: clear the in-memory session and delete the saved token.</summary>
    public void SignOut()
    {
        _session = null;
        _email = null;
        _store.Clear();
        RaiseSignedInChanged();
    }

    /// <summary>Return a valid id token, refreshing it (and re-persisting) if near expiry.</summary>
    public async Task<string> GetIdTokenAsync(CancellationToken ct = default)
    {
        if (_session is null || _auth is null) throw new InvalidOperationException("Not signed in.");
        if (_session.IsExpired)
        {
            _session = await _auth.RefreshAsync(_session.RefreshToken, ct);
            Persist();
        }
        return _session.IdToken;
    }

    /// <summary>Convenience: look up a selected word/phrase end-to-end.</summary>
    public async Task<LookupResponse> LookupAsync(string text, CancellationToken ct = default)
    {
        if (Lookup is null) throw new InvalidOperationException("Not configured.");
        var token = await GetIdTokenAsync(ct);
        return await Lookup.LookupAsync(text, token, ct);
    }

    private FirebaseAuthClient RequireAuth()
        => _auth ?? throw new InvalidOperationException("Configure the Firebase project first.");

    private void Persist()
    {
        if (_session is not null)
            _store.Save(_session.RefreshToken, _email);
    }

    private void RaiseSignedInChanged() => SignedInChanged?.Invoke(this, EventArgs.Empty);
}
