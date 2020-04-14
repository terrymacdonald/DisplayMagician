using CommandLine;
using CommandLine.Text;
using HeliosDisplayManagement.Shared;
using System.Windows.Forms;

namespace HeliosDisplayManagement
{

    class CommandLineOptions
    {

        [Option('a', "action", Default = HeliosStartupAction.None, Required = true, HelpText = "The startup action to perform: None (Show Helios Display Management), SwitchProfile (Change to another profile and optionally run an application/game), CreateShortcut (Create a Desktop Shortcut), EditProfile (Edit a profile)")]
        public HeliosStartupAction Action { get; set; }

        [Option('p', "profile", Required = true, HelpText = "The Profile Name or Profile ID of the profile to you want to use.")]
        public string Profile { get; set; }

        [Option('e', "execute", SetName = "ByExecutable", Required = false, HelpText = "(optional) The application/game to start when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut. Cannot be used with --steam or --uplay options.")]
        public string ExecuteFilename { get; set; }
        
        [Option('w', "waitfor", SetName = "ByExecutable", Required = false, HelpText = "The process name to wait for when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut. Cannot be used with --steam or --uplay options.")]
        public string ExecuteProcessName { get; set; }

        [Option('s', "steam", SetName = "ByGameId", Required = false, HelpText = "The Steam AppID to run for when we're temporarily switching profile and running the Steam application/game. Also can be used when creating a shortcut. Cannot be used with -e or -w options.")]
        public uint ExecuteSteamApp { get; set; }
        
        [Option("uplay", SetName = "ByGameId", Required = false, HelpText = "(optional)The Uplay AppID to run for when we're temporarily switching profile and running the Uplay application/game. Also can be used when creating a shortcut. Cannot be used with -e or -w options.")]
        public uint ExecuteUplayApp { get; set; }

        [Option('t', "timeout", Default = 30u, Required = false, HelpText = "(optional) The time in seconds we should delay starting the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.")]
        public uint ExecuteProcessTimeout { get; set; }

        [Option("arguments", Required = false, HelpText = "(optional) Extra arguments to pass to the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.")]
        public string ExecuteArguments { get; set; }



    }
}