using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using DisplayMagicianShared;
using Microsoft.Win32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DisplayMagician
{
    public class ContextMenu
    {

        // Create a new DesktopBackground context menu in Windows Registry using the 
        // list of Profiles stored in the ProfileRegistry class.
        public static bool InstallContextMenu()
        {
            // Create the ContextMenu Anchor Registry Key
            try
            {
                RegistryKey anchor = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\Classes\\DesktopBackground\Shell\DisplayMagician.ContextMenu.Anchor");
                if (anchor != null)
                {
                    // Set up the ContextMenu Anchor Registry Key contents
                    anchor.SetValue("MUIVerb", "DisplayMagician");
                    anchor.SetValue("ExtendedSubCommandsKey", "DisplayMagician.ContextMenus\\ContextMenus\\MainMenu");
                    anchor.SetValue("Icon", Path.Combine(AppContext.BaseDirectory, "DisplayMagician.exe"));
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            try
            {
                // Create the MainMenu (Level 1) Registry Key
                RegistryKey dp = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\Classes\\DisplayMagician.ContextMenus\\ContextMenus\\MainMenu\\Shell\\DisplayProfiles");
                if (dp  != null)
                {
                    // Set up the Display Profiles ContextMenu Registry Key contents
                    dp.SetValue("MUIVerb", "Display Profiles");
                    dp.SetValue("ExtendedSubCommandsKey", "DisplayMagician.ContextMenus\\ContextMenus\\ProfileMenu");
                    //dp.SetValue("Icon", AppContext.BaseDirectory + "\\DisplayMagician.exe");
                }

            }
            catch (Exception ex)
            {
                return false;
            }
            
            try
            {
                RegistryKey gs = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\Classes\\DisplayMagician.ContextMenus\\ContextMenus\\MainMenu\\Shell\\GameShortcuts");
                if (gs != null)
                {
                    // Set up the Display Profiles ContextMenu Registry Key contents
                    gs.SetValue("MUIVerb", "Game Shortcuts");
                    gs.SetValue("ExtendedSubCommandsKey", "DisplayMagician.ContextMenus\\ContextMenus\\ShortcutMenu");
                    //gs.SetValue("Icon", AppContext.BaseDirectory + "\\DisplayMagician.exe");
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            

            if (!UpdateProfileContextMenu())
            {
                return false;
            }
            if (!UpdateShortcutContextMenu())
            {
                return false;
            }

            return true;
        }

        // Remove the DesktopBackground context menu from Windows Registry.
        public static bool UninstallContextMenu()
        {
            // Delete the ContextMenu Anchor Registry Key
            Registry.CurrentUser.DeleteSubKeyTree("SOFTWARE\\Classes\\DesktopBackground\\Shell\\DisplayMagician.ContextMenu.Anchor");

            // Delete the MainMenu (Level 1) Registry Key and all subkeys
            Registry.CurrentUser.DeleteSubKeyTree("SOFTWARE\\Classes\\DisplayMagician.ContextMenus");
            return true;
        }

        // Update the list of Display Profiles in the DesktopBackground context menu
        public static bool UpdateProfileContextMenu()
        {
            try
            {
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree("SOFTWARE\\Classes\\DisplayMagician.ContextMenus\\ContextMenus\\ProfileMenu\\Shell");
                }
                catch(Exception ex)
                {
                    // Ignore if the key doesn't exist
                }

                RegistryKey dp = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\DisplayMagician.ContextMenus\\ContextMenus\\ProfileMenu\\Shell");

                // Create the ProfileMenu (Level 2) Profile Entry Registry Keys
                foreach (ProfileItem profile in ProfileRepository.AllProfiles)
                {
                    if (!profile.IsPossible) continue; // Skip invalid profiles (e.g. missing screens)


                    RegistryKey pm = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\DisplayMagician.ContextMenus\\ContextMenus\\ProfileMenu\\Shell\\" + profile.Name);
                    if (pm != null)
                    {
                        // Set up the ProfileMenu Registry Key contents
                        pm.SetValue("MUIVerb", profile.Name);
                        pm.SetValue("Icon", Path.Combine(Program.AppProfilePath, profile.SavedProfileIconCacheFilename));
                    }
                    // Set up the ProfileMenu command

                    RegistryKey pmc = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\DisplayMagician.ContextMenus\\ContextMenus\\ProfileMenu\\Shell\\" + profile.Name + "\\command");
                    if (pmc != null)
                    {
                        pmc.SetValue(null, Path.Combine(AppContext.BaseDirectory, "DisplayMagician.exe") + " ChangeProfile " + profile.UUID);
                    }

                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        // Update the list of Display Profiles in the DesktopBackground context menu
        public static bool UpdateShortcutContextMenu()
        {
            try
            {
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree("SOFTWARE\\Classes\\DisplayMagician.ContextMenus\\ContextMenus\\ShortcutMenu\\Shell");
                }
                catch (Exception ex)
                {
                    // Ignore if the key doesn't exist
                }
                RegistryKey dp = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\DisplayMagician.ContextMenus\\ContextMenus\\ShortcutMenu\\Shell");
 
                // Create the ProfileMenu (Level 2) Profile Entry Registry Keys
                foreach (ShortcutItem shortcut in ShortcutRepository.AllShortcuts)
                {
                    if (shortcut.IsValid != ShortcutValidity.Valid) continue; // Skip invalid shortcuts (e.g. missing files)

                    RegistryKey gs = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\DisplayMagician.ContextMenus\\ContextMenus\\ShortcutMenu\\Shell\\" + shortcut.Name);
                    if (gs != null)
                    {
                        // Set up the ProfileMenu Registry Key contents
                        gs.SetValue("MUIVerb", shortcut.Name);
                        gs.SetValue("Icon", Path.Combine(Program.AppShortcutPath, shortcut.SavedShortcutIconCacheFilename));
                    }
                    // Set up the ProfileMenu command
                    RegistryKey gsc = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\DisplayMagician.ContextMenus\\ContextMenus\\ShortcutMenu\\Shell\\" + shortcut.Name + "\\command");
                    if (gsc != null)
                    {
                        gsc.SetValue(null, Path.Combine(AppContext.BaseDirectory, "DisplayMagician.exe") + " RunShortcut " + shortcut.UUID);
                    }

                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

    }
}
