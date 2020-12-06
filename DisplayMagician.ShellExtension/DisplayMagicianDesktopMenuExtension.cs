using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using SharpShell.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32;

namespace DisplayMagician.ShellExtension
{
    [ComVisible(true)]
    [Guid("de271cd7-fa82-439f-b128-202d473bb51e")]
    [RegistrationName("DisplayMagician.ShellExtension")]
    [COMServerAssociation(AssociationType.DesktopBackground)]
    public class DisplayMagicianDesktopMenuExtension : SharpContextMenu
    {
        // Other constants that are useful
        internal static Version _version = new Version(0, 1, 0);
        internal static string AlternateAppHomePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "DisplayMagician");
        internal static string AppDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician");
        private static string AppProfileStoragePath = System.IO.Path.Combine(AppDataPath, $"Profiles");
        private static string _profileStorageJsonFileName = System.IO.Path.Combine(AppProfileStoragePath, $"DisplayProfiles_1.0.json");
        internal static string registryDisplayMagician = @"SOFTWARE\DisplayMagician";
        string DisplayMagicianFullname = "";
        string DisplayMagicianInstallDir = "";
        Process DisplayMagicianProcess = null;

        public DisplayMagicianDesktopMenuExtension()
        { }

        protected override bool CanShowMenu()
        {
            //Logging.Log($"Starting CanShowMenu");
            // Only show this menu if DisplayMagician is installed
            DisplayMagicianInstallDir = "";
            try
            {
                RegistryKey DisplayMagicianKey = Registry.LocalMachine.OpenSubKey(registryDisplayMagician, RegistryKeyPermissionCheck.ReadSubTree);
                DisplayMagicianInstallDir = DisplayMagicianKey.GetValue("InstallDir", AlternateAppHomePath).ToString();
            }
            catch (Exception)
            {
                DisplayMagicianInstallDir = AlternateAppHomePath;
            }

            DisplayMagicianFullname = Path.Combine(DisplayMagicianInstallDir, "DisplayMagician.exe");

            //Logging.Log($"DisplayMagician is installed in {DisplayMagicianFullname}");


            if (File.Exists(DisplayMagicianFullname))
            {
                //Logging.Log($"CanShowMenu is returning true (can show menu)");
                return true;
            }
            else
            {
                //Logging.Log($"CanShowMenu is returning false (cannot show menu)");
                return false;
            }   
        }

        protected override ContextMenuStrip CreateMenu()
        {
            //Logging.Log($"Starting CreateMenu");
            var explorerMenuStrip = new ContextMenuStrip();

            Dictionary<string, string> profiles = new Dictionary<string, string>();

            if (File.Exists(_profileStorageJsonFileName))
            {
                //Logging.Log($"{_profileStorageJsonFileName} file exists");
                MatchCollection mc;
                string uuid = "";
                string profileName = "";

                foreach (string aLine in File.ReadLines(_profileStorageJsonFileName, Encoding.Unicode))
                {
                    //Logging.Log($"Processing line: {_profileStorageJsonFileName}");
                    string lineToProcess = aLine;
                    if (lineToProcess.StartsWith("    \"UUID\""))
                    {
                        //Logging.Log($"Line starts with 4 spaces and UUID");
                        mc = Regex.Matches(lineToProcess, "    \"UUID\": \"(.*)\"");
                        uuid = mc[0].Groups[1].ToString();
                    }
                    else if (lineToProcess.StartsWith("    \"Name\""))
                    {
                        //Logging.Log($"Line starts with 4 spaces and Name");
                        mc = Regex.Matches(lineToProcess, "    \"Name\": \"(.*)\"");
                        profileName = mc[0].Groups[1].ToString();
                        if (!uuid.Equals(""))
                            profiles.Add(profileName, uuid);
                    }

                }

            }

            var extensionMenu = new ToolStripMenuItem("DisplayMagician: Change display profiles...", Properties.Resources.DisplayMagicianMenuImage);
            explorerMenuStrip.Items.Add(extensionMenu);

            // Add the first menu to create a new Displaay Profile
            extensionMenu.DropDownItems.Add(new ToolStripMenuItem("Create a new display profile", null,
                (sender, args) =>
                {
                    //Logging.Log(DisplayMagicianFullname + $" CreateProfile");
                    DisplayMagicianProcess = Process.Start(DisplayMagicianFullname, $"CreateProfile");
                    //Logging.Log(DisplayMagicianProcess.ToString());
                }
            ));

            if (profiles.Count > 0)
            {
                extensionMenu.DropDownItems.Add(new ToolStripSeparator());

                foreach (KeyValuePair<string, string> pair in profiles.OrderBy(key => key.Key))
                {
                    extensionMenu.DropDownItems.Add(new ToolStripMenuItem(pair.Key, null,
                        (sender, args) =>
                        {
                            //Logging.Log(DisplayMagicianFullname + $" ChangeProfile \"{pair.Value}\"");
                            DisplayMagicianProcess = Process.Start(DisplayMagicianFullname,$"ChangeProfile \"{pair.Value}\"");
                            //Logging.Log(DisplayMagicianProcess.ToString());
                        }
                    ));
                }
            }

            return explorerMenuStrip;
        }
    }
}