# Swift Gateway foundation

Add this local Swift package to an iOS 16+ app. The package has no third-party dependencies and uses URLSession, Security, CryptoKit, and Keychain. Run `swift test` on macOS with Xcode selected. Native tests cover public TLS/QR fixtures and the shared operation/watch contract.

The app owns QR scanning, a stable device ID, UI, grant-aware buttons, WatchConnectivity, and its Local Network privacy explanation (`NSLocalNetworkUsageDescription`). Retain one `DMGateway` actor per paired host and do not put bearer tokens in app preferences or iCloud backup. Invoke APIs from a task and update phone UI state on MainActor.

App Transport Security configuration belongs to the containing app. `NSAllowsLocalNetworking` covers local-network host forms, while arbitrary private DNS names with a self-signed certificate may require an ATS exception. The owner deferred the final containing-app exception choice until the Xcode/device test (2026-10-22); no ATS exception has been approved or applied, and this package does not modify an app's Info.plist. Its own transport enforces HTTPS, TLS 1.2+, hostname/validity checks, QR SPKI pinning, host thumbprint verification, and no redirects independently of the app's ATS settings.

See Apple's [manual server trust documentation](https://developer.apple.com/documentation/foundation/performing-manual-server-trust-authentication) and [local networking ATS setting](https://developer.apple.com/documentation/bundleresources/information-property-list/nsapptransportsecurity/nsallowslocalnetworking), then follow the [acceptance checklist](../README.md#acceptance-checklist). Passing `SecTrust` unit tests alone does not validate URLSession/ATS behavior on an iPhone.
