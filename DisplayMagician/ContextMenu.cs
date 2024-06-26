using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DisplayMagicianShared;
using Microsoft.Win32;

namespace DisplayMagician
{
    public class ContextMenu
    {

        // Create a new DesktopBackground context menu in Windows Registry using the 
        // list of Profiles stored in the ProfileRegistry class.
        public static bool InstallContextMenu()
        {
            // Create the ContextMenu Anchor Registry Key
            Registry.ClassesRoot.CreateSubKey(@"DesktopBackground\Shell\DisplayMagician.ContextMenu.Anchor");
            using (Registry.ClassesRoot.OpenSubKey("DesktopBackground\\Shell\\DisplayMagician.ContextMenu.Anchor"))
            {
                // Set up the ContextMenu Anchor Registry Key contents
                Registry.ClassesRoot.SetValue("MUIVerb", "DisplayMagician");
                Registry.ClassesRoot.SetValue("ExtendedSubCommandsKey", "DisplayMagician.ContextMenus\\ContextMenus\\MainMenu");
                Registry.ClassesRoot.SetValue("Icon", Path.Combine(AppContext.BaseDirectory,"DisplayMagician.exe"));
            }

            // Create the MainMenu (Level 1) Registry Key
            Registry.ClassesRoot.CreateSubKey("DisplayMagician.ContextMenus\\ContextMenus\\MainMenu\\Shell\\DisplayProfiles");
            Registry.ClassesRoot.CreateSubKey("DisplayMagician.ContextMenus\\ContextMenus\\MainMenu\\Shell\\GameShortcuts");
            using (Registry.ClassesRoot.OpenSubKey("DisplayMagician.ContextMenus\\ContextMenus\\MainMenu\\Shell\\DisplayProfiles"))
            {
                // Set up the Display Profiles ContextMenu Registry Key contents
                Registry.ClassesRoot.SetValue("MUIVerb", "Display Profiles");
                Registry.ClassesRoot.SetValue("ExtendedSubCommandsKey", "DisplayMagician.ContextMenus\\ContextMenus\\ProfileMenu");
                //Registry.ClassesRoot.SetValue("Icon", AppContext.BaseDirectory + "\\DisplayMagician.exe");
            }
            using (Registry.ClassesRoot.OpenSubKey("DisplayMagician.ContextMenus\\ContextMenus\\MainMenu\\Shell\\GameShortcuts"))
            {
                // Set up the Display Profiles ContextMenu Registry Key contents
                Registry.ClassesRoot.SetValue("MUIVerb", "Game Shortcuts");
                Registry.ClassesRoot.SetValue("ExtendedSubCommandsKey", "DisplayMagician.ContextMenus\\ContextMenus\\ShortcutMenu");
                //Registry.ClassesRoot.SetValue("Icon", AppContext.BaseDirectory + "\\DisplayMagician.exe");
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
            Registry.ClassesRoot.DeleteSubKeyTree("DesktopBackground\\Shell\\DisplayMagician.ContextMenu.Anchor");

            // Delete the MainMenu (Level 1) Registry Key and all subkeys
            Registry.ClassesRoot.DeleteSubKeyTree("DisplayMagician.ContextMenus");
            return true;
        }

        // Update the list of Display Profiles in the DesktopBackground context menu
        public static bool UpdateProfileContextMenu()
        {
            // Create the ProfileMenu (Level 2) Registry Key
            if (Registry.ClassesRoot.CreateSubKey("DisplayMagician.ContextMenus\\ContextMenus\\MainMenu\\Shell\\DisplayProfiles") == null) 
            {
                // We must have already created the menu, so clear it instead
                Registry.ClassesRoot.DeleteSubKeyTree("DisplayMagician.ContextMenus\\ContextMenus\\MainMenu\\Shell\\DisplayProfiles");
                Registry.ClassesRoot.CreateSubKey("DisplayMagician.ContextMenus\\ContextMenus\\MainMenu\\Shell\\DisplayProfiles");
            }
            // Create the ProfileMenu (Level 2) Profile Entry Registry Keys
            foreach (ProfileItem profile in ProfileRepository.AllProfiles)
            {
                Registry.ClassesRoot.CreateSubKey("DisplayMagician.ContextMenus\\ContextMenus\\ProfileMenu\\Shell\\" + profile.Name);
                using (Registry.ClassesRoot.OpenSubKey("DisplayMagician.ContextMenus\\ContextMenus\\ProfileMenu\\Shell\\" + profile.Name))
                {
                    // Set up the ProfileMenu Registry Key contents
                    Registry.ClassesRoot.SetValue("MUIVerb", profile.Name);
                    Registry.ClassesRoot.SetValue("Icon", Path.Combine(Program.AppProfilePath,profile.SavedProfileIconCacheFilename));
                }
                // Set up the ProfileMenu command
                Registry.ClassesRoot.CreateSubKey("DisplayMagician.ContextMenus\\ContextMenus\\ProfileMenu\\Shell\\" + profile.Name + "\\command");
                using (Registry.ClassesRoot.OpenSubKey("DisplayMagician.ContextMenus\\ContextMenus\\ProfileMenu\\Shell\\" + profile.Name + "\\command"))
                {
                    Registry.ClassesRoot.SetValue(null, Path.Combine(AppContext.BaseDirectory, "DisplayMagician.exe") + " ChangeProfile " + profile.UUID);
                }

            }
            return true;
        }

        // Update the list of Display Profiles in the DesktopBackground context menu
        public static bool UpdateShortcutContextMenu()
        {
            // Create the ProfileMenu (Level 2) Registry Key
            if (Registry.ClassesRoot.CreateSubKey("DisplayMagician.ContextMenus\\ContextMenus\\MainMenu\\Shell\\ShortcutProfiles") == null)
            {
                // We must have already created the menu, so clear it instead
                Registry.ClassesRoot.DeleteSubKeyTree("DisplayMagician.ContextMenus\\ContextMenus\\MainMenu\\Shell\\ShortcutProfiles");
                Registry.ClassesRoot.CreateSubKey("DisplayMagician.ContextMenus\\ContextMenus\\MainMenu\\Shell\\ShortcutProfiles");
            }
            // Create the ProfileMenu (Level 2) Profile Entry Registry Keys
            foreach (ShortcutItem shortcut in ShortcutRepository.AllShortcuts)
            {
                Registry.ClassesRoot.CreateSubKey("DisplayMagician.ContextMenus\\ContextMenus\\ProfileMenu\\Shell\\" + shortcut.Name);
                using (Registry.ClassesRoot.OpenSubKey("DisplayMagician.ContextMenus\\ContextMenus\\ProfileMenu\\Shell\\" + shortcut.Name))
                {
                    // Set up the ProfileMenu Registry Key contents
                    Registry.ClassesRoot.SetValue("MUIVerb", shortcut.Name);
                    Registry.ClassesRoot.SetValue("Icon", Path.Combine(Program.AppShortcutPath, shortcut.SavedShortcutIconCacheFilename));
                }
                // Set up the ProfileMenu command
                Registry.ClassesRoot.CreateSubKey("DisplayMagician.ContextMenus\\ContextMenus\\ProfileMenu\\Shell\\" + shortcut.Name + "\\command");
                using (Registry.ClassesRoot.OpenSubKey("DisplayMagician.ContextMenus\\ContextMenus\\ProfileMenu\\Shell\\" + shortcut.Name + "\\command"))
                {
                    Registry.ClassesRoot.SetValue(null, Path.Combine(AppContext.BaseDirectory, "DisplayMagician.exe") + " RunShortcut " + shortcut.UUID);
                }

            }
            return true;
        }

    }
}
