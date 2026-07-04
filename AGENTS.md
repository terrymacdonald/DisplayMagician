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
