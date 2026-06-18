namespace Vocb.Firebase;

/// <summary>
/// Firebase project settings the Windows client needs. There is no official
/// Firebase SDK for Windows/.NET, so we talk to Firebase over REST:
///   - Auth      → Identity Toolkit REST (secureToken/identitytoolkit)
///   - Firestore → Firestore REST (firestore.googleapis.com/v1)
///   - lookupWord→ the HTTPS callable endpoint on Cloud Functions
/// </summary>
public sealed record FirebaseConfig(
    string ApiKey,
    string ProjectId,
    string FunctionsRegion = "us-central1")
{
    public string FirestoreBase =>
        $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents";

    public string CallableUrl(string name) =>
        $"https://{FunctionsRegion}-{ProjectId}.cloudfunctions.net/{name}";
}
