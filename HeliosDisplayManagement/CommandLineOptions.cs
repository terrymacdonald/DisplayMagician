using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
//using CommandLine;
//using CommandLine.Text;
using HeliosDisplayManagement.Resources;
using HeliosDisplayManagement.Shared;
using System.Runtime.CompilerServices;

namespace HeliosDisplayManagement
{

    internal class CommandLineOptions
    {
        private CommandLineOptions()
        {
        }

        public static HeliosStartupAction Action { get; set; } 
        public static string ExecuteArguments { get; set; } 
        public static string ExecuteFilename { get; set; } 
        public static string ExecuteProcessName { get; set; }
        public static uint ExecuteProcessTimeout { get; set; }
        public static uint ExecuteSteamApp { get; set; }
        public static string ProfileId { get; set; }
    
    }
}