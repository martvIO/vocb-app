// swift-tools-version:5.9
import PackageDescription

// VocabKit is the pure-Swift core shared by the iPad and macOS apps (and their
// Share/Action extensions). It deliberately has NO Firebase dependency so it
// stays fast to compile and fully unit-testable; the apps wire the concrete
// Firebase repository to the protocols defined here.
let package = Package(
    name: "VocabKit",
    platforms: [.iOS(.v16), .macOS(.v13)],
    products: [
        .library(name: "VocabKit", targets: ["VocabKit"])
    ],
    targets: [
        .target(name: "VocabKit"),
        .testTarget(name: "VocabKitTests", dependencies: ["VocabKit"])
    ]
)
