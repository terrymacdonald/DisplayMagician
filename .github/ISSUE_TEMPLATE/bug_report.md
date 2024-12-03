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
If applicable, add screenshots to this section to help explain your problem.

**Reporting Logs**
If DisplayMagician starts:
- Open DisplayMagician and perform whatever actions are needed to make the bug occur.
- Click on the Settings button on the main DisplayMagician window
- Click on 'Create a Support Zip File' button
- Save the Support Zip File on your computer.
- Come back here, and upload the Support Zip File through your web browser and attach it to this issue.

If DisplayMagician does not start:
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
 - Number and Type of Video Cards: [e.g. 1x Asus NVIDIA GTX4070, 1x AMD 9800X3D iGPU]
 - Number and make/model of displays: [e.g. 2x Dell UH2718H, 1x LG G27U17D]
 - How displays are conencted (and if any adapters being used): [e.g. Dell monitors connected using DP 2.1a, LG connected using HDMI]
 - Video Card driver version: [e.g. AMD Adrenalin Version 24.10.1, NVIDIA GeForce Game Ready Driver v566.14]
 - Have you run Windows Update?: [e.g. Yes, 2 days ago]

**Additional context**
Add any other context about the problem here.
