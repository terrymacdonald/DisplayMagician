namespace DisplayMagician.UIForms
{
    partial class PairDeviceForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            if (disposing)
            {
                pb_qr_code.Image?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            tab_pairing = new System.Windows.Forms.TabControl();
            tab_new_device = new System.Windows.Forms.TabPage();
            rdo_remote_network = new System.Windows.Forms.RadioButton();
            rdo_local_lan = new System.Windows.Forms.RadioButton();
            pb_qr_code = new System.Windows.Forms.PictureBox();
            lbl_qr_endpoint = new System.Windows.Forms.Label();
            lbl_qr_expiry = new System.Windows.Forms.Label();
            lbl_qr_result = new System.Windows.Forms.Label();
            tab_requests = new System.Windows.Forms.TabPage();
            lv_pairing_requests = new System.Windows.Forms.ListView();
            col_device = new System.Windows.Forms.ColumnHeader();
            col_capabilities = new System.Windows.Forms.ColumnHeader();
            col_expires = new System.Windows.Forms.ColumnHeader();
            btn_refresh_requests = new System.Windows.Forms.Button();
            btn_approve_request = new System.Windows.Forms.Button();
            btn_reject_request = new System.Windows.Forms.Button();
            lbl_request_result = new System.Windows.Forms.Label();
            btn_close = new System.Windows.Forms.Button();
            tab_pairing.SuspendLayout();
            tab_new_device.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pb_qr_code).BeginInit();
            tab_requests.SuspendLayout();
            SuspendLayout();
            tab_pairing.Controls.Add(tab_new_device);
            tab_pairing.Controls.Add(tab_requests);
            tab_pairing.Location = new System.Drawing.Point(12, 12);
            tab_pairing.Name = "tab_pairing";
            tab_pairing.SelectedIndex = 0;
            tab_pairing.Size = new System.Drawing.Size(620, 510);
            tab_new_device.BackColor = System.Drawing.Color.Black;
            tab_new_device.Controls.Add(rdo_remote_network);
            tab_new_device.Controls.Add(rdo_local_lan);
            tab_new_device.Controls.Add(pb_qr_code);
            tab_new_device.Controls.Add(lbl_qr_endpoint);
            tab_new_device.Controls.Add(lbl_qr_expiry);
            tab_new_device.Controls.Add(lbl_qr_result);
            tab_new_device.Location = new System.Drawing.Point(4, 24);
            tab_new_device.Name = "tab_new_device";
            tab_new_device.Size = new System.Drawing.Size(612, 482);
            tab_new_device.Text = "Pair New Device";
            rdo_local_lan.AutoSize = true;
            rdo_local_lan.Checked = true;
            rdo_local_lan.ForeColor = System.Drawing.Color.White;
            rdo_local_lan.Location = new System.Drawing.Point(20, 18);
            rdo_local_lan.Name = "rdo_local_lan";
            rdo_local_lan.Text = "Local LAN";
            rdo_local_lan.CheckedChanged += rdo_gateway_location_CheckedChanged;
            rdo_remote_network.AutoSize = true;
            rdo_remote_network.ForeColor = System.Drawing.Color.White;
            rdo_remote_network.Location = new System.Drawing.Point(140, 18);
            rdo_remote_network.Name = "rdo_remote_network";
            rdo_remote_network.Text = "Remote Network";
            rdo_remote_network.CheckedChanged += rdo_gateway_location_CheckedChanged;
            pb_qr_code.BackColor = System.Drawing.Color.White;
            pb_qr_code.Location = new System.Drawing.Point(166, 50);
            pb_qr_code.Name = "pb_qr_code";
            pb_qr_code.Size = new System.Drawing.Size(280, 280);
            pb_qr_code.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            lbl_qr_endpoint.ForeColor = System.Drawing.Color.White;
            lbl_qr_endpoint.Location = new System.Drawing.Point(20, 350);
            lbl_qr_endpoint.Name = "lbl_qr_endpoint";
            lbl_qr_endpoint.Size = new System.Drawing.Size(572, 24);
            lbl_qr_endpoint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbl_qr_expiry.ForeColor = System.Drawing.Color.LightGray;
            lbl_qr_expiry.Location = new System.Drawing.Point(20, 378);
            lbl_qr_expiry.Name = "lbl_qr_expiry";
            lbl_qr_expiry.Size = new System.Drawing.Size(572, 24);
            lbl_qr_expiry.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbl_qr_result.ForeColor = System.Drawing.Color.White;
            lbl_qr_result.Location = new System.Drawing.Point(20, 410);
            lbl_qr_result.Name = "lbl_qr_result";
            lbl_qr_result.Size = new System.Drawing.Size(572, 46);
            lbl_qr_result.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            tab_requests.BackColor = System.Drawing.Color.Black;
            tab_requests.Controls.Add(lv_pairing_requests);
            tab_requests.Controls.Add(btn_refresh_requests);
            tab_requests.Controls.Add(btn_approve_request);
            tab_requests.Controls.Add(btn_reject_request);
            tab_requests.Controls.Add(lbl_request_result);
            tab_requests.Location = new System.Drawing.Point(4, 24);
            tab_requests.Name = "tab_requests";
            tab_requests.Size = new System.Drawing.Size(612, 482);
            tab_requests.Text = "Pending Requests";
            lv_pairing_requests.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { col_device, col_capabilities, col_expires });
            lv_pairing_requests.FullRowSelect = true;
            lv_pairing_requests.Location = new System.Drawing.Point(14, 16);
            lv_pairing_requests.MultiSelect = false;
            lv_pairing_requests.Name = "lv_pairing_requests";
            lv_pairing_requests.Size = new System.Drawing.Size(584, 330);
            lv_pairing_requests.UseCompatibleStateImageBehavior = false;
            lv_pairing_requests.View = System.Windows.Forms.View.Details;
            col_device.Text = "Device";
            col_device.Width = 150;
            col_capabilities.Text = "Requested capabilities";
            col_capabilities.Width = 290;
            col_expires.Text = "Expires";
            col_expires.Width = 130;
            btn_refresh_requests.Location = new System.Drawing.Point(14, 365);
            btn_refresh_requests.Name = "btn_refresh_requests";
            btn_refresh_requests.Size = new System.Drawing.Size(120, 30);
            btn_refresh_requests.Text = "Refresh";
            btn_refresh_requests.Click += btn_refresh_requests_Click;
            btn_approve_request.Location = new System.Drawing.Point(330, 365);
            btn_approve_request.Name = "btn_approve_request";
            btn_approve_request.Size = new System.Drawing.Size(120, 30);
            btn_approve_request.Text = "Approve";
            btn_approve_request.Click += btn_approve_request_Click;
            btn_reject_request.Location = new System.Drawing.Point(478, 365);
            btn_reject_request.Name = "btn_reject_request";
            btn_reject_request.Size = new System.Drawing.Size(120, 30);
            btn_reject_request.Text = "Reject";
            btn_reject_request.Click += btn_reject_request_Click;
            lbl_request_result.ForeColor = System.Drawing.Color.White;
            lbl_request_result.Location = new System.Drawing.Point(14, 410);
            lbl_request_result.Name = "lbl_request_result";
            lbl_request_result.Size = new System.Drawing.Size(584, 46);
            lbl_request_result.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            btn_close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_close.Location = new System.Drawing.Point(544, 534);
            btn_close.Name = "btn_close";
            btn_close.Size = new System.Drawing.Size(88, 28);
            btn_close.Text = "Close";
            btn_close.Click += btn_close_Click;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.Black;
            CancelButton = btn_close;
            ClientSize = new System.Drawing.Size(644, 574);
            Controls.Add(tab_pairing);
            Controls.Add(btn_close);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PairDeviceForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Pair Remote Device";
            Load += PairDeviceForm_Load;
            tab_pairing.ResumeLayout(false);
            tab_new_device.ResumeLayout(false);
            tab_new_device.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pb_qr_code).EndInit();
            tab_requests.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.TabControl tab_pairing;
        private System.Windows.Forms.TabPage tab_new_device;
        private System.Windows.Forms.RadioButton rdo_remote_network;
        private System.Windows.Forms.RadioButton rdo_local_lan;
        private System.Windows.Forms.PictureBox pb_qr_code;
        private System.Windows.Forms.Label lbl_qr_endpoint;
        private System.Windows.Forms.Label lbl_qr_expiry;
        private System.Windows.Forms.Label lbl_qr_result;
        private System.Windows.Forms.TabPage tab_requests;
        private System.Windows.Forms.ListView lv_pairing_requests;
        private System.Windows.Forms.ColumnHeader col_device;
        private System.Windows.Forms.ColumnHeader col_capabilities;
        private System.Windows.Forms.ColumnHeader col_expires;
        private System.Windows.Forms.Button btn_refresh_requests;
        private System.Windows.Forms.Button btn_approve_request;
        private System.Windows.Forms.Button btn_reject_request;
        private System.Windows.Forms.Label lbl_request_result;
        private System.Windows.Forms.Button btn_close;
    }
}
