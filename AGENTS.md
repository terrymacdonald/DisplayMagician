# AGENTS

## UI Forms
- Create WinForms UI as Designer-backed forms so they open and can be adjusted in the Visual Studio Forms Designer.
- Do not build whole forms at runtime unless there is an extremely compelling reason. If runtime construction is required, document the reason in code comments.
- Keep static layout and control declarations in `*.Designer.cs`; keep runtime-only behavior in the main `*.cs` form file.
- Open sub forms modally and center them on the parent window.
- Show MessageBox dialogs as modal dialogs, and pass the parent window where available.
- When display layout changes, center the main window on the primary display, and center child forms on their parent windows.

## Functions
- Prefer fewer functions overall. A function can perform multiple steps in one process.
- If a function gets very long or repeats sections, then consider splitting it.
- Create/keep separate functions when:
  - More than two distinct things are being performed by that unit of logic, or
  - The same logic is called from multiple places in the codebase.

## Ask the user
- Ask the user if you have any question about the requested work, an important fact is unclear, or a choice would materially affect behaviour, data, security, compatibility, or scope.
- Do this before making the consequential change. Do not silently choose a product policy, recovery behaviour, user-visible workflow, or compatibility trade-off on the user's behalf.
- If the request is unambiguous and a normal implementation step is safe, proceed without unnecessary confirmation.

## Project conventions
- Follow the existing C# style: PascalCase for public types, methods, properties, and enums; `_camelCase` for private fields; explicit types where they improve readability.
- Organise larger classes with the existing `#region` pattern: class variables, constructors, properties, and methods.
- Keep domain and persistence behaviour in the relevant repository, service, or model class. Keep WinForms code focused on control state, input validation, and user interaction.
- Prefer clear imperative code, existing project helpers, and small local changes over introducing new frameworks, patterns, or abstraction layers.

## Component architecture and contracts
- Follow this runtime dependency direction:

  ```text
  WinForms --------------------> ControlService --> UserAgent
  DisplayMagicianConsole ------> ControlService --> UserAgent
  future local/paired client --> ControlService --> UserAgent

  ControlService --> SessionLauncher --> UserAgent process start only

  all runtime clients and hosts --> DisplayMagician.Contracts
  WinForms and UserAgent --------> DisplayMagician.ConfigurationDefinitions
  ```

- WinForms and DisplayMagicianConsole use the Control Service named-pipe API. They do not call UserAgent, SessionLauncher, display libraries, or authoritative per-user storage directly.
- ControlService routes user-scoped commands/events to UserAgent through the shared Contracts protocol. It may call SessionLauncher only when it has authorised a request and needs the interactive UserAgent started. SessionLauncher starts the fixed signed UserAgent payload only; it does not route normal commands back to it.
- `DisplayMagicianPackage` publishes and installs WinForms, Console, ControlService, SessionLauncher, UserAgent, and the identity package. `DisplayMagicianBundle` chains that MSI with required runtime prerequisites. These are build/package dependencies, not runtime application APIs.
- `DisplayMagician.Contracts` is the sole home for versioned Control Service requests, responses, events, shared enums, and client-safe views. WinForms, DisplayMagicianConsole, and every future client must use those same types.
- Do not create client-specific Control Service DTOs, list results, or view models. A client may format a shared contract differently for its UI or command-line output, but any transport-model change belongs in `DisplayMagician.Contracts` and must remain backwards compatible.
- Use `DisplayMagician.Contracts` for transport envelopes, message types, requests, responses, events, client-safe list/view models, shared cross-process enums, retry policy, and the common support-log/correlation types. `ConfigurationDefinitions` also references it only where a portable definition needs one of those shared enums.
- `DisplayMagician.ConfigurationDefinitions` owns portable persisted definitions, schema versions, JSON conversion, and pure validation. WinForms uses it to edit and validate portable configuration; UserAgent uses it to persist, validate, and execute those definitions. It must not gain UI, hardware, file, process, named-pipe, or runtime display/audio behaviour.
- `DisplayMagician.ControlService` owns authenticated local routing, machine-wide coordination, display-operation serialization, durable machine state, audit, diagnostics, and service scheduling. It must not show UI or directly perform interactive display, audio, game, process, Steam, or desktop-monitoring work.
- `DisplayMagician.UserAgent` owns interactive-session display/audio execution, game and process monitoring, shortcut lifecycle/recovery, per-user messages, and notifications. Keep that runtime behaviour out of the desktop application and Control Service.
- `DisplayMagician.SessionLauncher` is a narrow, authenticated LocalSystem broker used only by Control Service to demand-start the signed UserAgent in an authorised interactive session. Do not widen its access, make it a general launcher, or give it display/audio/UI responsibilities.
- Represent operation progress and user decisions with the shared operation contracts. A pending decision must be answerable by any authorised client for that user; where a recovery decision expires, preserve the agreed default of `Continue` unless the user changes that policy.

## Logging and error handling
- Use the project NLog convention: `ClassName/MethodName: descriptive message`.
- Include useful operational context in logs, such as the affected profile, path, setting, or action. Pass the exception to NLog when one exists.
- Use the shared support-log layout and correlation scope for runtime component logs. Preserve `component`, `source`, `operation_id`, and `request_id` so logs from different components can be merged; do not invent replacement correlation IDs downstream.
- Keep each structured support-log event on one physical line and do not place secrets, tokens, passwords, or sensitive unredacted arguments in logs.
- Handle expected filesystem, registry, hardware, network, and deserialisation failures at the operation boundary. Preserve usable state and return the project-appropriate failure result (`false`, an enum value, or a user-facing error) rather than allowing an avoidable crash.
- Do not silently ignore failures unless absence is explicitly expected; add a short comment when it is.

## Persistence and compatibility
- Treat persisted JSON files as backward-compatible user-data contracts.
- When changing a persisted model, update its version and add an explicit migration in `ConfigMigrationRunner` where required. Back up existing settings before a substantive migration.
- Use the project’s JSON.NET conventions and preserve unrelated existing settings, profiles, shortcuts, and user choices.
- Write persistent data safely so an interrupted write does not discard the last valid file.

## WinForms behaviour
- Keep event handlers responsible for validation, calling existing domain operations, refreshing affected controls, and showing the outcome.
- Keep UI updates on the WinForms UI thread; marshal changes from background work with `Invoke` or `BeginInvoke`.
- Validate destructive or irreversible actions with a confirmation dialog and give users a clear success or failure message.
- Reuse existing modal forms and the owning form when opening dialogs.
- Show normal operation progress through non-blocking notifications/toasts. Use a modal decision dialog only when the operation needs an explicit `Continue` or `StopAndRestore` answer; status and decisions still flow through the shared Control Service contracts so another authorised client can respond.

## Diagnostics and support bundles
- The user-selected Support ZIP must collect all retained logs and the approved configuration snapshot from DesktopApp, DesktopConsole, UserAgent, ControlService, and SessionLauncher. Machine-owned logs/configuration are staged by Control Service; desktop clients must not read protected machine paths directly.
- Keep `support-manifest.json` complete and backwards compatible: record included files, formats/versions, components, and collection warnings. Do not silently omit unavailable requested sources.
- Exclude credentials, pairing secrets, passwords, tokens, unrelated user documents, media, wallpapers, and cached icons unless the user explicitly approves their inclusion.

## Scripts and documentation
- Use lowercase underscore-separated PowerShell script names, for example `verify_log_message_formatting.ps1`.
- Put user-run entry-point scripts in the repository root. Put project-local developer/build scripts in that project's `BuildScripts` folder, and cross-project developer/build scripts in the root `BuildScripts` folder.
- Keep a `README.md` in every solution component directory. Update the affected README when a component's responsibility, public usage, build/test command, or packaging behaviour changes.
- Use the Windows Sandbox workflow for installer/debugging validation when the task needs an isolated installed environment; do not treat the sandbox harness as a shipped product component.

## Change discipline
- Before adding new code, search for an existing repository, service, helper, or form that already owns the behaviour and extend it where practical.
- Keep public APIs and serialized member names stable unless an explicit migration or compatibility change accompanies them.
- Build the affected project or solution after changes, and run the most relevant existing verification available.
- When changing direct NLog calls, run `verify_log_message_formatting.ps1`; remove its generated report after review unless it is intentionally being retained as an artefact.
- Use CRLF (`\r\n`) line endings for text files changed or created in this Windows repository.
