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
using Microsoft.Web.WebView2.WinForms;
using Markdig;

namespace DisplayMagician.UIForms
{
    public partial class StartMessageForm : Form
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string MessageMode
        { get; set; } = "rtf";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Filename
        { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string URL
        { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string HeadingText
        { get; set; } = "DisplayMagician Message";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
                    FullPath = Path.IsPathRooted(Filename) ? Filename : Path.Combine(Application.StartupPath, Filename);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"StartMessageForm/StartMessageForm_Load: Filename supplied (\"{Filename}\") cannot be resolved as a valid path.");
                    this.Close();
                    return;
                }

                // Try to load the Filename if it's supplied
                try
                {
                    if (File.Exists(FullPath))
                    {
                        if (MessageMode == "rtf")
                        {
                            rtb_message.Show();
                            rtb_message.LoadFile(FullPath, RichTextBoxStreamType.RichText);
                        }
                        else if (MessageMode == "txt")
                        {
                            rtb_message.Show();
                            rtb_message.LoadFile(FullPath, RichTextBoxStreamType.PlainText);
                        }
                        else if (MessageMode == "html" || MessageMode == "md" || MessageMode == "markdown")
                        {
                            try
                            {
                                string fileContent = File.ReadAllText(FullPath);
                                string htmlDoc;
                                if (MessageMode == "html")
                                {
                                    htmlDoc = fileContent;
                                }
                                else
                                {
                                    string htmlBody = Markdown.ToHtml(fileContent, new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());
                                    htmlDoc = $"<!DOCTYPE html><html><head><meta charset='utf-8'><style>body{{font-family:'Segoe UI',sans-serif;padding:20px;line-height:1.45;color:#1a1a1a;}} pre{{background:#f4f4f4;padding:10px;overflow:auto;}} code{{font-family:Consolas,monospace;}} table{{border-collapse:collapse;}} th,td{{border:1px solid #ddd;padding:6px 8px;}}</style></head><body>{htmlBody}</body></html>";
                                }

                                // Designers-backed Form utilizes WebView2 dynamic injection on runtime as a fallback scenario to provide advanced rendering
                                WebView2 webView = new WebView2
                                {
                                    Dock = DockStyle.Fill,
                                };
                                pnl_richtextbox.Controls.Add(webView);
                                webView.BringToFront();
                                rtb_message.Hide();
                                webView.NavigateToString(htmlDoc);
                            }
                            catch (Exception webEx)
                            {
                                logger.Warn(webEx, $"StartMessageForm/StartMessageForm_Load: WebView2 initialization or rendering failed; falling back to rich text display window.");
                                string rawContent = File.ReadAllText(FullPath);
                                rtb_message.Show();
                                rtb_message.Text = rawContent;
                            }
                        }
                        else
                        {
                            logger.Error($"StartMessageForm/StartMessageForm_Load: Message from file {FullPath} is in an unsupported MessageMode: {MessageMode}");
                            this.Close();
                            return;
                        }
                    }
                    else
                    {
                        logger.Error($"StartMessageForm/StartMessageForm_Load: Couldn't find the Filename supplied (\"{FullPath}\") and load it into the message view");
                        this.Close();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"StartMessageForm/StartMessageForm_Load: Exception while trying to load the Filename supplied (\"{FullPath}\") into the message view");
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
#pragma warning disable SYSLIB0014
                WebClient client = new WebClient();
#pragma warning restore SYSLIB0014
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
