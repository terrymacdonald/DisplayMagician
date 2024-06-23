using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.WindowsAPICodePack.ShellExtensions; 

namespace DisplayMagicianShellExtension
{

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    public class DisplayMagicianShellCommand : IExplorerCommand
    {
        IExplorerCommand

        // Define a unique GUID for your command
        public static readonly Guid CommandId = new Guid("YOUR-COMMAND-GUID-HERE");

        // Implement the Invoke method to define what happens when your command is executed
        public void Invoke(IShellItemArray psiItemArray, IBindCtx pbc)
        {
            // Here, you would add the code to perform your command's action
            // For example, showing a message box or opening a file
            System.Windows.Forms.MessageBox.Show("MyCustomCommand executed!");
        }

        // Implement the GetState method to define the command's availability
        public EXPCMDSTATE GetState(IShellItemArray psiItemArray)
        {
            // This example always enables the command, but you could add logic to disable it under certain conditions
            return EXPCMDSTATE.ECS_ENABLED;
        }

        // Implement the GetIcon method if you want to provide an icon for your command
        public string GetIcon(IShellItemArray psiItemArray)
        {
            // Return the path to your command's icon
            // For simplicity, this example returns null, meaning no icon
            return null;
        }

        // Implement the GetTitle method to provide the display name of your command
        public string GetTitle(IShellItemArray psiItemArray)
        {
            // Return the title of your command
            return "My Custom Command";
        }

        // Implement the GetToolTip method to provide a tooltip for your command
        public string GetToolTip(IShellItemArray psiItemArray)
        {
            // Return the tooltip text for your command
            // For simplicity, this example returns null, meaning no tooltip
            return null;
        }

        // Implement the GetCanonicalName method to provide a unique name for your command
        public Guid GetCanonicalName()
        {
            // Return the GUID of your command
            return CommandId;
        }

        // Implement the EnumSubCommands method if your command has subcommands
        // This example does not use subcommands, so it returns null
        public IEnumExplorerCommand EnumSubCommands()
        {
            // Return null as this command does not have subcommands
            return null;
        }
    }

}
