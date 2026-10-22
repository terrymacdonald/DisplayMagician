import Foundation
import CryptoKit
import Security

public enum DMGatewayError: Error {
    case invalidPairing, identityChanged, protocolMismatch, invalidMutation, storage(OSStatus)
    case http(status: Int, problem: DMProblemDetails?, retryAfter: String?, location: String?)
}

public struct DMResponse<Value: Sendable>: Sendable {
    public let status: Int
    public let value: Value?
    public let etag: String?
    public let location: String?
    public let retryAfter: String?
}

public struct DMEmpty: Codable, Sendable { public init() {} }

public enum DMController {
    public static let capabilities = ["status-read", "decisions-read", "decisions-answer", "display-profiles-read", "display-profiles-apply", "audio-profiles-read", "audio-profiles-apply", "shortcuts-read", "shortcuts-run", "operations-cancel"]
}

public struct DMTarget: Codable, Sendable, Equatable {
    public let origin: String
    public let hostId: String
    public let hostIdentityPublicKeyJwk: String
    public let tlsSpkiSha256: String

    public init(qr: DMPairingQrPayload) throws {
        guard let url = URLComponents(string: qr.gatewayUri), url.scheme == "https", url.host != nil,
              url.user == nil, url.password == nil, url.query == nil, url.fragment == nil,
              url.path.isEmpty || url.path == "/", let pin = Data(base64Encoded: qr.tlsSpkiSha256), pin.count == 32,
              pin.base64EncodedString() == qr.tlsSpkiSha256 else { throw DMGatewayError.invalidPairing }
        self.origin = qr.gatewayUri.hasSuffix("/") ? String(qr.gatewayUri.dropLast()) : qr.gatewayUri
        self.hostId = qr.hostId
        self.hostIdentityPublicKeyJwk = qr.hostIdentityPublicKeyJwk
        self.tlsSpkiSha256 = qr.tlsSpkiSha256
    }
}

public enum DMPairing {
    internal static func verifyIdentity(_ identity: DMGatewayIdentity, target: DMTarget) throws {
        guard identity.hostId == target.hostId, identity.tlsSpkiSha256 == target.tlsSpkiSha256,
              let bytes = identity.hostIdentityPublicKeyJwk.data(using: .utf8),
              let key = try JSONSerialization.jsonObject(with: bytes) as? [String: String],
              let expectedBytes = target.hostIdentityPublicKeyJwk.data(using: .utf8),
              let expected = try JSONSerialization.jsonObject(with: expectedBytes) as? [String: String], key == expected,
              key["kty"] == "EC", key["crv"] == "P-256", let x = key["x"], let y = key["y"],
              decodeBase64url(x)?.count == 32, decodeBase64url(y)?.count == 32 else { throw DMGatewayError.identityChanged }
        let canonical = try JSONSerialization.data(withJSONObject: ["crv": "P-256", "kty": "EC", "x": x, "y": y], options: [.sortedKeys, .withoutEscapingSlashes])
        guard base64url(Data(SHA256.hash(data: canonical))) == target.hostId else { throw DMGatewayError.identityChanged }
    }

    public static func decodeAppLink(_ link: String, now: Date = Date()) throws -> DMPairingQrPayload {
        guard let url = URLComponents(string: link), url.scheme == "displaymagician", url.host == "pair",
              url.fragment == nil, url.path.isEmpty,
              let query = url.queryItems, query.count == 1, query[0].name == "payload", let encoded = query[0].value,
              let bytes = decodeBase64url(encoded) else { throw DMGatewayError.invalidPairing }
        let qr = try JSONDecoder().decode(DMPairingQrPayload.self, from: bytes)
        let dates = ISO8601DateFormatter()
        dates.formatOptions = [.withInternetDateTime, .withFractionalSeconds]
        let fractional = dates.date(from: qr.expiresAt)
        dates.formatOptions = [.withInternetDateTime]
        guard qr.version == 1, let expiry = fractional ?? dates.date(from: qr.expiresAt), expiry > now,
              decodeBase64url(qr.pairingSecret)?.count == 32, UUID(uuidString: qr.pairingRequestId) != nil else { throw DMGatewayError.invalidPairing }
        _ = try DMTarget(qr: qr)
        return qr
    }

    internal static func base64url(_ value: Data) -> String { value.base64EncodedString().replacingOccurrences(of: "+", with: "-").replacingOccurrences(of: "/", with: "_").replacingOccurrences(of: "=", with: "") }
    internal static func decodeBase64url(_ value: String) -> Data? {
        guard value.allSatisfy({ $0.isASCII && ($0.isLetter || $0.isNumber || $0 == "-" || $0 == "_") }), value.count % 4 != 1 else { return nil }
        let padded = value.replacingOccurrences(of: "-", with: "+").replacingOccurrences(of: "_", with: "/") + String(repeating: "=", count: (4 - value.count % 4) % 4)
        guard let bytes = Data(base64Encoded: padded), base64url(bytes) == value else { return nil }
        return bytes
    }
    internal static func randomSecret() throws -> String {
        var bytes = [UInt8](repeating: 0, count: 32)
        let result = SecRandomCopyBytes(kSecRandomDefault, bytes.count, &bytes)
        guard result == errSecSuccess else { throw DMGatewayError.storage(result) }
        return base64url(Data(bytes))
    }
}

public struct DMPendingMutation: Codable, Sendable, CustomStringConvertible, CustomDebugStringConvertible {
    public var description: String { "DMPendingMutation(redacted)" }
    public var debugDescription: String { description }
    public let key: String
    public let method: String
    public let pathAndQuery: String
    public let body: Data?
    public var responseStatus: Int?
    public var responseLocation: String?
}

private struct DMCandidate: Codable { let requestId: String; let pollingSecret: String }
private struct DMStoredState: Codable {
    let target: DMTarget
    let deviceId: String
    var credential: String?
    var candidate: DMCandidate?
    var pending: [String: DMPendingMutation] = [:]
}

/// Own one actor per paired host. Each mutation key is persisted with its exact request before transmission.
public actor DMGateway {
    internal static var encoder: JSONEncoder { let value = JSONEncoder(); value.outputFormatting = [.sortedKeys, .withoutEscapingSlashes]; return value }
    private var state: DMStoredState
    private let storageKey: String
    private let session: URLSession
    private var identityVerified = false
    private var nextPairingPoll = Date.distantPast

    public init(target: DMTarget, deviceId: String, storageKey: String) throws {
        guard let origin = URLComponents(string: target.origin), origin.scheme == "https", origin.host != nil,
              origin.user == nil, origin.password == nil, origin.path.isEmpty, origin.query == nil, origin.fragment == nil else { throw DMGatewayError.invalidPairing }
        self.storageKey = storageKey
        if let saved = try DMKeychain.read(storageKey) {
            let loaded = try JSONDecoder().decode(DMStoredState.self, from: saved)
            guard loaded.target == target, loaded.deviceId == deviceId else { throw DMGatewayError.identityChanged }
            self.state = loaded
        } else {
            self.state = DMStoredState(target: target, deviceId: deviceId)
        }
        let configuration = URLSessionConfiguration.ephemeral
        configuration.urlCache = nil
        configuration.httpCookieStorage = nil
        configuration.timeoutIntervalForRequest = 15
        configuration.timeoutIntervalForResource = 30
        configuration.tlsMinimumSupportedProtocolVersion = .TLSv12
        self.session = URLSession(configuration: configuration, delegate: DMPinnedSession(target: target), delegateQueue: nil)
    }

    deinit { session.invalidateAndCancel() }

    public func verifyIdentity() async throws {
        let response = try await getIdentity()
        guard let identity = response.value else { throw DMGatewayError.identityChanged }
        try DMPairing.verifyIdentity(identity, target: state.target)
        identityVerified = true
    }

    public func startPairing(qr: DMPairingQrPayload, displayName: String, capabilities: [String] = DMController.capabilities) async throws -> DMResponse<DMPairingSubmissionAccepted> {
        guard try DMTarget(qr: qr) == state.target else { throw DMGatewayError.identityChanged }
        let polling = try DMPairing.randomSecret()
        state.candidate = DMCandidate(requestId: qr.pairingRequestId, pollingSecret: polling)
        try save()
        let body = DMPairingSubmissionRequest(pairingRequestId: qr.pairingRequestId, pairingSecret: qr.pairingSecret, pollingSecret: polling,
            deviceId: state.deviceId, displayName: displayName, clientType: "displaymagician-ios", requestedCapabilities: capabilities)
        return try await requestPairing(body: body, idempotencyKey: UUID().uuidString.lowercased())
    }

    public func pollPairing() async throws -> DMResponse<DMPairingPollResult> {
        let wait = max(0, nextPairingPoll.timeIntervalSinceNow)
        nextPairingPoll = Date().addingTimeInterval(wait + 2)
        if wait > 0 { try await Task.sleep(nanoseconds: UInt64(wait * 1_000_000_000)) }
        guard let candidate = state.candidate else { throw DMGatewayError.invalidPairing }
        let response: DMResponse<DMPairingPollResult>
        do { response = try await getPairingRequest(pairingRequestId: candidate.requestId, pollingSecret: candidate.pollingSecret) }
        catch DMGatewayError.http(let status, let problem, let retryAfter, let location) where status == 410 {
            try clearCandidate()
            throw DMGatewayError.http(status: status, problem: problem, retryAfter: retryAfter, location: location)
        }
        if let result = response.value, result.status == "approved" {
            guard let credential = result.credential, !credential.isEmpty else { throw DMGatewayError.invalidPairing }
            let previous = state
            state.credential = credential
            state.candidate = nil
            state.pending = state.pending.filter { $0.value.pathAndQuery != "/v1/pairing-requests" }
            do { try save() } catch { state = previous; throw error } // A lost durable delivery requires pairing again.
        }
        if response.value?.status == "rejected" { try clearCandidate() }
        return response
    }

    private func clearCandidate() throws {
        state.candidate = nil
        state.pending = state.pending.filter { $0.value.pathAndQuery != "/v1/pairing-requests" }
        try save()
    }

    public func pendingMutations() -> [DMPendingMutation] { Array(state.pending.values) }
    public func waitForOperation(_ operationId: String, onUpdate: @Sendable (DMOperation) async -> Void) async throws -> DMOperation {
        var seconds = 1
        while true {
            try Task.checkCancellation()
            do {
                guard let operation = try await getOperation(operationId: operationId).value else { throw DMGatewayError.protocolMismatch }
                await onUpdate(operation)
                if operation.isFinished { return operation }
            } catch DMGatewayError.http(let status, _, let retryAfter, _) where status == 429 || status == 503 {
                try await Task.sleep(nanoseconds: UInt64(max(1, Int(retryAfter ?? "") ?? seconds)) * 1_000_000_000)
                continue
            } catch let error as URLError {
                guard [.notConnectedToInternet, .timedOut, .networkConnectionLost, .cannotConnectToHost, .cannotFindHost, .dnsLookupFailed].contains(error.code) else { throw error }
                // Safe GET polling may resume after network loss; mutations require their original persisted key.
            }
            try await Task.sleep(nanoseconds: UInt64(seconds) * 1_000_000_000)
            seconds = min(seconds * 2, 5)
        }
    }
    public static func forgetPairing(storageKey: String) throws { try DMKeychain.delete(storageKey) }
    public func acknowledgeMutation(_ key: String) throws { state.pending.removeValue(forKey: key); try save() }
    public func retryMutation<T: Decodable & Sendable>(_ key: String, as: T.Type) async throws -> DMResponse<T> {
        guard let pending = state.pending[key] else { throw DMGatewayError.invalidMutation }
        let authorization: String?
        if pending.pathAndQuery == "/v1/pairing-requests" { authorization = nil }
        else { authorization = try bearer() }
        return try await execute(method: pending.method, pathAndQuery: pending.pathAndQuery, body: pending.body, key: key,
            authorization: authorization, etag: nil)
    }
    internal func bearer() throws -> String { guard let credential = state.credential else { throw DMGatewayError.invalidPairing }; return "Bearer " + credential }
    internal static func segment(_ value: String) -> String { value.addingPercentEncoding(withAllowedCharacters: CharacterSet(charactersIn: "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~"))! }
    internal func send<T: Decodable & Sendable>(method: String, path: String, query: [String: String?], body: Data?, key: String?, authorization: String?, etag: String?) async throws -> DMResponse<T> {
        var components = URLComponents()
        components.queryItems = query.sorted { $0.key < $1.key }.compactMap { key, value in value.map { URLQueryItem(name: key, value: $0) } }
        return try await execute(method: method, pathAndQuery: path + (components.percentEncodedQuery.map { $0.isEmpty ? "" : "?" + $0 } ?? ""), body: body, key: key, authorization: authorization, etag: etag)
    }
    private func execute<T: Decodable & Sendable>(method: String, pathAndQuery: String, body: Data?, key: String?, authorization: String?, etag: String?) async throws -> DMResponse<T> {
        if pathAndQuery != "/v1/identity" && !identityVerified { try await verifyIdentity() }
        guard pathAndQuery.hasPrefix("/v1/"), !pathAndQuery.contains("#"), let url = URL(string: state.target.origin + pathAndQuery) else { throw DMGatewayError.invalidPairing }
        if let key {
            guard let uuid = UUID(uuidString: key), uuid.uuidString.lowercased() == key, key[key.index(key.startIndex, offsetBy: 14)] == "4",
                  "89ab".contains(key[key.index(key.startIndex, offsetBy: 19)]) else { throw DMGatewayError.invalidMutation }
            if let saved = state.pending[key] {
                guard saved.method == method, saved.pathAndQuery == pathAndQuery, saved.body == body else { throw DMGatewayError.invalidMutation }
            } else {
                state.pending[key] = DMPendingMutation(key: key, method: method, pathAndQuery: pathAndQuery, body: body)
            }
            try save()
        }
        var request = URLRequest(url: url)
        request.httpMethod = method
        request.httpBody = body
        request.setValue("application/json", forHTTPHeaderField: "Accept")
        if body != nil { request.setValue("application/json", forHTTPHeaderField: "Content-Type") }
        request.setValue(authorization, forHTTPHeaderField: "Authorization")
        request.setValue(key, forHTTPHeaderField: "Idempotency-Key")
        request.setValue(etag, forHTTPHeaderField: "If-None-Match")
        let hello: [String: Any] = ["MinimumProtocolVersion": dmProtocolVersion, "MaximumProtocolVersion": dmProtocolVersion, "ClientKind": 5,
            "ClientId": state.deviceId, "DeviceId": state.deviceId, "DisplayName": "DisplayMagician iOS", "RequiredCapabilities": ["protocol-negotiation"], "OptionalCapabilities": []]
        request.setValue(DMPairing.base64url(try JSONSerialization.data(withJSONObject: hello, options: [.sortedKeys])), forHTTPHeaderField: "DisplayMagician-Protocol-Hello")
        let (data, raw) = try await session.data(for: request)
        guard let http = raw as? HTTPURLResponse else { throw DMGatewayError.protocolMismatch }
        let location = http.value(forHTTPHeaderField: "Location")
        if let key { state.pending[key]?.responseStatus = http.statusCode; state.pending[key]?.responseLocation = location; try save() }
        guard (200..<300).contains(http.statusCode) || http.statusCode == 304 else {
            throw DMGatewayError.http(status: http.statusCode, problem: try? JSONDecoder().decode(DMProblemDetails.self, from: data), retryAfter: http.value(forHTTPHeaderField: "Retry-After"), location: location)
        }
        guard let welcome = http.value(forHTTPHeaderField: "DisplayMagician-Protocol-Welcome"), let bytes = DMPairing.decodeBase64url(welcome),
              let json = try JSONSerialization.jsonObject(with: bytes) as? [String: Any], json["SelectedProtocolVersion"] as? Int == dmProtocolVersion, json["EndpointKind"] as? String == "Gateway",
              (json["SupportedCapabilities"] as? [String])?.contains("protocol-negotiation") == true else { throw DMGatewayError.protocolMismatch }
        let value: T?
        if http.statusCode == 304 { value = nil }
        else if data.isEmpty && T.self == DMEmpty.self { value = DMEmpty() as? T }
        else { value = try JSONDecoder().decode(T.self, from: data) }
        return DMResponse(status: http.statusCode, value: value, etag: http.value(forHTTPHeaderField: "ETag"), location: location, retryAfter: http.value(forHTTPHeaderField: "Retry-After"))
    }
    private func save() throws { try DMKeychain.write(storageKey, data: Self.encoder.encode(state)) }
}

private enum DMKeychain {
    static func delete(_ account: String) throws {
        let query: [String: Any] = [kSecClass as String: kSecClassGenericPassword, kSecAttrService as String: "org.displaymagician.gateway", kSecAttrAccount as String: account]
        let status = SecItemDelete(query as CFDictionary)
        guard status == errSecSuccess || status == errSecItemNotFound else { throw DMGatewayError.storage(status) }
    }
    static func read(_ account: String) throws -> Data? {
        let query: [String: Any] = [kSecClass as String: kSecClassGenericPassword, kSecAttrService as String: "org.displaymagician.gateway", kSecAttrAccount as String: account, kSecReturnData as String: true]
        var result: CFTypeRef?
        let status = SecItemCopyMatching(query as CFDictionary, &result)
        if status == errSecItemNotFound { return nil }
        guard status == errSecSuccess else { throw DMGatewayError.storage(status) }
        return result as? Data
    }
    static func write(_ account: String, data: Data) throws {
        let query: [String: Any] = [kSecClass as String: kSecClassGenericPassword, kSecAttrService as String: "org.displaymagician.gateway", kSecAttrAccount as String: account]
        let values: [String: Any] = [kSecValueData as String: data, kSecAttrAccessible as String: kSecAttrAccessibleAfterFirstUnlockThisDeviceOnly]
        var status = SecItemUpdate(query as CFDictionary, values as CFDictionary)
        if status == errSecItemNotFound { status = SecItemAdd(query.merging(values) { _, new in new } as CFDictionary, nil) }
        guard status == errSecSuccess else { throw DMGatewayError.storage(status) }
    }
}

internal final class DMPinnedSession: NSObject, URLSessionDelegate, URLSessionTaskDelegate, @unchecked Sendable {
    private let target: DMTarget
    init(target: DMTarget) { self.target = target; super.init() }
    func urlSession(_ session: URLSession, didReceive challenge: URLAuthenticationChallenge, completionHandler: @escaping (URLSession.AuthChallengeDisposition, URLCredential?) -> Void) {
        guard challenge.protectionSpace.authenticationMethod == NSURLAuthenticationMethodServerTrust else { completionHandler(.performDefaultHandling, nil); return }
        guard let trust = challenge.protectionSpace.serverTrust,
              Self.validate(trust: trust, host: challenge.protectionSpace.host, target: target) else { completionHandler(.cancelAuthenticationChallenge, nil); return }
        completionHandler(.useCredential, URLCredential(trust: trust))
    }
    internal static func validate(trust: SecTrust, host: String, target: DMTarget) -> Bool {
        let presentedHost = host.trimmingCharacters(in: CharacterSet(charactersIn: "[]"))
        guard let expectedHost = URL(string: target.origin)?.host?.trimmingCharacters(in: CharacterSet(charactersIn: "[]")), presentedHost.caseInsensitiveCompare(expectedHost) == .orderedSame,
              let certificate = (SecTrustCopyCertificateChain(trust) as? [SecCertificate])?.first,
              let key = SecCertificateCopyKey(certificate), let exported = SecKeyCopyExternalRepresentation(key, nil) as Data?, exported.count == 65, exported.first == 4 else { return false }
        // Gateway TLS keys are P-256. Security exports the point, so prepend its standard SPKI algorithm/curve header.
        let prefix: [UInt8] = [0x30,0x59,0x30,0x13,0x06,0x07,0x2a,0x86,0x48,0xce,0x3d,0x02,0x01,0x06,0x08,0x2a,0x86,0x48,0xce,0x3d,0x03,0x01,0x07,0x03,0x42,0x00]
        let pin = Data(SHA256.hash(data: Data(prefix) + exported)).base64EncodedString()
        guard pin == target.tlsSpkiSha256,
              SecTrustSetAnchorCertificates(trust, [certificate] as CFArray) == errSecSuccess,
              SecTrustSetAnchorCertificatesOnly(trust, true) == errSecSuccess,
              SecTrustSetPolicies(trust, SecPolicyCreateSSL(true, presentedHost as CFString)) == errSecSuccess,
              SecTrustEvaluateWithError(trust, nil) else { return false }
        return true
    }
    func urlSession(_ session: URLSession, task: URLSessionTask, willPerformHTTPRedirection response: HTTPURLResponse, newRequest request: URLRequest, completionHandler: @escaping (URLRequest?) -> Void) { completionHandler(nil) }
}
