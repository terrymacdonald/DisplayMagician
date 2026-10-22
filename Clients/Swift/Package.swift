// swift-tools-version: 5.9
import PackageDescription

let package = Package(
    name: "DisplayMagicianGateway",
    platforms: [.iOS(.v16), .macOS(.v13)],
    products: [.library(name: "DisplayMagicianGateway", targets: ["DisplayMagicianGateway"])],
    targets: [
        .target(name: "DisplayMagicianGateway"),
        .testTarget(name: "DisplayMagicianGatewayTests", dependencies: ["DisplayMagicianGateway"])
    ]
)
