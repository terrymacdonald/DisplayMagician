# DisplayMagician Administrator Timeline Import Contract

## Purpose

This document defines the offline input contract for the future separate DisplayMagician administrator timeline application. The application imports Support ZIP files without changing them, presents parsed events as table cells, and supports fast multi-select filtering across all retained component logs.

## Compatibility

- A Support ZIP contains `support-manifest.json` at its root.
- `SchemaVersion` identifies the ZIP layout and log contract version.
- The importer must reject only the unreadable entry that caused an error. It must retain and display all other events and report the rejected entry/line as an import warning.
- Unknown manifest fields, ZIP entries, and log fields are forward-compatible and must be preserved in raw-event detail where practical.

## Runtime component names

Only these component values are part of the contract:

| Component | Role |
|---|---|
| `DesktopApp` | WinForms application, including its in-process update handling. |
| `DesktopConsole` | Command-line client diagnostics. |
| `UserAgent` | Per-user background runtime. |
| `ControlService` | Machine-wide coordination service. |
| `SessionLauncher` | LocalSystem broker that starts an authorized User Agent. |
| `Installer` | Installation, repair, upgrade, and uninstall diagnostics. |

## Structured one-line event format

Each new-format event occupies exactly one UTF-8 physical line and uses a logfmt-style sequence of space-separated key/value fields. The required fields are emitted in this order:

```text
ts=2026-09-22T08:15:12.3456789Z component=ControlService level=ERROR source=ControlClientPipeServer/CreateUserSupportBundleAsync operation_id=7fe7d61d-06c8-454e-af5b-846ea8da1cf1 request_id=8c2f99d4-4d89-4df6-a178-e9fe073bc3d3 msg="Could not stage machine logs"
```

Required fields:

| Field | Meaning |
|---|---|
| `ts` | ISO-8601 UTC timestamp, sortable as text. |
| `component` | One of the runtime component names. |
| `level` | `TRACE`, `DEBUG`, `INFO`, `WARN`, `ERROR`, or `FATAL`. |
| `source` | Explicit `ClassName/MethodName` source. |
| `operation_id` | One multi-step user operation GUID, or `-` when not applicable. |
| `request_id` | One pipe/control-request GUID, or `-` when not applicable. |
| `msg` | Human-readable event message. |

Exception fields are optional and appear only for an event with an exception:

```text
ex_type=System.UnauthorizedAccessException ex_message="Access was denied." ex_stack="System.UnauthorizedAccessException: Access was denied.\\r\\n   at ..."
```

Values may be bare only when they contain letters, digits, `.`, `_`, `-`, `/`, or `:`. All other values are double-quoted. Inside quoted values, `\\`, `\"`, `\r`, `\n`, and `\t` represent backslash, quote, carriage return, line feed, and tab respectively. The importer must unescape these values after parsing; it must never split an escaped exception stack into multiple events.

## Support ZIP layout

```text
support-manifest.json
Logs/                         # DesktopApp, DesktopConsole, UserAgent retained logs
MachineLogs/                  # ControlService and SessionLauncher retained logs
MachineLogs/Installer-LastTransaction.log  # last available MSI repair/install/upgrade transaction log
Configuration/Profiles/
Configuration/AudioProfiles/
Configuration/Shortcuts/
Configuration/Settings/
Configuration/LegacyFiles/
Configuration/Migration.json
Configuration/Machine/        # approved service state
```

The manifest records the product and component versions, ZIP creation UTC time, user SID, included-entry inventory, source format/version, and collection warnings. Sensitive credentials, tokens, unrelated user files, media, wallpapers, and cached icons are excluded.

The installer enables verbose MSI logging without the property-dump mode and records the most recent transaction path. If that temporary MSI log has already been removed or cannot be read, it is absent from the ZIP and the importer must rely on the manifest warning rather than treating the ZIP as invalid.

Each `IncludedEntries` record identifies the ZIP `Path`, byte `Length`, `Category` (`Log`, `LegacyLog`, or `Configuration`), applicable runtime `Component`, input `Format` (`logfmt-v1`, `legacy-text`, `msi-verbose`, `json`, or `file`), and `CollectionResult` (`Included`). Sources that could not be collected are represented by a manifest warning rather than a fictitious ZIP entry.

## Privacy and redaction

The log writer redacts values labelled as passwords, tokens, secrets, or API keys, and Bearer credentials, from messages and exception detail before writing. Support ZIP collection excludes credentials, pairing data, unrelated documents, media, wallpapers, and cached icons. Future components must not log unrestricted command arguments or credentials; reduce personal paths and values when they are not diagnostically necessary.

## Import model and table

Parse each line into one event record. The default table shows values, never raw field names:

| Time (UTC) | Component | Level | Function | Operation | Request | Message |
|---|---|---|---|---|---|---|

The detail view shows the complete exception, raw source line, ZIP entry path, and line number. Preserve `operation_id` and `request_id` as full GUID values while allowing abbreviated display.

Order parsed events by UTC timestamp. For identical timestamps, use ZIP entry path then line number as a deterministic tie-breaker.

## Filtering

The UI must allow multiple selections within each filter and intersections across filter categories:

- time range;
- one or more components;
- one or more levels;
- one or more sources/functions;
- one or more operation IDs;
- one or more request IDs; and
- case-insensitive free-text search across `msg`, exception message, exception stack, and raw line.

Selections within a category are ORed; categories are ANDed. For example, selecting `ControlService` and `UserAgent`, `WARN` and `ERROR`, and one operation ID shows warning/error events from either selected component for that operation only.

## Legacy logs

The importer must identify legacy entries through the manifest and render them using a legacy parser or a raw-line fallback. A malformed line becomes a visible warning/raw event; it must not prevent other lines or ZIP entries from loading.

## Fixture set

The timeline application repository should retain immutable Support ZIP fixtures for normal shortcut execution, profile-apply failure, Agent restart/recovery, SessionLauncher Agent start, and installation/upgrade diagnostics. Each fixture must include expected event counts, component names, correlation IDs, and filter-result assertions.
