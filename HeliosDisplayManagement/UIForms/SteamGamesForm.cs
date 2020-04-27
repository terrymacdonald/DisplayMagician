using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HeliosPlus.GameLibraries;

namespace HeliosPlus.UIForms
{
    public partial class SteamGamesForm : Form
    {
        public SteamGamesForm()
        {
            InitializeComponent();
        }

        public SteamGame SteamGame { get; private set; }

        private void lv_games_DoubleClick(object sender, EventArgs e)
        {
            if (btn_ok.Enabled)
            {
                btn_ok.PerformClick();
            }
        }

        private void lv_games_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lv_games.SelectedItems.Count > 0)
            {
                SteamGame = lv_games.SelectedItems[0].Tag as SteamGame;
            }
            else
            {
                SteamGame = null;
            }

            btn_ok.Enabled = SteamGame != null;
        }

        private async void SteamGamesForm_Load(object sender, EventArgs e)
        {
            foreach (var game in SteamGame.GetAllInstalledGames().OrderByDescending(game => game.GameName))
            {
                il_games.Images.Add(game.GameIcon);

                if (!Visible)
                {
                    return;
                }

                lv_games.Items.Add(new ListViewItem
                {
                    Text = game.GameName,
                    Tag = game,
                    ImageIndex = il_games.Images.Count - 1
                });
            }
        }
    }
}