# Operation status and decision verification

Run these checks in the Windows Sandbox after installing the Debug Bundle. They validate the hardware- and UI-dependent parts of Phase E Part 3 without changing the host machine.

## Preparation

1. Start the sandbox through `debug_displaymagician.ps1` in Windows Sandbox mode.
2. Install the mapped Debug Bundle.
3. Start DisplayMagician and DisplayMagicianConsole in the sandbox user session.
4. Confirm `DisplayMagicianConsole Status` succeeds and `DisplayMagicianConsole ListDecisions` shows no pending decisions.

## Status updates

1. Run a normal display-profile apply and a normal shortcut.
2. Confirm DisplayMagician shows progress and a terminal toast for each operation.
3. Run `DisplayMagicianConsole Status -p` while the shortcut is active. Confirm each line contains operation ID, sequence, phase, terminal/success flags, UTC timestamp, and message.
4. Confirm sequence values only increase within one operation and the terminal record has the same operation ID.

## Decisions

For each of these deliberate failures, verify the same behaviour: display-profile apply, audio-profile apply, pre-game start-program, and pre-game stop-program.

1. Start a shortcut configured to hit the failure.
2. Confirm a modal asks whether to Continue or Stop and restore, with Continue as the default button.
3. In a second command prompt run `DisplayMagicianConsole ListDecisions -p`, copy the prompt ID, then run `DisplayMagicianConsole AnswerDecision <Prompt_ID> Continue`.
4. Confirm the WinForms modal closes and the shortcut proceeds using its current display/audio/program state.
5. Repeat using `StopAndRestore`; confirm the shortcut stops and temporary state is restored.
6. Leave the prompt unanswered until expiry. Confirm it defaults to Continue and that a later `AnswerDecision` command is rejected.

## Recovery and isolation

1. While a decision is pending, close and reopen DisplayMagician. Confirm it fetches and displays the still-pending decision.
2. Confirm a different Windows user/session cannot list or resolve the pending decision.
3. Export a Support ZIP and confirm the machine configuration includes `OperationDecisions.json` when one exists.

Record the Sandbox Windows version, bundle version, steps performed, result, and relevant operation/prompt IDs with any defect report.
