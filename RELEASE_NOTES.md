# DisplayMagician 3.0.0 Release Notes

DisplayMagician 3.0.0 is a major release focused on broader multi-GPU support, more complete game-shortcut automation, and a much more dependable display-profile experience. It also moves the application to .NET 10 and refreshes the interface for modern high-DPI displays.

## Donation
This is the result of hundreds and hundreds of hours of work over the last year to build a robust reliable application for you to use. There are now over 36,500 DisplayMagician users, so that weight of expectancy does weigh heavily. I've made I really want to make something great for people to use. I'd relly appreciate a donation from you to help buy a coffee or two! 

<a href="https://www.buymeacoffee.com/displaymagician" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-orange.png" alt="Buy Me A Coffee" height="41" width="174"></a><a href="https://github.com/sponsors/terrymacdonald" target="_blank">  <img src="https://github.com/terrymacdonald/DisplayMagician/raw/main/READMEAssets/gh-sponsor.png" alt="Github Sponsor" height="41" width="122"></a>

The following incredibly generous people get special mention for their extra large donations!
* Nigel-CY (Thanks for the monthly donation!) <---- MASSIVE THANKS FOR THE EXTRA LARGE DONATION!!!!!
* zaneyard (Thanks for the monthly donation!) <---- MASSIVE THANKS FOR THE EXTRA LARGE DONATION!!!!!
* jonathanprl (Thanks for the monthly donation!) <---- MASSIVE THANKS FOR THE EXTRA LARGE DONATION!!!!!
* domenic (Thanks for the monthly donation!) <---- MASSIVE THANKS FOR THE EXTRA LARGE DONATION!!!!!
* luke (Thanks for the 15 coffees!) <---- MASSIVE THANKS FOR THE EXTRA LARGE DONATION!!!!!
* mattmazgaj (Thanks for the monthly donation!) <---- MASSIVE THANKS FOR THE EXTRA LARGE DONATION!!!!!
* frcooper (Thanks for the monthly donation!) <---- MASSIVE THANKS FOR THE EXTRA LARGE DONATION!!!!!
* RBZL (Thanks for the monthly donation!) <---- MASSIVE THANKS FOR THE EXTRA LARGE DONATION!!!!!
* sean (Thanks for the 100 coffees!) <---- MASSIVE THANKS FOR THE EXTRA LARGE DONATION!!!!!

## Highlights

### Multi-GPU support

DisplayMagician now handles multi-GPU systems. It can support any combinations of Intel, NVIDIA, AMD or other GPUs, and will keep track of the settings of the adapters found and the displays connected to them. This is especially useful for multi-GPU gaming laptops.

### Intel GPU and Combined Display support

DisplayMagician can now capture, save, compare, and apply Intel display configurations, including Intel Combined Displays. This brings Intel systems alongside the existing NVIDIA Surround, AMD Eyefinity, and standard Windows multi-display workflows.

The Intel integration uses the Intel Graphics Command Center APIs and includes improved handling for display identifiers, cloned/combined display transitions, HDR, refresh rate, scaling, rotation, and display colour settings.

### Brand new video libraries

This DisplayMagician release swaps from handcrafted to video libraries to a brand new automated build process. We have build a brand new process for creating our video libraries, which allows us to quickly and (fairly) easily update our wrapper libraries to make use of any new features released by NVIDIA, AMD or Intel. The three libraries are NVAPIWrapper, ALDXWrapper and IGCLWrapper. These have been made free to use and opensource to help any other future developers build something great for people to use.

### Built for .NET 10

DisplayMagician now runs on .NET 10. The installer includes the required runtime, so no separate runtime installation should be needed. This modernises the application platform and updates the supporting display, AMD, NVIDIA, and Intel libraries. It also allows us to start monitoring modern UWP applications.

### Audio Profiles

You can now create audio profiles and attach one to a game or application shortcut. A shortcut can apply its display profile and its selected audio-device configuration together, making it easier to move between desktop, headset, sim-racing, streaming, and living-room setups.

DisplayMagician also handles delayed audio devices after a display change, provides clearer audio-profile recovery and failure guidance, and explains when Windows microphone privacy permissions prevent audio settings from being accessed. A denied permission no longer prevents the rest of a shortcut from running.

### Use Streamdecks, buttons and joysticks as hotkeys

This release allows you to use button box buttons, Streamdecks and joysticks as hotkeys to perform various tasks. You can set things up so you can press a button on your button box an bring up the DisplayMagician Game Shortcut Library window. Or you can map a hotkey on your keyboard and then set up a Streamdeck button to press that key combination when yuou press it. I have this set up on my Streamdeck so I can press a single button to run Assetto Corsa Rally and start all my helper applications with a single button press.

### More capable and reliable game shortcuts

Game shortcuts have been substantially improved:

- More reliable process-tree monitoring means DisplayMagician is better at recognising when a launched game or application has really finished, including launcher, child-process, local executable, and UWP scenarios.
- Shortcuts can start helper programs before a game and stop selected programs when required, with optional restoration after the game closes.
- Rollback profiles are only applied after a successful display-profile change, and stalled shortcuts can be restarted manually instead of leaving the user blocked.
- Steam, Epic, GOG, EA/Origin, Ubisoft Connect, Xbox, installed Windows apps, UWP apps, and direct executables received compatibility and launch-argument improvements.

### Display profiles are more robust

This release contains extensive work to make profile detection, loading, saving, and application more reliable:

- Display profiles now preserve and compare more display details, including HDR, colour configuration, refresh rate, dynamic refresh rate, scaling, rotation, taskbar placement, wallpaper choices, and multi-adapter layouts.
- NVIDIA, AMD, Intel, and Windows profile handling has been aligned with the VideoInfo test tool, including safer adapter-ID patching after Windows changes adapter IDs across reboots.
- Profiles are normalised when loaded, reducing failures caused by older or incomplete JSON data.
- Profile data is now written atomically with a backup, helping protect the last valid configuration if a write is interrupted.
- The UI performs a live recheck before it decides that a selected profile is already active, so unobserved adapter-level, colour, or HDR changes do not incorrectly hide the Apply action.
- Profile application has better retry, timeout, rollback, preflight, and failure-recovery behaviour for difficult display hardware and slow monitors.

### Wallpaper and display experience improvements

Display profiles can now retain fuller wallpaper settings, including per-display wallpaper behaviour and Windows Spotlight/slideshow handling. Profile settings also allow repeated application attempts for displays that need more time to settle.

The display-profile UI has been redesigned and improved for scaling, large text, maximised windows, DPI-aware layout, cloned displays, and clearer advisory messages.

## Also new and improved

- New multi-key keyboard, joystick, controller, and Stream Deck-friendly hotkey support through DirectInput.
- A refreshed, resizable, DPI-aware WinForms interface with a new application icon and improved tray/icon behaviour.
- Improved Windows package identity and installer behaviour, including better support for UWP applications, Xbox titles, app permissions, upgrades, and clean uninstallation.
- Improved installation/removal process that allows optional full removal of DisplayMagician configuration if desired.
- Signed installation using SignPath certificates
- In-app messages and release/update information, so important announcements can be shown inside DisplayMagician.
- Safer settings migration with backups and a migration summary shown after upgrades.
- New DisplayMagician logo that is almost the same as the old one!
- Updated installation, signing, release, dependency, diagnostics, and telemetry infrastructure to support the new release process.

## Important notes

- DisplayMagician 3.0.0 requires 64-bit Windows 10 version 1809 or later, or Windows 11.
- NVIDIA Surround support requires a compatible NVIDIA GPU and the latest NVIDIA Game Ready driver.
- AMD Eyefinity support requires AMD Radeon Software Adrenalin 2020 Edition 21.2.1 or later.
- Intel Combined Display support requires Intel Graphics Command Center 1.100 or later.
- Audio-profile features will require enabling microphone privacy access for DisplayMagician in Windows Settings. This permission is required to allow DisplayMagician to interact with the audio devices, so audio profiles will be unavailable if this permission is not given.

**Full changelog:** [Compare `main` with `release-3.0.0`](https://github.com/terrymacdonald/DisplayMagician/compare/main...release-3.0.0)
