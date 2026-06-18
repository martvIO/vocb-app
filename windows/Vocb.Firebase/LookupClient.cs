using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vocb.Core;

namespace Vocb.Firebase;

public sealed class LookupException(string message) : Exception(message);

/// <summary>
/// Calls the lookupWord HTTPS callable. The callable wire format wraps the
/// payload in {"data": ...} and the response in {"result": ...} (or {"error": ...}).
/// </summary>
public sealed class LookupClient
{
    private readonly HttpClient _http;
    private readonly FirebaseConfig _config;

    public LookupClient(HttpClient http, FirebaseConfig config)
    {
        _http = http;
        _config = config;
    }

    /// <summary>Send raw selected text; the backend normalizes and returns the entry.</summary>
    public async Task<LookupResponse> LookupAsync(string text, string idToken, CancellationToken ct = default)
    {
        var payload = new JsonObject { ["data"] = new JsonObject { ["text"] = text } };
        using var req = new HttpRequestMessage(HttpMethod.Post, _config.CallableUrl("lookupWord"))
        {
            Content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json")
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        var resp = await _http.SendAsync(req, ct);
        var json = await resp.Content.ReadAsStringAsync(ct);
        var root = JsonNode.Parse(json) as JsonObject;

        if (root?["error"] is JsonObject error)
            throw new LookupException(error["message"]?.GetValue<string>() ?? "lookup failed");

        resp.EnsureSuccessStatusCode();

        var result = root?["result"] ?? throw new LookupException("missing result");
        return result.Deserialize<LookupResponse>()
               ?? throw new LookupException("could not parse result");
    }
}
