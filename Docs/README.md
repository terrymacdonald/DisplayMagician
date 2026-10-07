# DisplayMagician v4 API documentation

This directory documents the supported integration surfaces for DisplayMagician v4 (DMv4).

## API surfaces

| Surface | Audience | Transport | Specification |
|---|---|---|---|
| Local IPC | Same-device applications running as the same signed-in Windows user | Authenticated Windows named pipes | [IPC specification](IPC/IPC-Specification.md) |
| Remote API | Paired and authorised devices | HTTPS REST and Server-Sent Events (SSE) through DisplayMagician.Gateway | [OpenAPI description](REST/openapi.yaml) |
| Internal IPC | DMv4 components only | Restricted Windows named pipes | [Internal architecture appendix](IPC/IPC-Specification.md#appendix-a-internal-ipc-architecture) |

The local IPC API is the primary, richer integration surface. The remote REST API is intentionally narrower: it supports remote-controller workflows for status, events, listing and running saved items, operation decisions, and device pairing. Editing, diagnostics, support bundles, machine settings, and service administration remain local-only.

## Documents

- [IPC specification](IPC/IPC-Specification.md) is the normative human-readable protocol specification.
- [IPC JSON Schema](IPC/schemas/displaymagician-ipc.schema.json) describes the common envelope and public payload structures.
- [IPC AsyncAPI description](IPC/asyncapi.yaml) is a machine-readable message catalogue. The named-pipe details are represented with extension fields because AsyncAPI has no standard Windows named-pipe binding.
- [REST OpenAPI description](REST/openapi.yaml) documents the currently implemented Gateway routes.
- [REST API documentation](REST/README.md) defines the remote product boundary and client architecture.
- [REST API roadmap](REST/REST-API-ROADMAP.md) defines the target phone-controller features, planned routes, capabilities, priorities, and acceptance criteria.
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
