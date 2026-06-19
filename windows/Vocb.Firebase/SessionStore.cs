using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text.Json;

namespace Vocb.Firebase;

/// <summary>
/// Persists a signed-in session on disk so the app can restore it on the next
/// launch without the user re-entering credentials.
///
/// Only the long-lived refresh token (plus the email, for display) is stored — the
/// short-lived id token is re-minted from the refresh token on demand. The blob is
/// encrypted with Windows DPAPI scoped to the current user, so it can't be read by
/// another user or copied to another machine and decrypted.
///
/// DPAPI is Windows-only; the type is annotated accordingly (the Windows client is
/// the only consumer of this library).
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class SessionStore
{
    /// <summary>What we persist between launches.</summary>
    public sealed record Persisted(string RefreshToken, string? Email);

    private readonly string _path;

    /// <param name="path">
    /// Override the storage file (used by tests). Defaults to
    /// %LocalAppData%\Vocb\session.dat.
    /// </param>
    public SessionStore(string? path = null)
    {
        if (path is not null)
        {
            _path = path;
        }
        else
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Vocb");
            Directory.CreateDirectory(dir);
            _path = Path.Combine(dir, "session.dat");
        }
    }

    public void Save(string refreshToken, string? email)
    {
        var plain = JsonSerializer.SerializeToUtf8Bytes(new Persisted(refreshToken, email));
        var cipher = ProtectedData.Protect(plain, optionalEntropy: null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(_path, cipher);
    }

    /// <summary>Returns the stored session, or null if there is none / it can't be read.</summary>
    public Persisted? Load()
    {
        try
        {
            if (!File.Exists(_path)) return null;
            var cipher = File.ReadAllBytes(_path);
            var plain = ProtectedData.Unprotect(cipher, optionalEntropy: null, DataProtectionScope.CurrentUser);
            return JsonSerializer.Deserialize<Persisted>(plain);
        }
        catch
        {
            // Missing, corrupt, or encrypted for a different user/machine — treat as "no session".
            return null;
        }
    }

    public void Clear()
    {
        try { if (File.Exists(_path)) File.Delete(_path); }
        catch { /* best effort — a leftover blob is harmless; it just won't decrypt elsewhere */ }
    }
}
