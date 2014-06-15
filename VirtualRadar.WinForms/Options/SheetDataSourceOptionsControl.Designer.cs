namespace VirtualRadar.WinForms.Options
{
    partial class SheetDataSourceOptionsControl
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
            this.fileDatabaseFileName = new VirtualRadar.WinForms.Controls.FileNameControl();
            this.label1 = new System.Windows.Forms.Label();
            this.folderFlags = new VirtualRadar.WinForms.Controls.FolderControl();
            this.label2 = new System.Windows.Forms.Label();
            this.folderSilhouettes = new VirtualRadar.WinForms.Controls.FolderControl();
            this.label3 = new System.Windows.Forms.Label();
            this.folderPictures = new VirtualRadar.WinForms.Controls.FolderControl();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBoxSearchPictureSubFolders = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panelContent.SuspendLayout();
            this.splitContainerControlsDescription.Panel1.SuspendLayout();
            this.splitContainerControlsDescription.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningProvider)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelContent
            // 
            this.panelContent.Controls.Add(this.groupBox1);
            this.panelContent.Size = new System.Drawing.Size(493, 191);
            // 
            // splitContainerControlsDescription
            // 
            this.splitContainerControlsDescription.Size = new System.Drawing.Size(493, 280);
            this.splitContainerControlsDescription.SplitterDistance = 194;
            // 
            // fileDatabaseFileName
            // 
            this.fileDatabaseFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.warningProvider.SetIconAlignment(this.fileDatabaseFileName, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.errorProvider.SetIconAlignment(this.fileDatabaseFileName, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.fileDatabaseFileName.Location = new System.Drawing.Point(192, 19);
            this.fileDatabaseFileName.Name = "fileDatabaseFileName";
            this.fileDatabaseFileName.Size = new System.Drawing.Size(295, 20);
            this.fileDatabaseFileName.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "::DatabaseFileName:::";
            // 
            // folderFlags
            // 
            this.folderFlags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.errorProvider.SetIconAlignment(this.folderFlags, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.warningProvider.SetIconAlignment(this.folderFlags, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.folderFlags.Location = new System.Drawing.Point(192, 46);
            this.folderFlags.Name = "folderFlags";
            this.folderFlags.Size = new System.Drawing.Size(295, 20);
            this.folderFlags.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "::FlagsFolder:::";
            // 
            // folderSilhouettes
            // 
            this.folderSilhouettes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.errorProvider.SetIconAlignment(this.folderSilhouettes, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.warningProvider.SetIconAlignment(this.folderSilhouettes, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.folderSilhouettes.Location = new System.Drawing.Point(192, 72);
            this.folderSilhouettes.Name = "folderSilhouettes";
            this.folderSilhouettes.Size = new System.Drawing.Size(295, 20);
            this.folderSilhouettes.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "::SilhouettesFolder:::";
            // 
            // folderPictures
            // 
            this.folderPictures.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.errorProvider.SetIconAlignment(this.folderPictures, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.warningProvider.SetIconAlignment(this.folderPictures, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.folderPictures.Location = new System.Drawing.Point(192, 98);
            this.folderPictures.Name = "folderPictures";
            this.folderPictures.Size = new System.Drawing.Size(295, 20);
            this.folderPictures.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 103);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "::PicturesFolder:::";
            // 
            // checkBoxSearchPictureSubFolders
            // 
            this.checkBoxSearchPictureSubFolders.AutoSize = true;
            this.errorProvider.SetIconAlignment(this.checkBoxSearchPictureSubFolders, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.warningProvider.SetIconAlignment(this.checkBoxSearchPictureSubFolders, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.checkBoxSearchPictureSubFolders.Location = new System.Drawing.Point(192, 125);
            this.checkBoxSearchPictureSubFolders.Name = "checkBoxSearchPictureSubFolders";
            this.checkBoxSearchPictureSubFolders.Size = new System.Drawing.Size(158, 17);
            this.checkBoxSearchPictureSubFolders.TabIndex = 8;
            this.checkBoxSearchPictureSubFolders.Text = "::SearchPictureSubFolders::";
            this.checkBoxSearchPictureSubFolders.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBoxSearchPictureSubFolders);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.folderPictures);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.folderSilhouettes);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.folderFlags);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.fileDatabaseFileName);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(493, 148);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "::AircraftData::";
            // 
            // SheetDataSourceOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "SheetDataSourceOptionsControl";
            this.Size = new System.Drawing.Size(493, 280);
            this.panelContent.ResumeLayout(false);
            this.splitContainerControlsDescription.Panel1.ResumeLayout(false);
            this.splitContainerControlsDescription.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningProvider)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.FileNameControl fileDatabaseFileName;
        private System.Windows.Forms.Label label1;
        private Controls.FolderControl folderFlags;
        private System.Windows.Forms.Label label2;
        private Controls.FolderControl folderSilhouettes;
        private System.Windows.Forms.Label label3;
        private Controls.FolderControl folderPictures;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBoxSearchPictureSubFolders;
        private System.Windows.Forms.GroupBox groupBox1;

    }
}
