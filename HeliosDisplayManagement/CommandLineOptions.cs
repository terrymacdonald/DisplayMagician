using System;
using System.Linq;
using System.Windows.Forms;
using CommandLine;
using CommandLine.Text;
using HeliosDisplayManagement.Resources;
using HeliosDisplayManagement.Shared;

namespace HeliosDisplayManagement
{
    internal class CommandLineOptions
    {
        private static CommandLineOptions _defaultObject;

        private CommandLineOptions()
        {
        }

        [Option('a', @"action", HelpText = @"The action to perform",
             DefaultValue = HeliosStartupAction.None)]
        public HeliosStartupAction Action { get; set; }

        [Option('p', @"p", HelpText = @"Profile name to switch to.", DefaultValue = null)]
        public string ProfileName { get; set; }

        [Option('e', @"execute",
             HelpText = @"Program's address to execute, for temporarily switch or to create shortcut.",
             DefaultValue = null)]
        public string ExecuteFilename { get; set; }

        [Option('s', @"steam",
             HelpText = @"AppId of the Steam game, for temporarily switch or to create shortcut.",
             DefaultValue = 0u)]
        public uint ExecuteSteamApp { get; set; }

        [Option(@"arguments", HelpText = @"Program's argument to execute, for temporarily switch or to create shortcut.",
             DefaultValue = null)]
        public string ExecuteArguments { get; set; }

        [Option('w', @"waitfor",
             HelpText =
                 @"Program's process name to wait for end of execution before rolling back the settings, for temporarily switch or to create shortcut; Useful when there is a launcher involved.",
             DefaultValue = null)]
        public string ExecuteProcessName { get; set; }

        [Option('t', @"timeout",
             HelpText =
                 @"The maximum time in seconds to wait for the process since the execution of the program, for temporarily switch or to create shortcut.",
             DefaultValue = 30u)]
        public uint ExecuteProcessTimeout { get; set; }

        public static CommandLineOptions Default
        {
            get
            {
                if (_defaultObject == null)
                {
                    _defaultObject = new CommandLineOptions();
                    Parser.Default.ParseArguments(Environment.GetCommandLineArgs().Skip(1).ToArray(), _defaultObject);
                    Console.WriteLine(string.Join(" ", Environment.GetCommandLineArgs().Skip(1)));
                    if ((_defaultObject.LastParserState != null) && (_defaultObject.LastParserState.Errors.Count > 0))
                        throw new Exception(_defaultObject.LastParserState.Errors[0].ToString());
                }
                return _defaultObject;
            }
        }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        // ReSharper disable once UnusedMember.Global
        public string GetUsage()
        {
            var help = HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
            MessageBox.Show(help, Language.Help, MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            Environment.Exit(0);
            return help;
        }
    }
}