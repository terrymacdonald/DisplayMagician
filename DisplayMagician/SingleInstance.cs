using DisplayMagician.UIForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private static readonly Queue<string[]> _pendingCommandLineArguments = new Queue<string[]>();
        private static readonly object _pendingCommandLock = new object();
        private static bool _readyForCommands = false;
        private const int MaximumForwardedCommandBytes = 64 * 1024;
        private const int ForwardedCommandTimeoutMilliseconds = 10000;

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static string GetMutexName() => $@"Mutex_{UniqueName}";
        private static string GetPipeName() => $@"Pipe_{UniqueName}";

        public static void executeAnActionCallback(string[] args)
        {
            if (args == null)
            {
                logger.Warn($"SingleInstance/executeAnActionCallback: Received a null commandline from another DisplayMagician instance.");
                return;
            }

            logger.Trace($"SingleInstance/executeAnActionCallback: Received data from another DisplayMagician instance: {String.Join(" ",args)}");

            if (args.Any(argument => string.Equals(argument, Program.TestUpdateFeedCommandLineOption, StringComparison.OrdinalIgnoreCase)))
            {
                Program.EnableTestUpdateFeed("a forwarded command line from another DisplayMagician instance", checkForUpdatesNow: true);
                return;
            }

            int commandIndex = FindCommandIndex(args);

            if (commandIndex >= 0)
            {
                // Setup a regex to match the UUID format we use
                Regex uuid = new Regex("[0-9A-Fa-f]{8}-([0-9A-Fa-f]{4}-){3}[0-9A-Fa-f]{12}");
                string command = args[commandIndex];
                string commandArgument = args.Length > commandIndex + 1 ? args[commandIndex + 1] : string.Empty;

                // Now we check for the three commandline parameters that we support
                switch (command)
                {
                    case "RunShortcut":
                        logger.Trace($"SingleInstance/executeAnActionCallback: Other DisplayMagician instance provided the RunShortcut command: '{command} {commandArgument}'");
                        if (uuid.IsMatch(commandArgument)) 
                        {
                            Program.RunShortcut(commandArgument);
                        }
                        else
                        {
                            logger.Warn($"SingleInstance/executeAnActionCallback: Other DisplayMagician instance provided an invalid shortcut UUID to the RunShortcut command: '{commandArgument}'");
                        }
                        break;
                    case "ChangeProfile":
                        logger.Trace($"SingleInstance/executeAnActionCallback: Other DisplayMagician instance provided the ChangeProfile command: '{command} {commandArgument}'");
                        if (uuid.IsMatch(commandArgument))
                        {
                            Program.RunProfile(commandArgument);
                        }
                        else
                        {
                            logger.Warn($"SingleInstance/executeAnActionCallback: Other DisplayMagician instance provided an invalid profile UUID to the ChangeProfile command: '{commandArgument}'");
                        }
                        break;
                    case "CreateProfile":
                        logger.Trace($"SingleInstance/executeAnActionCallback: Other DisplayMagician instance provided the CreateProfile command.");
                        Program.CreateProfile();
                        break;
                    default:
                        logger.Warn($"SingleInstance/executeAnActionCallback: Other DisplayMagician instance provided an unsupported command: '{command}'");
                        break;
                }
            }
            else
            {
                // If we only have the path, we assume they just want to bring the topmost window to the foreground
                // Replace the selected code with the following to ensure UI thread safety using Invoke
                logger.Trace($"SingleInstance/executeAnActionCallback: Other DisplayMagician instance didn't provide any supported commandline arguments. Opening the Main Display Window.");
                MainForm myMainForm = Program.AppMainForm;
                if (myMainForm != null)
                {
                    if (myMainForm.InvokeRequired)
                    {
                        myMainForm.Invoke(new Action(() =>
                        {
                            myMainForm.openApplicationWindow();                            
                        }));
                    }
                    else
                    {
                        myMainForm.openApplicationWindow();
                    }
                }
            }
        }           

        private static int FindCommandIndex(string[] args)
        {
            if (args == null)
                return -1;

            for (int i = 0; i < args.Length; i++)
            {
                if (Enum.TryParse(args[i], ignoreCase: true, out DisplayMagicianStartupAction action))
                {
                    if (action == DisplayMagicianStartupAction.RunShortcut ||
                        action == DisplayMagicianStartupAction.ChangeProfile ||
                        action == DisplayMagicianStartupAction.CreateProfile)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public static void MarkReadyForCommands()
        {
            List<string[]> queuedCommands = new List<string[]>();

            lock (_pendingCommandLock)
            {
                _readyForCommands = true;
                while (_pendingCommandLineArguments.Count > 0)
                {
                    queuedCommands.Add(_pendingCommandLineArguments.Dequeue());
                }
            }

            foreach (string[] queuedCommand in queuedCommands)
            {
                DispatchCommandLineArguments(queuedCommand);
            }
        }

        private static void DispatchCommandLineArguments(string[] args)
        {
            lock (_pendingCommandLock)
            {
                if (!_readyForCommands)
                {
                    _pendingCommandLineArguments.Enqueue(args);
                    logger.Trace($"SingleInstance/DispatchCommandLineArguments: DisplayMagician is not ready to process commands yet. Queued forwarded commandline.");
                    return;
                }
            }

            MainForm mainForm = Program.AppMainForm;
            if (mainForm != null && mainForm.IsHandleCreated)
            {
                mainForm.BeginInvoke(new Action(() => _otherInstanceCallback(args)));
                return;
            }

            if (_syncContext != null)
            {
                _syncContext.Post(_ => _otherInstanceCallback(args), null);
            }
            else
            {
                _otherInstanceCallback(args);
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
                logger.Trace($"SingleInstance/LaunchOrReturn: Creating the NamedPipeServer ready to wait for other DisplayMaigicans to send us commands they want us to run.");
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
                logger.Trace($"SingleInstance/LaunchOrReturn: Sending the primary DisplayMagician the following commandline: {Environment.GetCommandLineArgs().ToString()}.");

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

            if (_firstApplicationInstance)
            {
                // We are the first instance
                logger.Trace($"SingleInstance/IsApplicationFirstInstance: This is the first instance of DisplayMagician.");
            }
            else
            {
                // We are not the first instance
                logger.Trace($"SingleInstance/IsApplicationFirstInstance: This is NOT the first instance of DisplayMagician.");
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
                logger.Trace($"SingleInstance/NamedPipeClientSendOptions: Sending the primary DisplayMagician the message through the NamedPipe.");

                using (var namedPipeClientStream = new NamedPipeClientStream(".", GetPipeName(), PipeDirection.InOut))
                {
                    namedPipeClientStream.Connect(3000); // Maximum wait 3 seconds
                    namedPipeClientStream.ReadTimeout = ForwardedCommandTimeoutMilliseconds;
                    namedPipeClientStream.WriteTimeout = ForwardedCommandTimeoutMilliseconds;
                    byte[] payloadBytes = SerializePayload(namedPipePayload);
                    using (var writer = new BinaryWriter(namedPipeClientStream, Encoding.UTF8, leaveOpen: true))
                    {
                        writer.Write(payloadBytes.Length);
                        writer.Write(payloadBytes);
                        writer.Flush();
                    }
                    namedPipeClientStream.Flush();

                    using (var reader = new StreamReader(namedPipeClientStream, Encoding.UTF8, false, 1024, leaveOpen: true))
                    {
                        reader.ReadLine();
                    }
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
            logger.Trace($"SingleInstance/NamedPipeServerCreateServer: Sending the primary DisplayMagician the message through the NamedPipe.");

            var pipeSecurity = new PipeSecurity();
            // A forwarded command can start a shortcut or change a display profile.
            // Only another instance owned by this same Windows user may send one.
            SecurityIdentifier sidUser = WindowsIdentity.GetCurrent().User;
            if (sidUser != null)
            {
                PipeAccessRule accessRule = new PipeAccessRule(sidUser, PipeAccessRights.FullControl, AccessControlType.Allow);
                pipeSecurity.AddAccessRule(accessRule);
            }
            else
            {
                throw new InvalidOperationException("The current Windows user has no SID for the single-instance pipe.");
            }

            try
            {
                // Create pipe and start the async connection wait
                _namedPipeServerStream = NamedPipeServerStreamAcl.Create(
                    GetPipeName(),
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous,
                    0,
                    0,
                    pipeSecurity,
                    HandleInheritability.None);

            }
            catch (PlatformNotSupportedException ex)
            {
                //Console.WriteLine($"SingleInstance/NamedPipeServerCreateServer: Cannot create a named pipe server. This NamedPipeServerStream function does not support this platform.");
                logger.Warn(ex, $"SingleInstance/NamedPipeServerCreateServer: Cannot create a named pipe server. This NamedPipeServerStream function does not support this platform.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SingleInstance/NamedPipeServerCreateServer: Exception - Source: {ex.Source} {ex.TargetSite} - {ex.Message} - {ex.StackTrace}");
                logger.Warn(ex, $"SingleInstance/NamedPipeServerCreateServer: Exception - Source: {ex.Source} {ex.TargetSite} - {ex.Message} - {ex.StackTrace}");
            }
            // Begin async wait for connections
            if (_namedPipeServerStream != null)
            {
                _namedPipeServerStream.BeginWaitForConnection(NamedPipeServerConnectionCallback, _namedPipeServerStream);
            }
        }

        /// <summary>
        ///     The function called when a client connects to the named pipe. Note: This method is called on a non-UI thread.
        /// </summary>
        /// <param name="iAsyncResult"></param>
        private static void NamedPipeServerConnectionCallback(IAsyncResult iAsyncResult)
        {
            try
            {
                logger.Trace($"SingleInstance/NamedPipeServerConnectionCallback: Yay! Another DisplayMagician finally send us something! Stopping the current named pipe server so we can process things.");

                // End waiting for the connection
                _namedPipeServerStream.EndWaitForConnection(iAsyncResult);

                logger.Trace($"SingleInstance/NamedPipeServerConnectionCallback: Reading what the other DisplayMagician sent us.");

                // Read data and prevent access to _namedPipeXmlPayload during threaded operations
                lock (_namedPiperServerThreadLock)
                {

                    using System.Threading.Timer timeout = new System.Threading.Timer(_ => _namedPipeServerStream?.Dispose(), null, ForwardedCommandTimeoutMilliseconds, System.Threading.Timeout.Infinite);
                    Payload payload = DeserializePayload(_namedPipeServerStream);

                    logger.Trace($"SingleInstance/NamedPipeServerConnectionCallback: The other DisplayMagician sent us the following commandline: {payload.CommandLineArguments.ToString()}");

                    // payload contains the data sent from the other instance
                    DispatchCommandLineArguments(payload.CommandLineArguments.ToArray());

                    using (var writer = new StreamWriter(_namedPipeServerStream, Encoding.UTF8, 1024, leaveOpen: true))
                    {
                        writer.AutoFlush = true;
                        writer.WriteLine("OK");
                    }
                }
            }
            catch (ObjectDisposedException ex)
            {
                // EndWaitForConnection will exception when someone calls closes the pipe before connection made
                // In that case we dont create any more pipes and just return
                // This will happen when app is closing and our pipe is closed/disposed
                logger.Trace(ex, $"SingleInstance/NamedPipeServerConnectionCallback: ObjectDisposedException: The other DisplayMagician closed the pipe before a connection was made.");

                return;
            }
            catch (Exception ex)
            {
                // ignored
                logger.Warn(ex, $"SingleInstance/NamedPipeServerConnectionCallback: Exception: The other DisplayMagician closed the pipe before a connection was made.");
            }
            finally
            {
                // Close the original pipe (we will create a new one each time)
                logger.Trace($"SingleInstance/NamedPipeServerConnectionCallback: Disposing of the previous named pipe server memory.");
                _namedPipeServerStream.Dispose();
            }

            // Create a new pipe for next connection
            logger.Trace($"SingleInstance/NamedPipeServerConnectionCallback: Creating a new named pipe server in preparation for any DisplayMagicians to send us something in the future.");
            NamedPipeServerCreateServer();
        }

        private static byte[] SerializePayload(Payload payload)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                new DataContractJsonSerializer(typeof(Payload)).WriteObject(stream, payload);
                if (stream.Length == 0 || stream.Length > MaximumForwardedCommandBytes)
                {
                    throw new InvalidDataException("The forwarded command is outside the permitted size.");
                }

                return stream.ToArray();
            }
        }

        private static Payload DeserializePayload(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true))
            {
                int length = reader.ReadInt32();
                if (length <= 0 || length > MaximumForwardedCommandBytes)
                {
                    throw new InvalidDataException("The forwarded command has an invalid size.");
                }

                byte[] bytes = reader.ReadBytes(length);
                if (bytes.Length != length)
                {
                    throw new EndOfStreamException("The forwarded command ended before its declared length.");
                }

                using (MemoryStream payloadStream = new MemoryStream(bytes, writable: false))
                {
                    return (Payload)new DataContractJsonSerializer(typeof(Payload)).ReadObject(payloadStream);
                }
            }
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
