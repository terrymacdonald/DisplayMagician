using System;
using System.Runtime.InteropServices;
using DesktopNotifications;
using Microsoft.QueryStringDotNET;
using System.Windows.Forms;
using DisplayMagician.UIForms;
using DisplayMagicianShared;

namespace DisplayMagician
{
    // The GUID must be unique to your app. Create a new GUID if copying this code.
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [Guid(Program.AppActivationId), ComVisible(true)]
    
    public class DesktopNotificationActivator : NotificationActivator
    {
        public override void OnActivated(string arguments, NotificationUserInput userInput, string appUserModelId)
        {
            // Invoke the code we're running on the UI Thread to avoid
            // cross thread exceptions
            Program.AppMainForm.BeginInvoke((MethodInvoker)delegate
            {
                // This code is running on the main UI thread!
                // Parse the query string (using NuGet package QueryString.NET)
                QueryString args = QueryString.Parse(arguments);

                foreach (QueryStringParameter myArg in args)
                {
                    if (myArg.Name.Equals("action",StringComparison.OrdinalIgnoreCase))
                    {
                        // See what action is being requested 
                        switch (args["action"])
                        {
                            // Open the Main window
                            case "open":

                                // Open the Main DisplayMagician Window
                                Program.AppMainForm.openApplicationWindow();
                                break;

                            // Exit the application
                            case "exit":

                                // Exit the application (overriding the close restriction)
                                Program.AppMainForm.exitApplication();
                                break;

                            // Stop waiting so that the monitoring stops, and the UI becomes free
                            case "stopWaiting":

                                Program.AppCancellationTokenSource.Cancel();
                                break;

                            default:
                                break;
                        }
                    }
                }
            });
        }

        private void OpenWindowIfNeeded()
        {
            // Make sure we have a window open (in case user clicked toast while app closed)
            if (Program.AppMainForm == null)
            {
                Program.AppMainForm = new MainForm();
            }

            // Activate the window, bringing it to focus
            Program.AppMainForm.openApplicationWindow();
        }
    }
}
