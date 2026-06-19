using Vocb.Firebase;

namespace Vocb.App.Services;

/// <summary>
/// The Vocb Firebase project's client configuration, baked into the app so users
/// never have to type it — the only thing they enter is their email + password.
///
/// A Firebase *web* API key identifies the project; it is not a secret (it ships in
/// every Firebase web/mobile client). Real protection comes from Firebase Auth +
/// Firestore security rules, not from hiding this key. Values mirror
/// backend/.firebaseConfig.json.
/// </summary>
public static class FirebaseDefaults
{
    public const string ProjectId = "vocb-8ed9d";
    public const string ApiKey = "AIzaSyDa_iutua8CRmoxL__4-ZWChsVXTrqvp3E";
    public const string FunctionsRegion = "us-central1";

    public static FirebaseConfig Config => new(ApiKey, ProjectId, FunctionsRegion);
}
