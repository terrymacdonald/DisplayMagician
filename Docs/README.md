# DisplayMagician v4 API documentation

This directory documents the supported integration surfaces for DisplayMagician v4 (DMv4).

## API surfaces

| Surface | Audience | Transport | Specification |
|---|---|---|---|
| Local IPC API | Same-device applications running as the same signed-in Windows user | Authenticated Windows named pipes | [IPC specification](IPC/IPC-Specification.md) |
| Remote REST API | Paired and authorised devices | HTTPS REST and Server-Sent Events (SSE) through DisplayMagician.Gateway | [Target specification](REST/REST-API-Specification.md) / [prototype OpenAPI](REST/openapi.yaml) |
| Internal IPC | DMv4 components only | Restricted Windows named pipes | [Internal architecture appendix](IPC/IPC-Specification.md#appendix-a-internal-ipc-architecture) |

The local IPC API is the primary, richer integration surface used for trusted components that run on the Windows PC running DisplayMagician. The remote REST API is intentionally narrower: it supports remote-controller workflows for status, events, listing and running saved items, operation decisions, and device pairing. Editing, diagnostics, support bundles, machine settings, and service administration remain local-only.

## Documents

- [IPC specification](IPC/IPC-Specification.md) is the normative human-readable protocol specification.
- [Local IPC client guide](IPC/Local-IPC-Client-Guide.md) explains how same-PC controller integrations connect, negotiate, list actions, start work, follow operations, and recover events.
- [Internal component communication](IPC/Internal-Component-Communication.md) describes how WinForms, Console, ControlService, UserAgent, SessionLauncher, and Gateway exchange information.
- [IPC JSON Schema](IPC/schemas/displaymagician-ipc.schema.json) describes the common envelope and public payload structures.
- [IPC AsyncAPI description](IPC/asyncapi.yaml) is a machine-readable message catalogue. The named-pipe details are represented with extension fields because AsyncAPI has no standard Windows named-pipe binding.
- [REST OpenAPI description](REST/openapi.yaml) documents the currently implemented Gateway routes.
- [REST API documentation](REST/README.md) defines the remote product boundary and client architecture.
- [REST API specification](REST/REST-API-Specification.md) defines the target REST contract and clearly marks incomplete details.
- [REST API client guide](REST/REST-API-Client-Guide.md) explains pairing, commands, operation tracking, events, recovery, and client implementation.
- [REST API architecture](REST/REST-API-Architecture.md) explains component routing, trust, credential/certificate design, watches, and Telegram.
- [REST API roadmap](REST/REST-API-ROADMAP.md) tracks delivery work, acceptance criteria, deferred features, and unresolved contract decisions.
- [API capability matrix](IPC/API-Capability-Matrix.md) maps features across contracts, local IPC, events, REST, and tests.
- [API gap analysis](IPC/API-Gap-Analysis.md) separates confirmed implementation gaps from deliberate product boundaries and future design decisions.
- [Wire examples](IPC/examples/) show the nested JSON-string payload used by protocol version 1.

## Sources of truth

The C# types in `DisplayMagician.Contracts` and the shipping endpoint implementations remain the executable sources of truth. These documents must be updated when serialized members, enum numeric values, routes, capabilities, framing, limits, or compatibility rules change.

OpenAPI is used for REST. Markdown plus JSON Schema is used for the named-pipe protocol, with AsyncAPI as a supplementary catalogue. The IPC API is not represented as fictional HTTP operations in OpenAPI.

## Compatibility policy

- Public serialized property names and enum numeric values are compatibility contracts.
- Additive changes are preferred within a protocol version.
- Breaking wire changes require a new negotiated protocol version and, where necessary, new versioned pipe names.
- A transport capability is not an authorisation grant.
- Internal component pipes are not public extension points.
