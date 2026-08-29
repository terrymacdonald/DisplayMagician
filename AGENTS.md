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
- Ask the user if you have any questions about the tasks that the user has asked you to perform. 

## Project conventions
- Follow the existing C# style: PascalCase for public types, methods, properties, and enums; `_camelCase` for private fields; explicit types where they improve readability.
- Organise larger classes with the existing `#region` pattern: class variables, constructors, properties, and methods.
- Keep domain and persistence behaviour in the relevant repository, service, or model class. Keep WinForms code focused on control state, input validation, and user interaction.
- Prefer clear imperative code, existing project helpers, and small local changes over introducing new frameworks, patterns, or abstraction layers.

## Logging and error handling
- Use the project NLog convention: `ClassName/MethodName: descriptive message`.
- Include useful operational context in logs, such as the affected profile, path, setting, or action. Pass the exception to NLog when one exists.
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

## Change discipline
- Before adding new code, search for an existing repository, service, helper, or form that already owns the behaviour and extend it where practical.
- Keep public APIs and serialized member names stable unless an explicit migration or compatibility change accompanies them.
- Build the affected project or solution after changes, and run the most relevant existing verification available.
- Use CRLF (`\r\n`) line endings for text files changed or created in this Windows repository.
