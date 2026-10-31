# DisplayMagician Static Client Sync and Anonymous Metrics Rollout

## Status and scope

This document replaces the former DMv3 update-and-telemetry specification.
It defines the target delivery model for DisplayMagician updates, messages, and
anonymous metrics.

There are currently **no clients using the DisplayMagician Admin Server**.
Consequently, the Admin Server may introduce this new protocol without a
server-data migration or a compatibility layer for Admin Server clients.

There is one separate legacy-client concern: existing released clients obtain
their update information from the static document at:

```text
https://displaymagician.littlebitbig.com/update/update.json
```

That document remains the bridge to the new client release. It must retain the
legacy update-document shape and eventually advertise a mandatory update to the
first DisplayMagician release that supports the new static sync protocol.

## Goals

1. Deliver routine update and message information without consuming Cloudfare Worker
   request quota.
2. Make routine client configuration checks occur every 24 to 36 hours, not on
   every application start.
3. Serve message bodies and message images as immutable static files so they do not consume Cloudflare Worker daily quota.
4. Collect a small, privacy-preserving set of anonymous metrics no more than
   weekly.
5. Keep a manual update check immediate and keep an emergency path available.
6. Preserve installer verification, elevation, download, and restart behaviour
   provided by AutoUpdater.NET.

## Non-goals

- Do not collect personally identifying information.
- Do not use a permanent WebSocket, push service, or frequent background poll.
- Do not expose private administrator data through static assets.
- Do not redirect, replace, or otherwise break the legacy
  `displaymagician.littlebitbig.com/update/update.json` contract before the
  bridge release is live and verified.

## Target architecture

```text
Admin UI / D1 / private R2
        |
        | Publish static distribution snapshot
        v
Public R2 static delivery
  /sync/client-sync.json
  /sync/messages/<message-id>-<sha256>.html
  /sync/media/<sha256>.<extension>
        ^
        | 24-36 hour scheduled client sync; no Worker invocation
        |
DisplayMagician client
        |
        | weekly only, plus an installed-version change
        v
Dynamic Worker
  POST /metrics/v1/heartbeat
        |
        v
D1 dashboard aggregates
```

The static URLs are stored in a separate public R2 bucket and delivered from
the dedicated `sync.displaymagician.com` custom domain. A matching `/sync/*`
request must not invoke the Worker. Do not configure a Worker route or
`run_worker_first` for public sync paths.

The Worker remains responsible for the protected Admin UI, the metrics API,
audit records, and release/message source-of-truth operations. It must not be
on the normal client configuration, message-body, or message-image delivery
path.

## Public static contract

### Client sync document

The static document lives at:

```text
https://sync.displaymagician.com/sync/client-sync.json
```

### Test client-sync mode

The existing DisplayMagician `--test-update-feed` command-line option must
switch the DMv3 client from the normal URL to:

```text
https://sync.displaymagician.com/sync/test-client-sync.json
```

It must not fetch the legacy `/update/test_update.json` endpoint once the
client is using the DMv3 static-sync implementation. The generated test
document is identical to `client-sync.json`—including messages, tombstones,
media URLs, installer URLs, checksums, and update policies—except that the
second version component for both `updates.stable.version` and
`updates.prerelease.version` is increased by one. This permits the update flow
to be tested without publishing a release. Test-sync mode must not send
anonymous metrics.

Its initial shape is:

```json
{
  "schemaVersion": 1,
  "publishedUtc": "2026-08-28T00:00:00Z",
  "updates": {
    "stable": {
      "version": "3.0.0.0",
      "changelog": "https://...",
      "url": "https://...",
      "mandatory": { "value": false, "mode": 0, "minVersion": "0.0.0.0" },
      "checksum": { "value": "<sha256>", "hashingAlgorithm": "SHA256" }
    },
    "prerelease": {
      "version": "3.1.0.0",
      "changelog": "https://...",
      "url": "https://...",
      "mandatory": { "value": false, "mode": 0, "minVersion": "0.0.0.0" },
      "checksum": { "value": "<sha256>", "hashingAlgorithm": "SHA256" }
    }
  },
  "messages": [
    {
      "id": "uuid",
      "status": "published",
      "title": "Example",
      "url": "/sync/messages/uuid-<sha256>.html",
      "format": "html",
      "sha256": "<sha256>",
      "showOnStartup": true,
      "publishedUtc": "2026-08-28T00:00:00Z",
      "minVersion": null,
      "maxVersion": null,
      "startUtc": null,
      "endUtc": null,
      "vendors": [],
      "kind": "standard",
      "media": [
        {
          "url": "/sync/media/<sha256>.png",
          "sha256": "<sha256>",
          "contentType": "image/png"
        }
      ]
    }
  ]
}
```

`updates.stable` and `updates.prerelease` use the same complete update schema;
only their release values differ. The update fields remain compatible with the
values consumed by the existing AutoUpdater.NET integration. The combined
wrapper is new and is consumed only by the new client release.

The new schema deliberately omits `manualUpgrade`, `updatesDisplayProfiles`,
`updatesGameShortcuts`, and `updatesSettings`. Each release's `changelog`
provides the specific, human-readable description of its changes. The legacy
bridge document retains whatever legacy fields older clients require.

Message and media URLs are content-addressed: changing a body or image produces
a new SHA-256 filename. Successful clients may cache such files for one year as
`immutable`. The sync document itself must have a short browser freshness
period appropriate to the client schedule, for example:

```text
Cache-Control: public, max-age=86400, stale-while-revalidate=43200
```

Static content is public. Do not place credentials, installation identifiers,
administrator information, unpublished drafts, or restricted message content in
the document or its assets. Version/vendor/date filtering remains client-side,
as it is today.

### Retractions

A deletion is represented in the sync document with the existing tombstone
shape:

```json
{ "id": "uuid", "status": "deleted", "deletedUtc": "..." }
```

Clients remove local message content and local media associated with that
message after receiving a tombstone. Static files may remain deployed until
normal release cleanup; a tombstone removes them from the client-visible index.

## Anonymous metrics contract

Use the term **anonymous metrics** in the client, UI, API names, dashboard
labels, logs, and user documentation. Do not call this feature telemetry as 
this is incorrect terminology.

The dynamic endpoint is:

```text
POST https://www.displaymagician.com/metrics/v1/heartbeat
```

Request body:

```json
{
  "schemaVersion": 1,
  "installId": "per-install UUID",
  "appVersion": "3.0.0.0",
  "updateChannel": "stable",
  "launches": 42,
  "activeMinutes": 1380,
  "graphicsLibrary": "nvidia",
  "connectedScreenCount": 2,
  "windowsBuild": "10.0.26100"
}
```

Only these values are in scope:

| Field | Purpose |
| --- | --- |
| `installId` | Stable per-install UUID. The server HMAC-hashes it immediately and never stores the raw value. |
| `appVersion` | Current-version and all-time adoption metrics. |
| `updateChannel` | Stable versus prerelease adoption. |
| `launches` | Cumulative application-launch count. |
| `activeMinutes` | Cumulative application runtime in whole minutes. |
| `graphicsLibrary` | Primary graphics library: `nvidia`, `amd`, `intel`, `other`, or `unknown`. |
| `connectedScreenCount` | Currently connected screens, bounded to `0` through `16`; no display identity or layout data. |
| `windowsBuild` | Windows release build in `major.minor.build` form, for example `10.0.19045` or `10.0.26100`. The dashboard derives Windows 10/11 groups from this; no edition, locale, device name, or update-revision value is sent. |

Do not send user name, email, IP address, computer name, display layout or
identity, GPU make/model, profile names, game data, executable paths, hardware
serials, message read state, or application logs.

The client exposes a setting named **Share anonymous usage metrics** in the Program
Settings window that allows the user to opt out of anonymous metric collection. It is
checked by default for new and migrated installations, and the explanation
text specified in Phase 2.5 appears directly below it. When the user clears
the checkbox, the client does not call this endpoint. Updates and messages
continue to work normally because they are static assets.

The server validates UUID and version formats, accepts only the documented
graphics-library enum and a `major.minor.build` Windows build format, bounds
all counts and counters, HMAC-hashes the UUID using `TELEMETRY_SECRET`, and
stores only the maximum cumulative counter values. A repeated request is
therefore safe.

## Client timing policy

### Static client sync

1. Persist `NextClientSyncUtc`.
2. A fresh installation chooses an initial time randomly distributed over the
   first 12 hours after startup.
3. After a successful sync, persist the next time as:

   ```text
   now + 24 hours + a random delay per install from 0 to 12 hours
   ```

4. Do not fetch merely because DisplayMagician starts.
5. When a due time occurs while the application is running, fetch in the
   background without delaying startup or blocking the UI.
6. On failure, retry with exponential backoff starting at one hour and capped
   at 24 hours. Keep the last successful sync time unchanged.
7. Provide a **Check for new messages** action in the client within the Messages UI. 
   It immediatel runs the same combined static sync, bypassing `NextClientSyncUtc`, 
   so the user sees newly published messages straight away.
8. If a sync is already in progress, the action joins that operation rather
   than starting a second download. There is no arbitrary hours-long delay on
   a deliberate user request.
9. A successful manual sync resets the next normal background sync using the
   usual 24-36 hour schedule. It does not send an anonymous-metrics heartbeat
   merely because the user pressed the button.

This produces an effective 24-36 hour normal delivery window. At 38,000
clients this is about 25,000-38,000 static document downloads per day; those
are static-asset requests rather than Worker invocations.

### Anonymous metrics

1. Persist `NextMetricsHeartbeatUtc` and
   `LastMetricsReportedVersion`.
2. Spread installations deterministically across seven daily slots using a
   stable hash of the installation UUID, with an additional 0-6 hour delay.
3. Send one heartbeat when the weekly time is due.
4. Send one additional heartbeat after a successful installed-version change.
5. Do not send metrics from `--test-update-feed` mode.
6. Do not make an additional request simply because the client sync document
   was fetched.

At 38,000 clients with the default setting enabled this averages about 5,430
dynamic metrics requests per day. The Admin Server must measure actual D1
write use, including index writes, during the pilot.

## Phase 0 - decisions and release boundary

Before implementation:

1. The first release containing static sync and metrics support is
   **DisplayMagician v3.0.0**. It is the **bridge target release**. Use its
   exact updater-version representation (for example `3.0.0.0`, if that is the
   installer version) in generated update documents.
2. Confirm the signed installer URL and checksum that legacy clients can
   download.
3. The earliest legacy client version that must receive the mandatory bridge
   update is **v2.0.1.0**. Configure the legacy updater's mandatory-update
   rule accordingly and validate the boundary with v2.0.1.0 and the
   immediately older supported version.
4. Reserve the public URL layout in this document. New-client static sync uses
   `https://sync.displaymagician.com`; anonymous metrics alone use
   `https://www.displaymagician.com`; and the legacy bridge remains at
   `https://displaymagician.littlebitbig.com/update/update.json`. All URLs use
   HTTPS; do not retain the old port-based HTTP URLs in the new client.
5. Decide the retention window for pseudonymous metrics records and document
   it in the privacy information shown to users.
6. Keep the legacy static update file unchanged until Phase 5.

## Phase 1 - Admin Server first

The Admin Server must be ready before the new client is released.

### 1.1 Public R2 delivery

1. Keep `/admin/*`, `/metrics/*`, `/health*`, and `/favicon.png*` as Worker
   routes. Keep `/sync/*` out of `wrangler.json` production routes.
2. Use the separate `displaymagician-sync-public` R2 bucket only for generated
   client-visible sync documents, bodies, and media. Never expose the private
   `displaymagician-payloads` bucket.
3. Attach `sync.displaymagician.com` as the bucket custom domain and disable
   its `r2.dev` URL. Configure a 308 Redirect Rule from
   `www.displaymagician.com/sync/*` to the dedicated domain for compatibility.
4. Configure a Cloudflare Cache Rule on the dedicated domain which respects R2
   origin cache headers: one day plus 12-hour stale revalidation for the main
   document, no cache for test sync, and one year immutable for hash-named
   bodies/media.
5. Verify that `CF-Cache-Status` is returned for public static paths and that
   Worker analytics does not record matching `/sync/*` requests.

### 1.2 Publication pipeline

The protected Admin Worker creates the public R2 snapshot only when an
administrator changes publishable content:

1. D1 and private R2 remain the admin source of truth for messages and release
   configuration.
2. Publishing/replacing/retracting a message or promoting a release invokes the
   Worker snapshot publisher after the source-of-truth change succeeds.
3. The Worker creates the static snapshot:
   - `client-sync.json`;
   - hash-named message bodies;
   - hash-named message images;
4. It writes `test-client-sync.json` first and `client-sync.json` last, so a
   normal client never observes a partially written snapshot.
5. After success, the Admin UI records the distribution revision, timestamp,
   hash, and deployment result in the audit trail.
6. The UI must visibly distinguish **saved in Admin Server** from
   **published to clients**. Do not silently report success before static
   distribution succeeds.

The Settings tab includes a protected **Publish current client sync** control
for the initial deployment and safe manual retries. No GitHub Action or
browser-held Cloudflare credential is used.

### 1.3 Static snapshot generator

Implement a server-side generator that:

1. Reads only published and deleted messages, active release channels, and
   approved message media.
2. Writes immutable body/media filenames from SHA-256 values.
3. Emits deleted-message tombstones.
4. Uses relative URLs in the sync document and message HTML where practical.
5. Validates each emitted body and media SHA-256 before deployment.
6. Fails the publication if any referenced static file is absent or has a
   checksum mismatch.
7. Omits `manualUpgrade`, `updatesDisplayProfiles`,
   `updatesGameShortcuts`, and `updatesSettings` from every generated stable
   and prerelease update object. These retired fields must not be copied from
   Admin Server release records into `client-sync.json`.
8. Leaves static files from older revisions available until the client
   retention window makes cleanup safe.

### 1.4 Metrics API and storage

1. Add `POST /metrics/v1/heartbeat`.
2. Add a dedicated request validator and a clear rate limit per pseudonymous
   installation.
3. Retain the existing HMAC approach; never persist raw installation UUIDs.
4. Make the endpoint idempotent by storing only maximum cumulative counters.
5. Record only one current-install row and one version-history row when
   necessary. Do not write a daily-presence row for every heartbeat unless the
   pilot demonstrates it remains safely within D1 limits.
6. Update dashboard wording from *telemetry* to *anonymous metrics*.
7. Define active installations as last seen within 14 days, not as a precise
   daily count.
8. Audit administrative deletion of metrics data, but do not audit individual
   client heartbeat requests.

### 1.5 Admin UI

Add:

- a static-distribution status panel;
- publish/retry distribution controls appropriate to the chosen CI flow;
- last distribution version/time/result;
- anonymous metrics disclosure and retention text;
- current-version, all-time-adoption, launch, runtime, and active-in-14-days
  dashboard labels;
- anonymous metrics breakdowns for graphics library, connected-screen count,
  Windows 10/11, and individual Windows release builds; and
- the existing multi-select version-data deletion control, described as metrics
  data rather than tracking/telemetry where applicable.

### 1.6 Admin Server tests

Test locally and in a non-production deployment:

1. `/sync/client-sync.json`, a body, and an image are served as static
   assets without Worker invocation.
2. The static paths use expected cache headers and content types.
3. A publication produces a complete self-consistent snapshot.
4. Generated stable and prerelease update objects do not contain any of the
   four retired update fields.
5. A retraction emits a tombstone and does not expose the removed message in
   the active list.
6. The metrics endpoint rejects invalid data and stores only HMAC hashes.
7. The metrics endpoint rejects invalid graphics-library, screen-count, and
   Windows-build values.
8. Repeated metrics payloads do not inflate counters.
9. The admin route remains protected by Cloudflare Access.
10. A failed distribution is visible and retryable without publishing partial
   client state.

#### Local static-file test procedure

Yes: local delivery uses the same Worker publisher and a locally bound
`SYNC_BUCKET`. Wrangler cannot emulate the dedicated custom domain locally, so
the Worker provides a local-only `/sync/*` proxy to that bucket; production
clients request the R2 custom domain directly.

Run the local launcher, which applies migrations, starts Wrangler, and invokes
the protected local publisher:

```powershell
.\start_local_displaymagician_server.ps1
```

Then request these local URLs:

```text
http://127.0.0.1:8788/sync/client-sync.json
http://127.0.0.1:8788/sync/messages/<message-id>-<sha256>.html
http://127.0.0.1:8788/sync/media/<sha256>.<extension>
```

The local test must verify the response content, SHA-256 values, content type,
and cache metadata; it must also verify a missing `/sync/*` file is a 404 and
that `/admin/*` and `/metrics/*` still reach their dynamic handlers. Before
production, manually configure and verify the dedicated R2 custom domain,
redirect rule, and Cache Rule at the public URL and confirm Worker analytics has no matching `/sync/*`
invocations.

## Phase 2 - Client implementation

### 2.1 Persisted scheduling

Add a small, versioned client-sync state model to `ProgramSettings`:

```text
NextClientSyncUtc
LastSuccessfulClientSyncUtc
NextMetricsHeartbeatUtc
LastMetricsReportedVersion
ConsecutiveSyncFailures
```

Persist `ProgramSettings` updates atomically. Existing installations without
these values use the 0-12 hour initial distribution rule.

### 2.2 Replace startup polling

The current client starts a forced message sync and an automatic AutoUpdater
request at application startup. Replace both with one background
`RunScheduledClientSyncAsync` operation:

1. Return immediately if static sync is not due.
2. Download and parse `client-sync.json`.
3. Apply message tombstones, filtering, body downloads, and media downloads.
4. Evaluate the selected update channel from the same document.
5. Persist the next sync time only after the static document was processed
   successfully.
6. Show existing message/update UI only on the UI thread and only after the
   background work completes.
7. Add a client **Check for new messages** command that calls the same sync
   operation with an explicit manual override. It bypasses the due-time check,
   shares any in-progress request, and reports completion/failure to the user.

The existing message-store and SHA-256 verification logic should be reused.
Extend it from the current UUID-only `/messages/media/<UUID>` matcher to
consume the explicit static `media` list.

### 2.3 AutoUpdater.NET adapter

The current AutoUpdater.NET path fetches its own URL. Refactor the existing
parse/update selection code into a reusable method that accepts the `updates`
section of `client-sync.json`.

Keep AutoUpdater.NET for:

- version comparison;
- installer checksum verification;
- download;
- elevation;
- restart;
- the existing mandatory, skip, and remind-later user experience.

Do not make AutoUpdater.NET perform a second automatic feed download after the
combined static document has been fetched. A manual update check may invoke the
same static sync flow immediately.

### 2.4 Static message media

For each message:

1. Download its immutable body only when the SHA-256 differs from the local
   verified copy.
2. Download each media entry only when that content hash is missing locally.
3. Verify body and media hashes before making them available to the UI.
4. Store media by content hash and extension, not only by message-media UUID.
5. Rewrite local rendering references to the stored local virtual host, as the
   current message viewer does.
6. Retain valid local static content through temporary network failures.

### 2.5 Anonymous metrics client

1. Add the **Share anonymous usage metrics** setting.
2. The setting is **enabled by default** for new installs and for existing
   installations which have no persisted value yet. It must remain a normal,
   immediately effective opt-out: clearing the checkbox prevents all future
   metrics heartbeats without affecting update or message sync.
3. Place this explanation directly underneath the checkbox in the Settings UI:

   > Help improve DisplayMagician by sharing anonymous usage metrics. This
   > information is used only to understand how DisplayMagician is used and to
   > guide future development. It is anonymised before it is stored.

   The text must be visible without hovering, clicking an information icon, or
   opening another page. Do not describe these metrics as telemetry in the UI.
3. Generate/reuse the existing stable installation UUID; do not create a second
   identifier.
4. Count one primary interactive application launch. Do not count helpers,
   updater processes, or child processes.
5. Accumulate runtime with a monotonic clock, persist periodically and on
   orderly shutdown, and send whole cumulative minutes.
6. Determine the active graphics library, count connected screens, and obtain
   the Windows release build in `major.minor.build` form when creating a
   heartbeat. Do not collect GPU make/model, display identities, layout,
   Windows edition, locale, device name, or update revision.
7. On a scheduled heartbeat or version-change report, POST the minimum payload.
8. If disabled, do not queue or retry metrics. Static updates/messages still
   work.
9. `--test-update-feed` mode must use `test-client-sync.json` and must not
   report anonymous metrics.

## Phase 3 - integration and pilot

1. Deploy the Admin Server changes, including the static distribution pipeline
   and metrics endpoint, before shipping a client that uses them.
2. Create a test static snapshot containing a harmless test message and image.
3. Install a development build with the new client switch on a small set of
   test machines.
4. Verify:
   - no configuration request on every restart;
   - a due sync fetches only static paths;
   - unchanged content does not redownload;
   - message body/image rendering works offline after first download;
   - deletion tombstones remove local messages;
   - **Check for new messages** performs an immediate combined sync;
   - `--test-update-feed` downloads `test-client-sync.json`, advertises the
     higher test versions, and does not emit metrics;
   - metrics arrive only when enabled and scheduled; and
   - graphics-library, connected-screen-count, and Windows-build aggregate
     values are correctly validated and shown without exposing hardware or
     display identity.
5. Observe Worker invocations, D1 writes, static-asset serving, client logs,
   and admin dashboard output for at least two complete sync windows and two
   metrics weeks.
6. Do not publish the legacy bridge until all pilot acceptance criteria pass.

## Phase 4 - release the bridge target client

1. Publish the signed **DisplayMagician v3.0.0** release containing the new
   static sync and anonymous-metrics implementation.
2. Configure it in the Admin Server static snapshot and test it with the
   `--test-update-feed` command-line option, which uses
   `test-client-sync.json`.
3. Confirm the normal update dialog, installer checksum validation, post-update
   restart, message rendering, and metrics opt-out behaviour.
4. Keep the legacy static update document unchanged during this verification.

## Phase 5 - legacy-client bridge

Only after Phase 4 passes:

1. Update the static legacy file at:

   ```text
   https://displaymagician.littlebitbig.com/update/update.json
   ```

2. Keep its legacy JSON schema exactly as old clients expect. Do not replace it
   with `client-sync.json`, redirect it, add a port requirement, or require a
   new parser.
3. Set the bridge target release as mandatory according to the legacy updater's
   supported mandatory-update fields and include the final installer URL and
   SHA-256 checksum.
4. Set the legacy mandatory-update policy so **v2.0.1.0** receives the v3.0.0
   bridge target release, and verify the immediately older supported version
   has the intended behaviour.
5. Validate with an isolated old-client installation before public release:
   - it reads the legacy static file;
   - it recognizes the update as mandatory;
   - it downloads the signed installer;
   - it upgrades successfully;
   - the new client then uses `/sync/client-sync.json`, not the legacy
     polling behaviour.
6. Monitor upgrade success and failure reports. Keep the legacy static file
   available as long as old clients remain in the field.

The legacy file is a one-way client-release bridge, not a migration of data
into the Admin Server. No Admin Server client records require migration.

## Phase 6 - steady state and cleanup

1. Use the Admin UI and publication pipeline for all routine releases and
   messages.
2. Keep client sync at 24-36 hours and anonymous metrics at weekly cadence.
3. Use the static distribution status rather than Worker request counts as the
   primary publication health signal.
4. Keep content-hashed static files until they are no longer referenced by any
   retained client-sync revision or locally cached-client support window.
5. Review D1 metrics retention and dashboard accuracy after the first month.
6. Do not remove the legacy update document until a separately approved end of
   support decision.

## Production security checklist

Complete these controls before the first production deployment and recheck
them whenever Cloudflare configuration or the public-sync architecture changes:

1. Protect only `/admin*` with a Cloudflare Access application. Restrict its
   policy to the named administrators and require the intended identity provider
   and MFA policy. Verify an unapproved identity receives no Admin content.
2. Use a dedicated least-privilege deployment API token. It must have only the
   Worker, D1, R2, and DNS permissions required by the idempotent deployment
   script; do not use a global API key. Configure the compatibility redirect
   and Cache Rule manually in the Cloudflare dashboard.
3. Store `TELEMETRY_SECRET` only as a Worker secret. Rotate it through the
   deployment process if exposure is suspected, accepting that this starts a
   new HMAC pseudonym namespace.
4. Apply a Cloudflare rate-limit rule to `POST /metrics/v1/heartbeat` sized for
   the weekly default-enabled client cadence. It must return a non-success status on
   excess traffic and protect D1 from abusive writes without restricting the
   normal client population.
5. The public sync R2 bucket must be separate from private source storage and
   contain only generated `/sync/*` artefacts. Disable its `r2.dev` public URL;
   expose it only through its required `sync.displaymagician.com` custom domain.
6. Keep `/sync/*` outside the Worker route list. Confirm the manually managed
   redirect and cache rules target only their intended paths.
7. Confirm Admin write APIs require a valid Access JWT and same-origin request.
   Keep message/release HTML sanitisation enabled and do not introduce raw HTML
   rendering or unbounded uploads.
8. Run the deployment smoke tests, review audit records, and inspect Worker,
   R2, D1, and Cloudflare security-event logs after every production change.

## Emergency updates

Routine cadence is not an emergency-delivery guarantee. For a critical release:

1. Publish the corrected static snapshot immediately.
2. Use the legacy bridge document for old clients when appropriate.
3. If a shorter new-client check interval is required, ship an explicit,
   time-limited emergency policy in a previously known static marker or a
   dedicated static emergency document. Do not turn routine polling back into
   dynamic Worker requests.
4. Return to the 24-36 hour cadence after the emergency.

## Acceptance checklist

- [ ] Admin Server has a dedicated public R2 sync domain and redirect path.
- [ ] Static snapshot publishing is explicit, auditable, and retryable.
- [ ] Sync document, message bodies, and images are all static assets.
- [ ] New client does not make automatic requests merely on startup.
- [ ] New client syncs every 24-36 hours with persisted scheduling.
- [ ] New client verifies and caches static bodies and media by SHA-256.
- [ ] Anonymous metrics are opt-out capable, weekly, minimal, and HMAC
      pseudonymized on the server.
- [ ] Anonymous metrics include only the graphics library, connected-screen
      count, and Windows release build described in this document; they do not
      include GPU model, display identity, Windows edition, or update revision.
- [ ] `--test-update-feed` uses `/sync/test-client-sync.json` and sends no
      anonymous metrics.
- [ ] Legacy `/update/update.json` remains legacy-compatible.
- [ ] An isolated old-client test has successfully upgraded through the
      mandatory bridge release.
- [ ] Worker and D1 usage are monitored throughout pilot and rollout.

---

# Authoritative DMv3 Client Implementation Handoff for an LLM

This section is deliberately detailed. It is the authoritative handoff for an
LLM changing `C:\vs-code\displaymagician` to DMv3. Where it differs from an
older statement elsewhere in this document, **this section wins**. Do not
change the Admin Server protocol while implementing the client unless the
owner explicitly approves a coordinated protocol change.

## 1. What is already deployed and must be treated as fixed

The Admin Server is already live and has been tested. It publishes a complete,
public, static snapshot to a separate R2 bucket. Normal DMv3 client activity
must not call the Admin Worker to discover updates or messages.

| Purpose | Canonical production URL | Served by |
| --- | --- | --- |
| Combined normal sync | `https://sync.displaymagician.com/sync/client-sync.json` | R2 + Cloudflare edge cache |
| Combined test sync | `https://sync.displaymagician.com/sync/test-client-sync.json` | R2, intentionally no-cache |
| Message body | `https://sync.displaymagician.com/sync/messages/<id>-<sha256>.html` or `.md` | R2 + edge cache |
| Message media | `https://sync.displaymagician.com/sync/media/<sha256>.<extension>` | R2 + edge cache |
| Anonymous metrics only | `POST https://www.displaymagician.com/metrics/v1/heartbeat` | Worker + D1 |
| Legacy pre-DMv3 update bridge | `https://displaymagician.littlebitbig.com/update/update.json` | existing legacy static host |

`https://www.displaymagician.com/sync/*` currently redirects with HTTP 308 to
the dedicated sync hostname. New client code must use the dedicated hostname
directly; the redirect is compatibility-only and is not a client API.

The R2 cache rule has been verified: normal `client-sync.json` requests return
`CF-Cache-Status: HIT`. The static hostname is intentionally public. Never
send a credential, administrator value, install ID, or user-specific value to
any `/sync/*` URL.

The server writes `test-client-sync.json` first and normal
`client-sync.json` last. A normal client therefore sees either the old complete
snapshot or the new complete snapshot, never a partial publication.

The client does not generate or upload either document. The Admin Server
regenerates both static documents whenever an administrator saves and deploys a
stable or prerelease channel, and whenever a message publication/retraction
changes the public message set. The refactor is therefore a consumer-only
change in the DisplayMagician repository: do not add a GitHub Action, client
upload, Worker API call, or client credential as part of this work.

## 2. Existing client code that the LLM must inspect before editing

Do not create a second competing update/message subsystem. Refactor and reuse
these existing pieces where practical:

| Existing location | Current responsibility | DMv3 direction |
| --- | --- | --- |
| `DisplayMagician/Program.cs` | Startup work, `CheckForUpdates`, AutoUpdater events, test-feed flag | Replace separate automatic startup requests with one combined sync coordinator. |
| `Program.CheckForUpdates(bool automatic, ...)` | Builds legacy URL and calls `AutoUpdater.Start(url)` | Make it consume the update object already downloaded by the combined sync; no second automatic URL download. |
| `Program.MessageManifestUrl` and `MessageSyncService` construction | Separate legacy messages manifest | Replace with the combined sync URL and a model that contains both updates and messages. |
| `Messaging/MessageSyncService.cs` | Local message store, SHA-256 checking, tombstones, vendor/version/date filtering, media persistence | Preserve these proven behaviours; extend it for static body/media URLs and explicit media hashes. |
| `Messaging/MessageManifestModels.cs` | Old message-only JSON DTOs | Add/replace with DMv3 combined DTOs. Do not silently deserialize unknown schema versions. |
| `Messaging/MessageStoreModels.cs` | Persisted local messages and read state | Extend without losing existing read state during migration. |
| `ProgramSettings.cs` | Persistent `Settings.json`, current file version is `5`, existing stable `InstallId` and `EnsureInstallIdentity` | Add persisted sync/metrics state, migrate safely, and increase the settings-file version. |
| `UIForms/SettingsForm.cs` and the existing messages UI | User settings/actions | Add the default-enabled metrics preference, its required explanatory text, and **Check for new messages** command. |

Current legacy constants include HTTP/port-8787 URLs for updates and the
message manifest. They are obsolete for DMv3 automatic operation. Do not carry
those legacy URLs, `install_id`, or `id` query parameters into the new static
sync request.

## 3. Non-negotiable implementation rules

1. A normal automatic client sync is one download of `client-sync.json`, not
   one update request plus one messages request.
2. It runs only when due: 24 hours plus a deterministic 0-12 hour jitter after
   success. It never runs merely because DM starts.
3. A user-triggered **Check for new messages** runs immediately and uses the
   same combined operation. It may also discover an update; it must not send a
   metrics heartbeat merely because it was pressed.
4. Only one combined sync can execute per process. Concurrent automatic/manual
   callers must join the same `Task`, not send duplicate requests.
5. Never replace a known-good local message/body/media copy with an unverified
   download. Failed network or verification work must leave the prior local
   state usable offline.
6. Never use the test feed unless `--test-update-feed` was supplied. Test mode
   must make **zero** metrics requests.
7. Do not add a client-side shared secret, Cloudflare token, Admin credential,
   or custom header to static sync. The content is public by design.
8. Preserve existing AutoUpdater.NET download, SHA-256 verification,
   elevation, restart, mandatory, skipped-version, and remind-later behaviour.
   The change is only where update metadata comes from.

## 4. Exact `client-sync.json` contract

The document has UTF-8 JSON, `schemaVersion: 1`, a `publishedUtc` ISO-8601 UTC
string, two update objects, and a `messages` array. Both update versions are
numeric four-part versions after server normalization, for example `3.0.0.0`.
Do not expect prerelease suffix text in the version string; channel selection
is determined by the local `UpgradeToPreReleases` setting.

```json
{
  "schemaVersion": 1,
  "publishedUtc": "2026-08-28T21:06:38.000Z",
  "updates": {
    "stable": {
      "version": "3.0.0.0",
      "changelog": "https://...",
      "url": "https://...",
      "mandatory": { "value": false, "mode": 0, "minVersion": "0.0.0.0" },
      "checksum": { "value": "64 UPPERCASE HEX CHARACTERS", "hashingAlgorithm": "SHA256" }
    },
    "prerelease": { "same fields as stable" }
  },
  "messages": [ "published message objects and deleted tombstones" ]
}
```

The four old update fields are deliberately absent and must not be reintroduced:
`manualUpgrade`, `updatesDisplayProfiles`, `updatesGameShortcuts`, and
`updatesSettings`. Release notes/changelog describe release-specific effects.

Required client validation before applying a snapshot:

- Require `schemaVersion == 1`; fail safely for a future schema rather than
  guessing its semantics.
- Require both `updates.stable` and `updates.prerelease`.
- Validate every selected update: canonical numeric version, HTTPS `url` and
  `changelog`, 64-hex SHA-256 checksum, `hashingAlgorithm == "SHA256"`, and
  a mandatory mode of `0` (default), `1` (force update), or `2` (automatic
  background install). Preserve the existing AutoUpdater.NET behaviour for
  each of those modes rather than inventing new semantics.
- Treat malformed individual message entries as faulty and skip them without
  discarding other valid entries. Treat malformed top-level JSON as a failed
  sync that does not advance `NextClientSyncUtc`.
- Ignore unknown additive fields for forward compatibility, but do not infer
  meaning from them.

### URL resolution and host allow-list

All `message.url` and `media.url` values currently start with `/sync/...`.
Resolve them against `https://sync.displaymagician.com/`, **not** against the
`www` Worker host and not the legacy host. Require HTTPS and the exact
`sync.displaymagician.com` host for static body/media downloads. Refuse path
traversal, another hostname, credentials in a URL, or an unexpected redirect.
This prevents a compromised document from turning the client into an arbitrary
downloader.

## 5. Message contract and local-store migration

A published message has `id` (UUID), `status: "published"`, `title`, `url`,
`format` (`html` or `md`), lowercase/uppercase-insensitive 64-hex `sha256`,
`showOnStartup`, optional `publishedUtc`, `minVersion`, `maxVersion`,
`startUtc`, `endUtc`, `vendors`, and `media` entries. A release announcement
may additionally contain `kind: "releaseAnnouncement"`, `releaseVersion`,
`releaseChannel`, `githubReleaseId`, and `updateAction: "installIfAvailable"`.
Keep the current graceful fallback for unknown message kinds.

Each media entry is:

```json
{ "url": "/sync/media/<sha256>.png", "sha256": "<64 hex>", "contentType": "image/png" }
```

Supported media types are exactly `image/png`, `image/jpeg`, `image/gif`, and
`image/webp`; map JPEG to `.jpg`. Verify bytes against the advertised SHA-256,
store them under a content-hash filename, and render only from the verified
local file. Do not trust a file extension or server content type by itself.

A tombstone is only:

```json
{ "id": "uuid", "status": "deleted", "deletedUtc": "UTC timestamp" }
```

On a tombstone, remove the message from the local index, delete its local body
and associated local media when no retained message references them, and do not
resurrect it from an older local copy. Preserve `IsRead` when a still-published
message has the same ID and verified SHA. A changed hash is a changed immutable
artifact and should be downloaded afresh.

`client-sync.json` is a **complete authoritative snapshot**, not a sequence of
deltas. After successfully parsing a complete top-level snapshot, reconcile the
client's downloaded-message index against it: a locally retained server message
which is absent from the valid snapshot must be removed using the same cleanup
rules as a tombstone. This is important because the server retains deletion
tombstones for a finite period (currently 24 months), then eventually removes
them from future snapshots. Do not apply this absence rule after a failed or
partially parsed top-level document, and never use a malformed individual entry
as evidence that a formerly valid local message was removed.

Message bodies are already server-sanitised. The client must nevertheless
render local HTML safely: no remote script execution, no arbitrary remote image
loading, and no untrusted local file navigation. Rewrite static media references
to the existing local message/media presentation mechanism. Retain the current
version/vendor/date eligibility checks; filtering happens on the client because
the static document is public and shared by all users.

## 6. Combined sync algorithm

Implement one service, for example `ClientSyncService`, with a single public
operation such as `RunAsync(Automatic|Manual, CancellationToken)`. The name is
not prescribed; the behaviour is.

1. Determine normal/test URL from the immutable process-wide test-feed flag.
2. For automatic mode, return without network I/O when `UtcNow <
   NextClientSyncUtc`. Manual mode bypasses only this due-time check.
3. Acquire/join a process-wide async gate.
4. Download the one JSON document with a bounded timeout, response-size limit,
   HTTPS, and no credentials. Do not cache-bust the normal URL: Cloudflare’s
   cache headers are part of the capacity design. Test URL is already no-cache.
5. Parse and validate the top-level snapshot before mutating persistent state.
6. Reconcile the complete message snapshot: apply tombstones, remove locally
   retained server messages absent from the valid snapshot, determine eligible
   published messages, and download only body or media artifacts missing locally
   or whose verified hash differs. Hash every downloaded byte sequence before
   atomically moving it into the local store. Never let a malformed individual
   entry cause removal of a known-good local message.
7. Use the selected update object (`prerelease` only when
   `UpgradeToPreReleases == true`; otherwise `stable`) to invoke/refactor the
   existing updater UI flow. Do not call `AutoUpdater.Start` with another HTTP
   feed URL after this point. If the installed AutoUpdater.NET version cannot
   accept parsed metadata, write a narrow adapter around the existing events and
   installer verifier rather than restoring a second feed request.
8. Commit the message index and sync scheduling state atomically only after the
   document was successfully processed. UI notifications must marshal to the
   WinForms UI thread.
9. On failure, log the URL class/status/error without logging message bodies,
   IDs beyond normal diagnostic needs, or metrics install IDs. Keep all valid
   local content and schedule bounded retry backoff.

## 7. Persistent settings and timing

Add the following to `ProgramSettings` and migrate existing settings without
resetting the existing `InstallId`, update preferences, messages, or read state:

```text
NextClientSyncUtc                 nullable UTC DateTime
LastSuccessfulClientSyncUtc       nullable UTC DateTime
ConsecutiveClientSyncFailures     non-negative integer
NextMetricsHeartbeatUtc           nullable UTC DateTime
LastMetricsReportedVersion        nullable canonical version string
ShareAnonymousUsageMetrics        boolean; default `true` when absent during migration
TotalAnonymousMetricLaunches      non-negative cumulative integer
TotalAnonymousMetricActiveMinutes non-negative cumulative integer
```

Increase `CurrentProgramSettingsFileVersion` from `5` and use the existing
`Settings.json` save path/serialization conventions. Persist timestamps as UTC.
When loading a pre-DMv3 settings file, a missing
`ShareAnonymousUsageMetrics` value means `true`; do not accidentally map a
missing JSON boolean to `false` through a language default. The Settings UI
must initially show the checkbox checked and persist a user clearing it before
any future heartbeat is attempted.
On first DMv3 run, choose `NextClientSyncUtc` uniformly across the next 12
hours. After success schedule `UtcNow + 24 hours + stable per-install jitter
0..12 hours`; derive that jitter from a stable hash of the already-existing
`InstallId`, not a new random value on every schedule. On failure use 1h, 2h,
4h, 8h, 16h, then 24h maximum; do not erase a good last-success value.

Use a timer/background task while the app is running, but never make startup
wait for it. A manual sync that succeeds resets the same 24-36 hour schedule.

## 8. Anonymous metrics: exact request and privacy boundary

Metrics are enabled by default but are always user-controllable and separate
from sync. They are not required for any DisplayMagician feature. The current
server requires all fields below; `launches` and `activeMinutes` are not
optional in the actual validator and must be non-negative whole cumulative
values.

```json
{
  "schemaVersion": 1,
  "installId": "existing ProgramSettings.InstallId UUID",
  "appVersion": "3.0.0.0",
  "updateChannel": "stable",
  "launches": 42,
  "activeMinutes": 1380,
  "graphicsLibrary": "nvidia",
  "connectedScreenCount": 2,
  "windowsBuild": "10.0.26100"
}
```

Allowed `graphicsLibrary`: `nvidia`, `amd`, `intel`, `other`, `unknown`.
`connectedScreenCount` is integer 0..16. `windowsBuild` must match
`major.minor.build`, with one/two digit major/minor and four/five digit build;
send the release build only, never UBR/revision. Use the app’s active graphics
library/backend, not GPU make/model. Count currently connected screens only;
do not send names, dimensions, layout, EDID, serials, or identities.

Post JSON with `Content-Type: application/json`; HTTP `204 No Content` is
success. The Worker returns `400` for invalid JSON/values, `413` for >4 KiB,
`415` for wrong type, `429` when rate-limited, and `503` when unavailable.
Treat failure as best-effort: retain scheduling/backoff, never interrupt normal
use, and never retry in a tight loop.

Use the existing stable `InstallId`; never create another identifier. The
server immediately HMAC-hashes it and does not persist raw UUIDs. Do not send
IP, user name/email, computer name, hardware serial, GPU model, profile/game
data, executable paths, message-read state, logs, Windows edition/locale, or
display metadata. The public endpoint is deliberately unauthenticated, so its
data is directional product analytics, not an anti-fraud source of truth.

Schedule heartbeats, when the default-enabled setting remains checked, weekly
using a stable `InstallId` hash spread over seven days plus 0-6 hour jitter.
Send one extra successful heartbeat after an installed-version change. Do not
send one at startup, after every sync, from test-feed mode, or while the user
has cleared the setting. Count one primary
interactive application launch only; use a monotonic clock for active runtime,
persist periodically and at orderly shutdown, and report whole cumulative
minutes.

## 9. Test mode, local testing, and release sequence

`--test-update-feed` is a process/session test switch. It must select only
`test-client-sync.json`. The server creates it by incrementing the **second**
numeric version component of both channels: e.g. normal `2.7.2.26` becomes
test `2.8.2.26`. Every other update/message/media value is identical. Never
persist test mode as a normal user preference and never send metrics in it.

For client development, start the Admin Server locally with
`start_local_displaymagician_server.ps1`. Local Wrangler exposes equivalent
published static content at `http://127.0.0.1:8788/sync/...`; this is a
local-only proxy to the locally bound sync bucket. Production DMv3 code must
still use the HTTPS production domain; local endpoint substitution belongs only
in a deliberate development/test configuration, never a released build.

Before shipping DM v3, test at minimum:

- New install: no immediate automatic request; first sync is within 0-12h.
- Due sync: exactly one static JSON request; no Worker update/message request.
- Manual messages action: immediate combined sync and joined concurrency.
- Valid/invalid JSON, future schema, timeout, 404, checksum mismatch, and
  interrupted download preserve known-good local state.
- HTML and Markdown message rendering; body/media hash verification; offline
  re-display after a successful prior sync; tombstone removal; vendor/version/
  start/end filtering; read-state migration.
- Stable/prerelease update selection, mandatory flow, skipped/remind-later,
  installer checksum/download/elevation/restart, and no second AutoUpdater feed
  HTTP request.
- Test-feed selection, higher second-octet version, and zero metrics traffic.
- Metrics default-enabled/opt-out behaviour, all allowed graphics values, screen bounds, Windows 10
  and 11 builds, weekly schedule, version-change send, 204/400/429/503 paths.

Release order is fixed: first ship and validate DM v3.0.0; only then change the
legacy `displaymagician.littlebitbig.com/update/update.json` to offer v3.0.0
as the mandatory bridge to clients from v2.0.1.0 onward. Keep legacy JSON
shape and URL untouched for old clients. It is not `client-sync.json`, must not
redirect to the new document, and must remain available until a separately
approved end-of-support decision.

## 10. Explicit anti-patterns the LLM must avoid

- Do not poll on every launch, hourly, or separately for updates/messages.
- Do not fetch `/sync/*` through the Worker, Cloud Connector, or old port-8787
  endpoints.
- Do not use `www.displaymagician.com` as the base for relative static URLs.
- Do not let AutoUpdater.NET make a second network feed request after combined
  sync has already downloaded metadata.
- Do not mark a sync successful before hashes and local persistence succeed.
- Do not delete old verified local content merely because a refresh failed.
- Do not add the four retired update fields to the schema.
- Do not send metrics when disabled, in test mode, on manual sync, or on every
  application start.
- Do not treat anonymous metrics as authoritative licensing, security, or
  anti-piracy data.
- Do not alter the legacy updater bridge while implementing the client.
