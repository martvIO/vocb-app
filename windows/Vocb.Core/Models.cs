using System.Text.Json.Serialization;

namespace Vocb.Core;

// These shapes mirror the backend WordEntry (backend/functions/src/types.ts) and
// the Apple VocabKit models, so the same data round-trips across all clients.
// JSON names use camelCase to match the callable response / Firestore fields.

public sealed class WordSense
{
    [JsonPropertyName("partOfSpeech")] public string PartOfSpeech { get; set; } = "";
    [JsonPropertyName("meaning")] public string Meaning { get; set; } = "";
}

public enum WordKind
{
    Word,
    Phrase
}

public sealed class WordEntry
{
    [JsonPropertyName("lemma")] public string Lemma { get; set; } = "";
    [JsonPropertyName("text")] public string Text { get; set; } = "";

    [JsonPropertyName("kind")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WordKind Kind { get; set; } = WordKind.Word;

    [JsonPropertyName("lookupCount")] public int LookupCount { get; set; }
    [JsonPropertyName("firstSeen")] public long FirstSeen { get; set; }
    [JsonPropertyName("lastSeen")] public long LastSeen { get; set; }

    [JsonPropertyName("phonetic")] public string Phonetic { get; set; } = "";
    [JsonPropertyName("audioUrl")] public string AudioUrl { get; set; } = "";

    [JsonPropertyName("learnerDefinition")] public string LearnerDefinition { get; set; } = "";
    [JsonPropertyName("senses")] public List<WordSense> Senses { get; set; } = new();
    [JsonPropertyName("examples")] public List<string> Examples { get; set; } = new();
    [JsonPropertyName("synonyms")] public List<string> Synonyms { get; set; } = new();
    [JsonPropertyName("antonyms")] public List<string> Antonyms { get; set; } = new();

    [JsonPropertyName("deckIds")] public List<string> DeckIds { get; set; } = new();
    [JsonPropertyName("tags")] public List<string> Tags { get; set; } = new();

    [JsonPropertyName("generatedBy")] public string GeneratedBy { get; set; } = "";
    [JsonPropertyName("schemaVersion")] public int SchemaVersion { get; set; }

    [JsonIgnore]
    public DateTimeOffset FirstSeenDate => DateTimeOffset.FromUnixTimeMilliseconds(FirstSeen);
    [JsonIgnore]
    public DateTimeOffset LastSeenDate => DateTimeOffset.FromUnixTimeMilliseconds(LastSeen);
}

public sealed class Deck
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("color")] public string Color { get; set; } = "";
    [JsonPropertyName("createdAt")] public long CreatedAt { get; set; }
}
