import Foundation
#if canImport(AVFoundation)
import AVFoundation
#endif

/// Pronounces a word aloud — preferring the dictionary's audio clip, falling back to
/// the system text-to-speech voice. Shared by the apps, the macOS menu-bar agent, and
/// the popup card so every surface speaks the same way.
///
/// (The App's `Pronunciation` helper now delegates here, so there's a single TTS path.)
public enum Speaker {
    #if canImport(AVFoundation)
    private static let synthesizer = AVSpeechSynthesizer()
    // Held so the player isn't deallocated mid-playback.
    private static var player: AVPlayer?
    #endif

    /// Pronounce a word entry: play its audio clip if it has one, else synthesize.
    public static func pronounce(_ entry: WordEntry) {
        pronounce(text: entry.text, audioUrl: entry.audioUrl)
    }

    /// Pronounce arbitrary text, optionally preferring an audio-clip URL.
    public static func pronounce(text: String, audioUrl: String) {
        #if canImport(AVFoundation)
        if !audioUrl.isEmpty, let url = URL(string: audioUrl) {
            let p = AVPlayer(url: url)
            player = p
            p.play()
        } else {
            speak(text)
        }
        #endif
    }

    /// Speak text using the system voice.
    public static func speak(_ text: String) {
        #if canImport(AVFoundation)
        guard !text.isEmpty else { return }
        let utterance = AVSpeechUtterance(string: text)
        utterance.voice = AVSpeechSynthesisVoice(language: "en-US")
        synthesizer.speak(utterance)
        #endif
    }
}
