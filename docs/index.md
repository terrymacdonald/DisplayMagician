[![](https://img.shields.io/github/license/terrymacdonald/DisplayMagician.svg?style=flat-square)](https://github.com/terrymacdonald/DisplayMagician/blob/main/LICENSE)
[![](https://img.shields.io/github/commit-activity/y/terrymacdonald/DisplayMagician.svg?style=flat-square)](https://github.com/terrymacdonald/DisplayMagician/commits/main)
[![](https://img.shields.io/github/issues/terrymacdonald/DisplayMagician.svg?style=flat-square)](https://github.com/terrymacdonald/DisplayMagician/issues)

DisplayMagician is an open source tool for automatically configuring your displays and sound for a game or application from a single Windows Shortcut. DisplayMagician is designed to change your display profile, change audio devices, start extra programs and then run the game or application you want. It will even reset things back to the way they were for you once you've closed the game or application!

<div style="text-align:center"><img src="https://github.com/terrymacdonald/DisplayMagician/raw/main/READMEAssets/DisplayMagicianMainScreen.png"/></div>

## Download
[![](https://img.shields.io/github/downloads/terrymacdonald/DisplayMagician/total.svg?style=flat-square)](https://github.com/terrymacdonald/DisplayMagician/releases)
[![](https://img.shields.io/github/tag-date/terrymacdonald/DisplayMagician.svg?label=version&style=flat-square)](https://github.com/terrymacdonald/DisplayMagician/releases)

The latest version of this application is available for download via the [release](https://github.com/terrymacdonald/DisplayMagician/releases) page.

## What it does

Different games require your displays configured in different ways. If you're a simracer like me, you also require a lot of additional 'helper' applications the give you the additional functionality to game the way you want. Making all those changes each time I wanted to play each game REALLY started annoying me, and I thought there must be a better way.

There is now. DisplayMagician allows you to configure multiple different display profiles, and then use those different display profiles to create Game Shortcuts. These Game Shortcuts allow you to have your game or application start exactly the way you like it.

Do you like running Dirt Rally 2.0 on a single NVidia Surround window across triple screens, and yet you like to run Project Cars 2 across four individual screens (a triple and one above)? Do you like running SimHub when you play iRacing, yet you want to start Twitch when you play Call of Duty? Well with DisplayMagician you can do all that with a single Desktop Shortcut (you can even start games with a Hotkey)!

DisplayMagician also allows you to automatically change to a different audio device just for one game, and will revert that change when you close the game. Great if you have some special audio devices you use only for certain games. No more fiddling with audio settings - just play the game!

Please read through the README for features (current and planned) and issues you may encounter while using the program. 

Feel free to report missing features or bugs using the project [issue tracker](https://github.com/terrymacdonald/DisplayMagician/issues).

## Current features

DisplayMagician lets you set up the following information for each game or application:
* Create and save a Display Profile to be used within future Desktop Shortcuts
* Save a Game Shortcut that will automatically change to a different Display Profile and start your Game when you double-click on it.
* Run your Game Shortcut using a keyboard shortcut (Hotkey).
* Or start your games by right-clicking on the DisplayMagician Notification Tray icon.
* Choose which Audio Device you want the shortcut to use. Like using the wireless headset when driving? This lets you with a single click.
* Add one or more programs to pre-start before your game or application
* Works with Steam, Ubisoft Uplay, Electronic Arts Origin and Epic Games Game launchers!
* Optionally rollback to your previous Display profile once the game or application has closed.
* Or maybe just create a Shortcut that permanently changes to a different Display Profile! The options are endless.
* Also comes with a Shell Extension that allows you to change to a different Display Profile by right-clicking on the desktop background!
* Supports NVIDIA Surround setups (but doesn't support AMD Eyefinity yet... maybe someone wants to donate an AMD videocard?)

## Planned features

* Support of AMD Eyefinity if someone donates an AMD video card to me (Need to create a C# wrapper for AMD ADL)
* Add Battlenet Game Launcher
* Add Bethesda Game Launcher
* Add Galaxy of Games Game Launcher (maybe?)
* Add Unit Tests!
* Change UI from Winforms to .NET6 and MAUI

## Donation
I am doing this work to scratch a programming itch I've had for a while. It's pretty fun to take something carefully crafted by another developer and extend it with a lot of other awesome features. That said, I'd appreciate a donation to help buy a coffee or two! If you're so inclined, you can [sponsor me on GitHub Sponsors](https://github.com/sponsors/terrymacdonald). 

## Usage

### Screenshots
<div style="text-align:center"><img src="https://github.com/terrymacdonald/DisplayMagician/raw/main/READMEAssets/DisplayMagicianMainScreen.png"/></div>
<div style="text-align:center"><img src="https://github.com/terrymacdonald/DisplayMagician/raw/main/READMEAssets/DisplayMagicianDisplayProfiles.png"/></div>
<div style="text-align:center"><img src="https://github.com/terrymacdonald/DisplayMagician/raw/main/READMEAssets/DisplayMagicianShortcutLibrary.png"/></div>
<div style="text-align:center"><img src="https://github.com/terrymacdonald/DisplayMagician/raw/main/READMEAssets/DisplayMagicianConfigureShortcut1.png"/></div>
<div style="text-align:center"><img src="https://github.com/terrymacdonald/DisplayMagician/raw/main/READMEAssets/DisplayMagicianConfigureShortcut2.png"/></div>
<div style="text-align:center"><img src="https://github.com/terrymacdonald/DisplayMagician/raw/main/READMEAssets/DisplayMagicianConfigureShortcut3.png"/></div>
<div style="text-align:center"><img src="https://github.com/terrymacdonald/DisplayMagician/raw/main/READMEAssets/DisplayMagicianConfigureShortcut4.png"/></div>
<div style="text-align:center"><img src="https://github.com/terrymacdonald/DisplayMagician/raw/main/READMEAssets/DisplayMagicianConfigureShortcut5.png"/></div>
<div style="text-align:center"><img src="https://github.com/terrymacdonald/DisplayMagician/raw/main/READMEAssets/DisplayMagicianDisplayProfileHotkey.png"/></div>
<div style="text-align:center"><img src="https://github.com/terrymacdonald/DisplayMagician/raw/main/READMEAssets/DisplayMagicianShellExtension.png"/></div>

### Initial Setup:

1. Install 'DisplayMagician'
2. Run 'DisplayMagician', and click on the 'Display Profiles' button
    * Use 'Windows Display Settings' or 'NVIDIA Control Panel' to configure your display(s) exactly as you would like them
    * ALT-TAB back to DisplayMagician, and you should see the new display configuration shown in the Display Profile window.
    * Click the 'Save As/Rename' button so that DisplayMagician will remember the current Display Profile so you can use it in your Shortcuts.
    * You will need to do this for each different display configuration you want to use in any of your games or applications.
    * Click 'Back' button to go back to the main DisplayMagician window.
2. Now that you have some Display Profiles set up, you can create some Game Shortcuts that will use them! Click on the 'Game Shortcuts' button.
3. You will be shown your Shortcut Library window. This is where all your Shortcuts live, and where you have to go if you every want to edit them.
4. Click the 'New' button to create a new Game Shortcut
    * Choose the Display Profile you want to use with the Game Shortcut. Make sure it matches what the game expects :).
    * Click on the '2. Choose Audio' tab, to modify which speakers you use for sound, or which microphone you use to talk into.
    * Click on the '3. Choose what happens before' tab, to choose any other programs you want to start before you start the main Game or Application. You can choose if you want to shut them down afterwards too!
    * Click on the '4. Choose Game to start' tab, to choose the main game or application that the Game Shortcut will start and monitor.
    * Choose the Game from the list shown (be sure to click the > button), or if it's not listed there then select the game or application executable. 
    * Click on the '5. Chose what happens afterwards' tab, and choose if you want to rollback any changes made by the Game Shortcut when it runs, or if you want to keep them.
    * If the 'auto-suggest name' option is enabled then you should have a Shortcut Name already entered in automatically. 
    * You can optionally create a keyboard shortcut (Hotkey) for this Game Shortcut. To do this, click on the 'Hotkey' button.
    * Click the 'Save' button to save the Shortcut to the Shortcut Library. If you can only see the outline of a button, then you have some missing fields you need to fill in. The Save button only shows if you have a valid Shortcut set up.
5. Once you've saved the Short cut, you should see it in the Shortcut Library.
6. To create a Desktop shortcut file from your Shortcut, select it in the list in your Shortcut Library, and click the 'Save to Desktop' button. This will then write the Shortcut to your computer, ready to use!
<div style="text-align:center"><img src="https://github.com/terrymacdonald/DisplayMagician/raw/main/READMEAssets/DisplayMagicianShortcutOnDesktop.png"/></div>
7. You can now double-click on the Desktop shortcut you just saved, and DisplayMagician will do exactly what you asked it to!


## License

Copyright © Terry MacDonald 2020-2021

Original HelioDisplayManagement - copyright © Soroush Falahati 2017-2020

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License along
with this program; if not, write to the Free Software Foundation, Inc.,
51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.


## Credits
Thanks for the work and the time that all of our contributors put into making this a better project. Following is a short list, containing the name of some of these people:

* Original HelioDisplayManagement project created by the amazing Soroush Falahati 
* Various icons made by Freepik from www.flaticon.com