namespace VirtualRadar.Plugin.TileServerCache.WinForms
{
    partial class OptionsView
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.chkPluginEnabled = new System.Windows.Forms.CheckBox();
            this.chkOfflineModeEnabled = new System.Windows.Forms.CheckBox();
            this.chkUseDefaultCacheFolder = new System.Windows.Forms.CheckBox();
            this.fldCacheFolderOverride = new VirtualRadar.WinForms.Controls.FolderControl();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.nudTileServerTimeoutSeconds = new System.Windows.Forms.NumericUpDown();
            this.errErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.chkCacheLayerTiles = new System.Windows.Forms.CheckBox();
            this.btnRecentRequests = new System.Windows.Forms.Button();
            this.chkCacheMapTiles = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudTileServerTimeoutSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(513, 191);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "::Cancel::";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(432, 191);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 11;
            this.buttonOK.Text = "::OK::";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // chkPluginEnabled
            // 
            this.chkPluginEnabled.AutoSize = true;
            this.errErrorProvider.SetIconAlignment(this.chkPluginEnabled, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.chkPluginEnabled.Location = new System.Drawing.Point(181, 12);
            this.chkPluginEnabled.Name = "chkPluginEnabled";
            this.chkPluginEnabled.Size = new System.Drawing.Size(106, 17);
            this.chkPluginEnabled.TabIndex = 0;
            this.chkPluginEnabled.Text = "::PluginEnabled::";
            this.chkPluginEnabled.UseVisualStyleBackColor = true;
            // 
            // chkOfflineModeEnabled
            // 
            this.chkOfflineModeEnabled.AutoSize = true;
            this.errErrorProvider.SetIconAlignment(this.chkOfflineModeEnabled, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.chkOfflineModeEnabled.Location = new System.Drawing.Point(181, 35);
            this.chkOfflineModeEnabled.Name = "chkOfflineModeEnabled";
            this.chkOfflineModeEnabled.Size = new System.Drawing.Size(134, 17);
            this.chkOfflineModeEnabled.TabIndex = 1;
            this.chkOfflineModeEnabled.Text = "::OfflineModeEnabled::";
            this.chkOfflineModeEnabled.UseVisualStyleBackColor = true;
            // 
            // chkUseDefaultCacheFolder
            // 
            this.chkUseDefaultCacheFolder.AutoSize = true;
            this.errErrorProvider.SetIconAlignment(this.chkUseDefaultCacheFolder, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.chkUseDefaultCacheFolder.Location = new System.Drawing.Point(181, 58);
            this.chkUseDefaultCacheFolder.Name = "chkUseDefaultCacheFolder";
            this.chkUseDefaultCacheFolder.Size = new System.Drawing.Size(151, 17);
            this.chkUseDefaultCacheFolder.TabIndex = 2;
            this.chkUseDefaultCacheFolder.Text = "::UseDefaultCacheFolder::";
            this.chkUseDefaultCacheFolder.UseVisualStyleBackColor = true;
            this.chkUseDefaultCacheFolder.CheckedChanged += new System.EventHandler(this.ChkUseDefaultCacheFolder_CheckedChanged);
            // 
            // fldCacheFolderOverride
            // 
            this.fldCacheFolderOverride.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.errErrorProvider.SetIconAlignment(this.fldCacheFolderOverride, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.fldCacheFolderOverride.Location = new System.Drawing.Point(181, 81);
            this.fldCacheFolderOverride.Name = "fldCacheFolderOverride";
            this.fldCacheFolderOverride.Size = new System.Drawing.Size(407, 20);
            this.fldCacheFolderOverride.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 86);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "::CacheFolderOverride:::";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(264, 109);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "::Seconds::";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 110);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(150, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "::TileServerTimeoutSeconds:::";
            // 
            // nudTileServerTimeoutSeconds
            // 
            this.errErrorProvider.SetIconAlignment(this.nudTileServerTimeoutSeconds, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.nudTileServerTimeoutSeconds.Location = new System.Drawing.Point(181, 107);
            this.nudTileServerTimeoutSeconds.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudTileServerTimeoutSeconds.Name = "nudTileServerTimeoutSeconds";
            this.nudTileServerTimeoutSeconds.Size = new System.Drawing.Size(77, 20);
            this.nudTileServerTimeoutSeconds.TabIndex = 6;
            this.nudTileServerTimeoutSeconds.ThousandsSeparator = true;
            // 
            // errErrorProvider
            // 
            this.errErrorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errErrorProvider.ContainerControl = this;
            // 
            // chkCacheLayerTiles
            // 
            this.chkCacheLayerTiles.AutoSize = true;
            this.errErrorProvider.SetIconAlignment(this.chkCacheLayerTiles, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.chkCacheLayerTiles.Location = new System.Drawing.Point(181, 156);
            this.chkCacheLayerTiles.Name = "chkCacheLayerTiles";
            this.chkCacheLayerTiles.Size = new System.Drawing.Size(117, 17);
            this.chkCacheLayerTiles.TabIndex = 9;
            this.chkCacheLayerTiles.Text = "::CacheLayerTiles::";
            this.chkCacheLayerTiles.UseVisualStyleBackColor = true;
            // 
            // btnRecentRequests
            // 
            this.btnRecentRequests.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRecentRequests.Location = new System.Drawing.Point(15, 191);
            this.btnRecentRequests.Name = "btnRecentRequests";
            this.btnRecentRequests.Size = new System.Drawing.Size(147, 23);
            this.btnRecentRequests.TabIndex = 10;
            this.btnRecentRequests.Text = "::RecentRequests::";
            this.btnRecentRequests.UseVisualStyleBackColor = true;
            this.btnRecentRequests.Click += new System.EventHandler(this.BtnRecentRequests_Click);
            // 
            // chkCacheMapTiles
            // 
            this.chkCacheMapTiles.AutoSize = true;
            this.errErrorProvider.SetIconAlignment(this.chkCacheMapTiles, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.chkCacheMapTiles.Location = new System.Drawing.Point(181, 133);
            this.chkCacheMapTiles.Name = "chkCacheMapTiles";
            this.chkCacheMapTiles.Size = new System.Drawing.Size(112, 17);
            this.chkCacheMapTiles.TabIndex = 8;
            this.chkCacheMapTiles.Text = "::CacheMapTiles::";
            this.chkCacheMapTiles.UseVisualStyleBackColor = true;
            // 
            // OptionsView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 226);
            this.Controls.Add(this.chkCacheMapTiles);
            this.Controls.Add(this.chkCacheLayerTiles);
            this.Controls.Add(this.btnRecentRequests);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.nudTileServerTimeoutSeconds);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.fldCacheFolderOverride);
            this.Controls.Add(this.chkUseDefaultCacheFolder);
            this.Controls.Add(this.chkOfflineModeEnabled);
            this.Controls.Add(this.chkPluginEnabled);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsView";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "::OptionsDialogTitle::";
            ((System.ComponentModel.ISupportInitialize)(this.nudTileServerTimeoutSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errErrorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.CheckBox chkPluginEnabled;
        private System.Windows.Forms.CheckBox chkOfflineModeEnabled;
        private System.Windows.Forms.CheckBox chkUseDefaultCacheFolder;
        private VirtualRadar.WinForms.Controls.FolderControl fldCacheFolderOverride;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nudTileServerTimeoutSeconds;
        private System.Windows.Forms.ErrorProvider errErrorProvider;
        private System.Windows.Forms.Button btnRecentRequests;
        private System.Windows.Forms.CheckBox chkCacheLayerTiles;
        private System.Windows.Forms.CheckBox chkCacheMapTiles;
    }
}
