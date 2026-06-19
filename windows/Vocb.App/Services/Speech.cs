using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;

namespace Vocb.App.Services;

/// <summary>
/// Text-to-speech for the Windows client. Pronounces a word aloud — preferring a
/// dictionary audio clip when one is available, otherwise the system TTS voice
/// (mirrors the Apple client's Pronunciation helper). All calls are fire-and-forget
/// and never throw: audio is a nicety, not something that should crash the UI.
/// </summary>
public static class Speech
{
    // One reused player so we don't leak a native player per click. SpeechSynthesizer
    // is created per call (cheap) and disposed, so voice resources aren't held open.
    private static readonly MediaPlayer Player = new();

    /// <summary>Speak the given text using the system voice.</summary>
    public static void Speak(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        _ = SpeakAsync(text);
    }

    /// <summary>Play the word's audio clip if it has one, else fall back to TTS.</summary>
    public static void Pronounce(string? text, string? audioUrl)
    {
        if (!string.IsNullOrWhiteSpace(audioUrl) && Uri.TryCreate(audioUrl, UriKind.Absolute, out var uri))
        {
            try
            {
                Player.Source = MediaSource.CreateFromUri(uri);
                Player.Play();
                return;
            }
            catch { /* clip failed — fall back to synthesized speech */ }
        }
        Speak(text);
    }

    private static async Task SpeakAsync(string text)
    {
        try
        {
            using var synth = new SpeechSynthesizer();
            var stream = await synth.SynthesizeTextToStreamAsync(text);
            Player.Source = MediaSource.CreateFromStream(stream, stream.ContentType);
            Player.Play();
        }
        catch { /* TTS is best-effort */ }
    }
}
