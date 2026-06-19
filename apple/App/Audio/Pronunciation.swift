import VocabKit

/// Pronounces a word — preferring the dictionary's audio clip, falling back to the
/// system text-to-speech voice. Kept as a thin wrapper over `VocabKit.Speaker` so the
/// app and the macOS agent share one TTS path; existing call sites stay unchanged.
enum Pronunciation {
    static func pronounce(_ entry: WordEntry) { Speaker.pronounce(entry) }
    static func speak(_ text: String) { Speaker.speak(text) }
}
