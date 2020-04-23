using System;
using System.Windows.Forms;
using HeliosPlus.Shared;
using HeliosPlus.ShellExtension.Resources;

namespace HeliosPlus.ShellExtension
{
    internal class HeliosDisplayManagement
    {
        public static void Open(
            HeliosStartupAction action = HeliosStartupAction.None,
            Profile profile = null,
            string programAddress = null,
            bool asAdmin = false)
        {
            try
            {
                Helios.Open(action, profile, programAddress, asAdmin);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), string.Format(Language.Failed_to_execute_action_Error_Message, e.Message),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void OpenSteamGame(
            HeliosStartupAction action = HeliosStartupAction.None,
            Profile profile = null,
            uint steamAppId = 0)
        {
            try
            {
                Helios.OpenSteamGame(action, profile, steamAppId);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), string.Format(Language.Failed_to_execute_action_Error_Message, e.Message),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}