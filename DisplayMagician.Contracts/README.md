# DisplayMagician.Contracts

This class library is the stable cross-process contract shared by the desktop application, console, Control Service, Session Launcher, and User Agent.

It contains versioned control envelopes, protocol message types, request/response models, shared enums, client events, Gateway event replay records, retry policy, and the common support-log layout and correlation scope. It contains no UI, hardware access, files, or static application state.

`GetActiveWindowPlacementSession` and `SaveWindowPlacements` let a same-user client capture the windows of an eligible active shortcut run. Save requests must carry that run's operation ID and shortcut ID; UserAgent rejects stale or ineligible runs. `ClearWindowPlacements` removes a shortcut's saved snapshot after the client confirms the action with the user.

Successful saved-definition edits may include `SavedResourceChanges` with the repository kind, stable ID, and upsert/deleted action. Gateway replay records use the corresponding REST resource type, ID, action, and link for upserts.

## Compatibility rules

- Treat public serialized fields and enum numeric values as compatibility contracts.
- Add protocol changes in a backwards-compatible form and preserve the serializer conventions in `ControlEnvelopeSerializer`.
- Control envelopes have a shared 5 MiB maximum serialized size. Keep normal list responses compact; use paging or separate assets for data that could grow beyond that boundary.
- `DisplayProfileView` carries a compact thumbnail and optional detailed layout preview and desktop `.ico` data. Profile lists are compact by default, including when the request payload is absent. `ProfileListRequest` lets a client request detail for one saved profile, the active profile, or the current layout; ordinary lists omit large previews and icons.
- Shortcut list icons use the saved composite image selected by the user when available, so local and future remote clients show the same image.
- Every request and response must preserve `ProtocolVersion`, `MessageType`, and `RequestId`; clients must validate all three before accepting a response.
- Every envelope carries transport-neutral `ProtocolHello`; the recipient validates the supported version range and required capabilities before performing work, then returns `ProtocolWelcome`. This is deliberately usable over named pipes today and REST later. `ProtocolHello` is compatibility metadata, not authentication or a licence grant.
- Set `ClientKind` and a stable `ClientId` using `ControlProtocol.CreateHello`; do not rely on the `Unknown` default. Capability IDs belong in `ControlCapabilities`. Required capabilities reject an incompatible endpoint; optional capabilities are returned as the negotiated subset so a client can disable unavailable features gracefully.
- Same-device/same-Windows-user callers use local pipes. Cross-device or cross-user callers use authenticated REST pairing; keep remote pairing DTOs transport-neutral and do not expose Control Service directly to a network. QR pairing sessions last ten minutes, persist only a hash of their one-time secret, and require approval by the owning user's WinForms app or an already-paired client holding `RemoteClientCapabilities.PairingApprove`. Approval grants exactly the candidate-requested capabilities. Remote-device authorisation scopes belong in `RemoteClientCapabilities`; do not use negotiated `ControlCapabilities` as grants.
- Use the shared connection and response timeouts. A caller must not wait indefinitely for a pipe connection or response.
- Reuse a request ID only to retry the identical operation. Control Service retains successful mutating request results for 24 hours so a lost reply cannot repeat an operation.
- Do not place persisted configuration definitions here; they belong in `DisplayMagician.ConfigurationDefinitions`.
- Internal launch/stop replies use `UserAgentLaunchResult`, including `ErrorCode` and `ProtocolWelcome`. ControlService requires `protocol-negotiation` and `session-launcher` from SessionLauncher. These are compatibility capabilities, not permission grants. Deploy the caller and broker together: missing hello metadata is rejected before broker execution, and a successful reply without a compatible welcome is rejected by the caller. The version-1 envelope and pipe name are unchanged.

## Build

Successful replies must satisfy both the negotiated protocol version and every capability required by the caller. Undefined numeric client kinds are rejected during hello validation.

```powershell
dotnet build .\DisplayMagician.Contracts\DisplayMagician.Contracts.csproj
```
