using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using NLog;
using WindowsAudioWrapper;
using AudioProfile = WindowsAudioWrapper.Models.AudioProfile;

namespace DisplayMagicianShared
{
    
    public enum ApplyAudioProfileResult
    {
        Successful,
        Cancelled,
        Error
    }


    public struct AudioProfileFile
    {
        public string AudioProfileFileVersion;
        public DateTime LastUpdated;
        public List<AudioProfileItem> AudioProfiles;

        public override bool Equals(object obj) => obj is AudioProfileFile other && this.Equals(other);
        public bool Equals(AudioProfileFile other)
        => AudioProfileFileVersion.Equals(other.AudioProfileFileVersion) &&
           LastUpdated.Equals(other.LastUpdated) &&
           AudioProfiles.SequenceEqual(other.AudioProfiles);
        public override int GetHashCode()
        {
            return (AudioProfileFileVersion, LastUpdated, AudioProfiles).GetHashCode();
        }

        public static bool operator ==(AudioProfileFile lhs, AudioProfileFile rhs) => lhs.Equals(rhs);

        public static bool operator !=(AudioProfileFile lhs, AudioProfileFile rhs) => !(lhs == rhs);
    }

    public static class AudioProfileRepository
    {
        #region Class Variables
        // Common items to the class
        private static List<AudioProfileItem> _allAudioProfiles = new List<AudioProfileItem>();
        private static bool _audioProfilesLoaded = false;
        private static AudioProfileItem _currentAudioProfile;
        private static WindowsAudioController _audioController = new WindowsAudioController();


        private static bool _userChangingAudioProfiles = false;

        // Other constants that are useful
        public static string AppDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician");
        public static string AppIconPath = System.IO.Path.Combine(AppDataPath, $"Icons");
        public static string AppDisplayMagicianIconFilename = System.IO.Path.Combine(AppIconPath, @"DisplayMagician.ico");
        private static readonly string AppAudioProfileStoragePath = System.IO.Path.Combine(AppDataPath, $"AudioProfiles");
        private static string _audioProfileFileVersion = "1";
        private static readonly string _audioProfileStorageJsonFileName = "AudioProfiles.json";
        private static readonly string _audioProfileStorageJsonFullFileName = System.IO.Path.Combine(AppAudioProfileStoragePath, _audioProfileStorageJsonFileName);

        #endregion

        #region Class Constructors
        static AudioProfileRepository()
        {

            try
            {
                // Create the AudioProfile Storage Path if it doesn't exist so that it's avilable for all the program
                if (!Directory.Exists(AppAudioProfileStoragePath))
                {
                    SharedLogger.logger.Debug($"AudioProfileRepository/AudioProfileRepository: Creating the AudioProfiles storage folder {AppAudioProfileStoragePath}.");
                    Directory.CreateDirectory(AppAudioProfileStoragePath);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                SharedLogger.logger.Fatal(ex, $"AudioProfileRepository/AudioProfileRepository: DisplayMagician doesn't have permissions to create the AudioProfiles storage folder {AppAudioProfileStoragePath}.");
            }
            catch (ArgumentException ex)
            {
                SharedLogger.logger.Fatal(ex, $"AudioProfileRepository/AudioProfileRepository: DisplayMagician can't create the AudioProfiles storage folder {AppAudioProfileStoragePath} due to an invalid argument.");
            }
            catch (PathTooLongException ex)
            {
                SharedLogger.logger.Fatal(ex, $"AudioProfileRepository/AudioProfileRepository: DisplayMagician can't create the AudioProfiles storage folder {AppAudioProfileStoragePath} as the path is too long.");
            }
            catch (DirectoryNotFoundException ex)
            {
                SharedLogger.logger.Fatal(ex, $"AudioProfileRepository/AudioProfileRepository: DisplayMagician can't create the AudioProfiles storage folder {AppAudioProfileStoragePath} as the parent folder isn't there.");
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Warn(ex, $"AudioProfileRepository/AudioProfileRepository: Exception creating the AudioProfiles storage folder.");
            }

        }
        #endregion

        #region Class Properties
        public static List<AudioProfileItem> AllAudioProfiles
        {
            get
            {
                if (!_audioProfilesLoaded)
                    // Load the AudioProfiles from storage if they need to be
                    LoadAudioProfiles();
                return _allAudioProfiles;
            }
        }

        public static AudioProfileItem CurrentAudioProfile
        {
            get
            {
                if (_currentAudioProfile == null)
                    UpdateActiveAudioProfile();
                return _currentAudioProfile;
            }
            set
            {
                if (value is AudioProfileItem)
                {
                    _currentAudioProfile = value;
                    // And if we have the _originalBitmap we can also save the Bitmap overlay, but only if the AudioProfileToUse is set
                    //if (_originalBitmap is Bitmap)
                    //    _shortcutBitmap = ToBitmapOverlay(_originalBitmap, AudioProfileToUse.AudioProfileTightestBitmap, 256, 256);
                }
            }
        }

        public static int AudioProfileCount
        {
            get
            {
                if (!_audioProfilesLoaded)
                    // Load the AudioProfiles from storage if they need to be
                    LoadAudioProfiles();


                return _allAudioProfiles.Count;
            }
        }

        public static string AudioProfileStorageFileName
        {
            get => _audioProfileStorageJsonFullFileName;
        }

        public static bool AudioProfilesLoaded {
            get
            {
                return _audioProfilesLoaded;
            }
            set
            {
                _audioProfilesLoaded = value;
            }
        }

        public static bool UserChangingAudioProfiles
        {
            get
            {
                return _userChangingAudioProfiles;
            }
        }

        #endregion

        #region Class Methods
        //public static bool InitialiseRepository(FORCED_VIDEO_MODE forcedVideoMode = FORCED_VIDEO_MODE.DETECT)
        public static bool InitialiseRepository()
        {
            if (!_audioProfilesLoaded)
            {
                if (!LoadAudioProfiles())
                {
                    return false;
                }
            }

            return true;
        }


        public static bool AddAudioProfile(AudioProfileItem audioProfileItem)
        {
            if (!(audioProfileItem is AudioProfileItem))
                return false;

            SharedLogger.logger.Debug($"AudioProfileRepository/AddAudioProfile: Adding audioProfile {audioProfileItem.Name} to our audioProfile repository");

            // Doublecheck if it already exists
            // Because then we just update the one that already exists
            if (!ContainsAudioProfile(audioProfileItem))
            {
                // Add the AudioProfile to the list of AudioProfiles
                _allAudioProfiles.Add(audioProfileItem);

                // Save the AudioProfiles JSON as it's different
                SaveAudioProfiles();
            }

            // Refresh the audioProfiles to see whats valid


            //Doublecheck it's been added
            if (ContainsAudioProfile(audioProfileItem))
            {
                return true;
            }
            else
                return false;

        }


        public static bool RemoveAudioProfile(AudioProfileItem audioProfileItem)
        {
            if (!(audioProfileItem is AudioProfileItem))
                return false;

            SharedLogger.logger.Debug($"AudioProfileRepository/RemoveAudioProfile: Removing audioProfile {audioProfileItem.Name} if it exists in our audioProfile repository");

            // Remove the AudioProfile from the list.
            int numRemoved = _allAudioProfiles.RemoveAll(item => item.UUID.Equals(audioProfileItem.UUID));

            if (numRemoved == 1)
            {
                SaveAudioProfiles();
                UpdateActiveAudioProfile();
                return true;
            }
            else if (numRemoved == 0)
                return false;
            else
                throw new AudioProfileRepositoryException();
        }


        public static bool RemoveAudioProfile(string audioProfileName)
        {

            if (String.IsNullOrWhiteSpace(audioProfileName))
                return false;

            SharedLogger.logger.Debug($"AudioProfileRepository/RemoveAudioProfile2: Removing audioProfile {audioProfileName} if it exists in our audioProfile repository");

            // Remove the AudioProfile from the list.
            int numRemoved = _allAudioProfiles.RemoveAll(item => item.Name.Equals(audioProfileName));

            if (numRemoved == 1)
            {
                SaveAudioProfiles();
                UpdateActiveAudioProfile();
                return true;
            }
            else if (numRemoved == 0)
                return false;
            else
                throw new AudioProfileRepositoryException();

        }

        public static bool RemoveAudioProfile(uint audioProfileId)
        {
            if (audioProfileId == 0)
                return false;

            SharedLogger.logger.Debug($"AudioProfileRepository/RemoveAudioProfile3: Removing audioProfile wih audioProfileId {audioProfileId} if it exists in our audioProfile repository");

            string audioProfileIdStr = audioProfileId.ToString();

            // Remove the AudioProfile from the list.
            int numRemoved = _allAudioProfiles.RemoveAll(item => item.UUID.Equals(audioProfileIdStr));

            if (numRemoved == 1)
            {
                SaveAudioProfiles();
                UpdateActiveAudioProfile();
                return true;
            }
            else if (numRemoved == 0)
                return false;
            else
                throw new AudioProfileRepositoryException();
        }


        public static bool ContainsAudioProfile(AudioProfileItem audioProfileItem)
        {
            if (!(audioProfileItem is AudioProfileItem))
                return false;

            SharedLogger.logger.Debug($"AudioProfileRepository/ContainsAudioProfile: Checking if our audioProfile repository contains a audioProfile called {audioProfileItem.Name}");

            foreach (AudioProfileItem testAudioProfile in _allAudioProfiles)
            {
                if (testAudioProfile.Equals(audioProfileItem))
                {
                    SharedLogger.logger.Debug($"AudioProfileRepository/ContainsAudioProfile: Our audioProfile repository does contain a audioProfile called {audioProfileItem.Name}");
                    return true;
                }
            }
            SharedLogger.logger.Debug($"AudioProfileRepository/ContainsAudioProfile: Our audioProfile repository doesn't contain a audioProfile called {audioProfileItem.Name}");
            return false;
        }

        public static bool ContainsAudioProfile(string AudioProfileNameOrId)
        {
            if (String.IsNullOrWhiteSpace(AudioProfileNameOrId))
                return false;

            SharedLogger.logger.Debug($"AudioProfileRepository/ContainsAudioProfile2: Checking if our audioProfile repository contains a audioProfile with UUID or Name {AudioProfileNameOrId}");

            if (AudioProfileItem.IsValidUUID(AudioProfileNameOrId))
                foreach (AudioProfileItem testAudioProfile in _allAudioProfiles)
                {
                    if (testAudioProfile.UUID.Equals(AudioProfileNameOrId))
                    {
                        SharedLogger.logger.Debug($"AudioProfileRepository/ContainsAudioProfile2: Our audioProfile repository does contain a audioProfile with UUID {AudioProfileNameOrId}");
                        return true;
                    }

                }
            else
                foreach (AudioProfileItem testAudioProfile in _allAudioProfiles)
                {
                    if (testAudioProfile.Name.Equals(AudioProfileNameOrId))
                    {
                        SharedLogger.logger.Debug($"AudioProfileRepository/ContainsAudioProfile2: Our audioProfile repository does contain a audioProfile with Name {AudioProfileNameOrId}");
                        return true;
                    }

                }

            SharedLogger.logger.Debug($"AudioProfileRepository/ContainsAudioProfile2: Our audioProfile repository doesn't contain a audioProfile with a UUID or Name {AudioProfileNameOrId}");
            return false;

        }

        public static bool ContainsCurrentAudioProfile(out string savedAudioProfileName)
        {
            savedAudioProfileName = "";

            if (!(_currentAudioProfile is AudioProfileItem))
            {
                return false;
            }


            SharedLogger.logger.Debug($"AudioProfileRepository/ContainsCurrentAudioProfile: Checking if our audioProfile repository contains the display audioProfile currently in use");

            foreach (AudioProfileItem testAudioProfile in _allAudioProfiles)
            {
                if (testAudioProfile.Equals(_currentAudioProfile))
                {
                    SharedLogger.logger.Debug($"AudioProfileRepository/ContainsAudioProfile: Our audioProfile repository does contain a audioProfile called {testAudioProfile.Name}");
                    savedAudioProfileName = testAudioProfile.Name;
                    return true;
                }
            }

            SharedLogger.logger.Debug($"AudioProfileRepository/ContainsCurrentAudioProfile: Our audioProfile repository doesn't contain the display audioProfile currently in use");
            return false;
        }

        public static AudioProfileItem GetAudioProfile(string AudioProfileNameOrId)
        {

            SharedLogger.logger.Debug($"AudioProfileRepository/GetAudioProfile: Finding and returning {AudioProfileNameOrId} if it exists in our audioProfile repository");

            if (String.IsNullOrWhiteSpace(AudioProfileNameOrId))
            {
                SharedLogger.logger.Error($"AudioProfileRepository/GetAudioProfile: AudioProfile to get was empty or only whitespace");
                return null;
            }


            if (AudioProfileItem.IsValidUUID(AudioProfileNameOrId))
                foreach (AudioProfileItem testAudioProfile in _allAudioProfiles)
                {
                    if (testAudioProfile.UUID.Equals(AudioProfileNameOrId))
                    {
                        SharedLogger.logger.Debug($"AudioProfileRepository/GetAudioProfile: Returning audioProfile with UUID {AudioProfileNameOrId}");
                        return testAudioProfile;
                    }

                }
            else
                foreach (AudioProfileItem testAudioProfile in _allAudioProfiles)
                {
                    if (testAudioProfile.Name.Equals(AudioProfileNameOrId))
                    {
                        SharedLogger.logger.Debug($"AudioProfileRepository/GetAudioProfile: Returning audioProfile with Name {AudioProfileNameOrId}");
                        return testAudioProfile;
                    }

                }

            SharedLogger.logger.Debug($"AudioProfileRepository/GetAudioProfile: Didn't match any audioProfiles with UUD or Name {AudioProfileNameOrId}");
            return null;
        }

        public static string GetAudioProfileName(string AudioProfileNameOrId)
        {

            SharedLogger.logger.Debug($"AudioProfileRepository/GetAudioProfileName: Finding and returning {AudioProfileNameOrId} if it exists in our audioProfile repository");

            if (String.IsNullOrWhiteSpace(AudioProfileNameOrId))
            {
                SharedLogger.logger.Error($"AudioProfileRepository/GetAudioProfileName: AudioProfile to get was empty or only whitespace");
                return string.Empty;
            }


            if (AudioProfileItem.IsValidUUID(AudioProfileNameOrId))
                foreach (AudioProfileItem testAudioProfile in _allAudioProfiles)
                {
                    if (testAudioProfile.UUID.Equals(AudioProfileNameOrId))
                    {
                        SharedLogger.logger.Debug($"AudioProfileRepository/GetAudioProfileName: Returning audioProfile name '{testAudioProfile.Name}' with UUID {AudioProfileNameOrId}");
                        return testAudioProfile.Name;
                    }

                }
            else
                foreach (AudioProfileItem testAudioProfile in _allAudioProfiles)
                {
                    if (testAudioProfile.Name.Equals(AudioProfileNameOrId))
                    {
                        SharedLogger.logger.Debug($"AudioProfileRepository/GetAudioProfileName: Returning audioProfile name '{testAudioProfile.Name}' with Name {AudioProfileNameOrId}");
                        return testAudioProfile.Name;
                    }

                }

            SharedLogger.logger.Debug($"AudioProfileRepository/GetAudioProfileName: Didn't match any audioProfiles with UUD or Name {AudioProfileNameOrId}");
            return string.Empty;
        }

        public static bool RenameAudioProfile(AudioProfileItem audioProfile, string renamedName)
        {
            if (!(audioProfile is AudioProfileItem))
            {
                SharedLogger.logger.Error($"AudioProfileRepository/RenameAudioProfile: AudioProfile to rename was empty or only whitespace");
                return false;
            }


            SharedLogger.logger.Debug($"AudioProfileRepository/RenameAudioProfile: Attempting to rename audioProfile {audioProfile.Name} to {renamedName}");

            if (!IsValidFilename(renamedName))
            {
                SharedLogger.logger.Error($"AudioProfileRepository/RenameAudioProfile: The name the user wanted to renamed to audioProfile to is not a valid filename");
                return false;
            }

            string oldAudioProfileName = audioProfile.Name;
            audioProfile.Name = GetValidFilename(renamedName);


            // If it's been added to the list of AllAudioProfiles
            // then we also need to reproduce the Icons
            if (ContainsAudioProfile(audioProfile))
            {
                // Save the AudioProfiles JSON as it's different now
                SaveAudioProfiles();
                SharedLogger.logger.Debug($"AudioProfileRepository/RenameAudioProfile: The audioProfile was successfully renamed from {oldAudioProfileName} to {renamedName}");
                return true;
            }
            else
            {
                SharedLogger.logger.Debug($"AudioProfileRepository/RenameAudioProfile: The audioProfile was not renamed from {oldAudioProfileName} to {renamedName}");
                return false;
            }
        }

        public static void UpdateActiveAudioProfile()
        {

            SharedLogger.logger.Debug($"AudioProfileRepository/UpdateActiveAudioProfile: Updating the audioProfile currently active (in use now).");

            AudioProfileItem audioProfile;
            SharedLogger.logger.Debug($"AudioProfileRepository/UpdateActiveAudioProfile: Attempting to access configuration through NVIDIA, then AMD, then Windows CCD interfaces, in that order.");
            audioProfile = new AudioProfileItem();            

            if (_audioProfilesLoaded && _allAudioProfiles.Count > 0)
            {

                foreach (AudioProfileItem loadedAudioProfile in AudioProfileRepository.AllAudioProfiles)
                {
                    if (loadedAudioProfile.Equals(audioProfile))
                    {
                        _currentAudioProfile = loadedAudioProfile;
                        SharedLogger.logger.Debug($"AudioProfileRepository/UpdateActiveAudioProfile: The Audio Profile '{loadedAudioProfile.Name}' is currently active (in use now).");
                        return;
                    }
                }
            }
            SharedLogger.logger.Debug($"AudioProfileRepository/UpdateActiveAudioProfile: The current Audio Profile is a new Audio Profile that doesn't already exist in the Audio Profile Repository.");
            _currentAudioProfile = audioProfile;
        }

        public static AudioProfileItem GetActiveAudioProfile()
        {
            if (!(_currentAudioProfile is AudioProfileItem))
                return null;

            SharedLogger.logger.Debug($"AudioProfileRepository/GetActiveAudioProfile: Retrieving the currently active Audio Profile.");

            return _currentAudioProfile;
        }

        public static bool IsActiveAudioProfile(AudioProfileItem audioProfile)
        {
            if (audioProfile == null)
            {
                SharedLogger.logger.Error($"AudioProfileRepository/IsActiveAudioProfile: The requested Audio Profile is null. Not changing anything, and reporting an error");
                return false;
            }

            SharedLogger.logger.Trace($"AudioProfileRepository/IsActiveAudioProfile: Checking whether the Audio Profile {audioProfile.Name} is the currently active Audio Profile.");
            if (_currentAudioProfile == null)
            {
                SharedLogger.logger.Error($"AudioProfileRepository/IsActiveAudioProfile: The current Audio Profile is null, so can't test it against anything.");
                return false;
            }

            if (audioProfile.Equals(_currentAudioProfile))
            {
                SharedLogger.logger.Debug($"AudioProfileRepository/IsActiveAudioProfile: The Audio Profile {audioProfile.Name} is the currently active Audio Profile.");
                return true;
            }
            else
            {
                SharedLogger.logger.Debug($"AudioProfileRepository/IsActiveAudioProfile: The Audio Profile {audioProfile.Name} is not the currently active Audio Profile.");
                return false;
            }
        }


        private static bool LoadAudioProfiles()
        {
            SharedLogger.logger.Debug($"AudioProfileRepository/LoadAudioProfiles: Loading Audio Profiles from {_audioProfileStorageJsonFullFileName} into the Audio Profile Repository");

            _audioProfilesLoaded = false;

            if (File.Exists(_audioProfileStorageJsonFullFileName))
            {
                string json = "";
                try
                {
                    json = File.ReadAllText(_audioProfileStorageJsonFullFileName, Encoding.Unicode);
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"AudioProfileRepository/LoadAudioProfiles: Tried to read the JSON file {_audioProfileStorageJsonFullFileName} to memory but File.ReadAllTextthrew an exception.");
                }

                // Temporarily removing as not needed at present. May need this for future format migrations.
                // Migrate any previous entries to the latest version of the file format to the latest one
                //json = MigrateJsonToLatestVersion(json);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    List<string> jsonErrors = new List<string>();

                    try
                    {
                        JsonSerializerSettings mySerializerSettings = new JsonSerializerSettings
                        {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Include,
                            DefaultValueHandling = DefaultValueHandling.Populate,
                            TypeNameHandling = TypeNameHandling.Auto,
                            ObjectCreationHandling = ObjectCreationHandling.Replace,
                            Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                            {
                                jsonErrors.Add($"JSON.net Error: {args.ErrorContext.Error.Source}:{args.ErrorContext.Error.StackTrace} - {args.ErrorContext.Error.Message} | InnerException:{args.ErrorContext.Error.InnerException?.Source}:{args.ErrorContext.Error.InnerException?.StackTrace} - {args.ErrorContext.Error.InnerException?.Message}");
                                args.ErrorContext.Handled = true;
                            },
                        };

                        AudioProfileFile audioProfileFile = JsonConvert.DeserializeObject<AudioProfileFile>(json, mySerializerSettings);

                        if (audioProfileFile.AudioProfiles == null)
                        {
                            throw new Exception("AudioProfileRepository/LoadAudioProfiles: The Audio Profiles file is null.");
                        }

                        _allAudioProfiles = audioProfileFile.AudioProfiles;
                    }
                    catch (JsonReaderException ex)
                    {
                        // If there is a error in the JSON format
                        if (ex.HResult == -2146233088)
                        {
                            SharedLogger.logger.Error(ex, $"AudioProfileRepository/LoadAudioProfiles: JSONReaderException - The Audio Profiles file {_audioProfileStorageJsonFullFileName} contains a syntax error. Please check the file for correctness with a JSON validator.");
                        }
                        else
                        {
                            SharedLogger.logger.Error(ex, $"AudioProfileRepository/LoadAudioProfiles: JSONReaderException while trying to process the Audio Profiles json data file {_audioProfileStorageJsonFullFileName} but JsonConvert threw an exception.");
                        }
                        MessageBox.Show($"The Audio Profiles file {_audioProfileStorageJsonFullFileName} contains a syntax error. Please check the file for correctness with a JSON validator.", "Error loading the Audio Profiles", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }
                    catch (Exception)
                    {
                        SharedLogger.logger.Error($"AudioProfileRepository/LoadAudioProfiles: Exception while trying to process the Audio Profiles json data file {_audioProfileStorageJsonFullFileName} but JsonConvert threw an exception.");
                    }


                    // If we have any JSON.net errors, then we need to records them in the logs
                    if (jsonErrors.Count > 0)
                    {
                        foreach (string jsonError in jsonErrors)
                        {
                            SharedLogger.logger.Error($"AudioProfileRepository/LoadAudioProfiles: {jsonError}");
                        }
                    }

                    // Sort the audioProfiles alphabetically
                    _allAudioProfiles.Sort();

                }
                else
                {
                    SharedLogger.logger.Debug($"AudioProfileRepository/LoadAudioProfiles: The {_audioProfileStorageJsonFullFileName} Audio Profile JSON file exists but is empty! So we're going to treat it as if it didn't exist.");
                }
            }
            else
            {
                // If we get here, then we don't have any Audio Profiles saved!
                // So we gotta start from scratch
                SharedLogger.logger.Debug($"AudioProfileRepository/LoadAudioProfiles: Couldn't find the {_audioProfileStorageJsonFullFileName} Audio Profile JSON file that contains the Audio Profiles. This is likely due to none being saved yet.");
            }
            _audioProfilesLoaded = true;


            return true;
        }

        public static bool SaveAudioProfiles()
        {
            SharedLogger.logger.Debug($"AudioProfileRepository/SaveAudioProfiles: Attempting to save the Audio Profiles repository to the {AppAudioProfileStoragePath}.");

            if (!Directory.Exists(AppAudioProfileStoragePath))
            {
                try
                {
                    Directory.CreateDirectory(AppAudioProfileStoragePath);
                }
                catch (UnauthorizedAccessException ex)
                {
                    SharedLogger.logger.Fatal(ex, $"AudioProfileRepository/SaveAudioProfiles: DisplayMagician doesn't have permissions to create the Audio Profiles storage folder {AppAudioProfileStoragePath}.");
                }
                catch (ArgumentException ex)
                {
                SharedLogger.logger.Fatal(ex, $"AudioProfileRepository/SaveAudioProfiles: DisplayMagician can't create the Audio Profiles storage folder {AppAudioProfileStoragePath} due to an invalid argument.");
                }
                catch (PathTooLongException ex)
                {
                    SharedLogger.logger.Fatal(ex, $"AudioProfileRepository/SaveAudioProfiles: DisplayMagician can't create the AudioProfiles storage folder {AppAudioProfileStoragePath} as the path is too long.");
                }
                catch (DirectoryNotFoundException ex)
                {
                    SharedLogger.logger.Fatal(ex, $"AudioProfileRepository/SaveAudioProfiles: DisplayMagician can't create the Audio Profiles storage folder {AppAudioProfileStoragePath} as the parent folder isn't there.");
                }
            }
            else
            {
                SharedLogger.logger.Debug($"AudioProfileRepository/SaveAudioProfiles: Audio Profiles folder {AppAudioProfileStoragePath} exists.");
            }

            // Sort the _allAudioProfile so that the display audioProfiles are in name order in the saved file
            _allAudioProfiles.Sort();

            List<string> jsonErrors = new List<string>();
            List<AudioProfileRepositoryException> errors = new List<AudioProfileRepositoryException>();

            try
            {
                SharedLogger.logger.Debug($"AudioProfileRepository/SaveAudioProfiles: Converting the objects to JSON format.");

                JsonSerializerSettings mySerializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    TypeNameHandling = TypeNameHandling.Auto,
                    MissingMemberHandling = MissingMemberHandling.Error,
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                    {
                        jsonErrors.Add($"JSON.net Error: {args.ErrorContext.Error.Source}:{args.ErrorContext.Error.StackTrace} - {args.ErrorContext.Error.Message} | InnerException:{args.ErrorContext.Error.InnerException?.Source}:{args.ErrorContext.Error.InnerException?.StackTrace} - {args.ErrorContext.Error.InnerException?.Message}");
                        //errors.Add(new AudioProfileRepositoryException(String.Format("Parse error: {0}", args.ErrorContext.Error.Message), args.ErrorContext.Error));
                        args.ErrorContext.Handled = true;
                    },
                };

                AudioProfileFile audioProfileFile = new AudioProfileFile
                {
                    AudioProfileFileVersion = _audioProfileFileVersion,
                    LastUpdated = DateTime.Now,
                    AudioProfiles = _allAudioProfiles
                };

                var json = JsonConvert.SerializeObject(audioProfileFile, Formatting.Indented, mySerializerSettings);

                // If we have any JSON.net errors, then we need to record them in the logs
                if (jsonErrors.Count > 0)
                {
                    foreach (string jsonError in jsonErrors)
                    {
                        SharedLogger.logger.Error($"AudioProfileRepository/SaveAudioProfiles: {jsonError}");
                    }

                    SharedLogger.logger.Error($"AudioProfileRepository/SaveAudioProfiles: JSON data: {json}");
                }


                if (!string.IsNullOrWhiteSpace(json))
                {
                    SharedLogger.logger.Debug($"AudioProfileRepository/SaveAudioProfiles: Saving the audioProfile repository to the {_audioProfileStorageJsonFullFileName}.");

                    File.WriteAllText(_audioProfileStorageJsonFullFileName, json, Encoding.Unicode);
                    if (ValidateAudioProfiles())
                    {
                        SharedLogger.logger.Debug($"AudioProfileRepository/SaveAudioProfiles: Validated that we successfully saved the Audio Profiles repository to {_audioProfileStorageJsonFullFileName}.");
                        return true;
                    }
                    else
                    {
                        SharedLogger.logger.Error($"AudioProfileRepository/SaveAudioProfiles: Validatation of saving the Audio Profiles repository to {_audioProfileStorageJsonFullFileName} failed. The Audio Profiles repository was unable to be saved the first time. Attempting to save again.");

                        // Waiting a second to let any transient issue pass.
                        Thread.Sleep(1000);

                        SharedLogger.logger.Debug($"AudioProfileRepository/SaveAudioProfiles: Saving the Audio Profiles repository to the {_audioProfileStorageJsonFullFileName} for a second time.");

                        File.WriteAllText(_audioProfileStorageJsonFullFileName, json, Encoding.Unicode);

                        if (ValidateAudioProfiles())
                        {
                            SharedLogger.logger.Debug($"AudioProfileRepository/SaveAudioProfiles: Validated that we successfully saved the Audio Profiles repository to {_audioProfileStorageJsonFullFileName} on the second try.");
                            return true;
                        }
                        else
                        {
                            SharedLogger.logger.Error($"AudioProfileRepository/SaveAudioProfiles: Validatation of saving the Audio Profiles repository to {_audioProfileStorageJsonFullFileName} a second time failed. The Audio Profiles repository was unable to be saved twice. There is an underlying issue here.");
                            return false;
                        }
                    }

                }
                else
                {
                    SharedLogger.logger.Error($"AudioProfileRepository/SaveAudioProfiles: Problem saving the Audio Profiles repository to {_audioProfileStorageJsonFullFileName} as the JSON file contents are null or whitespace.");
                    SharedLogger.logger.Error($"AudioProfileRepository/SaveAudioProfiles: JSON data: {json}");
                    return false;
                }

            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"AudioProfileRepository/SaveAudioProfiles: Unable to save the Audio Profiles repository to the {_audioProfileStorageJsonFullFileName}.");
                SharedLogger.logger.Error(ex, $"AudioProfileRepository/SaveAudioProfiles: JSON.net Error: {ex.Source}:{ex.StackTrace} - {ex.Message} | InnerException:{ex.InnerException?.Source}:{ex.InnerException?.StackTrace} - {ex.InnerException?.Message}\"");
                return false;
            }
        }


        private static bool ValidateAudioProfiles()
        {
            SharedLogger.logger.Debug($"AudioProfileRepository/ValidateAudioProfiles: Loading Audio Profiles from {_audioProfileStorageJsonFullFileName} to compare the Audio Profiles repository");

            try
            {
                if (File.Exists(_audioProfileStorageJsonFullFileName))
                {
                    List<AudioProfileItem> audioProfilesToValidate = new List<AudioProfileItem>(); ;

                    string json = "";
                    try
                    {
                        json = File.ReadAllText(_audioProfileStorageJsonFullFileName, Encoding.Unicode);
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"AudioProfileRepository/ValidateAudioProfiles: Tried to read the JSON file {_audioProfileStorageJsonFullFileName} to memory but File.ReadAllTextthrew an exception.");
                    }

                    // Temporarily removing as not needed at present. May need this for future format migrations.
                    // Migrate any previous entries to the latest version of the file format to the latest one
                    //json = MigrateJsonToLatestVersion(json);

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        List<string> jsonErrors = new List<string>();

                        try
                        {
                            JsonSerializerSettings mySerializerSettings = new JsonSerializerSettings
                            {
                                MissingMemberHandling = MissingMemberHandling.Ignore,
                                NullValueHandling = NullValueHandling.Include,
                                DefaultValueHandling = DefaultValueHandling.Populate,
                                TypeNameHandling = TypeNameHandling.Auto,
                                ObjectCreationHandling = ObjectCreationHandling.Replace,
                                Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                                {
                                    jsonErrors.Add($"JSON.net Error: {args.ErrorContext.Error.Source}:{args.ErrorContext.Error.StackTrace} - {args.ErrorContext.Error.Message} | InnerException:{args.ErrorContext.Error.InnerException?.Source}:{args.ErrorContext.Error.InnerException?.StackTrace} - {args.ErrorContext.Error.InnerException?.Message}");
                                    args.ErrorContext.Handled = true;
                                },
                            };

                            AudioProfileFile audioProfilesFile = JsonConvert.DeserializeObject<AudioProfileFile>(json, mySerializerSettings);
                            audioProfilesToValidate = audioProfilesFile.AudioProfiles;                        

                        }
                        catch (JsonReaderException ex)
                        {
                            // If there is a error in the JSON format
                            if (ex.HResult == -2146233088)
                            {
                                SharedLogger.logger.Error(ex, $"AudioProfileRepository/ValidateAudioProfiles: JSONReaderException - The Display AudioProfiles file {_audioProfileStorageJsonFullFileName} contains a syntax error. Please check the file for correctness with a JSON validator.");
                            }
                            else
                            {
                                SharedLogger.logger.Error(ex, $"AudioProfileRepository/ValidateAudioProfiles: JSONReaderException while trying to process the AudioProfiles json data file {_audioProfileStorageJsonFullFileName} but JsonConvert threw an exception.");
                            }
                            return false;
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"AudioProfileRepository/ValidateAudioProfiles: Tried to parse the JSON in the {_audioProfileStorageJsonFullFileName} but the JsonConvert threw an exception.");
                            return false;
                        }

                        // If we have any JSON.net errors, then we need to records them in the logs
                        if (jsonErrors.Count > 0)
                        {
                            foreach (string jsonError in jsonErrors)
                            {
                                SharedLogger.logger.Error($"AudioProfileRepository/ValidateAudioProfiles: {jsonError}");
                            }
                        }

                        // Sort the audioProfiles alphabetically so they match the loaded audioProfiles
                        audioProfilesToValidate.Sort();

                        // Actually perform the validation
                        if (audioProfilesToValidate.SequenceEqual(_allAudioProfiles))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    }
                    else
                    {
                        if (_audioProfilesLoaded && _allAudioProfiles.Count > 0)
                        {
                            // We don't have a audioProfile repository file, yet we have some audioProfiles. This means the file and audioProfiles don't match. Return false.
                            SharedLogger.logger.Debug($"AudioProfileRepository/ValidateAudioProfiles: The {_audioProfileStorageJsonFullFileName} audioProfile JSON file exists but is empty! We don't have a audioProfile repository file, yet we have some display audioProfiles. This means the file and audioProfiles don't match.");
                            return false;
                        }
                        else
                        {
                            // We don't have a audioProfile repository file, and we don't have any audioProfiles. This means the file and audioProfiles match. Return true.
                            SharedLogger.logger.Debug($"AudioProfileRepository/ValidateAudioProfiles: The {_audioProfileStorageJsonFullFileName} audioProfile JSON file exists but is empty! We also don't have any display audioProfiles, so that matches. This is expected.");
                            return true;
                        }
                    }
                }
                else
                {
                    if (_audioProfilesLoaded && _allAudioProfiles.Count > 0)
                    {
                        // We don't have a audioProfile repository file, yet we have some audioProfiles. This means the file and audioProfiles don't match. Return false.
                        SharedLogger.logger.Debug($"AudioProfileRepository/ValidateAudioProfiles: Couldn't find the {_audioProfileStorageJsonFullFileName} audioProfile JSON file that contains the AudioProfiles. We don't have a audioProfile repository file, yet we have some display audioProfiles. This means the file and audioProfiles don't match.");
                        return false;
                    }
                    else
                    {
                        // We don't have a audioProfile repository file, and we don't have any audioProfiles. This means the file and audioProfiles match. Return true.
                        SharedLogger.logger.Debug($"AudioProfileRepository/ValidateAudioProfiles: Couldn't find the {_audioProfileStorageJsonFullFileName} audioProfile JSON file that contains the AudioProfiles. We also don't have any display audioProfiles, so that matches. This is expected.");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"AudioProfileRepository/ValidateAudioProfiles: Exception within ValidateAudioProfiles function - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                return false;
            }            
        }

        public static bool IsValidFilename(string testName)
        {
            SharedLogger.logger.Trace($"AudioProfileRepository/IsValidFilename: Checking whether {testName} is a valid filename");
            string strTheseAreInvalidFileNameChars = new string(System.IO.Path.GetInvalidFileNameChars());
            Regex regInvalidFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

            if (regInvalidFileName.IsMatch(testName)) {
                SharedLogger.logger.Trace($"AudioProfileRepository/IsValidFilename: {testName} isn't a valid filename as it contains one of these characters [" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");
                return false;
            }
            else
            {
                SharedLogger.logger.Debug($"AudioProfileRepository/IsValidFilename: {testName} is a valid filename");
                return true;
            }
        }

        public static string GetValidFilename(string uncheckedFilename)
        {
            SharedLogger.logger.Trace($"AudioProfileRepository/GetValidFilename: Modifying filename {uncheckedFilename} to be a valid filename for this filesystem");
            string invalid = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
            foreach (char c in invalid)
            {
                uncheckedFilename = uncheckedFilename.Replace(c.ToString(), "");
            }
            SharedLogger.logger.Trace($"AudioProfileRepository/GetValidFilename: Modified filename {uncheckedFilename} so it is a valid filename for this filesystem");
            return uncheckedFilename;
        }

        // ApplyAudioProfile lives here so that the UI works.
        public static ApplyAudioProfileResult ApplyAudioProfile(AudioProfileItem audioProfile)
        {
            SharedLogger.logger.Trace($"Program/ApplyAudioProfile: Starting");
            // We try to time the audioProfile display swap
            Stopwatch stopWatch = new Stopwatch();
            ApplyAudioProfileResult result = ApplyAudioProfileResult.Successful;

            if (audioProfile == null)
            {
                SharedLogger.logger.Debug($"AudioProfileRepository/ApplyAudioProfile: The supplied audioProfile is null! Can't be used.");
                return ApplyAudioProfileResult.Error;
            }

            try
            {
                // We start the timer just before we attempt the display change
                stopWatch.Start();

                // We also set the variable that the user is changing audioProfiles
                _userChangingAudioProfiles = true;

                if (!(audioProfile.SetActive()))
                {
                    SharedLogger.logger.Error($"AudioProfileRepository/ApplyAudioProfile: Error applying the {audioProfile.Name} AudioProfile!");
                    result = ApplyAudioProfileResult.Error;
                }
                else
                {
                    SharedLogger.logger.Trace($"AudioProfileRepository/ApplyAudioProfile: Successfully applied the  {audioProfile.Name} AudioProfile!");
                    result = ApplyAudioProfileResult.Successful;
                }

                if (audioProfile.ApplyProfileDelay > 0 && audioProfile.ApplyProfileDelay <= 1000)
                {
                    // we have more than one audioProfile attempt to go, so delay the requested amount, converting seconds to milliseconds.
                    // Note - using Thread.Sleep instead of Task.Delay, as this is not a UI thread and we want to delay this thread.
                    Thread.Sleep(audioProfile.ApplyProfileDelay * 1000);
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"AudioProfileRepository/ApplyAudioProfile: Failed to complete changing the Windows Display layout");
                result = ApplyAudioProfileResult.Error;
            }
            finally
            {
                // We stop the stop watch
                stopWatch.Stop();
                // We unset the variable that the user is changing audioProfiles
                _userChangingAudioProfiles = false;
                // Pause for a bit to let things settle
                Thread.Sleep(200);
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = stopWatch.Elapsed;
                string resultString = "failed";
                if (result == ApplyAudioProfileResult.Successful)
                {
                    resultString = "was successful";
                    AudioProfileRepository.UpdateActiveAudioProfile();

                }
                // Display the TimeSpan time and result.
                SharedLogger.logger.Debug($"AudioProfileRepository/ApplyAudioProfile: Display change attempt took {ts.Minutes}:{ts.Seconds}.{ts.Milliseconds} and {resultString}.");
            }
            return result;
        }

        #endregion

    }


    [global::System.Serializable]
    public class AudioProfileRepositoryException : Exception
    {
        public AudioProfileRepositoryException() { }
        public AudioProfileRepositoryException(string message) : base(message) { }
        public AudioProfileRepositoryException(string message, Exception inner) : base(message, inner) { }
    }

}

