using Newtonsoft.Json;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using WindowsAudioWrapper;
using AudioProfile = WindowsAudioWrapper.Models.AudioProfile;

namespace DisplayMagicianShared
{

    public class AudioProfileItem : IComparable<AudioProfileItem>, IEquatable<AudioProfileItem>
    {
        private AudioProfile _windowsAudioConfig;

        private int _applyProfileDelay = 0;

        internal static string AppDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician");
        private static readonly string uuidV4Regex = @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$";

        public static string SkipAudioProfilesChangeName = "No Change";
        public static string SkipAudioProfilesChangeUUID = "00000000-0000-4000-8000-000000000000";


        private string _uuid = "";
        private bool _isPossible = false;

        public AudioProfileItem()
        {
            // Create a default audioProfile Name to avoid null exceptions
            Name = "Current Windows Audio Profile";
            
            // Fill out a new NVIDIA and AMD object when a audioProfile is being created
            // so that it will save correctly. Json.NET will save null references by default
            // unless we fill them up first, and that in turn causes NullReference errors when
            // loading the DisplayProfiles_2.0.json into DisplayMagician next time.
            // We cannot make the structs themselves create the default entry, so instead, we 
            // make each library create the default.
            try
            {
                using (WindowsAudioController controller = new WindowsAudioController())
                {
                 _windowsAudioConfig =  controller.GetCurrentProfile();   
                }                                
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex,$"AudioProfileItem/AudioProfileItem: Exception getting the default configuration from WindowsAudioController - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
            }
        }

        public static Version Version = new Version(1, 1);

        #region Instance Properties

        [DefaultValue("")]

        public string UUID
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_uuid))
                    _uuid = Guid.NewGuid().ToString("D");
                return _uuid;
            }
            set
            {
                Match match = Regex.Match(value, uuidV4Regex, RegexOptions.IgnoreCase);
                if (match.Success)
                    _uuid = value;
            }
        }

        [JsonIgnore]
        public virtual bool IsPossible
        {
            get
            {
                // Return the cached answer
                return _isPossible;
            }
        }

        [JsonIgnore]
        public virtual bool IsActive
        {
            get
            {
                
                if (this.Equals(ProfileRepository.CurrentProfile))
                    return true;
                else
                    return false;

            }
        }


        [DefaultValue("")]
        public virtual string Name { get; set; }

        [JsonRequired]
        public AudioProfile WindowsAudioConfig
        {
            get
            {
                return _windowsAudioConfig;
            }
            set
            {
                _windowsAudioConfig = value;
            }
        }
        // The delay in seconds between audioProfile attempts. Is only used if there is more than one attempt set in ApplyProfileCount
        [DefaultValue(0)]
        public int ApplyProfileDelay
        {
            get
            {
                return _applyProfileDelay;
            }
            set
            {
                _applyProfileDelay = value;
            }
        }

        #endregion

        public static bool IsValidName(string testName)
        {
            foreach (AudioProfileItem loadedProfile in AudioProfileRepository.AllAudioProfiles)
            {
                if (loadedProfile.Name == testName)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidUUID(string testId)
        {
            Match match = Regex.Match(testId, uuidV4Regex, RegexOptions.IgnoreCase);
            if (match.Success)
                return true;
            else
                return false;
        }

        public bool IsValid()
        {
            return true;
        }

        public virtual bool CopyTo(AudioProfileItem audioProfile, bool overwriteId = true)
        {
            if (overwriteId == true)
                audioProfile.UUID = UUID;

            // Copy all our audioProfile data over to the other audioProfile
            audioProfile.Name = Name;
            audioProfile.ApplyProfileDelay = ApplyProfileDelay;
            audioProfile.WindowsAudioConfig = WindowsAudioConfig; 
            return true;
        }

        public bool CreateProfileFromCurrentAudioSettings()
        {
            
            try
            {
                using (WindowsAudioController controller = new WindowsAudioController())
                {
                 _windowsAudioConfig =  controller.GetCurrentProfile();   
                }                                
                _applyProfileDelay = 0;

                return true;
                
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"AudioProfileItem/CreateProfileFromCurrentAudioSettings: Exception within CreateProfileFromCurrentAudioSettings function - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                return false;
            }
        }

        public virtual void RefreshPossbility()
        {            
            // Set isPossible to true unless we find it can't be done.
            _isPossible = true;
        }

        // Actually set this audioProfile active
        public bool SetActive(int delayInMs = 500)
        {
            try
            {
                bool itWorkedforWindows = false;

                // Now we need to apply the Windows audioProfile settings, and then if they worked we record that fact
                using (WindowsAudioController controller = new WindowsAudioController())
                {
                    SharedLogger.logger.Trace($"AudioProfileItem/SetActive: Attempting to apply Windows audio  config {Name}...");                
                    controller.ApplyProfile(WindowsAudioConfig); 
                    itWorkedforWindows = true;
                    Thread.Sleep(delayInMs);
                }   
                
                // Now we reports on the results of the Windows audioProfile application
                if (itWorkedforWindows)
                {
                    SharedLogger.logger.Trace($"AudioProfileItem/SetActive: The Windows Audio Profile {Name} was successfully applied.");
                    return true;

                }
                else
                {
                    SharedLogger.logger.Error($"AudioProfileItem/SetActive: The Windows Audio Profile {Name} was NOT applied correctly.");
                    return false;
                }
            
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"AudioProfileItem/SetActive: Exception within SetActive function - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                return false;
            }

        }

        public int CompareTo(AudioProfileItem other)
        {

            int result = CompareToValues(other);

            // If comparison based solely on values
            // returns zero, indicating that two instances
            // are equal in those fields they have in common,
            // only then we break the tie by comparing
            // data types of the two instances.
            if (result == 0)
                result = CompareTypes(other);

            return result;

        }

        protected virtual int CompareToValues(AudioProfileItem other)
        {

            if (object.ReferenceEquals(other, null))
                return 1;   // All instances are greater than null

            // Base class simply compares Mark properties
            return Name.CompareTo(other.Name);

        }

        protected int CompareTypes(AudioProfileItem other)
        {

            // Base type is considered less than derived type
            // when two instances have the same values of
            // base fields.

            // Instances of two distinct derived types are
            // ordered by comparing full names of their
            // types when base fields are equal.
            // This is consistent comparison rule for all
            // instances of the two derived types.

            int result = 0;

            Type thisType = this.GetType();
            Type otherType = other.GetType();

            if (otherType.IsSubclassOf(thisType))
                result = -1;    // other is subclass of this class
            else if (thisType.IsSubclassOf(otherType))
                result = 1;     // this is subclass of other class
            else if (thisType != otherType)
                result = thisType.FullName.CompareTo(otherType.FullName);
            // cut the tie with a test that returns
            // the same value for all objects

            return result;

        }


        // The object specific Equals
        public bool Equals(AudioProfileItem other)
        {
            // Check references
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            // Check the object fields
            // ProfileDisplayIdentifiers may be the same but in different order within the array, so we need to handle
            // that fact.                        
            return WindowsAudioConfig.Equals(other.WindowsAudioConfig) &&
                   ApplyProfileDelay.Equals(other.ApplyProfileDelay);
        }

        // The public override for the Object.Equals
        public override bool Equals(Object obj)
        {
            // Check references
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            // If different types then can't be true
            if (obj.GetType() != this.GetType()) return false;
            if (!(obj is AudioProfileItem)) return false;
            // Check the object fields as this must the same object as obj, and we need to test in more detail
            return Equals((AudioProfileItem) obj);
        }

        // If Equals() returns true for this object compared to  another
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {
            // Calculate the hash code for the product.
            return (WindowsAudioConfig, ApplyProfileDelay).GetHashCode();

        }

        public static bool operator ==(AudioProfileItem lhs, AudioProfileItem rhs)
        {
            return Equals(lhs, rhs);
        }

        public static bool operator !=(AudioProfileItem lhs, AudioProfileItem rhs)
        {
            return !Equals(lhs, rhs);
        }

        public override string ToString()
        {
            return (Name ?? "No Audio Profile Available");
        }

        public string GenerateSettingsText()
        {
            if (WindowsAudioConfig == null)
                return ("No Settings Available");

            string settings = "Audio Profile Name: " + Name + Environment.NewLine;
            settings += Environment.NewLine + "Speaker Settings:" + Environment.NewLine;
            settings += "\tPlayback Multimedia Device: " + WindowsAudioConfig.Playback.MultimediaDevice.FriendlyName + Environment.NewLine;
            settings += "\tPlayback Communication Device: " + WindowsAudioConfig.Playback.CommunicationsDevice.FriendlyName + Environment.NewLine;
            settings += "\tPlayback Console Device: " + WindowsAudioConfig.Playback.ConsoleDevice.FriendlyName + Environment.NewLine;
            settings += "\tPlayback Volume: " + WindowsAudioConfig.Playback.VolumePercent + Environment.NewLine;
            settings += "\tPlayback Mute: " + WindowsAudioConfig.Playback.IsMuted + Environment.NewLine;
            settings += Environment.NewLine + "Microphone Settings:" + Environment.NewLine;
            settings += "\tRecording Multimedia Device: " + WindowsAudioConfig.Recording.MultimediaDevice.FriendlyName + Environment.NewLine;
            settings += "\tRecording Communication Device: " + WindowsAudioConfig.Recording.CommunicationsDevice.FriendlyName + Environment.NewLine;
            settings += "\tRecording Console Device: " + WindowsAudioConfig.Recording.ConsoleDevice.FriendlyName + Environment.NewLine;
            settings += "\tRecording Volume: " + WindowsAudioConfig.Recording.VolumePercent + Environment.NewLine;
            settings += "\tRecording Mute: " + WindowsAudioConfig.Recording.IsMuted + Environment.NewLine;
            settings += Environment.NewLine + "System Settings:" + Environment.NewLine;
            settings += "\tMono Audio: " + (WindowsAudioConfig.System.IsMonoAudioEnabled
                ? (WindowsAudioConfig.System.MonoAudio ? "On" : "Off")
                : "Not captured") + Environment.NewLine;
            settings += "\tSystem Audio Enabled: " + WindowsAudioConfig.System.IsSystemAudioEnabled + Environment.NewLine;

            return settings;
        }

        /// <summary>
        /// Waits for the audio devices listed in the Audio Profile to become available (up to a supplied timeout period) and then applies the audio profile settings to the system. Returns true if successful, false if not.
        /// Is designed to handle HDMI, Display Port and USB display audio devices that may not be available immediately after a display profile change.
        /// </summary>
        /// <param name="timeoutInMs">Maximum time in milliseconds to wait in total for required audio devices to become available. Defaults to 20000ms (20 seconds).</param>
        /// <param name="delayInMs">Delay in milliseconds to wait after successfully applying the profile. Defaults to 500ms.</param>
        /// <returns>true if successful, false if not.</returns>
        public bool TrySetActive(int timeoutInMs = 20000, int delayInMs = 500)
        {
            List<string> ignoredMissingDeviceNames;
            return TrySetActive(timeoutInMs, delayInMs, out ignoredMissingDeviceNames);
        }

        /// <summary>
        /// Waits for the audio devices listed in the Audio Profile to become available using short round-robin polling slices,
        /// then applies the profile. Returns missing devices in out parameter if timeout expires before all devices appear.
        /// </summary>
        /// <param name="timeoutInMs">Maximum time in milliseconds to wait in total for required audio devices to become available.</param>
        /// <param name="delayInMs">Delay in milliseconds to wait after applying the profile.</param>
        /// <param name="missingDeviceNames">Any audio device names that did not become available before timeout.</param>
        /// <returns>true if profile applied and all devices became available; false if devices were still missing at timeout or if an exception occurs.</returns>
        public bool TrySetActive(int timeoutInMs, int delayInMs, out List<string> missingDeviceNames)
        {
            missingDeviceNames = new List<string>();

            try
            {
                int clampedTimeoutInMs = Math.Max(0, timeoutInMs);
                const int pollSliceMs = 250;

                SharedLogger.logger.Trace($"AudioProfileItem/TrySetActive: Waiting for audio devices in profile {Name} to become available (global timeout: {clampedTimeoutInMs}ms, poll slice: {pollSliceMs}ms)...");

                using (WindowsAudioController controller = new WindowsAudioController())
                {
                    // Collect all distinct endpoint references from the profile that have a DeviceId.
                    // HDMI, Display Port and USB audio devices may not be present immediately after a
                    // display profile change, so we cycle across endpoints until all appear or timeout.
                    var endpointsToWaitFor = new List<WindowsAudioWrapper.Models.AudioEndpointReference>();

                    void AddIfNew(WindowsAudioWrapper.Models.AudioEndpointReference ep)
                    {
                        if (ep != null && !string.IsNullOrEmpty(ep.DeviceId) &&
                            !endpointsToWaitFor.Any(e => e.DeviceId == ep.DeviceId))
                            endpointsToWaitFor.Add(ep);
                    }

                    string GetEndpointDisplayName(WindowsAudioWrapper.Models.AudioEndpointReference endpoint)
                    {
                        if (!string.IsNullOrWhiteSpace(endpoint?.FriendlyName))
                            return endpoint.FriendlyName;
                        if (!string.IsNullOrWhiteSpace(endpoint?.DeviceId))
                            return endpoint.DeviceId;
                        return "Unknown Audio Device";
                    }

                    if (WindowsAudioConfig?.Playback != null)
                    {
                        AddIfNew(WindowsAudioConfig.Playback.MultimediaDevice);
                        AddIfNew(WindowsAudioConfig.Playback.CommunicationsDevice);
                        AddIfNew(WindowsAudioConfig.Playback.ConsoleDevice);
                    }

                    if (WindowsAudioConfig?.Recording != null)
                    {
                        AddIfNew(WindowsAudioConfig.Recording.MultimediaDevice);
                        AddIfNew(WindowsAudioConfig.Recording.CommunicationsDevice);
                        AddIfNew(WindowsAudioConfig.Recording.ConsoleDevice);
                    }

                    var pendingEndpoints = new List<WindowsAudioWrapper.Models.AudioEndpointReference>(endpointsToWaitFor);
                    var waitStopwatch = Stopwatch.StartNew();
                    int endpointIndex = 0;

                    while (pendingEndpoints.Count > 0 && waitStopwatch.ElapsedMilliseconds < clampedTimeoutInMs)
                    {
                        if (endpointIndex >= pendingEndpoints.Count)
                            endpointIndex = 0;

                        var endpoint = pendingEndpoints[endpointIndex];
                        int remainingMs = clampedTimeoutInMs - (int)waitStopwatch.ElapsedMilliseconds;
                        int thisCheckTimeoutInMs = Math.Min(pollSliceMs, Math.Max(0, remainingMs));

                        if (thisCheckTimeoutInMs <= 0)
                            break;

                        SharedLogger.logger.Trace($"AudioProfileItem/TrySetActive: Polling audio device '{GetEndpointDisplayName(endpoint)}' ({endpoint.DeviceId}) for up to {thisCheckTimeoutInMs}ms...");
                        bool appeared = controller.WaitForAudioDeviceToAppear(endpoint, thisCheckTimeoutInMs);
                        if (appeared)
                        {
                            SharedLogger.logger.Trace($"AudioProfileItem/TrySetActive: Audio device '{GetEndpointDisplayName(endpoint)}' ({endpoint.DeviceId}) is now available.");
                            pendingEndpoints.RemoveAt(endpointIndex);
                            if (endpointIndex >= pendingEndpoints.Count)
                                endpointIndex = 0;
                        }
                        else
                        {
                            endpointIndex++;
                        }
                    }

                    if (pendingEndpoints.Count > 0)
                    {
                        missingDeviceNames = pendingEndpoints
                            .Select(GetEndpointDisplayName)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();

                        SharedLogger.logger.Warn($"AudioProfileItem/TrySetActive: Timed out after {clampedTimeoutInMs}ms waiting for audio devices: {string.Join(", ", missingDeviceNames)}. Attempting to apply profile anyway.");
                    }

                    // Apply the audio profile now that we've waited for the devices
                    SharedLogger.logger.Trace($"AudioProfileItem/TrySetActive: Applying Windows audio profile {Name}...");
                    var applyResult = controller.ApplyProfile(WindowsAudioConfig);
                    bool profileAppliedSuccessfully = applyResult != null && applyResult.Successful;
                    Thread.Sleep(delayInMs);

                    if (!profileAppliedSuccessfully)
                    {
                        SharedLogger.logger.Error($"AudioProfileItem/TrySetActive: The Windows Audio Profile {Name} was not applied correctly.");
                        return false;
                    }
                }

                if (missingDeviceNames.Count > 0)
                {
                    SharedLogger.logger.Warn($"AudioProfileItem/TrySetActive: The Windows Audio Profile {Name} was applied, but some expected audio devices did not appear in time.");
                    return false;
                }

                SharedLogger.logger.Trace($"AudioProfileItem/TrySetActive: The Windows Audio Profile {Name} was successfully applied.");
                return true;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"AudioProfileItem/TrySetActive: Exception within TrySetActive function - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                return false;
            }
        }

        public string CreateCommand()
        {
            return $"{Application.ExecutablePath} {DisplayMagicianStartupAction.ChangeProfile} \"{UUID}\"";
        }

    }
}


