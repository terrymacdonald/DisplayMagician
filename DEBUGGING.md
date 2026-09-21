# Visual Studio Debugging

## Full-stack debugging

Use the installed Debug build for workflows that cross the Control Service and
User Agent boundary. This preserves the production identities and named-pipe
ACLs:

1. Build the Debug x64 installer with `build_displaymagician.ps1`.
2. Install the resulting MSI on the development machine. The installer starts
   `DisplayMagicianControlService` automatically; the Session Launcher starts
   only when the Control Service needs it.
3. Start DisplayMagician from the installed location and reproduce the issue.
4. In Visual Studio 2026, use **Debug > Attach to Process** and attach managed
   code debuggers to the relevant processes:
   - `DisplayMagician.exe` for the WinForms client.
   - `DisplayMagician.UserAgent.exe` for display, audio, game, shortcut, and
     per-user message work.
   - `DisplayMagician.ControlService.exe` for authorization, routing,
     migration, recovery, audit, and machine-level operations.
   - `DisplayMagician.SessionLauncher.exe` only when diagnosing on-demand
     User Agent startup.

The Debug installer carries matching PDBs, so breakpoints resolve to this
solution's source. Build and reinstall after changing a component that is
being debugged.

## Component debugging with F5

The checked-in launch profiles provide direct project startup for focused work:

- `DM Local Client Debug` starts the WinForms client with debug logging.
- `DisplayMagician.ControlService (Console)` runs the Control Service as a
  console host. `AddWindowsService` uses normal console lifetime when the
  process is not started by the Windows Service Control Manager.
- `DisplayMagician.UserAgent` starts an Agent in the current interactive
  session.

To select one, right-click its project, choose **Set as Startup Project**,
choose the named profile in the debug target dropdown, and press F5.

Starting the Control Service and User Agent independently under F5 is useful
for startup, registration, migration, and pipe-listener debugging. It is not
a full end-to-end substitute for the installed stack: the Agent intentionally
accepts command-pipe requests only from the `LocalService` account, while a
Control Service started by Visual Studio runs as the signed-in developer.
This preserves the production authorization boundary; use the full-stack
attach workflow for profile application and shortcut execution.