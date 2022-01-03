using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace DisplayMagician
{
    /// <summary>
    /// Helper for creating or passing command args to a single application instance
    /// </summary>
    public static class SingleInstance
    {
        /// <summary>
        /// Unique name to base the single instance decision on. Default's to a hash based on the executable location.
        /// </summary>
        public static string UniqueName { get; set; } = $"DisplayMagician";

        private static Mutex _mutexApplication;
        private static readonly object _mutexLock = new object();
        private static bool _firstApplicationInstance;
        private static NamedPipeServerStream _namedPipeServerStream;
        private static SynchronizationContext _syncContext;
        private static Action<string[]> _otherInstanceCallback;
        private static readonly object _namedPiperServerThreadLock = new object();

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        //private static string GetMutexName() => $@"Mutex_{Environment.UserDomainName}_{Environment.UserName}_{UniqueName}";
        //private static string GetPipeName() => $@"Pipe_{Environment.UserDomainName}_{Environment.UserName}_{UniqueName}";
        private static string GetMutexName() => $@"Mutex_{UniqueName}";
        private static string GetPipeName() => $@"Pipe_{UniqueName}";

        public static void executeAnActionCallback(string[] args)
        {
            logger.Trace($"SingleInstance/executeAnActionCallback: Received data from another DisplayMagician instance: {String.Join(" ",args)}");
            // Now we want to figure out if it's an actionable command
            // The command is in an array, and the first item is the full path to the displaymagician instance that ran. 
            // We only want to see if we should action this command if it has more args than just the single file path
            if (args.Length > 1)
            {
                // Setup a regex to match the UUID format we use
                Regex uuid = new Regex("[0-9A-Fa-f]{8}-([0-9A-Fa-f]{4}-){3}[0-9A-Fa-f]{12}");
                // Now we check for the three commandline parameters that we support
                switch (args[1])
                {
                    case "RunShortcut":
                        logger.Trace($"SingleInstance/executeAnActionCallback: Other DisplayMagician instance provided the RunShortcut command: '{args[1]} {args[2]}'");
                        if (uuid.IsMatch(args[2])) 
                        {
                            Program.RunShortcut(args[2]);
                        }
                        else
                        {
                            logger.Warn($"SingleInstance/executeAnActionCallback: Other DisplayMagician instance provided an invalid shortcut UUID to the RunShortcut command: '{args[2]}'");
                        }
                        break;
                    case "ChangeProfile":
                        logger.Trace($"SingleInstance/executeAnActionCallback: Other DisplayMagician instance provided the ChangeProfile command: '{args[1]} {args[2]}'");
                        if (uuid.IsMatch(args[2]))
                        {
                            Program.RunProfile(args[2]);
                        }
                        else
                        {
                            logger.Warn($"SingleInstance/executeAnActionCallback: Other DisplayMagician instance provided an invalid profile UUID to the ChangeProfile command: '{args[2]}'");
                        }
                        break;
                    case "CreateProfile":
                        logger.Trace($"SingleInstance/executeAnActionCallback: Other DisplayMagician instance provided the CreateProfile command: '{args[1]} {args[2]}'");
                        Program.CreateProfile();
                        break;
                    default:
                        logger.Warn($"SingleInstance/executeAnActionCallback: Other DisplayMagician instance provided an unsupported command: '{args[1]}'");
                        break;
                }
            }
            else if (args.Length == 1)
            {
                logger.Trace($"SingleInstance/executeAnActionCallback: Other DisplayMagician instance didn't provide any commandline arguments, only the path '{args[0]}'");
            }
            else
            {
                logger.Warn($"SingleInstance/executeAnActionCallback: Other DisplayMagician instance didn't provide any commandline arguments at all. THat's not supposed to happen.");
            }
        }           


        /// <summary>
        /// Determines if the application should continue launching or return because it's not the first instance.
        /// When not the first instance, the command line args will be passed to the first one. 
        /// </summary>
        /// <param name="otherInstanceCallback">Callback to execute on the first instance with command line args from subsequent launches.
        /// Will not run on the main thread, marshalling may be required.</param>
        /// <param name="args">Arguments from Main()</param>
        /// <returns>true if the first instance, false if it's not the first instance.</returns>
        public static bool LaunchOrReturn(string[] args)
        {
            _otherInstanceCallback = executeAnActionCallback;

            if (IsApplicationFirstInstance())
            {
                _syncContext = SynchronizationContext.Current;
                // Setup Named Pipe listener
                NamedPipeServerCreateServer();
                return true;
            }
            else
            {
                // We are not the first instance, send the named pipe message with our payload and stop loading
                var namedPipeXmlPayload = new Payload
                {
                    CommandLineArguments = Environment.GetCommandLineArgs().ToList()
                };

                // Send the message
                NamedPipeClientSendOptions(namedPipeXmlPayload);
                return false; // Signal to quit
            }
        }

        /// <summary>
        ///     Checks if this is the first instance of this application. Can be run multiple times.
        /// </summary>
        /// <returns></returns>
        private static bool IsApplicationFirstInstance()
        {
            if (_mutexApplication == null)
            {
                lock (_mutexLock)
                {
                    // Allow for multiple runs but only try and get the mutex once
                    if (_mutexApplication == null)
                    {
                        _mutexApplication = new Mutex(true, GetMutexName(), out _firstApplicationInstance);
                    }
                }
            }

            return _firstApplicationInstance;
        }

        /// <summary>
        ///     Uses a named pipe to send the currently parsed options to an already running instance.
        /// </summary>
        /// <param name="namedPipePayload"></param>
        private static void NamedPipeClientSendOptions(Payload namedPipePayload)
        {
            try
            {
                using (var namedPipeClientStream = new NamedPipeClientStream(".", GetPipeName(), PipeDirection.Out))
                {
                    namedPipeClientStream.Connect(3000); // Maximum wait 3 seconds

                    var ser = new DataContractJsonSerializer(typeof(Payload));
                    ser.WriteObject(namedPipeClientStream, namedPipePayload);
                }
            }
            catch (Exception)
            {
                // Error connecting or sending
            }
        }

        /// <summary>
        ///     Starts a new pipe server if one isn't already active.
        /// </summary>
        private static void NamedPipeServerCreateServer()
        {
            // Create a new pipe accessible by local authenticated users, disallow network
            var sidNetworkService = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);
            var sidWorld = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

            var pipeSecurity = new PipeSecurity();

            // Deny network access to the pipe
            var accessRule = new PipeAccessRule(sidNetworkService, PipeAccessRights.ReadWrite, AccessControlType.Deny);
            pipeSecurity.AddAccessRule(accessRule);

            // Alow Everyone to read/write
            accessRule = new PipeAccessRule(sidWorld, PipeAccessRights.ReadWrite, AccessControlType.Allow);
            pipeSecurity.AddAccessRule(accessRule);

            // Current user is the owner
            SecurityIdentifier sidOwner = WindowsIdentity.GetCurrent().Owner;
            if (sidOwner != null)
            {
                accessRule = new PipeAccessRule(sidOwner, PipeAccessRights.FullControl, AccessControlType.Allow);
                pipeSecurity.AddAccessRule(accessRule);
            }

            try
            {
                // Create pipe and start the async connection wait
                _namedPipeServerStream = new NamedPipeServerStream(
                    GetPipeName(),
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous,
                    0,
                    0,
                    pipeSecurity);

            }
            catch (PlatformNotSupportedException ex)
            {
                Console.WriteLine($"SingleInstance/NamedPipeServerCreateServer: Cannot create a named pipe server. This NamedPipeServerStream function does not support this platform.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SingleInstance/NamedPipeServerCreateServer: Exception - Source: {ex.Source} {ex.TargetSite} - {ex.Message} - {ex.StackTrace}");
            }
            // Begin async wait for connections
            _namedPipeServerStream.BeginWaitForConnection(NamedPipeServerConnectionCallback, _namedPipeServerStream);
        }

        /// <summary>
        ///     The function called when a client connects to the named pipe. Note: This method is called on a non-UI thread.
        /// </summary>
        /// <param name="iAsyncResult"></param>
        private static void NamedPipeServerConnectionCallback(IAsyncResult iAsyncResult)
        {
            try
            {
                // End waiting for the connection
                _namedPipeServerStream.EndWaitForConnection(iAsyncResult);

                // Read data and prevent access to _namedPipeXmlPayload during threaded operations
                lock (_namedPiperServerThreadLock)
                {

                    var ser = new DataContractJsonSerializer(typeof(Payload));
                    var payload = (Payload)ser.ReadObject(_namedPipeServerStream);

                    // payload contains the data sent from the other instance
                    if (_syncContext != null)
                    {
                        _syncContext.Post(_ => _otherInstanceCallback(payload.CommandLineArguments.ToArray()), null);
                    }
                    else
                    {
                        _otherInstanceCallback(payload.CommandLineArguments.ToArray());
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // EndWaitForConnection will exception when someone calls closes the pipe before connection made
                // In that case we dont create any more pipes and just return
                // This will happen when app is closing and our pipe is closed/disposed
                return;
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                // Close the original pipe (we will create a new one each time)
                _namedPipeServerStream.Dispose();
            }

            // Create a new pipe for next connection
            NamedPipeServerCreateServer();
        }

        /*private static string GetRunningProcessHash()
        {
            using (var hash = SHA256.Create())
            {
                var processPath = Process.GetCurrentProcess().MainModule.FileName;
                var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(processPath));
                return Convert.ToBase64String(bytes);
            }
        }*/
    }

    [DataContract]
    public class Payload
    {
        /// <summary>
        ///     A list of command line arguments.
        /// </summary>
        [DataMember]
        public List<string> CommandLineArguments { get; set; } = new List<string>();
    }
}
