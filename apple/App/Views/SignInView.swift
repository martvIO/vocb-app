import SwiftUI
import FirebaseAuth

/// Email/password sign-in. (Sign in with Apple can be added with
/// `ASAuthorizationAppleIDProvider` + `OAuthProvider.appleCredential`; email is
/// the minimal path that works on both iPad and macOS.)
struct SignInView: View {
    @State private var email = ""
    @State private var password = ""
    @State private var error: String?
    @State private var notice: String?
    @State private var busy = false

    var body: some View {
        VStack(spacing: 16) {
            Text("Vocb").font(.largeTitle).bold()
            Text("Sign in to sync your vocabulary").foregroundStyle(.secondary)

            TextField("Email", text: $email)
                .textContentType(.username)
                #if os(iOS)
                .textInputAutocapitalization(.never)
                .keyboardType(.emailAddress)
                #endif
            SecureField("Password", text: $password)
                .textContentType(.password)

            if let error { Text(error).foregroundStyle(.red).font(.footnote) }
            if let notice { Text(notice).foregroundStyle(.green).font(.footnote) }

            HStack {
                Button("Create account") { authenticate(signUp: true) }
                    .buttonStyle(.bordered)
                Button("Sign in") { authenticate(signUp: false) }
                    .buttonStyle(.borderedProminent)
            }
            .disabled(busy || email.isEmpty || password.isEmpty)

            // Password reset only needs the email, so it's gated on email alone.
            Button("Forgot password?") { resetPassword() }
                .buttonStyle(.borderless)
                .font(.footnote)
                .disabled(busy || email.isEmpty)
        }
        .textFieldStyle(.roundedBorder)
        .frame(maxWidth: 360)
        .padding()
    }

    private func authenticate(signUp: Bool) {
        busy = true
        error = nil
        notice = nil
        Task {
            do {
                if signUp {
                    try await Auth.auth().createUser(withEmail: email, password: password)
                } else {
                    try await Auth.auth().signIn(withEmail: email, password: password)
                }
            } catch {
                self.error = error.localizedDescription
            }
            busy = false
        }
    }

    private func resetPassword() {
        busy = true
        error = nil
        notice = nil
        Task {
            do {
                try await Auth.auth().sendPasswordReset(withEmail: email)
                notice = "Password-reset email sent to \(email)."
            } catch {
                self.error = error.localizedDescription
            }
            busy = false
        }
    }
}
