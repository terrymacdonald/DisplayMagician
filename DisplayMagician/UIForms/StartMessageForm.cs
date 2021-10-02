using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;

namespace DisplayMagician.UIForms
{
    public partial class StartMessageForm : Form
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public string MessageMode
        { get; set; } = "RTF";

        public string Filename
        { get; set; }

        public string URL
        { get; set; }

        public string HeadingText
        { get; set; } = "DisplayMagician Message";

        public string ButtonText
        { get; set; } = "&Close";

        public StartMessageForm()
        {
            InitializeComponent();
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void StartMessageForm_Load(object sender, EventArgs e)
        {
            string FullPath;

            // Set the heading text if supplied
            if (!String.IsNullOrWhiteSpace(HeadingText))
            {
                lbl_heading_text.Text = HeadingText;
            }

            // Set the button text if supplied
            if (!String.IsNullOrWhiteSpace(ButtonText))
            {
                btn_back.Text = ButtonText;
            }

            // check if we're in Filename mode or URL mode
            if (!String.IsNullOrWhiteSpace(Filename))
            {
                // We're in filename mode
                // Figure out the full path of the filename
                try
                {
                    FullPath = Path.Combine(Application.StartupPath, Filename);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"StartMessageForm/StartMessageForm_Load: Filename supplied (\"{Filename}\") is not within the Application startup path (\"{Application.StartupPath}\")");
                    this.Close();
                    return;
                }

                // Try to load the Filename if it's supplied
                try
                {
                    if (File.Exists(Filename))
                    {
                        if (MessageMode == "rtf")
                        {
                            rtb_message.Show();
                            rtb_message.LoadFile(Filename, RichTextBoxStreamType.RichText);
                        }
                        else if (MessageMode == "txt")
                        {
                            rtb_message.Show();
                            rtb_message.LoadFile(Filename, RichTextBoxStreamType.PlainText);
                        }
                        else
                        {
                            logger.Error($"StartMessageForm/StartMessageForm_Load: Message from file {Filename} is in an unsupported MessageMode: {MessageMode}");
                            this.Close();
                            return;
                        }
                    }
                    else
                    {
                        logger.Error($"StartMessageForm/StartMessageForm_Load: Couldn't find the Filename supplied (\"{Filename}\") and load it into the RichTextBox message object");
                        this.Close();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"StartMessageForm/StartMessageForm_Load: Exception while trying to load the Filename supplied (\"{Filename}\") into the RichTextBox message object");
                    this.Close();
                    return;
                }
            }
            else
            {
                // We're in URL mode
                // See if the URL supplied is valid
                if (!IsURLValid(URL))
                {
                    logger.Error($"StartMessageForm/StartMessageForm_Load: URL {URL} pointing to the RTF file is invalid!");
                    this.Close();
                    return;
                }
                // If we get here, then the URL is good. See if we can access the URL supplied
                WebClient client = new WebClient();
                if (MessageMode == "rtf")
                {
                    try
                    {
                        byte[] byteArray = client.DownloadData(URL);
                        MemoryStream theMemStream = new MemoryStream();
                        theMemStream.Write(byteArray, 0, byteArray.Length);
                        theMemStream.Position = 0;
                        rtb_message.Show();
                        rtb_message.LoadFile(theMemStream, RichTextBoxStreamType.RichText);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"StartMessageForm/StartMessageForm_Load: Exception while trying to load the URL supplied (\"{URL}\") into the RichTextBox message object (RTF Mode)");
                        this.Close();
                        return;
                    }                
                }
                else if (MessageMode == "txt")
                {
                    try
                    {
                        string textToShow = client.DownloadString(URL);
                        rtb_message.Show();
                        rtb_message.Text = textToShow;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"StartMessageForm/StartMessageForm_Load: Exception while trying to load the URL supplied (\"{URL}\") into the RichTextBox message object (TXT Mode)");
                        this.Close();
                        return;
                    }
                }
                else
                {
                    logger.Error($"StartMessageForm/StartMessageForm_Load: Message from URL {URL} is in an unsupported MessageMode: {MessageMode}");
                    this.Close();
                    return;
                }
            }
        }

        public static bool IsURLValid(string url)
        {
            Uri uriResult;
            bool tryCreateResult = Uri.TryCreate(url, UriKind.Absolute, out uriResult);
            if (tryCreateResult == true && uriResult != null)
                return true;
            else
                return false;
        }
    }
}
