using System.Text.Json.Serialization;

namespace Vocb.Core;

/// <summary>Request body for the lookupWord callable (raw selected text).</summary>
public sealed class LookupRequest
{
    [JsonPropertyName("text")] public string Text { get; set; } = "";
}

/// <summary>Response from lookupWord: the entry plus whether it was newly created.</summary>
public sealed class LookupResponse
{
    [JsonPropertyName("entry")] public WordEntry Entry { get; set; } = new();
    [JsonPropertyName("created")] public bool Created { get; set; }
}
