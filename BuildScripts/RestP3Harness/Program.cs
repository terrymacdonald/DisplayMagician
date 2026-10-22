using System.Diagnostics;
using System.IO.Pipes;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text.Json;
using DisplayMagician.Contracts;

// Developer-only installed-environment harness. It uses same-user IPC to pair temporary test devices,
// then exercises the real HTTPS Gateway. It never submits display/audio/shortcut actions.
internal static class Program
{
    private static readonly JsonSerializerOptions Json = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    private static readonly List<object> Results = new List<object>();
    private static readonly List<string> Devices = new List<string>();
    private static string _origin = string.Empty;
    private static string _pin = string.Empty;
    private static readonly ProtocolHello Hello = ControlProtocol.CreateHello(ControlClientKind.RemoteApplication, "DisplayMagician.RestP3Harness");

    public static async Task<int> Main(string[] args)
    {
        if (args.Length != 2 || !Uri.TryCreate(args[0], UriKind.Absolute, out Uri? uri) || uri.Scheme != "https" || uri.UserInfo.Length > 0 || uri.AbsolutePath != "/" || uri.Query.Length > 0 || uri.Fragment.Length > 0)
        {
            Console.Error.WriteLine("Usage: RestP3Harness <https-origin> <sanitized-results.json>");
            return 2;
        }
        _origin = uri.GetLeftPart(UriPartial.Authority);
        int exitCode = 0;
        try
        {
            using HttpClient client = CreateClient();
            string credential = await PairAsync(client, RemoteClientCapabilities.All.ToArray());
            using HttpResponseMessage capabilities = await RequestAsync(client, HttpMethod.Get, "/v1/capabilities");
            GatewayCapabilitiesView info = (await capabilities.Content.ReadFromJsonAsync<GatewayCapabilitiesView>(Json))!;
            Check("SSE advertised", info.EventTransports.Contains("sse"));
            foreach (string collection in new[] { "display-profiles", "audio-profiles", "shortcuts", "operations", "decisions?status=pending" })
            {
                string path = "/v1/" + collection + (collection.Contains('?') ? "&" : "?") + "limit=1";
                using HttpResponseMessage page = await RequestAsync(client, HttpMethod.Get, path, credential);
                Check("Paged " + collection, page.StatusCode == HttpStatusCode.OK);
                using JsonDocument json = JsonDocument.Parse(await page.Content.ReadAsStringAsync());
                if (json.RootElement.TryGetProperty("nextCursor", out JsonElement cursor) && cursor.ValueKind == JsonValueKind.String)
                {
                    using HttpResponseMessage next = await RequestAsync(client, HttpMethod.Get, path + "&cursor=" + Uri.EscapeDataString(cursor.GetString()!), credential);
                    Check("Resume page " + collection, next.StatusCode == HttpStatusCode.OK);
                }
                else Results.Add(new { check = "Resume page " + collection, status = "needs-data", detail = "Create at least two saved items or retained operations/decisions to exercise another page." });
                if (collection is "display-profiles" or "audio-profiles" or "shortcuts" && json.RootElement.GetProperty("items").GetArrayLength() > 0)
                {
                    string href = json.RootElement.GetProperty("items")[0].GetProperty("href").GetString()!;
                    using HttpResponseMessage detail = await RequestAsync(client, HttpMethod.Get, href, credential);
                    string? etag = detail.Headers.ETag?.ToString();
                    Check("Strong ETag " + collection, detail.StatusCode == HttpStatusCode.OK && etag != null && !etag.StartsWith("W/", StringComparison.Ordinal));
                    using HttpResponseMessage unchanged = await RequestAsync(client, HttpMethod.Get, href, credential, etag: etag);
                    Check("Conditional GET " + collection, unchanged.StatusCode == HttpStatusCode.NotModified);
                }
            }
            using HttpResponseMessage first = await OpenStreamAsync(client, credential, null);
            string? head = await ReadSnapshotAsync(first);
            Check("Initial snapshot cursor", !string.IsNullOrWhiteSpace(head));
            using (HttpResponseMessage duplicate = await RequestAsync(client, HttpMethod.Get, "/v1/events", credential, stream: true))
                Check("One stream per device", duplicate.StatusCode == HttpStatusCode.TooManyRequests && duplicate.Headers.RetryAfter != null);

            List<HttpResponseMessage> additional = new List<HttpResponseMessage>();
            try
            {
                for (int index = 0; index < 3; index++)
                {
                    string otherCredential = await PairAsync(client, new[] { RemoteClientCapabilities.StatusRead });
                    HttpResponseMessage stream = await OpenStreamAsync(client, otherCredential, null);
                    additional.Add(stream);
                    await ReadSnapshotAsync(stream);
                }
                string fifth = await PairAsync(client, new[] { RemoteClientCapabilities.StatusRead });
                using HttpResponseMessage excess = await RequestAsync(client, HttpMethod.Get, "/v1/events", fifth, stream: true);
                Check("Four streams per user", excess.StatusCode == HttpStatusCode.TooManyRequests && excess.Headers.RetryAfter != null);
            }
            finally { foreach (HttpResponseMessage stream in additional) stream.Dispose(); }
            first.Dispose();

            using (HttpResponseMessage resumed = await OpenStreamAsync(client, credential, head))
            {
                using StreamReader reader = new StreamReader(await resumed.Content.ReadAsStreamAsync());
                using CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                try
                {
                    while (true)
                    {
                        string? line = await reader.ReadLineAsync(timeout.Token);
                        if (line == null) break;
                        Check("Retained cursor omits snapshot", line != "event: snapshot");
                        if (line.StartsWith("event:", StringComparison.Ordinal)) break;
                    }
                }
                catch (OperationCanceledException) { Results.Add(new { check = "Retained cursor omits snapshot", status = "passed" }); }
            }
            using HttpResponseMessage reset = await OpenStreamAsync(client, credential, "unknown-test-cursor");
            await ReadSnapshotAsync(reset);
            Stopwatch elapsed = Stopwatch.StartNew();
            await PipeAsync(ControlMessageType.RevokePairedClient, new RevokePairedClientRequest { DeviceId = Devices[0] });
            using (StreamReader reader = new StreamReader(await reset.Content.ReadAsStreamAsync()))
            using (CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(7)))
                while (await reader.ReadLineAsync(timeout.Token) != null) { }
            Check("Revocation closes stream", elapsed.Elapsed <= TimeSpan.FromSeconds(6));
            using HttpResponseMessage denied = await RequestAsync(client, HttpMethod.Get, "/v1/devices/current", credential);
            Check("Revoked credential rejected", denied.StatusCode == HttpStatusCode.Unauthorized);
        }
        catch (Exception ex)
        {
            exitCode = 1;
            Results.Add(new { check = "Installed P3 run", status = "failed", error = ex.GetType().Name });
            Console.Error.WriteLine("Installed P3 run failed. The sanitized report records the check and error type.");
        }
        finally
        {
            foreach (string device in Devices)
            {
                try { await PipeAsync(ControlMessageType.RevokePairedClient, new RevokePairedClientRequest { DeviceId = device }, allowFailure: true); }
                catch { Results.Add(new { check = "Temporary-device cleanup", status = "failed", detail = "Revoke the REST P3 test device from WinForms." }); exitCode = 1; }
            }
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1]))!);
            await File.WriteAllTextAsync(args[1], JsonSerializer.Serialize(new { capturedAt = DateTime.UtcNow, results = Results }, new JsonSerializerOptions(Json) { WriteIndented = true }));
        }
        Console.WriteLine("Wrote sanitized installed P3 results. Hardware actions and installer lifecycle were not exercised.");
        return exitCode;
    }

    private static void Check(string name, bool condition)
    {
        Results.Add(new { check = name, status = condition ? "passed" : "failed" });
        if (!condition) throw new InvalidOperationException("Installed P3 assertion failed.");
    }
    private static HttpClient CreateClient()
    {
        HttpClientHandler handler = new HttpClientHandler { AllowAutoRedirect = false };
        handler.ServerCertificateCustomValidationCallback = (_, certificate, _, errors) =>
            certificate != null && (errors & (SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable)) == 0 &&
            certificate.NotBefore.ToUniversalTime() <= DateTime.UtcNow && certificate.NotAfter.ToUniversalTime() > DateTime.UtcNow &&
            Convert.ToBase64String(SHA256.HashData(certificate.PublicKey.ExportSubjectPublicKeyInfo())) == _pin;
        return new HttpClient(handler) { BaseAddress = new Uri(_origin), Timeout = Timeout.InfiniteTimeSpan };
    }
    private static async Task<string> PairAsync(HttpClient client, string[] grants)
    {
        ControlResponse local = await PipeAsync(ControlMessageType.CreateDevicePairingQr, new CreateDevicePairingQrRequest { GatewayUri = _origin });
        DevicePairingQrCode qr = local.DevicePairingQrCode!;
        if (_pin.Length > 0 && _pin != qr.Gateway.TlsSpkiSha256) throw new InvalidOperationException("Gateway identity changed during the test.");
        _pin = qr.Gateway.TlsSpkiSha256;
        using HttpResponseMessage publicIdentity = await RequestAsync(client, HttpMethod.Get, "/v1/identity");
        GatewayPublicIdentityView identity = (await publicIdentity.Content.ReadFromJsonAsync<GatewayPublicIdentityView>(Json))!;
        Check("Pinned host identity", identity.HostId == qr.Gateway.HostId && identity.TlsSpkiSha256 == _pin && identity.HostIdentityPublicKeyJwk == qr.Gateway.HostIdentityPublicKeyJwk);
        string device = "rest-p3-test-" + Guid.NewGuid().ToString("N");
        Devices.Add(device);
        string polling = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        using HttpResponseMessage submission = await RequestAsync(client, HttpMethod.Post, "/v1/pairing-requests", body: new PairingSubmissionRequest
        {
            PairingRequestId = qr.PairingSessionId, PairingSecret = qr.PairingSecret, PollingSecret = polling,
            DeviceId = device, DisplayName = "REST P3 test device", ClientType = "rest-p3-harness", RequestedCapabilities = grants
        });
        Check("Pairing accepted", submission.StatusCode == HttpStatusCode.Accepted);
        await PipeAsync(ControlMessageType.ApproveDevicePairing, new ApproveDevicePairingRequest { PairingSessionId = qr.PairingSessionId, GrantedCapabilities = grants });
        using HttpResponseMessage poll = await RequestAsync(client, HttpMethod.Get, $"/v1/pairing-requests/{qr.PairingSessionId:D}", polling: polling);
        PairingPollView result = (await poll.Content.ReadFromJsonAsync<PairingPollView>(Json))!;
        Check("One-time credential delivered", poll.StatusCode == HttpStatusCode.OK && !string.IsNullOrWhiteSpace(result.Credential));
        return result.Credential!;
    }
    private static async Task<ControlResponse> PipeAsync(ControlMessageType type, object body, bool allowFailure = false)
    {
        using CancellationTokenSource timeout = new CancellationTokenSource(ControlProtocol.ResponseTimeout);
        using NamedPipeClientStream pipe = new NamedPipeClientStream(".", ControlProtocol.ClientPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(timeout.Token);
        ControlEnvelope request = new ControlEnvelope { MessageType = type, Hello = ControlProtocol.CreateHello(ControlClientKind.LocalIntegration, "DisplayMagician.RestP3Harness"), Payload = JsonSerializer.Serialize(body) };
        await ControlEnvelopeSerializer.WriteAsync(pipe, request, timeout.Token);
        ControlEnvelope reply = await ControlEnvelopeSerializer.ReadAsync(pipe, timeout.Token) ?? throw new IOException("Missing local response.");
        ControlResponse response = JsonSerializer.Deserialize<ControlResponse>(reply.Payload)!;
        if (reply.MessageType != request.MessageType || reply.RequestId != request.RequestId || reply.ProtocolVersion != request.ProtocolVersion || !ControlProtocol.IsCompatibleWelcome(request.Hello, response.ProtocolWelcome) || !response.IsSuccessful && !allowFailure) throw new IOException("Local test setup could not complete.");
        return response;
    }
    private static async Task<HttpResponseMessage> RequestAsync(HttpClient client, HttpMethod method, string path, string? credential = null, object? body = null, string? polling = null, string? etag = null, bool stream = false, string? cursor = null)
    {
        using HttpRequestMessage request = new HttpRequestMessage(method, path);
        request.Headers.Add("DisplayMagician-Protocol-Hello", Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(Hello)).TrimEnd('=').Replace('+', '-').Replace('/', '_'));
        if (credential != null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credential);
        if (polling != null) request.Headers.Authorization = new AuthenticationHeaderValue("DisplayMagician-Pairing", polling);
        if (method != HttpMethod.Get) request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("D"));
        if (etag != null) request.Headers.TryAddWithoutValidation("If-None-Match", etag);
        if (cursor != null) request.Headers.Add("Last-Event-ID", cursor);
        if (body != null) request.Content = JsonContent.Create(body, options: Json);
        using CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        HttpResponseMessage response = await client.SendAsync(request, stream ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead, timeout.Token);
        if ((int)response.StatusCode < 400)
        {
            string encoded = response.Headers.GetValues("DisplayMagician-Protocol-Welcome").Single();
            string padded = encoded.Replace('-', '+').Replace('_', '/') + new string('=', (4 - encoded.Length % 4) % 4);
            if (!ControlProtocol.IsCompatibleWelcome(Hello, JsonSerializer.Deserialize<ProtocolWelcome>(Convert.FromBase64String(padded)))) throw new InvalidDataException("Invalid HTTPS protocol welcome.");
        }
        return response;
    }
    private static async Task<HttpResponseMessage> OpenStreamAsync(HttpClient client, string credential, string? cursor)
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            HttpResponseMessage result = await RequestAsync(client, HttpMethod.Get, "/v1/events", credential, stream: true, cursor: cursor);
            if (result.StatusCode == HttpStatusCode.OK) return result;
            result.Dispose();
            await Task.Delay(1000); // Wait for the previous connection's cancellation to release its lease.
        }
        throw new InvalidOperationException("SSE connection could not start.");
    }
    private static async Task<string?> ReadSnapshotAsync(HttpResponseMessage response)
    {
        StreamReader reader = new StreamReader(await response.Content.ReadAsStreamAsync(), leaveOpen: true);
        using CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        string? id = null;
        while (await reader.ReadLineAsync(timeout.Token) is string line)
        {
            if (line.StartsWith("id: ", StringComparison.Ordinal)) id = line.Substring(4);
            if (line.StartsWith("event: ", StringComparison.Ordinal)) Check("Initial event is snapshot", line == "event: snapshot");
            if (line.StartsWith("data: ", StringComparison.Ordinal)) { using JsonDocument _ = JsonDocument.Parse(line.Substring(6)); return id; }
        }
        throw new IOException("SSE snapshot ended early.");
    }
}
