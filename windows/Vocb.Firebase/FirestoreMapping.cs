using System.Globalization;
using System.Text.Json.Nodes;
using Vocb.Core;

namespace Vocb.Firebase;

/// <summary>
/// Converts between our domain models and Firestore's REST "typed value" wire
/// format (stringValue / integerValue / doubleValue / arrayValue / mapValue).
/// </summary>
internal static class FirestoreMapping
{
    // ---- value builders ----
    private static JsonObject S(string s) => new() { ["stringValue"] = s };
    private static JsonObject I(long n) => new() { ["integerValue"] = n.ToString(CultureInfo.InvariantCulture) };
    private static JsonObject D(double d) => new() { ["doubleValue"] = d };
    private static JsonObject Arr(IEnumerable<JsonNode> items) =>
        new() { ["arrayValue"] = new JsonObject { ["values"] = new JsonArray(items.ToArray()) } };
    private static JsonObject Map(JsonObject fields) =>
        new() { ["mapValue"] = new JsonObject { ["fields"] = fields } };

    // ---- value readers ----
    private static string Str(JsonNode? v) => v?["stringValue"]?.GetValue<string>() ?? "";

    private static long Long(JsonNode? v)
    {
        if (v?["integerValue"] is JsonNode iv) return long.Parse(iv.GetValue<string>(), CultureInfo.InvariantCulture);
        if (v?["doubleValue"] is JsonNode dv) return (long)dv.GetValue<double>();
        return 0;
    }

    private static double Dbl(JsonNode? v)
    {
        if (v?["doubleValue"] is JsonNode dv) return dv.GetValue<double>();
        if (v?["integerValue"] is JsonNode iv) return double.Parse(iv.GetValue<string>(), CultureInfo.InvariantCulture);
        return 0;
    }

    private static List<string> StrList(JsonNode? v)
    {
        var result = new List<string>();
        if (v?["arrayValue"]?["values"] is JsonArray arr)
            foreach (var item in arr)
                result.Add(Str(item));
        return result;
    }

    // ---- WordEntry ----
    public static JsonObject ToFields(WordEntry e) => new()
    {
        ["lemma"] = S(e.Lemma),
        ["text"] = S(e.Text),
        ["kind"] = S(e.Kind == WordKind.Phrase ? "phrase" : "word"),
        ["lookupCount"] = I(e.LookupCount),
        ["firstSeen"] = I(e.FirstSeen),
        ["lastSeen"] = I(e.LastSeen),
        ["phonetic"] = S(e.Phonetic),
        ["audioUrl"] = S(e.AudioUrl),
        ["learnerDefinition"] = S(e.LearnerDefinition),
        ["senses"] = Arr(e.Senses.Select(s => (JsonNode)Map(new JsonObject
        {
            ["partOfSpeech"] = S(s.PartOfSpeech),
            ["meaning"] = S(s.Meaning)
        }))),
        ["examples"] = Arr(e.Examples.Select(x => (JsonNode)S(x))),
        ["synonyms"] = Arr(e.Synonyms.Select(x => (JsonNode)S(x))),
        ["antonyms"] = Arr(e.Antonyms.Select(x => (JsonNode)S(x))),
        ["deckIds"] = Arr(e.DeckIds.Select(x => (JsonNode)S(x))),
        ["tags"] = Arr(e.Tags.Select(x => (JsonNode)S(x))),
        ["generatedBy"] = S(e.GeneratedBy),
        ["schemaVersion"] = I(e.SchemaVersion),
    };

    public static WordEntry WordFromFields(JsonObject f)
    {
        var senses = new List<WordSense>();
        if (f["senses"]?["arrayValue"]?["values"] is JsonArray sarr)
            foreach (var item in sarr)
            {
                var mf = item?["mapValue"]?["fields"];
                senses.Add(new WordSense { PartOfSpeech = Str(mf?["partOfSpeech"]), Meaning = Str(mf?["meaning"]) });
            }

        return new WordEntry
        {
            Lemma = Str(f["lemma"]),
            Text = Str(f["text"]),
            Kind = Str(f["kind"]) == "phrase" ? WordKind.Phrase : WordKind.Word,
            LookupCount = (int)Long(f["lookupCount"]),
            FirstSeen = Long(f["firstSeen"]),
            LastSeen = Long(f["lastSeen"]),
            Phonetic = Str(f["phonetic"]),
            AudioUrl = Str(f["audioUrl"]),
            LearnerDefinition = Str(f["learnerDefinition"]),
            Senses = senses,
            Examples = StrList(f["examples"]),
            Synonyms = StrList(f["synonyms"]),
            Antonyms = StrList(f["antonyms"]),
            DeckIds = StrList(f["deckIds"]),
            Tags = StrList(f["tags"]),
            GeneratedBy = Str(f["generatedBy"]),
            SchemaVersion = (int)Long(f["schemaVersion"]),
        };
    }

    // ---- SrsState ----
    public static JsonObject ToFields(SrsState s)
    {
        var f = new JsonObject
        {
            ["repetitions"] = I(s.Repetitions),
            ["easeFactor"] = D(s.EaseFactor),
            ["intervalDays"] = I(s.IntervalDays),
            ["dueDate"] = I(s.DueDate),
        };
        if (s.LastReviewed is long lr) f["lastReviewed"] = I(lr);
        return f;
    }

    public static SrsState SrsFromFields(JsonObject f) => new()
    {
        Repetitions = (int)Long(f["repetitions"]),
        EaseFactor = Dbl(f["easeFactor"]),
        IntervalDays = (int)Long(f["intervalDays"]),
        DueDate = Long(f["dueDate"]),
        LastReviewed = f["lastReviewed"] is JsonNode lr ? Long(lr) : null,
    };

    // ---- Deck ----
    public static Deck DeckFromFields(JsonObject f) => new()
    {
        Id = Str(f["id"]),
        Name = Str(f["name"]),
        Color = Str(f["color"]),
        CreatedAt = Long(f["createdAt"]),
    };

    /// <summary>The document id is the last path segment of a Firestore resource name.</summary>
    public static string DocId(string name) => name.Split('/').Last();
}
