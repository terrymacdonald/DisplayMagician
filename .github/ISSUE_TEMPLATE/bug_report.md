---
name: Bug report
about: Create a bug report to help us fix something broken in DisplayMagician
title: ''
labels: bug
assignees: terrymacdonald

---

**Describe the bug**
A clear and concise description of what the bug is.

**To Reproduce**
Steps to reproduce the behaviour:
1. Go to '...'
2. Click on '....'
3. Scroll down to '....'
4. See error

**Expected behaviour**
A clear and concise description of what you expected to happen.

**Screenshots**
If applicable, add screenshots to help explain your problem.

**Reporting Logs**
If DisplayMagician starts:
- Open DisplayMagician and perform whatever actions are needed to make the bug occur.
- Click on the Settings button on the main DisplayMagician window
- Click on 'Create a Support Zip File' button
- Save the Support Zip File on your computer.
- Come back here, and upload the Support Zip File through your web browser and attach it to this issue.

If DisplayMagician does not:
- Open a terminal window and run `C:\Program Files\DisplayMagician\DisplayMagician.exe --trace` to create a TRACE-level DisplayMagician.log file.
- The above command will create a DisplayMagician.log file in `C:\Users\<yourusername>\AppData\Local\DisplayMagician\Logs` which should contain a LOT of detailed TRACE level log entries. 
- Come back here, and upload the DisplayMagician.log file through your web browser and attach it to this issue.
- Also attach the following files to this issue:
  - `C:\Users\<yourusername>\AppData\Local\DisplayMagician\Profiles\DisplayProfiles_2.1.json` 
  - `C:\Users\<yourusername>\AppData\Local\DisplayMagician\Shortcuts\Shortcuts_2.0.json`
  - `C:\Users\<yourusername>\AppData\Local\DisplayMagician\Settings_2.0.json`

**Enviroment (please complete the following information):**
 - Windows Version: [e.g. Win10]
 - DisplayMagician Version [e.g. 1.1.0]
 - Video Card: [e.g. Asus NVIDIA GTX1070]
 - Number and make/model of monitors: [e.g. 2xDell UH2718H, 1x LG G27U17D]
 - Date of last video driver update: [e.g. 2021-02-15]
 - Date of last windows update: [e.g. 2021-02-15]

**Additional context**
Add any other context about the problem here.
