namespace VirtualRadar.WinForms.SettingPage
{
    partial class PageWebSiteInitialSettings
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.linkLabelDesktopSite = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.linkLabelMobileSite = new System.Windows.Forms.LinkLabel();
            this.linkLabelSettingsPage = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxInitialSettings = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.linkLabelCopyFromClipboard = new System.Windows.Forms.LinkLabel();
            this.linkLabelCopyToClipboard = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // linkLabelDesktopSite
            // 
            this.linkLabelDesktopSite.AutoSize = true;
            this.linkLabelDesktopSite.Location = new System.Drawing.Point(200, 0);
            this.linkLabelDesktopSite.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.linkLabelDesktopSite.Name = "linkLabelDesktopSite";
            this.linkLabelDesktopSite.Size = new System.Drawing.Size(122, 13);
            this.linkLabelDesktopSite.TabIndex = 1;
            this.linkLabelDesktopSite.TabStop = true;
            this.linkLabelDesktopSite.Text = "<DESKTOP SITE URL>";
            this.linkLabelDesktopSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelSite_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "::DesktopSite:::";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "::MobileSite:::";
            // 
            // linkLabelMobileSite
            // 
            this.linkLabelMobileSite.AutoSize = true;
            this.linkLabelMobileSite.Location = new System.Drawing.Point(200, 19);
            this.linkLabelMobileSite.Margin = new System.Windows.Forms.Padding(3);
            this.linkLabelMobileSite.Name = "linkLabelMobileSite";
            this.linkLabelMobileSite.Size = new System.Drawing.Size(111, 13);
            this.linkLabelMobileSite.TabIndex = 3;
            this.linkLabelMobileSite.TabStop = true;
            this.linkLabelMobileSite.Text = "<MOBILE SITE URL>";
            this.linkLabelMobileSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelSite_LinkClicked);
            // 
            // linkLabelSettingsPage
            // 
            this.linkLabelSettingsPage.AutoSize = true;
            this.linkLabelSettingsPage.Location = new System.Drawing.Point(200, 38);
            this.linkLabelSettingsPage.Margin = new System.Windows.Forms.Padding(3);
            this.linkLabelSettingsPage.Name = "linkLabelSettingsPage";
            this.linkLabelSettingsPage.Size = new System.Drawing.Size(130, 13);
            this.linkLabelSettingsPage.TabIndex = 5;
            this.linkLabelSettingsPage.TabStop = true;
            this.linkLabelSettingsPage.Text = "<SETTINGS PAGE URL>";
            this.linkLabelSettingsPage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelSite_LinkClicked);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "::SettingsSite:::";
            // 
            // textBoxInitialSettings
            // 
            this.textBoxInitialSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxInitialSettings.Location = new System.Drawing.Point(203, 80);
            this.textBoxInitialSettings.Multiline = true;
            this.textBoxInitialSettings.Name = "textBoxInitialSettings";
            this.textBoxInitialSettings.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxInitialSettings.Size = new System.Drawing.Size(433, 345);
            this.textBoxInitialSettings.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(0, 80);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(102, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "::ExportedSettings:::";
            // 
            // linkLabelCopyFromClipboard
            // 
            this.linkLabelCopyFromClipboard.AutoSize = true;
            this.linkLabelCopyFromClipboard.Location = new System.Drawing.Point(200, 61);
            this.linkLabelCopyFromClipboard.Margin = new System.Windows.Forms.Padding(3);
            this.linkLabelCopyFromClipboard.Name = "linkLabelCopyFromClipboard";
            this.linkLabelCopyFromClipboard.Size = new System.Drawing.Size(110, 13);
            this.linkLabelCopyFromClipboard.TabIndex = 7;
            this.linkLabelCopyFromClipboard.TabStop = true;
            this.linkLabelCopyFromClipboard.Text = "::CopyFromClipboard::";
            this.linkLabelCopyFromClipboard.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelCopyFromClipboard_LinkClicked);
            // 
            // linkLabelCopyToClipboard
            // 
            this.linkLabelCopyToClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelCopyToClipboard.AutoSize = true;
            this.linkLabelCopyToClipboard.Location = new System.Drawing.Point(200, 431);
            this.linkLabelCopyToClipboard.Margin = new System.Windows.Forms.Padding(3);
            this.linkLabelCopyToClipboard.Name = "linkLabelCopyToClipboard";
            this.linkLabelCopyToClipboard.Size = new System.Drawing.Size(100, 13);
            this.linkLabelCopyToClipboard.TabIndex = 9;
            this.linkLabelCopyToClipboard.TabStop = true;
            this.linkLabelCopyToClipboard.Text = "::CopyToClipboard::";
            this.linkLabelCopyToClipboard.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelCopyToClipboard_LinkClicked);
            // 
            // PageWebSiteGoogleMaps
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.linkLabelCopyToClipboard);
            this.Controls.Add(this.linkLabelCopyFromClipboard);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxInitialSettings);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.linkLabelSettingsPage);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.linkLabelMobileSite);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.linkLabelDesktopSite);
            this.Name = "PageWebSiteGoogleMaps";
            this.Size = new System.Drawing.Size(636, 447);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel linkLabelDesktopSite;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel linkLabelMobileSite;
        private System.Windows.Forms.LinkLabel linkLabelSettingsPage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxInitialSettings;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.LinkLabel linkLabelCopyFromClipboard;
        private System.Windows.Forms.LinkLabel linkLabelCopyToClipboard;


    }
}
