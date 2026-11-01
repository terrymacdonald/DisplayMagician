# Full installation reset validation

Status: deferred by the user on 1 November 2026 for Terry's consolidated manual test run. These installed checks have not been executed/passed. Tracked in the [whole-app test checklist](../Docs/CODEBASE_BUG_REVIEW.md#whole-app-completion-checklist); source review can continue meanwhile.

Run only inside Windows Sandbox. This test deliberately deletes all DM data in that disposable environment. Do not run reset against host data to validate this feature.

1. Build the current Debug bundle with `build_displaymagician.ps1`. Run `debug_displaymagician.ps1` as administrator, select Windows Sandbox, and install that freshly built bundle. Follow the harness README for prerequisites.
2. Launch DM, create display/audio profiles and a shortcut, change preferences, and create a Support ZIP outside DM's data folders. If practical, pair a disposable client and record the public Gateway host/TLS identity.
3. Confirm machine, UserAgent and desktop logs exist. Add recognisable log files under the current user's legacy Logs folder and retained backup logs. Create a second test user's DM data to check the all-user scope.
4. In Settings > Support Settings, open Reset DisplayMagician. Cancel administrator approval or choose Cancel in recovery: no reset should occur. The recovery window should explain permanent deletion, all-user scope, retained logs and the optional pre-reset Support ZIP. Cancel should initially have focus.
5. Confirm Reset DisplayMagician. Other desktop/console clients should close. Running shortcuts should cancel and complete restoration before their Agents stop. New desktop/Agent startup during maintenance should wait/refuse without writing old configuration. A failed restoration or surviving Agent must prevent deletion and report the problem.
6. On success, close recovery and launch DM normally. Profiles, shortcuts and preferences should be fresh installation defaults. Retired legacy files and configuration backups must not be re-imported. Check both users' data. Old logs and the manually exported Support ZIP outside DM data should remain. No `.reset` files or reset archive should exist.
7. Verify ControlService and Gateway are running. SessionLauncher remains demand-start. Verify old pairing records are gone, Gateway consumed its identity-reset request under LocalService, host/TLS identities changed, and the disposable client must pair again.
8. Close DM/Agents, stop ControlService in the Sandbox, and damage configuration/replay JSON. Run the installed `DisplayMagician.exe --reset-installation`; the recovery window must open before normal configuration loading. Confirm reset and verify fresh startup and retained logs again.
9. Repeat with a locked configuration file: reset must report failure rather than success. Release the lock and retry. Reset has no rollback archive; partially completed deletion must be reported clearly and remain retryable.
10. Check the confirmation/progress window at 100%, 150% and 200% DPI. During reset, repeated clicks and closing must not create competing reset requests. Check that recovery failure does not leave the ordinary desktop silently saving old settings.

Source/unit tests substitute reset/identity/process operations and use temporary data. This checklist covers the installed SCM, ACL, UAC, LocalService key ownership and process-boundary behaviours they cannot establish.
