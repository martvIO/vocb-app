namespace Vocb.Firebase;

/// <summary>
/// An auth failure carrying a message that is safe to show the user.
/// <see cref="Code"/> holds the raw Identity Toolkit error code
/// (e.g. "EMAIL_NOT_FOUND") for logging/diagnostics.
/// </summary>
public sealed class FirebaseAuthException : Exception
{
    public string? Code { get; }

    public FirebaseAuthException(string message, string? code) : base(message)
    {
        Code = code;
    }
}

/// <summary>
/// Maps raw Identity Toolkit error codes to friendly, user-facing messages.
/// Kept as a pure function so it can be unit-tested and reused.
/// </summary>
public static class FirebaseAuthError
{
    public static string Friendly(string? code)
    {
        // The REST API sometimes appends detail after the code, e.g.
        // "WEAK_PASSWORD : Password should be at least 6 characters".
        var key = (code ?? "").Split(':')[0].Trim().ToUpperInvariant();
        return key switch
        {
            "EMAIL_EXISTS" => "That email already has an account — try signing in instead.",
            "EMAIL_NOT_FOUND" => "No account found with that email.",
            "INVALID_PASSWORD" => "Wrong password. Try again or reset it.",
            "INVALID_LOGIN_CREDENTIALS" => "Email or password is incorrect.",
            "INVALID_EMAIL" => "That doesn't look like a valid email address.",
            "MISSING_EMAIL" => "Enter your email address.",
            "MISSING_PASSWORD" => "Enter your password.",
            "WEAK_PASSWORD" => "Password must be at least 6 characters.",
            "USER_DISABLED" => "This account has been disabled.",
            "TOO_MANY_ATTEMPTS_TRY_LATER" => "Too many attempts. Please wait a moment and try again.",
            "OPERATION_NOT_ALLOWED" => "Email/password sign-in isn't enabled for this project.",
            "" => "Something went wrong. Please try again.",
            _ => "Sign-in failed. Please try again.",
        };
    }
}
