# DisplayMagicianConsole

`DisplayMagicianConsole.exe` controls the locally installed DisplayMagician instance from a command prompt, PowerShell script, Stream Deck action, or another application. It sends requests to the local Control Service; it does not directly alter display or audio settings itself.

Run it in the signed-in user's Windows session. DisplayMagician's Control Service and UserAgent must be running. Names with spaces must be enclosed in double quotes.

## Running the console

From a development build:

```powershell
.\DisplayMagicianConsole\bin\Debug\DisplayMagicianConsole.exe --help
```

From an installed copy, run `DisplayMagicianConsole.exe` from its installation directory. Use the full path when calling it from another program or script.

```powershell
& "C:\Program Files\DisplayMagician\DisplayMagicianConsole.exe" AllProfiles
```

Use `--help` for the command list, or add it after a command for that command's usage.

```powershell
DisplayMagicianConsole.exe --help
DisplayMagicianConsole.exe ChangeProfile --help
```

If no command is supplied, the console runs `CurrentProfile`.

## Common options

`-p` writes compact, pipe-delimited output intended for scripts. Put it after the command. For example:

```powershell
DisplayMagicianConsole.exe AllProfiles -p
```

The display-profile commands also support `-v` to print progress information.

## Display profile options

| Command | What it does | Example |
| --- | --- | --- |
| `CurrentProfile` | Shows the saved display profile matching the current display layout, or `UNKNOWN`. | `DisplayMagicianConsole.exe CurrentProfile` |
| `AllProfiles` | Lists all saved display profiles. | `DisplayMagicianConsole.exe AllProfiles -p` |
| `ChangeProfile <UUID\|Name>` | Applies a saved display profile by UUID or exact name. | `DisplayMagicianConsole.exe ChangeProfile "Desk and TV"` |
| `CreateProfile <Name>` | Saves the current display layout as a new display profile. | `DisplayMagicianConsole.exe CreateProfile "Desk only"` |
| `CreateProfile -force <Name>` | Replaces a saved profile with the same name. It will not replace a profile if the display settings are already saved. | `DisplayMagicianConsole.exe CreateProfile -force "Desk only"` |

`AllProfiles -p` and `CurrentProfile -p` use this format:

```text
Name|UUID
```

## Audio profile options

| Command | What it does | Example |
| --- | --- | --- |
| `CreateAudioProfile <Name>` | Saves the current audio setup as a new audio profile. | `DisplayMagicianConsole.exe CreateAudioProfile "Headphones"` |
| `AllAudioProfiles` | Lists all saved audio profiles. | `DisplayMagicianConsole.exe AllAudioProfiles -p` |
| `CurrentAudioProfile` | Shows the saved profile matching the current audio setup, or `UNKNOWN` if it is not saved. | `DisplayMagicianConsole.exe CurrentAudioProfile` |
| `ChangeAudioProfile <UUID\|Name>` | Applies a saved audio profile by UUID or exact name. | `DisplayMagicianConsole.exe ChangeAudioProfile "Headphones"` |

`AllAudioProfiles -p` and `CurrentAudioProfile -p` use this format:

```text
Name|UUID
```

## Shortcut options

| Command | What it does | Example |
| --- | --- | --- |
| `AllShortcuts` | Lists all saved game, application, and executable shortcuts. | `DisplayMagicianConsole.exe AllShortcuts -p` |
| `RunShortcut <UUID\|Name>` | Starts a saved shortcut by UUID or exact name. | `DisplayMagicianConsole.exe RunShortcut "Cyberpunk 2077"` |

`AllShortcuts -p` uses this format:

```text
Name|UUID|Category
```

Use the UUID returned by a list command when names are duplicated.

## Status and Decision options

Display-changing and shortcut operations are reported through the Control Service. Use these commands when an external integration needs to observe progress or answer a decision.

| Command | What it does | Example |
| --- | --- | --- |
| `Status` | Lists current and recently completed operations for this Windows session. | `DisplayMagicianConsole.exe Status -p` |
| `ListDecisions` | Lists pending decisions that this Windows session can answer. | `DisplayMagicianConsole.exe ListDecisions -p` |
| `AnswerDecision <Prompt_ID> <Choice>` | Answers a pending decision with `Continue` or `StopAndRestore`. | `DisplayMagicianConsole.exe AnswerDecision "<prompt-guid>" Continue` |

The pipe-delimited formats are:

```text
# Status -p
OperationId|Sequence|Phase|IsTerminal|IsSuccessful|UpdatedUtc|Message

# ListDecisions -p
PromptId|OperationId|DefaultChoice|ExpiresUtc|Title|Message
```

When a decision is not answered before it expires, DisplayMagician uses its supplied default choice. Current recovery decisions default to `Continue`.

## Exit codes

The console returns `0` on success. Common non-zero exit codes are:

| Code | Meaning |
| --- | --- |
| `1` | The user cancelled a display-profile change. |
| `100` | An unexpected error occurred. |
| `101` | The requested shortcut was not found. |
| `102` | The requested display or audio profile was not found. |
| `103` | A display or audio profile could not be applied. |
| `104` | The command or options were not recognised. |
| `105` | The current display settings are already saved as a profile. |
| `106` | A display profile with that name already exists. |
| `107` | A display or audio profile could not be created. |

For reliable scripting, check `$LASTEXITCODE` in PowerShell after invoking the executable.

```powershell
& "C:\Program Files\DisplayMagician\DisplayMagicianConsole.exe" ChangeProfile "Desk only"
if ($LASTEXITCODE -ne 0) {
    throw "DisplayMagician could not apply the profile (exit code $LASTEXITCODE)."
}
```
