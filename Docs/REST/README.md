# DisplayMagician v4 REST API

The REST API is the deliberately narrow remote-controller surface exposed by `DisplayMagician.Gateway` over HTTPS. It is not intended to match the full local IPC API.

## Documents

- [Current OpenAPI description](openapi.yaml) documents only routes implemented by the Gateway.
- [REST API product specification and roadmap](REST-API-ROADMAP.md) defines the intended phone-controller feature set and the work required to complete it.
- [Cross-transport gap analysis](../IPC/API-Gap-Analysis.md) records gaps spanning IPC, REST, contracts, and tooling.

## Intended clients

| Client | Gateway relationship |
|---|---|
| Android phone/tablet | Independently paired HTTPS client |
| Apple phone/tablet | Independently paired HTTPS client |
| Samsung/Wear OS watch | Relays through its paired Android companion app |
| Apple Watch | Relays through its paired Apple companion app |
| Stream Deck | Uses same-user local IPC, not REST |

Phones own Gateway discovery, certificate pinning, device keys, pairing, request signing, and reconnect behavior. Watch apps do not pair with Gateway and do not receive Gateway private keys. They send narrow controller requests to their companion phone through the platform-provided watch/phone channel.

## Product boundary

REST supports:

- Gateway identity and API capability discovery
- Device pairing and device management
- User-scoped operation status and events
- Listing and applying saved display profiles
- Listing and applying saved audio profiles
- Listing and running saved shortcuts
- Reading and answering operation decisions
- Reading and cancelling eligible operations

REST intentionally excludes:

- Creating, renaming, updating, or deleting profiles
- Editing shortcut definitions
- Repository snapshot/commit
- Game and application discovery used by editors
- Support bundles and diagnostic log controls
- Gateway listener and machine settings
- Metrics and application message synchronization
- UserAgent/service administration
- Forced display-control and recovery administration

Those operations remain local IPC responsibilities unless a later product decision and threat model explicitly move them.

## Source-of-truth rule

`openapi.yaml` must describe shipping behavior only. Planned endpoints remain in the roadmap until their contracts, authorization, handlers, tests, and HTTP semantics are implemented. Once implemented, the route and its complete schemas move into OpenAPI in the same change.

