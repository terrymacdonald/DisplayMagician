package org.displaymagician.gateway

import android.content.Context
import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import android.util.AtomicFile
import java.util.Base64
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.delay
import kotlinx.coroutines.withContext
import org.json.JSONArray
import org.json.JSONObject
import java.io.File
import java.io.ByteArrayOutputStream
import java.io.FileNotFoundException
import java.io.IOException
import java.net.URI
import java.net.URLEncoder
import java.net.InetAddress
import java.net.Socket
import java.nio.charset.StandardCharsets
import java.security.KeyStore
import java.security.MessageDigest
import java.security.SecureRandom
import java.security.cert.X509Certificate
import java.time.Instant
import java.util.UUID
import javax.crypto.Cipher
import javax.crypto.KeyGenerator
import javax.crypto.SecretKey
import javax.crypto.spec.GCMParameterSpec
import javax.net.ssl.HttpsURLConnection
import javax.net.ssl.SSLContext
import javax.net.ssl.X509TrustManager
import javax.net.ssl.SSLSocket
import javax.net.ssl.SSLSocketFactory

data class DMResponse<T>(val status: Int, val value: T?, val etag: String?, val location: String?, val retryAfter: String?)
object DMEmpty
object DMController { val capabilities = listOf("status-read", "decisions-read", "decisions-answer", "display-profiles-read", "display-profiles-apply", "audio-profiles-read", "audio-profiles-apply", "shortcuts-read", "shortcuts-run", "operations-cancel") }
class DMGatewayException(val status: Int, val problem: DMProblemDetails?, val retryAfter: String?, val location: String?) : Exception("Gateway returned HTTP $status")

data class DMTarget(val origin: String, val hostId: String, val hostIdentityPublicKeyJwk: String, val tlsSpkiSha256: String) {
    companion object {
        fun fromQr(qr: DMPairingQrPayload): DMTarget {
            val uri = URI(qr.gatewayUri)
            require(uri.scheme == "https" && uri.host != null && uri.rawUserInfo == null && uri.rawQuery == null && uri.rawFragment == null && uri.rawPath in listOf("", "/")) { "Invalid Gateway origin" }
            val pin = Base64.getDecoder().decode(qr.tlsSpkiSha256)
            require(pin.size == 32 && Base64.getEncoder().encodeToString(pin) == qr.tlsSpkiSha256) { "Invalid TLS pin" }
            return DMTarget(qr.gatewayUri.trimEnd('/'), qr.hostId, qr.hostIdentityPublicKeyJwk, qr.tlsSpkiSha256)
        }
    }
    internal fun toJson() = JSONObject().put("origin", origin).put("hostId", hostId).put("hostIdentityPublicKeyJwk", hostIdentityPublicKeyJwk).put("tlsSpkiSha256", tlsSpkiSha256)
}

object DMPairing {
    internal fun verifyIdentity(identity: DMGatewayIdentity, target: DMTarget) {
        val key = JSONObject(identity.hostIdentityPublicKeyJwk)
        val expected = JSONObject(target.hostIdentityPublicKeyJwk)
        require(identity.hostId == target.hostId && identity.tlsSpkiSha256 == target.tlsSpkiSha256 && key.getString("kty") == "EC" && key.getString("crv") == "P-256" && expected.getString("kty") == "EC" && expected.getString("crv") == "P-256") { "Gateway identity changed" }
        val x = key.getString("x")
        val y = key.getString("y")
        require(decodeBase64url(x).size == 32 && decodeBase64url(y).size == 32 && expected.getString("x") == x && expected.getString("y") == y) { "Gateway host key changed" }
        val canonical = "{\"crv\":\"P-256\",\"kty\":\"EC\",\"x\":\"$x\",\"y\":\"$y\"}"
        require(base64url(MessageDigest.getInstance("SHA-256").digest(canonical.toByteArray(StandardCharsets.UTF_8))) == target.hostId) { "Invalid host thumbprint" }
    }

    fun decodeAppLink(link: String, now: Instant = Instant.now()): DMPairingQrPayload {
        val uri = URI(link)
        require(uri.scheme == "displaymagician" && uri.host == "pair" && uri.rawPath.isNullOrEmpty() && uri.rawFragment == null) { "Invalid pairing link" }
        val query = uri.rawQuery ?: error("Missing pairing payload")
        require(query.startsWith("payload=") && !query.contains('&')) { "Invalid pairing payload" }
        val qr = DMPairingQrPayload.fromJson(JSONObject(String(decodeBase64url(query.substring(8)), StandardCharsets.UTF_8)))
        require(qr.version == 1 && Instant.parse(qr.expiresAt).isAfter(now) && decodeBase64url(qr.pairingSecret).size == 32) { "Expired or invalid pairing QR" }
        UUID.fromString(qr.pairingRequestId)
        DMTarget.fromQr(qr)
        return qr
    }
    internal fun base64url(bytes: ByteArray): String = Base64.getUrlEncoder().withoutPadding().encodeToString(bytes)
    internal fun decodeBase64url(value: String): ByteArray {
        require(value.matches(Regex("[A-Za-z0-9_-]*")) && value.length % 4 != 1) { "Invalid base64url" }
        val bytes = Base64.getUrlDecoder().decode(value)
        require(base64url(bytes) == value) { "Noncanonical base64url" }
        return bytes
    }
    internal fun randomSecret(): String = base64url(ByteArray(32).also { SecureRandom().nextBytes(it) })
}

data class DMPendingMutation(val key: String, val method: String, val pathAndQuery: String, val body: String?, val responseStatus: Int?, val responseLocation: String?) {
    override fun toString(): String = "DMPendingMutation(redacted)"
}

/** Own one client per paired host. No mutation is automatically retried with a different key. */
class DMGateway(context: Context, private val target: DMTarget, private val deviceId: String, storageKey: String) {
    private val stateLock = Any()
    private val vault = DMKeyStoreVault(context.applicationContext, storageKey)
    private var state: JSONObject = vault.read()?.let { JSONObject(it) } ?: JSONObject().put("target", target.toJson()).put("deviceId", deviceId).put("pending", JSONObject())
    @Volatile private var identityVerified = false
    private var nextPairingPoll = 0L
    private val socketFactory = DMPinnedSocketFactory(SSLContext.getInstance("TLS").apply { init(null, arrayOf(DMPinnedTrust(target.tlsSpkiSha256)), SecureRandom()) }.socketFactory)

    init {
        val origin = URI(target.origin)
        require(origin.scheme == "https" && origin.host != null && origin.rawPath.isNullOrEmpty() && origin.rawUserInfo == null && origin.rawQuery == null && origin.rawFragment == null) { "Invalid Gateway origin" }
        val saved = state.getJSONObject("target")
        require(saved.getString("origin") == target.origin && saved.getString("hostId") == target.hostId && saved.getString("tlsSpkiSha256") == target.tlsSpkiSha256 && state.getString("deviceId") == deviceId) { "Stored pairing belongs to another host or device" }
    }

    companion object { fun forgetPairing(context: Context, storageKey: String) { DMKeyStoreVault(context.applicationContext, storageKey).delete() } }

    suspend fun verifyIdentity() {
        val identity = getIdentity().value ?: error("Missing Gateway identity")
        DMPairing.verifyIdentity(identity, target)
        identityVerified = true
    }

    suspend fun startPairing(qr: DMPairingQrPayload, displayName: String, capabilities: List<String> = DMController.capabilities): DMResponse<DMPairingSubmissionAccepted> {
        require(DMTarget.fromQr(qr) == target && Instant.parse(qr.expiresAt).isAfter(Instant.now())) { "Invalid or expired pairing QR" }
        val polling = DMPairing.randomSecret()
        synchronized(stateLock) { state.put("candidate", JSONObject().put("requestId", qr.pairingRequestId).put("pollingSecret", polling)); save() }
        return requestPairing(body = DMPairingSubmissionRequest(qr.pairingRequestId, qr.pairingSecret, polling, deviceId, displayName, "displaymagician-android", capabilities), idempotencyKey = UUID.randomUUID().toString())
    }

    suspend fun pollPairing(): DMResponse<DMPairingPollResult> {
        val wait = synchronized(stateLock) { val now = android.os.SystemClock.elapsedRealtime(); val delay = (nextPairingPoll - now).coerceAtLeast(0); nextPairingPoll = now + delay + 2000; delay }
        if (wait > 0) delay(wait)
        val candidate = synchronized(stateLock) { state.getJSONObject("candidate").let { it.getString("requestId") to it.getString("pollingSecret") } }
        val response = try { getPairingRequest(pairingRequestId = candidate.first, pollingSecret = candidate.second) }
        catch (error: DMGatewayException) {
            if (error.status == 410) clearCandidate()
            throw error
        }
        response.value?.takeIf { it.status == "approved" }?.let { result ->
            val credential = result.credential ?: error("Missing one-time credential; pair again")
            synchronized(stateLock) {
                val next = JSONObject(state.toString())
                next.put("credential", credential).remove("candidate")
                val pending = next.getJSONObject("pending")
                pending.keys().asSequence().toList().filter { pending.getJSONObject(it).getString("path") == "/v1/pairing-requests" }.forEach { pending.remove(it) }
                vault.write(next.toString())
                state = next
            }
        }
        if (response.value?.status == "rejected") clearCandidate()
        return response
    }

    private fun clearCandidate() = synchronized(stateLock) {
        state.remove("candidate")
        val pending = state.getJSONObject("pending")
        pending.keys().asSequence().toList().filter { pending.getJSONObject(it).getString("path") == "/v1/pairing-requests" }.forEach { pending.remove(it) }
        save()
    }

    fun pendingMutations(): List<DMPendingMutation> = synchronized(stateLock) {
        val pending = state.getJSONObject("pending")
        pending.keys().asSequence().map { key -> pending.getJSONObject(key).let { DMPendingMutation(key, it.getString("method"), it.getString("path"), it.optNullableString("body"), if (it.has("status")) it.getInt("status") else null, it.optNullableString("location")) } }.toList()
    }
    fun acknowledgeMutation(key: String) = synchronized(stateLock) { state.getJSONObject("pending").remove(key); save() }
    suspend fun waitForOperation(operationId: String, onUpdate: (DMOperation) -> Unit): DMOperation {
        var seconds = 1L
        while (true) {
            try {
                val operation = getOperation(operationId).value ?: error("Missing operation")
                onUpdate(operation)
                if (operation.isFinished) return operation
            } catch (error: DMGatewayException) {
                if (error.status !in listOf(429, 503)) throw error
                delay((error.retryAfter?.toLongOrNull() ?: seconds).coerceAtLeast(1) * 1000)
                continue
            } catch (error: javax.net.ssl.SSLException) {
                throw error
            } catch (_: IOException) {
                // GET polling can resume after network loss; no mutation is automatically restarted.
            }
            delay(seconds * 1000)
            seconds = (seconds * 2).coerceAtMost(5)
        }
    }
    suspend fun <T> retryMutation(key: String, decode: (JSONObject) -> T): DMResponse<T> {
        val pending = pendingMutations().firstOrNull { it.key == key } ?: error("Unknown pending mutation")
        return execute(pending.method, pending.pathAndQuery, pending.body, key, if (pending.pathAndQuery == "/v1/pairing-requests") null else bearer(), null, decode)
    }
    internal fun bearer(): String = synchronized(stateLock) { "Bearer " + state.getString("credential") }
    internal fun segment(value: String): String = URLEncoder.encode(value, "UTF-8").replace("+", "%20").replace("%7E", "~").replace("*", "%2A")
    internal suspend fun <T> send(method: String, path: String, query: Map<String, String?>, body: String?, key: String?, authorization: String?, etag: String?, decode: (JSONObject) -> T): DMResponse<T> {
        val queryString = query.filterValues { it != null }.toSortedMap().entries.joinToString("&") { segment(it.key) + "=" + segment(it.value!!) }
        return execute(method, path + if (queryString.isEmpty()) "" else "?$queryString", body, key, authorization, etag, decode)
    }
    private suspend fun <T> execute(method: String, pathAndQuery: String, body: String?, key: String?, authorization: String?, etag: String?, decode: (JSONObject) -> T): DMResponse<T> {
        if (pathAndQuery != "/v1/identity" && !identityVerified) verifyIdentity()
        require(pathAndQuery.startsWith("/v1/") && !pathAndQuery.contains('#')) { "Invalid Gateway path" }
        if (key != null) synchronized(stateLock) {
            require(UUID.fromString(key).version() == 4 && UUID.fromString(key).variant() == 2 && UUID.fromString(key).toString() == key) { "Idempotency-Key must be a lowercase UUID v4" }
            val pending = state.getJSONObject("pending")
            val saved = pending.optJSONObject(key)
            if (saved != null) require(saved.getString("method") == method && saved.getString("path") == pathAndQuery && saved.optNullableString("body") == body) { "Mutation key reused for a different request" }
            else pending.put(key, JSONObject().put("method", method).put("path", pathAndQuery).put("body", body ?: JSONObject.NULL))
            save()
        }
        return withContext(Dispatchers.IO) {
            val connection = URI(target.origin + pathAndQuery).toURL().openConnection() as HttpsURLConnection
            try {
                connection.sslSocketFactory = socketFactory // Default platform hostname verification stays enabled.
                connection.instanceFollowRedirects = false
                connection.useCaches = false
                connection.connectTimeout = 15000
                connection.readTimeout = 30000
                connection.requestMethod = method
                connection.setRequestProperty("Accept", "application/json")
                authorization?.let { connection.setRequestProperty("Authorization", it) }
                key?.let { connection.setRequestProperty("Idempotency-Key", it) }
                etag?.let { connection.setRequestProperty("If-None-Match", it) }
                val hello = JSONObject().put("MinimumProtocolVersion", DM_PROTOCOL_VERSION).put("MaximumProtocolVersion", DM_PROTOCOL_VERSION).put("ClientKind", 5)
                    .put("ClientId", deviceId).put("DeviceId", deviceId).put("DisplayName", "DisplayMagician Android").put("RequiredCapabilities", JSONArray(listOf("protocol-negotiation"))).put("OptionalCapabilities", JSONArray())
                connection.setRequestProperty("DisplayMagician-Protocol-Hello", DMPairing.base64url(hello.toString().toByteArray(StandardCharsets.UTF_8)))
                if (body != null) { connection.doOutput = true; connection.setRequestProperty("Content-Type", "application/json"); connection.outputStream.use { it.write(body.toByteArray(StandardCharsets.UTF_8)) } }
                val status = connection.responseCode
                val location = connection.getHeaderField("Location")
                val retryAfter = connection.getHeaderField("Retry-After")
                val responseBody = (if (status >= 400) connection.errorStream else connection.inputStream)?.use { stream ->
                    val bytes = ByteArrayOutputStream()
                    val buffer = ByteArray(8192)
                    while (true) {
                        val count = stream.read(buffer)
                        if (count < 0) break
                        require(bytes.size() + count <= 1048576) { "Gateway response too large" }
                        bytes.write(buffer, 0, count)
                    }
                    String(bytes.toByteArray(), StandardCharsets.UTF_8)
                }.orEmpty()
                if (key != null) synchronized(stateLock) { state.getJSONObject("pending").getJSONObject(key).put("status", status).put("location", location ?: JSONObject.NULL); save() }
                if (status !in 200..299 && status != 304) throw DMGatewayException(status, runCatching { DMProblemDetails.fromJson(JSONObject(responseBody)) }.getOrNull(), retryAfter, location)
                val welcome = JSONObject(String(DMPairing.decodeBase64url(connection.getHeaderField("DisplayMagician-Protocol-Welcome") ?: error("Missing protocol welcome")), StandardCharsets.UTF_8))
                require(welcome.getInt("SelectedProtocolVersion") == DM_PROTOCOL_VERSION && welcome.getString("EndpointKind") == "Gateway" && (0 until welcome.getJSONArray("SupportedCapabilities").length()).any { welcome.getJSONArray("SupportedCapabilities").getString(it) == "protocol-negotiation" }) { "Incompatible protocol welcome" }
                DMResponse(status, if (status == 304) null else decode(if (responseBody.isEmpty()) JSONObject() else JSONObject(responseBody)), connection.getHeaderField("ETag"), location, retryAfter)
            } finally { connection.disconnect() }
        }
    }
    private fun save() { vault.write(state.toString()) }
}

private fun JSONObject.optNullableString(key: String): String? = if (isNull(key)) null else getString(key)

internal class DMPinnedTrust(private val pin: String) : X509TrustManager {
    override fun getAcceptedIssuers(): Array<X509Certificate> = emptyArray()
    override fun checkClientTrusted(chain: Array<X509Certificate>?, authType: String?) { throw java.security.cert.CertificateException("Client certificates are unsupported") }
    override fun checkServerTrusted(chain: Array<X509Certificate>?, authType: String?) {
        val leaf = chain?.firstOrNull() ?: throw java.security.cert.CertificateException("Missing TLS certificate")
        val actual = Base64.getEncoder().encodeToString(MessageDigest.getInstance("SHA-256").digest(leaf.publicKey.encoded))
        if (actual != pin) throw java.security.cert.CertificateException("TLS pin changed; local approval and pairing are required")
        leaf.checkValidity()
        leaf.verify(leaf.publicKey) // Gateway's pinned TLS certificate is self-signed.
        if (leaf.extendedKeyUsage?.contains("1.3.6.1.5.5.7.3.1") == false) throw java.security.cert.CertificateException("Certificate is not valid for a TLS server")
    }
}

internal class DMPinnedSocketFactory(private val delegate: SSLSocketFactory) : SSLSocketFactory() {
    private fun secure(socket: Socket): Socket = (socket as SSLSocket).apply {
        enabledProtocols = supportedProtocols.filter { it == "TLSv1.2" || it == "TLSv1.3" }.toTypedArray()
        if (enabledProtocols.isEmpty()) throw javax.net.ssl.SSLException("TLS 1.2 or later is required")
    }
    override fun getDefaultCipherSuites(): Array<String> = delegate.defaultCipherSuites
    override fun getSupportedCipherSuites(): Array<String> = delegate.supportedCipherSuites
    override fun createSocket(): Socket = secure(delegate.createSocket())
    override fun createSocket(socket: Socket, host: String, port: Int, close: Boolean): Socket = secure(delegate.createSocket(socket, host, port, close))
    override fun createSocket(host: String, port: Int): Socket = secure(delegate.createSocket(host, port))
    override fun createSocket(host: String, port: Int, local: InetAddress, localPort: Int): Socket = secure(delegate.createSocket(host, port, local, localPort))
    override fun createSocket(host: InetAddress, port: Int): Socket = secure(delegate.createSocket(host, port))
    override fun createSocket(host: InetAddress, port: Int, local: InetAddress, localPort: Int): Socket = secure(delegate.createSocket(host, port, local, localPort))
}

/** Ciphertext is stored under noBackupFilesDir; the nonexportable AES key stays in Android Keystore. */
private class DMKeyStoreVault(context: Context, storageKey: String) {
    private val identifier = MessageDigest.getInstance("SHA-256").digest(storageKey.toByteArray(StandardCharsets.UTF_8)).joinToString("") { "%02x".format(it) }
    private val file = AtomicFile(File(context.noBackupFilesDir, "dm-gateway-$identifier.bin"))
    private val alias = "org.displaymagician.gateway.$identifier"
    fun delete() { file.delete(); KeyStore.getInstance("AndroidKeyStore").apply { load(null); deleteEntry(alias) } }
    private fun key(): SecretKey {
        val store = KeyStore.getInstance("AndroidKeyStore").apply { load(null) }
        (store.getKey(alias, null) as? SecretKey)?.let { return it }
        return KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES, "AndroidKeyStore").apply {
            init(KeyGenParameterSpec.Builder(alias, KeyProperties.PURPOSE_ENCRYPT or KeyProperties.PURPOSE_DECRYPT).setKeySize(256).setBlockModes(KeyProperties.BLOCK_MODE_GCM).setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE).build())
        }.generateKey()
    }
    fun read(): String? {
        val bytes = try { file.readFully() } catch (_: FileNotFoundException) { return null }
        require(bytes.size > 12) { "Invalid encrypted Gateway state" }
        val cipher = Cipher.getInstance("AES/GCM/NoPadding")
        cipher.init(Cipher.DECRYPT_MODE, key(), GCMParameterSpec(128, bytes.copyOfRange(0, 12)))
        return String(cipher.doFinal(bytes.copyOfRange(12, bytes.size)), StandardCharsets.UTF_8)
    }
    fun write(value: String) {
        val cipher = Cipher.getInstance("AES/GCM/NoPadding")
        cipher.init(Cipher.ENCRYPT_MODE, key())
        val stream = file.startWrite()
        try { stream.write(cipher.iv + cipher.doFinal(value.toByteArray(StandardCharsets.UTF_8))); file.finishWrite(stream) }
        catch (error: Exception) { file.failWrite(stream); throw error }
    }
}
