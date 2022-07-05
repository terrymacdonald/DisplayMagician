using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using DisplayMagician;
using Newtonsoft.Json;

namespace DisplayMagician.AppLibraries
{
    public enum SupportedAppLibraryType
    {
        Unknown = 0,
        Local = 1,
    }

    public class AppLibrary
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Class Properties

        public static List<App> AllInstalledAppsInAllLibraries { get; set; }
        public static bool AppsLoaded { get; set; } = false;

        public static bool AppImagesLoaded { get; set; } = false;

        public virtual List<App> AllInstalledApps { get; set; }

        public virtual int InstalledAppCount { get; set; }

        public virtual string AppLibraryName { get; set; }

        public virtual SupportedAppLibraryType AppLibraryType { get; set; }
        
        public virtual string AppLibraryExe { get; set; }

        public virtual string AppLibraryPath { get; set; }

        public virtual bool IsAppLibraryInstalled { get; set; }

        public virtual bool IsRunning { get; set; }

        public virtual bool IsUpdating { get; set; }

        public virtual List<string> AppLibraryProcesses { get; set; }
        #endregion

        #region Class Methods
        public virtual bool AddApp(App App)
        {
            return false;
        }

        public virtual bool RemoveApp(App App)
        {
            return false;
        }

        public virtual bool RemoveAppById(string AppId)
        {
            return false;
        }

        public virtual bool RemoveApp(string AppNameOrId)
        {
            return false;
        }

        public virtual bool ContainsApp(App App)
        {
            return false;
        }

        public virtual bool ContainsAppById(string AppId)
        {
            return false;
        }

        public virtual bool ContainsApp(string AppNameOrId)
        {
            return false;
        }


        public virtual App GetApp(string AppNameOrId)
        {
            return null;
        }

        public virtual App GetAppById(string AppId)
        {
            return null;
        }

        public virtual bool LoadInstalledApps()
        {           
            return false;
        }

        public virtual List<Process> StartApp(App App, string AppArguments = "", ProcessPriority processPriority = ProcessPriority.Normal)
        {
            return null;
        }

        public static bool LoadAppsInBackground()
        {

            logger.Trace($"AppLibrary/LoadAppsInBackground: Attempting to load Apps from detected App libraries.");

            // Clear the App libraries in case this is a refresh
            LocalLibrary localLibrary = LocalLibrary.GetLibrary();
            localLibrary.AllInstalledApps.Clear();

            // Now lets prepare loading all the Local Apps we have installed
            Action loadLocalAppsAction = new Action(() =>
            {
                // Check wht local apps are installed
                //LocalLibrary localLibrary = LocalLibrary.GetLibrary();
                if (localLibrary.IsAppLibraryInstalled)
                {
                    // Load local library Apps
                    logger.Info($"AppLibrary/LoadAppsInBackground: Loading Locally Installed Apps");
                    if (!localLibrary.LoadInstalledApps())
                    {
                        logger.Info($"AppLibrary/LoadAppsInBackground: Cannot load Locally Installed Apps!");
                    }
                    logger.Info($"AppLibrary/LoadAppsInBackground: Loaded all Locally Installed Apps (found {localLibrary.InstalledAppCount})");
                }
                else
                {
                    logger.Info($"AppLibrary/LoadAppsInBackground: Locally installed app library not installed.");
                    Console.WriteLine("Locally installed app not installed.");
                }
            });

            
            // Store all the actions in a array so we can wait on them later
            List<Action> loadAppsActions = new List<Action>();
            loadAppsActions.Add(loadLocalAppsAction);

            try
            {
                logger.Debug($"AppLibrary/LoadAppsInBackground: Running App loading actions.");
                // Go through and start all the actions, making sure we only have one threat per action to avoid thread issues
                int threads = loadAppsActions.Count;
                ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = threads };
                Parallel.Invoke(options, loadAppsActions.ToArray());
                // Once we get here , we know that all the parallel actions have returned
                logger.Debug($"AppLibrary/LoadAppsInBackground: All App loading tasks finished");
            }
            catch (Exception ae)
            {
                logger.Error(ae, $"AppLibrary/LoadAppsInBackground: One or more exception during execution of loadAppsActions");
            }
            
            // Produce a single array of Apps we can reference later
            AppLibrary.AllInstalledAppsInAllLibraries = new List<App>();
            AppLibrary.AllInstalledAppsInAllLibraries.AddRange(localLibrary.AllInstalledApps);

            AppsLoaded = true;

            return true;
        }

        public static void RefreshAppBitmaps()
        {
            // Create App Bitmaps from the Apps so the rest of the program is faster later
            // Get the bitmap out of the IconPath 
            // IconPath can be an ICO, or an EXE
            foreach (var App in AppLibrary.AllInstalledAppsInAllLibraries)
            {
                List<ShortcutBitmap> bmList = new List<ShortcutBitmap>();
                try
                {
                    /*ArrayList filesToSearchForIcon = new ArrayList();
                    filesToSearchForIcon.Add(App.ExePath);
                    if (App.IconPath != App.ExePath)
                        filesToSearchForIcon.Add(App.IconPath);

                    bm = ImageUtils.GetMeABitmapFromFile(filesToSearchForIcon);*/

                    // We only want the icon location that the AppLibrary told us to use
                    // Note: This may be an icon file, or an exe file.
                    // This function tries to get a 256x256 Vista sized bitmap from the file
                    logger.Trace($"Program/LoadAppsInBackground: Attempting to get App bitmaps from {App.Name}.");
                    bmList.AddRange(ImageUtils.GetMeAllBitmapsFromFile(App.IconPath));
                    if (App.ExePath != App.IconPath)
                    {
                        bmList.AddRange(ImageUtils.GetMeAllBitmapsFromFile(App.ExePath));
                    }
                    logger.Trace($"Program/LoadAppsInBackground: Got App bitmaps from {App.Name}.");

                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Program/LoadAppsInBackground: Exception building App bitmaps for {App.Name} during load");
                }

                if (bmList.Count == 0)
                {
                    ShortcutBitmap bm = new ShortcutBitmap();
                    if (App.AppLibrary.Equals(SupportedAppLibraryType.Local))
                    {
                        bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.Steam, "Steam Icon", App.ExePath, bmList.Count);
                    }
                    else
                    {
                        bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.DisplayMagician.ToBitmap(), "DisplayMagician Icon", App.ExePath, bmList.Count);
                    }
                    // Add the shortcutbitmap to the list
                    bmList.Add(bm);

                }

                App.AvailableAppBitmaps = bmList;
                App.AppBitmap = ImageUtils.GetMeLargestAvailableBitmap(bmList);
            }
            AppImagesLoaded = true;
        }



        #endregion

    }

    [global::System.Serializable]
    public class AppLibraryException : Exception
    {
        public AppLibraryException() { }
        public AppLibraryException(string message) : base(message) { }
        public AppLibraryException(string message, Exception inner) : base(message, inner) { }
        protected AppLibraryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
