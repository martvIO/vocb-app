using Vocb.Firebase;
using Xunit;

namespace Vocb.Core.Tests;

public class FirebaseAuthErrorTests
{
    [Theory]
    [InlineData("EMAIL_NOT_FOUND")]
    [InlineData("INVALID_PASSWORD")]
    [InlineData("INVALID_LOGIN_CREDENTIALS")]
    [InlineData("EMAIL_EXISTS")]
    public void KnownCodes_GetSpecificMessages(string code)
    {
        var message = FirebaseAuthError.Friendly(code);
        Assert.False(string.IsNullOrWhiteSpace(message));
        // A specific message, not the generic fallback.
        Assert.NotEqual("Sign-in failed. Please try again.", message);
    }

    [Fact]
    public void WeakPassword_WithTrailingDetail_StillMaps()
    {
        // The REST API appends detail after the code; we should still recognize it.
        var message = FirebaseAuthError.Friendly("WEAK_PASSWORD : Password should be at least 6 characters");
        Assert.Equal("Password must be at least 6 characters.", message);
    }

    [Fact]
    public void NullOrUnknownCode_FallsBackGracefully()
    {
        Assert.False(string.IsNullOrWhiteSpace(FirebaseAuthError.Friendly(null)));
        Assert.False(string.IsNullOrWhiteSpace(FirebaseAuthError.Friendly("SOME_NEW_CODE")));
    }
}
