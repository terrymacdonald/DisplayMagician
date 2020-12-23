using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
//using Microsoft.Toolkit.Uwp.Notifications;
using DesktopNotifications;
using static DesktopNotifications.NotificationActivator;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Threading;
using Microsoft.QueryStringDotNET;
using System.Windows.Forms;

namespace DisplayMagician
{
    // The GUID must be unique to your app. Create a new GUID if copying this code.
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [Guid("56F14154-6339-4B94-8B82-80F78D5BCEAF"), ComVisible(true)]
    public class DesktopNotificationActivator : NotificationActivator
    {
        public override void OnActivated(string invokedArgs, NotificationUserInput userInput, string appUserModelId)
        {
            Dispatcher.CurrentDispatcher.Invoke(delegate
            {
                // Tapping on the top-level header launches with empty args
                /*if (arguments.Length == 0)
                {
                    // Perform a normal launch
                    OpenWindowIfNeeded();
                    return;
                }*/

                // Parse the query string (using NuGet package QueryString.NET)
                QueryString args = QueryString.Parse(invokedArgs);

                // See what action is being requested 
                switch (args["action"])
                {
                    // Open the image
                    case "open":

                        MessageBox.Show("User just asked to open DisplayMagician");
                        /*// The URL retrieved from the toast args
                        string imageUrl = args["imageUrl"];

                        // Make sure we have a window open and in foreground
                        OpenWindowIfNeeded();

                        // And then show the image
                        (App.Current.Windows[0] as MainWindow).ShowImage(imageUrl);*/

                        break;

                    // Background: Quick reply to the conversation
                    case "exit":

                        MessageBox.Show("User just asked to exit DisplayMagician");
                        /*// Get the response the user typed
                        string msg = userInput["tbReply"];

                        // And send this message
                        SendMessage(msg);

                        // If there's no windows open, exit the app
                        if (App.Current.Windows.Count == 0)
                        {
                            Application.Current.Shutdown();
                        }*/

                        break;

                    case "stop":

                        MessageBox.Show("User just asked DisplayMagician to stop monitoring the game");
                        /*// Get the response the user typed
                        string msg = userInput["tbReply"];

                        // And send this message
                        SendMessage(msg);

                        // If there's no windows open, exit the app
                        if (App.Current.Windows.Count == 0)
                        {
                            Application.Current.Shutdown();
                        }*/

                        break;

                    default:
                        break;
                }
            });
        }

        private void OpenWindowIfNeeded()
        {
            /*// Make sure we have a window open (in case user clicked toast while app closed)
            if (App.Current.Windows.Count == 0)
            {
                new MainWindow().Show();
            }

            // Activate the window, bringing it to focus
            App.Current.Windows[0].Activate();

            // And make sure to maximize the window too, in case it was currently minimized
            App.Current.Windows[0].WindowState = WindowState.Normal;*/
        }
    }
}
