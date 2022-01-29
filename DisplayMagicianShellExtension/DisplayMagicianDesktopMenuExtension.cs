using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32;
using System.Drawing;

namespace DisplayMagicianShellExtension
{

    [ComVisible(true)]
    [Guid("de271cd7-fa82-439f-b128-202d473bb51e")]
    [RegistrationName("DisplayMagician.ShellExtension")]
    [COMServerAssociation(AssociationType.DesktopBackground)]
    public class DisplayMagicianDesktopMenuExtension : SharpContextMenu
    {
        // Other constants that are useful
        internal static Version _version = new Version(2, 2, 0);
        internal static string AlternateAppHomePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "DisplayMagician");
        internal static string AppDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician");
        private static string AppProfileStoragePath = System.IO.Path.Combine(AppDataPath, $"Profiles");
        private static string _profileStorageJsonFileName = System.IO.Path.Combine(AppProfileStoragePath, $"DisplayProfiles_2.2.json");
        internal static string registryDisplayMagician = @"SOFTWARE\DisplayMagician";
        string DisplayMagicianFullname = "";
        string DisplayMagicianInstallDir = "";
        Process DisplayMagicianProcess = null;

        private struct ProfileData
        {
            public string UUID;
            public string Name;
            public Bitmap Bitmap;
        }

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

            List<ProfileData> profiles = new List<ProfileData>();

            if (File.Exists(_profileStorageJsonFileName))
            {
                //Logging.Log($"{_profileStorageJsonFileName} file exists");
                MatchCollection mc;
                string uuid = "";
                string profileName = "";
                string profileBitmapData = "";

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
                    }

                    
                    else if (lineToProcess.StartsWith("    \"ProfileBitmap\""))
                    {
                        //Logging.Log($"Line starts with 4 spaces and Name");
                        mc = Regex.Matches(lineToProcess, "    \"ProfileBitmap\": \"(.*)\"");
                        profileBitmapData = mc[0].Groups[1].ToString();
                        if (!String.IsNullOrEmpty(uuid) && !String.IsNullOrEmpty(profileName))
                        {
                            var bytes = Convert.FromBase64String(profileBitmapData);
                            Bitmap profileBitmap;
                            using (var ms = new MemoryStream(bytes))
                                profileBitmap = new Bitmap(Bitmap.FromStream(ms),16,16);


                            ProfileData newProfile = new ProfileData();
                            newProfile.UUID = uuid;
                            newProfile.Name = profileName;
                            if (profileBitmap is Bitmap)
                            {
                                newProfile.Bitmap = profileBitmap;
                            }
                            else
                                newProfile.Bitmap = null;

                            profiles.Add(newProfile);
                        }
                            
                    }

                }

            }

            var extensionMenu = new ToolStripMenuItem("DisplayMagician: Change display profiles...", Properties.Resources.MenuImage);
            explorerMenuStrip.Items.Add(extensionMenu);

            // Add the first menu to create a new Display Profile
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

                foreach (ProfileData profile in profiles.OrderBy(p => p.Name))
                {
                    extensionMenu.DropDownItems.Add(new ToolStripMenuItem(profile.Name, profile.Bitmap,
                        (sender, args) =>
                        {
                            DisplayMagicianProcess = Process.Start(DisplayMagicianFullname,$"ChangeProfile \"{profile.UUID}\"");
                        }
                    ));
                }
            }

            return explorerMenuStrip;
        }
    }
}