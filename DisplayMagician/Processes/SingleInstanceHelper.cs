using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace DisplayMagician.Processes
{
    public class SingleInstanceHelper
    {
        /// <summary>
        /// Windows message id to bring MP2-Client to front.
        /// </summary>
        /// <remarks>
        /// Ideally this field should be kept within it's parent application, not within <see cref="MediaPortal.Utilities"/>.
        /// It has been added here to keep <c>MediaPortal.Client</c> and <c>SkinEngine</c> projects independent from each other.
        /// </remarks>
        public static readonly uint SHOW_MP2_CLIENT_MESSAGE = WindowsAPI.RegisterWindowMessage("SHOW_MP2_CLIENT_MESSAGE");

        /// <summary>
        /// Windows message id to bring MP2-ServiceMonitor to front.
        /// </summary>
        /// <remarks>
        /// Ideally this field should be kept within it's parent application, not within <see cref="MediaPortal.Utilities"/>.
        /// It has been added here to be consistent to <c>MediaPortal.Client</c>.
        /// </remarks>
        public static readonly uint SHOW_MP2_SERVICEMONITOR_MESSAGE = WindowsAPI.RegisterWindowMessage("SHOW_MP2_SERVICEMONITOR_MESSAGE");

        /// <summary>
        /// Check if an application is running or not
        /// </summary>
        /// <returns>returns true if already running</returns>
        public static bool IsAlreadyRunning(string MUTEX_ID, out Mutex mutex)
        {
            // Allow only one instance
            mutex = new Mutex(false, MUTEX_ID);

            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                                                        MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            mutex.SetAccessControl(securitySettings);

            bool hasHandle = false;
            try
            {
                // Check if we can start the application
                hasHandle = mutex.WaitOne(500, false);
            }
            catch (AbandonedMutexException)
            {
                // The mutex was abandoned in another process, it will still get aquired
                hasHandle = true;
            }
            return !hasHandle;
        }

        /// <summary>
        /// Broadcasts a <param name="messageId">message</param> through the system to inform an existing instance to show up.
        /// </summary>
        /// <param name="messageId">The id to a static windows message, which should be broadcasted.</param>
        public static void SwitchToCurrentInstance(uint messageId)
        {
            // Send our Win32 message to make the currently running instance
            // Jump on top of all the other windows
            WindowsAPI.PostMessage((IntPtr)WindowsAPI.HWND_BROADCAST, messageId, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
