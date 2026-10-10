# DisplayMagician v4 REST API

The REST API is the deliberately narrow remote-controller surface exposed by `DisplayMagician.Gateway` over HTTPS. It is not intended to match the full local IPC API.

## Documents

- [REST API specification](REST-API-Specification.md) defines the target contract: resources, authentication, capabilities, operations, events, and HTTP behavior.
- [REST API client guide](REST-API-Client-Guide.md) provides pairing, command, progress, retry, reconnection, and integration walkthroughs.
- [REST API architecture](REST-API-Architecture.md) explains component responsibilities, trust boundaries, credential/certificate rationale, watches, and the future Telegram connector.
- [REST API implementation roadmap](REST-API-ROADMAP.md) tracks delivery phases, acceptance criteria, deferred features, and incomplete contract details.
- [Current Gateway OpenAPI description](openapi.yaml) documents P2 routes currently implemented by the Gateway.
- [Target OpenAPI draft](target-openapi.json) records approved design-only schemas as they are completed; it does not describe current Gateway behavior and still requires validation before client release.
- [Cross-transport gap analysis](../IPC/API-Gap-Analysis.md) records gaps spanning IPC, REST, contracts, and tooling.

## Read this first

The specification and client guide describe the **target API, which is not fully implemented**. The current OpenAPI covers P2 resource routes. Cursor pagination, ETags, SSE, and bounded retention remain P3 work.

- **Developing against today's build:** use the current Gateway OpenAPI and inspect the matching implementation.
- **Planning the target client:** read the specification, then the client guide; track unspecified details in the roadmap.
- **Working on server components:** read the architecture and roadmap alongside the specification.

No REST client has shipped against the prototype. No migration routes, compatibility aliases, or deprecation periods are needed.

## Intended clients

| Client | Gateway relationship |
|---|---|
| Android phone/tablet | Independently paired HTTPS client |
| Apple phone/tablet | Independently paired HTTPS client |
| Samsung/Wear OS watch | Relays through its paired Android companion app |
| Apple Watch | Relays through its paired Apple companion app |
| Stream Deck | Uses same-user local IPC, not REST |

In the target design, phones own Gateway discovery, TLS SPKI/host-identity verification, pairing, secure bearer-credential storage, and reconnect behavior. They do not sign each REST request. Watch apps do not pair with Gateway and do not receive Gateway credentials or private keys. They send narrow controller requests to their companion phone through the platform-provided watch/phone channel.

## Product boundary

The target REST API supports:

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

[openapi.yaml](openapi.yaml) must describe behavior available in the corresponding build only. The specification defines target behavior; the roadmap tracks its implementation. Update OpenAPI in the same change as each implemented route and its complete schemas, authorization, handlers, tests, and HTTP semantics.

Shared runtime contracts and client-safe views belong in `DisplayMagician.Contracts`. The Markdown guides explain those contracts; they do not create alternate client-specific transport models.
