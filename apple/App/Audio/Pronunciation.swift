import AVFoundation
import VocabKit

/// Pronounces a word — preferring the dictionary's audio clip, falling back to
/// the system text-to-speech voice.
enum Pronunciation {
    private static let synthesizer = AVSpeechSynthesizer()
    private static var player: AVPlayer?

    static func pronounce(_ entry: WordEntry) {
        if !entry.audioUrl.isEmpty, let url = URL(string: entry.audioUrl) {
            player = AVPlayer(url: url)
            player?.play()
        } else {
            speak(entry.text)
        }
    }

    static func speak(_ text: String) {
        let utterance = AVSpeechUtterance(string: text)
        utterance.voice = AVSpeechSynthesisVoice(language: "en-US")
        synthesizer.speak(utterance)
    }
}
