import XCTest
import Security
@testable import DisplayMagicianGateway

final class ClientContractTests: XCTestCase {
    func testPinHostnameRenewalAndPairingExpiry() throws {
        let file = URL(fileURLWithPath: #filePath).deletingLastPathComponent().deletingLastPathComponent().deletingLastPathComponent().deletingLastPathComponent().appendingPathComponent("Fixtures/tls_contract.json")
        let fixture = try JSONSerialization.jsonObject(with: Data(contentsOf: file)) as! [String: Any]
        let qr = try DMPairing.decodeAppLink(fixture["appLink"] as! String, now: Date(timeIntervalSince1970: 1792627200))
        XCTAssertThrowsError(try DMPairing.decodeAppLink(fixture["appLink"] as! String, now: Date(timeIntervalSince1970: 1924992000)))
        let target = try DMTarget(qr: qr)
        try DMPairing.verifyIdentity(DMGatewayIdentity(hostId: qr.hostId, hostIdentityPublicKeyJwk: qr.hostIdentityPublicKeyJwk, tlsSpkiSha256: qr.tlsSpkiSha256), target: target)
        XCTAssertThrowsError(try DMPairing.verifyIdentity(DMGatewayIdentity(hostId: "changed", hostIdentityPublicKeyJwk: qr.hostIdentityPublicKeyJwk, tlsSpkiSha256: qr.tlsSpkiSha256), target: target))
        XCTAssertNotEqual(fixture["certificate"] as! String, fixture["renewedCertificate"] as! String)
        XCTAssertEqual(String(describing: qr), "DMPairingQrPayload(redacted)")
        for property in ["certificate", "renewedCertificate"] {
            let data = Data(base64Encoded: fixture[property] as! String)!
            let certificate = SecCertificateCreateWithData(nil, data as CFData)!
            var trust: SecTrust?
            XCTAssertEqual(SecTrustCreateWithCertificates(certificate, SecPolicyCreateSSL(true, "displaymagician.test" as CFString), &trust), errSecSuccess)
            XCTAssertTrue(DMPinnedSession.validate(trust: trust!, host: "displaymagician.test", target: target))
            XCTAssertFalse(DMPinnedSession.validate(trust: trust!, host: "different.test", target: target))
        }
        var changed = fixture["qr"] as! [String: Any]
        changed["tlsSpkiSha256"] = Data(repeating: 0, count: 32).base64EncodedString()
        let wrongTarget = try DMTarget(qr: JSONDecoder().decode(DMPairingQrPayload.self, from: JSONSerialization.data(withJSONObject: changed)))
        let certificate = SecCertificateCreateWithData(nil, Data(base64Encoded: fixture["certificate"] as! String)! as CFData)!
        var trust: SecTrust?
        SecTrustCreateWithCertificates(certificate, SecPolicyCreateSSL(true, "displaymagician.test" as CFString), &trust)
        XCTAssertFalse(DMPinnedSession.validate(trust: trust!, host: "displaymagician.test", target: wrongTarget))
    }

    func testOperationAndWatchRelayContract() throws {
        let file = URL(fileURLWithPath: #filePath).deletingLastPathComponent().deletingLastPathComponent().deletingLastPathComponent().deletingLastPathComponent().appendingPathComponent("Fixtures/mobile_contract.json")
        let fixture = try JSONSerialization.jsonObject(with: Data(contentsOf: file)) as! [String: Any]
        let operation = try JSONDecoder().decode(DMOperation.self, from: JSONSerialization.data(withJSONObject: fixture["operation"]!))
        XCTAssertEqual(operation.sequence, 4294967296)
        let watch = DMWatchOperation(operation: operation, title: "iRacing", grants: ["operations-cancel"])
        let actual = try JSONSerialization.jsonObject(with: JSONEncoder().encode(watch)) as! NSDictionary
        XCTAssertEqual(actual, fixture["watch"] as! NSDictionary)
        XCTAssertFalse(DMWatchOperation(operation: operation, title: "iRacing", grants: []).canCancel)
        var state = DMWatchOperationState()
        XCTAssertTrue(state.accept(watch))
        XCTAssertFalse(state.accept(watch))
        let problem = try JSONDecoder().decode(DMProblemDetails.self, from: JSONSerialization.data(withJSONObject: fixture["problem"]!))
        XCTAssertEqual(problem.errorCode, "idempotency-outcome-unknown")
        XCTAssertFalse(problem.retryable)
    }
}
