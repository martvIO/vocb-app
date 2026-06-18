using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using Vocb.Core;

namespace Vocb.Firebase;

/// <summary>
/// Reads/writes the user's vocabulary, SRS state, and decks via the Firestore
/// REST API. Word documents are created by the lookupWord function; this client
/// reads them and writes SRS state as the user studies.
/// </summary>
public sealed class FirestoreClient
{
    private readonly HttpClient _http;
    private readonly FirebaseConfig _config;

    public FirestoreClient(HttpClient http, FirebaseConfig config)
    {
        _http = http;
        _config = config;
    }

    public async Task<List<WordEntry>> ListWordsAsync(string uid, string idToken, CancellationToken ct = default)
    {
        var docs = await ListDocumentsAsync($"users/{uid}/words", idToken, ct);
        return docs.Select(d => FirestoreMapping.WordFromFields(d.fields)).ToList();
    }

    public async Task<Dictionary<string, SrsState>> ListSrsAsync(string uid, string idToken, CancellationToken ct = default)
    {
        var docs = await ListDocumentsAsync($"users/{uid}/srs", idToken, ct);
        return docs.ToDictionary(
            d => FirestoreMapping.DocId(d.name),
            d => FirestoreMapping.SrsFromFields(d.fields));
    }

    public async Task<List<Deck>> ListDecksAsync(string uid, string idToken, CancellationToken ct = default)
    {
        var docs = await ListDocumentsAsync($"users/{uid}/decks", idToken, ct);
        return docs.Select(d => FirestoreMapping.DeckFromFields(d.fields)).ToList();
    }

    /// <summary>Create/replace the SRS document for a lemma (PATCH = upsert in Firestore REST).</summary>
    public async Task SaveSrsAsync(string uid, string lemma, SrsState state, string idToken, CancellationToken ct = default)
    {
        var url = $"{_config.FirestoreBase}/users/{uid}/srs/{Uri.EscapeDataString(lemma)}";
        var body = new JsonObject { ["fields"] = FirestoreMapping.ToFields(state) };
        using var req = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json")
        };
        Authorize(req, idToken);
        var resp = await _http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();
    }

    // ---- internals ----

    private record struct FirestoreDoc(string name, JsonObject fields);

    private async Task<List<FirestoreDoc>> ListDocumentsAsync(string collectionPath, string idToken, CancellationToken ct)
    {
        var result = new List<FirestoreDoc>();
        string? pageToken = null;
        do
        {
            var url = $"{_config.FirestoreBase}/{collectionPath}?pageSize=300";
            if (pageToken is not null) url += $"&pageToken={Uri.EscapeDataString(pageToken)}";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            Authorize(req, idToken);
            var resp = await _http.SendAsync(req, ct);
            if (resp.StatusCode == HttpStatusCode.NotFound) break; // empty collection
            resp.EnsureSuccessStatusCode();

            var root = JsonNode.Parse(await resp.Content.ReadAsStringAsync(ct)) as JsonObject;
            if (root?["documents"] is JsonArray docs)
                foreach (var doc in docs)
                    if (doc is JsonObject o && o["fields"] is JsonObject f)
                        result.Add(new FirestoreDoc(o["name"]?.GetValue<string>() ?? "", f));

            pageToken = root?["nextPageToken"]?.GetValue<string>();
        } while (!string.IsNullOrEmpty(pageToken));

        return result;
    }

    private static void Authorize(HttpRequestMessage req, string idToken)
        => req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
}
