namespace VirtualRadar.WinForms.SettingPage
{
    partial class PageDataSources
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
            this.checkBoxSearchPictureSubFolders = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.folderFlags = new VirtualRadar.WinForms.Controls.FolderControl();
            this.folderPictures = new VirtualRadar.WinForms.Controls.FolderControl();
            this.fileDatabaseFileName = new VirtualRadar.WinForms.Controls.FileNameControl();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.folderSilhouettes = new VirtualRadar.WinForms.Controls.FolderControl();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxLookupAircraftDetailsOnline = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.labelAircraftLookupSupplierCredits = new System.Windows.Forms.Label();
            this.linkLabelAircraftLookupSupplierUrl = new System.Windows.Forms.LinkLabel();
            this.label6 = new System.Windows.Forms.Label();
            this.labelAircraftLookupDataProvider = new System.Windows.Forms.Label();
            this.checkBoxDownloadWeather = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxOpenStreetMapTileServerUrl = new System.Windows.Forms.TextBox();
            this.checkBoxUseGoogleMapsKeyWithLocalRequests = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.textBoxGoogleMapsAPIKey = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBoxMapProvider = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBoxSearchPictureSubFolders
            // 
            this.checkBoxSearchPictureSubFolders.AutoSize = true;
            this.checkBoxSearchPictureSubFolders.Location = new System.Drawing.Point(200, 124);
            this.checkBoxSearchPictureSubFolders.Name = "checkBoxSearchPictureSubFolders";
            this.checkBoxSearchPictureSubFolders.Size = new System.Drawing.Size(158, 17);
            this.checkBoxSearchPictureSubFolders.TabIndex = 8;
            this.checkBoxSearchPictureSubFolders.Text = "::SearchPictureSubFolders::";
            this.checkBoxSearchPictureSubFolders.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 103);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "::PicturesFolder:::";
            // 
            // folderFlags
            // 
            this.folderFlags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.folderFlags.Location = new System.Drawing.Point(200, 46);
            this.folderFlags.Name = "folderFlags";
            this.folderFlags.Size = new System.Drawing.Size(499, 20);
            this.folderFlags.TabIndex = 3;
            // 
            // folderPictures
            // 
            this.folderPictures.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.folderPictures.Location = new System.Drawing.Point(200, 98);
            this.folderPictures.Name = "folderPictures";
            this.folderPictures.Size = new System.Drawing.Size(499, 20);
            this.folderPictures.TabIndex = 7;
            // 
            // fileDatabaseFileName
            // 
            this.fileDatabaseFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileDatabaseFileName.Location = new System.Drawing.Point(200, 19);
            this.fileDatabaseFileName.Name = "fileDatabaseFileName";
            this.fileDatabaseFileName.Size = new System.Drawing.Size(499, 20);
            this.fileDatabaseFileName.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "::SilhouettesFolder:::";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "::DatabaseFileName:::";
            // 
            // folderSilhouettes
            // 
            this.folderSilhouettes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.folderSilhouettes.Location = new System.Drawing.Point(200, 72);
            this.folderSilhouettes.Name = "folderSilhouettes";
            this.folderSilhouettes.Size = new System.Drawing.Size(499, 20);
            this.folderSilhouettes.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "::FlagsFolder:::";
            // 
            // checkBoxLookupAircraftDetailsOnline
            // 
            this.checkBoxLookupAircraftDetailsOnline.AutoSize = true;
            this.checkBoxLookupAircraftDetailsOnline.Location = new System.Drawing.Point(200, 19);
            this.checkBoxLookupAircraftDetailsOnline.Name = "checkBoxLookupAircraftDetailsOnline";
            this.checkBoxLookupAircraftDetailsOnline.Size = new System.Drawing.Size(169, 17);
            this.checkBoxLookupAircraftDetailsOnline.TabIndex = 0;
            this.checkBoxLookupAircraftDetailsOnline.Text = "::LookupAircraftDetailsOnline::";
            this.checkBoxLookupAircraftDetailsOnline.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.labelAircraftLookupSupplierCredits);
            this.groupBox1.Controls.Add(this.linkLabelAircraftLookupSupplierUrl);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.labelAircraftLookupDataProvider);
            this.groupBox1.Controls.Add(this.checkBoxLookupAircraftDetailsOnline);
            this.groupBox1.Location = new System.Drawing.Point(0, 308);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(705, 115);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "::AircraftDetailsOnlineLookup::";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 61);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "::WebSiteUrl:::";
            // 
            // labelAircraftLookupSupplierCredits
            // 
            this.labelAircraftLookupSupplierCredits.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAircraftLookupSupplierCredits.Location = new System.Drawing.Point(197, 80);
            this.labelAircraftLookupSupplierCredits.Margin = new System.Windows.Forms.Padding(3);
            this.labelAircraftLookupSupplierCredits.Name = "labelAircraftLookupSupplierCredits";
            this.labelAircraftLookupSupplierCredits.Size = new System.Drawing.Size(502, 32);
            this.labelAircraftLookupSupplierCredits.TabIndex = 5;
            this.labelAircraftLookupSupplierCredits.Text = "Line1\r\nLine2";
            // 
            // linkLabelAircraftLookupSupplierUrl
            // 
            this.linkLabelAircraftLookupSupplierUrl.AutoSize = true;
            this.linkLabelAircraftLookupSupplierUrl.Location = new System.Drawing.Point(197, 61);
            this.linkLabelAircraftLookupSupplierUrl.Margin = new System.Windows.Forms.Padding(3);
            this.linkLabelAircraftLookupSupplierUrl.Name = "linkLabelAircraftLookupSupplierUrl";
            this.linkLabelAircraftLookupSupplierUrl.Size = new System.Drawing.Size(19, 13);
            this.linkLabelAircraftLookupSupplierUrl.TabIndex = 4;
            this.linkLabelAircraftLookupSupplierUrl.TabStop = true;
            this.linkLabelAircraftLookupSupplierUrl.Text = "<>";
            this.linkLabelAircraftLookupSupplierUrl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelAircraftLookupSupplierUrl_LinkClicked);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 42);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(84, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "::DataProvider:::";
            // 
            // labelAircraftLookupDataProvider
            // 
            this.labelAircraftLookupDataProvider.AutoSize = true;
            this.labelAircraftLookupDataProvider.Location = new System.Drawing.Point(197, 42);
            this.labelAircraftLookupDataProvider.Margin = new System.Windows.Forms.Padding(3);
            this.labelAircraftLookupDataProvider.Name = "labelAircraftLookupDataProvider";
            this.labelAircraftLookupDataProvider.Size = new System.Drawing.Size(65, 13);
            this.labelAircraftLookupDataProvider.TabIndex = 2;
            this.labelAircraftLookupDataProvider.Text = "::Unknown::";
            // 
            // checkBoxDownloadWeather
            // 
            this.checkBoxDownloadWeather.AutoSize = true;
            this.checkBoxDownloadWeather.Location = new System.Drawing.Point(200, 147);
            this.checkBoxDownloadWeather.Name = "checkBoxDownloadWeather";
            this.checkBoxDownloadWeather.Size = new System.Drawing.Size(214, 17);
            this.checkBoxDownloadWeather.TabIndex = 9;
            this.checkBoxDownloadWeather.Text = "::DownloadGlobalAirPressureReadings::";
            this.checkBoxDownloadWeather.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.fileDatabaseFileName);
            this.groupBox2.Controls.Add(this.checkBoxDownloadWeather);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.folderSilhouettes);
            this.groupBox2.Controls.Add(this.checkBoxSearchPictureSubFolders);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.folderFlags);
            this.groupBox2.Controls.Add(this.folderPictures);
            this.groupBox2.Location = new System.Drawing.Point(0, 132);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(705, 170);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "::OptionsDataSourcesSheetTitle::";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.textBoxOpenStreetMapTileServerUrl);
            this.groupBox3.Controls.Add(this.checkBoxUseGoogleMapsKeyWithLocalRequests);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.textBoxGoogleMapsAPIKey);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.comboBoxMapProvider);
            this.groupBox3.Location = new System.Drawing.Point(0, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(705, 123);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "::MapProviderTitle::";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 98);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(158, 13);
            this.label8.TabIndex = 5;
            this.label8.Text = "::OpenStreetMapTileServerUrl:::";
            // 
            // textBoxOpenStreetMapTileServerUrl
            // 
            this.textBoxOpenStreetMapTileServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOpenStreetMapTileServerUrl.Location = new System.Drawing.Point(200, 95);
            this.textBoxOpenStreetMapTileServerUrl.MaxLength = 60;
            this.textBoxOpenStreetMapTileServerUrl.Name = "textBoxOpenStreetMapTileServerUrl";
            this.textBoxOpenStreetMapTileServerUrl.Size = new System.Drawing.Size(499, 20);
            this.textBoxOpenStreetMapTileServerUrl.TabIndex = 6;
            // 
            // checkBoxUseGoogleMapsKeyWithLocalRequests
            // 
            this.checkBoxUseGoogleMapsKeyWithLocalRequests.AutoSize = true;
            this.checkBoxUseGoogleMapsKeyWithLocalRequests.Location = new System.Drawing.Point(200, 72);
            this.checkBoxUseGoogleMapsKeyWithLocalRequests.Name = "checkBoxUseGoogleMapsKeyWithLocalRequests";
            this.checkBoxUseGoogleMapsKeyWithLocalRequests.Size = new System.Drawing.Size(228, 17);
            this.checkBoxUseGoogleMapsKeyWithLocalRequests.TabIndex = 4;
            this.checkBoxUseGoogleMapsKeyWithLocalRequests.Text = "::UseGoogleMapsKeyWithLocalRequests::";
            this.checkBoxUseGoogleMapsKeyWithLocalRequests.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 49);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(117, 13);
            this.label11.TabIndex = 2;
            this.label11.Text = "::GoogleMapsAPIKey:::";
            // 
            // textBoxGoogleMapsAPIKey
            // 
            this.textBoxGoogleMapsAPIKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxGoogleMapsAPIKey.Location = new System.Drawing.Point(200, 46);
            this.textBoxGoogleMapsAPIKey.MaxLength = 60;
            this.textBoxGoogleMapsAPIKey.Name = "textBoxGoogleMapsAPIKey";
            this.textBoxGoogleMapsAPIKey.Size = new System.Drawing.Size(499, 20);
            this.textBoxGoogleMapsAPIKey.TabIndex = 3;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 22);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(82, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "::MapProvider:::";
            // 
            // comboBoxMapProvider
            // 
            this.comboBoxMapProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMapProvider.FormattingEnabled = true;
            this.comboBoxMapProvider.Location = new System.Drawing.Point(200, 19);
            this.comboBoxMapProvider.Name = "comboBoxMapProvider";
            this.comboBoxMapProvider.Size = new System.Drawing.Size(249, 21);
            this.comboBoxMapProvider.TabIndex = 1;
            // 
            // PageDataSources
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "PageDataSources";
            this.Size = new System.Drawing.Size(705, 427);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxSearchPictureSubFolders;
        private System.Windows.Forms.Label label4;
        private Controls.FolderControl folderFlags;
        private Controls.FolderControl folderPictures;
        private Controls.FileNameControl fileDatabaseFileName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private Controls.FolderControl folderSilhouettes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBoxLookupAircraftDetailsOnline;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label labelAircraftLookupDataProvider;
        private System.Windows.Forms.LinkLabel linkLabelAircraftLookupSupplierUrl;
        private System.Windows.Forms.Label labelAircraftLookupSupplierCredits;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBoxDownloadWeather;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox comboBoxMapProvider;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBoxOpenStreetMapTileServerUrl;
        private System.Windows.Forms.CheckBox checkBoxUseGoogleMapsKeyWithLocalRequests;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox textBoxGoogleMapsAPIKey;
    }
}
