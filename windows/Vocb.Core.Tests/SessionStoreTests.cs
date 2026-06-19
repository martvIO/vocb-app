using System.IO;
using Vocb.Firebase;
using Xunit;

namespace Vocb.Core.Tests;

public class SessionStoreTests
{
    private static string TempPath()
        => Path.Combine(Path.GetTempPath(), "vocb-test-" + System.Guid.NewGuid().ToString("N") + ".dat");

    [Fact]
    public void SaveThenLoad_RoundTrips()
    {
        var path = TempPath();
        try
        {
            var store = new SessionStore(path);
            store.Save("refresh-token-123", "user@example.com");

            var loaded = store.Load();
            Assert.NotNull(loaded);
            Assert.Equal("refresh-token-123", loaded!.RefreshToken);
            Assert.Equal("user@example.com", loaded.Email);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Load_NoFile_ReturnsNull()
    {
        var store = new SessionStore(TempPath()); // path that doesn't exist yet
        Assert.Null(store.Load());
    }

    [Fact]
    public void Clear_RemovesPersistedSession()
    {
        var path = TempPath();
        var store = new SessionStore(path);
        store.Save("token", null);
        Assert.NotNull(store.Load());

        store.Clear();
        Assert.Null(store.Load());
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void StoredBytes_AreEncrypted_NotPlaintext()
    {
        var path = TempPath();
        try
        {
            new SessionStore(path).Save("super-secret-refresh", "a@b.com");
            var raw = File.ReadAllText(path);
            Assert.DoesNotContain("super-secret-refresh", raw);
        }
        finally { File.Delete(path); }
    }
}
