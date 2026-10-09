# DisplayMagician v4 REST API architecture

**Status:** Target architecture - not fully implemented

**Network boundary:** HTTPS Gateway for local and routed private networks in the first release

**Watch model:** Companion-phone relay only

## 1. Purpose

This guide explains component responsibilities, client topology, and design rationale. It does not duplicate the normative endpoint catalogue or authentication contract in the [specification](REST-API-Specification.md).

The target should provide a clear resource-oriented controller API, minimize certificate and credential usability failures, support first-party phones and companion watches, and allow future integrations. The first release does not configure Internet exposure. ControlService remains authoritative for authorization, operations, decisions, and machine coordination.

For existing internal IPC flows, see [internal component communication](../IPC/Internal-Component-Communication.md). For implementation work, see the [roadmap](REST-API-ROADMAP.md).

## 2. Client and trust architecture

### 2.1 Phone and tablet applications

```text
Android or Apple application
        |
        | HTTPS on local or routed private network
        v
DisplayMagician.Gateway
        |
        | authenticated internal Gateway pipe
        v
ControlService
        |
        v
UserAgent
```

Phones own:

- QR pairing;
- Gateway identity verification;
- TLS public-key pinning;
- device credential storage;
- REST requests;
- Server-Sent Event (SSE) reconnect behavior;
- watch command relay;
- capability-aware user interfaces.

### 2.2 Watch applications

```text
Apple Watch ---- WatchConnectivity ---- Apple phone app --\
                                                          +--> HTTPS Gateway
Wear OS watch -- Wear Data Layer -------- Android app ----/
```

Watch applications:

- do not connect directly to Gateway;
- do not discover the DMv4 host;
- do not pin Gateway certificates or keys;
- do not hold a Gateway bearer credential;
- do not independently pair or receive capabilities;
- receive only the state required for their UI;
- send narrow controller requests to their companion phone.

The phone/watch message contract is an application-internal protocol, not part of the Gateway REST API.

### 2.3 Future Telegram connector

```text
Personal Telegram chat
        |
        v
Telegram Bot API
        |
        | outbound long polling
        v
DMv4 Telegram Connector on the Windows PC
        |
        | HTTPS using its own paired credential
        v
DisplayMagician.Gateway
```

The Telegram connector:

- opens no inbound Internet port;
- does not expose Gateway or ControlService to Telegram;
- uses outbound HTTPS long polling;
- is paired as an independent restricted REST client;
- must not convert Telegram input directly into trusted local named-pipe commands;
- accepts commands only from approved immutable Telegram numeric user and private-chat IDs;
- rejects group/channel use by default;
- stores its DMv4 credential and Telegram bot token with Windows DPAPI;
- can be revoked independently.

## 3. Credential and certificate design rationale

The [credential and certificate requirements](REST-API-Specification.md#3-credential-and-certificate-lifecycle) are defined in the specification.

HTTPS protects transport confidentiality and authenticates the server according to the paired trust model. A bearer credential independently identifies the paired client and its authorized capabilities. Compatibility discovery and marketplace entitlement do not authenticate a client.

Pairing establishes trust from locally approved QR data, not from an unverified network advertisement. Pinning SPKI rather than an entire short-lived certificate allows normal certificate renewal to retain the same client trust anchor. A stable host identity is distinct from the TLS key and must not be confused with a client credential.

Automatic certificate renewal and long-lived independently revocable credentials reduce routine maintenance. They do not remove the need for secure storage, certificate validation, capability checks, revocation, or explicit handling of key loss and replacement.

The target uses bearer credentials rather than custom request signatures, HMAC, or client certificates. Current prototype request signing must not be assumed to be the target authentication contract.

### 3.1 Marketplace entitlement

Apple, Google, Elgato, or other marketplace purchase validation remains inside the corresponding client product. Gateway must not:

- accept store receipts as authentication;
- infer official-client status from a user agent, display name, or `clientType`;
- log or retain marketplace credentials.

Pairing means the DMv4 user authorized a controller. Marketplace entitlement means the user is licensed to run that client. They are separate concerns.

## 4. LAN discovery (deferred)

QR pairing remains the trust bootstrap.

The first released API uses the saved Gateway URI after pairing. Optional mDNS reconnect discovery is deferred; a later release may advertise:

```text
_displaymagician._tcp.local
```

Public-safe discovery metadata:

- stable host ID;
- REST API major version;
- HTTPS port;
- product name.

Discovery data is untrusted until matched to the paired host identity and pinned TLS SPKI. It must not advertise user names, profiles, device lists, credentials, secrets, or capability grants.

Failure to discover must not erase a saved trusted host.

## 5. Telegram connector requirements

The connector accepts only:

- updates from the configured bot;
- an approved immutable Telegram numeric user ID;
- an approved private-chat ID;
- supported command/update types.

The connector should:

- reject group and channel commands by default;
- rate-limit command execution;
- ignore unexpected forwarded or edited messages unless explicitly supported;
- use inline buttons for decisions;
- optionally require confirmation for disruptive commands;
- display operation progress/results without exposing sensitive machine data;
- store the Telegram bot token and DMv4 credential with DPAPI;
- redact tokens and message content from logs;
- expose a local disable/revoke control;
- stop processing immediately after its Gateway credential is revoked.

The Telegram Bot API and account become part of the integration's threat model; the connector needs no inbound Internet port or router forwarding for Gateway.

## 6. Runtime and contract boundaries

Gateway is an HTTPS adapter, not a second runtime coordinator. A successful remote command follows:

```text
paired client
  -> HTTPS Gateway: authenticate and decode HTTP request
  -> ControlService: verify device grant and user/default target
  -> UserAgent: execute interactive-session work
  -> ControlService: retain operation/decision state
  -> Gateway: format shared client-safe state as REST/SSE
  -> paired client: display result
```

- ControlService owns device/user/capability associations and remains authoritative for authorization.
- UserAgent owns interactive execution and per-user definitions.
- SessionLauncher participates only when ControlService needs an authorized Agent process started or stopped.
- Gateway cannot route remote input through the unrestricted public local-client pipe.
- Shared transport requests, responses, enums, and client-safe views belong in `DisplayMagician.Contracts`.
- REST uses its documented camelCase/string representation while IPC preserves its own wire representation.
- Do not introduce phone-specific or connector-specific ControlService models.
- Portable configuration validation remains in `DisplayMagician.ConfigurationDefinitions`, not Gateway or the phone app.

REST clients never choose an internal pipe, inspect protected machine paths, or perform direct display/audio work.

## 7. Network exposure and trust

First-release support covers local and routed private networks. A same-subnet source-address check is not part of Gateway trust. Gateway still requires pairing, credentials, capability checks, and user isolation.

Installer firewall rules permit Private and Domain Windows network profiles by default. An administrator may opt in to Public networks through WinForms Server Settings; this setting changes the Windows firewall allowance and does not configure port forwarding.

- Network advertisements can be forged; trust remains anchored in pairing and the pinned host.
- Do not configure port forwarding, automatic router exposure, or direct Internet access as part of discovery. Official user-enabled Internet access is deferred; deliberate router/firewall changes may still expose Gateway.
- A Telegram connector introduces an external command source but requires no inbound Internet access to Gateway.
- A controller running on the same PC does not automatically turn remote-origin input into trusted local IPC.
- Explicit revocation disables that device independently of marketplace entitlement and other paired devices.

Concrete listener/firewall enforcement and installed-environment tests belong in the implementation roadmap.
