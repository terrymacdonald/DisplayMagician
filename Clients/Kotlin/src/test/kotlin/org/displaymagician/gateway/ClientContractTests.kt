package org.displaymagician.gateway

import java.io.File
import java.io.ByteArrayInputStream
import java.security.cert.CertificateFactory
import java.security.cert.X509Certificate
import java.time.Instant
import java.util.Base64
import org.json.JSONObject

/** Runs on a JVM with org.json, and can also be called from an Android instrumented test. */
fun main(args: Array<String>) {
    val fixture = JSONObject(File(args.single()).readText())
    val operation = DMOperation.fromJson(fixture.getJSONObject("operation"))
    check(operation.sequence == 4294967296L)
    val watch = DMWatchOperation.fromOperation(operation, "iRacing", listOf("operations-cancel"))
    check(watch.toJson().similar(fixture.getJSONObject("watch")))
    check(!DMWatchOperation.fromOperation(operation, "iRacing", emptyList()).canCancel)
    check(!DMWatchOperation.fromOperation(operation.copy(isFinished = true), "iRacing", listOf("operations-cancel")).canCancel)
    val state = DMWatchOperationState()
    check(state.accept(watch))
    check(!state.accept(watch.copy(sequence = watch.sequence - 1)))
    val problem = DMProblemDetails.fromJson(fixture.getJSONObject("problem"))
    check(problem.errorCode == "idempotency-outcome-unknown" && !problem.retryable)
    check(operation.toJson().similar(fixture.getJSONObject("operation")))
    val tls = JSONObject(File(File(args.single()).parentFile, "tls_contract.json").readText())
    val qr = DMPairing.decodeAppLink(tls.getString("appLink"), Instant.parse("2026-10-22T00:00:00Z"))
    check(qr.hostId == tls.getJSONObject("qr").getString("hostId"))
    check(qr.toString() == "DMPairingQrPayload(redacted)")
    val target = DMTarget.fromQr(qr)
    val identity = DMGatewayIdentity(qr.hostId, qr.hostIdentityPublicKeyJwk, qr.tlsSpkiSha256)
    DMPairing.verifyIdentity(identity, target)
    check(runCatching { DMPairing.verifyIdentity(identity.copy(hostId = "changed"), target.copy(hostId = "changed")) }.isFailure)
    check(runCatching { DMPairing.decodeAppLink(tls.getString("appLink"), Instant.parse("2031-01-01T00:00:00Z")) }.isFailure)
    val certificates = listOf("certificate", "renewedCertificate").map {
        CertificateFactory.getInstance("X.509").generateCertificate(ByteArrayInputStream(Base64.getDecoder().decode(tls.getString(it)))) as X509Certificate
    }
    check(!certificates[0].encoded.contentEquals(certificates[1].encoded))
    for (certificate in certificates) DMPinnedTrust(tls.getString("pin")).checkServerTrusted(arrayOf(certificate), "EC")
    check(runCatching { DMPinnedTrust(Base64.getEncoder().encodeToString(ByteArray(32))).checkServerTrusted(arrayOf(certificates.first()), "EC") }.isFailure)
    val sockets = DMPinnedSocketFactory(javax.net.ssl.SSLContext.getDefault().socketFactory)
    (sockets.createSocket() as javax.net.ssl.SSLSocket).use { socket -> check(socket.enabledProtocols.all { it == "TLSv1.2" || it == "TLSv1.3" }) }
    println("Kotlin operation, 64-bit sequence, watch relay, retry, QR expiry, TLS pin rejection, and same-key renewal contract checks passed.")
}
