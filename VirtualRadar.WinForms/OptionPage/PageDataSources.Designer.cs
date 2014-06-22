namespace VirtualRadar.WinForms.OptionPage
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxSearchPictureSubFolders = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.folderPictures = new VirtualRadar.WinForms.Controls.FolderControl();
            this.label3 = new System.Windows.Forms.Label();
            this.folderSilhouettes = new VirtualRadar.WinForms.Controls.FolderControl();
            this.label2 = new System.Windows.Forms.Label();
            this.folderFlags = new VirtualRadar.WinForms.Controls.FolderControl();
            this.label1 = new System.Windows.Forms.Label();
            this.fileDatabaseFileName = new VirtualRadar.WinForms.Controls.FileNameControl();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxSearchPictureSubFolders);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.folderPictures);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.folderSilhouettes);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.folderFlags);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.fileDatabaseFileName);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(705, 149);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "::AircraftData::";
            // 
            // checkBoxSearchPictureSubFolders
            // 
            this.checkBoxSearchPictureSubFolders.AutoSize = true;
            this.checkBoxSearchPictureSubFolders.Location = new System.Drawing.Point(192, 125);
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
            this.label4.TabIndex = 7;
            this.label4.Text = "::PicturesFolder:::";
            // 
            // folderPictures
            // 
            this.folderPictures.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.folderPictures.Location = new System.Drawing.Point(192, 98);
            this.folderPictures.Name = "folderPictures";
            this.folderPictures.Size = new System.Drawing.Size(507, 20);
            this.folderPictures.TabIndex = 6;
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
            // folderSilhouettes
            // 
            this.folderSilhouettes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.folderSilhouettes.Location = new System.Drawing.Point(192, 72);
            this.folderSilhouettes.Name = "folderSilhouettes";
            this.folderSilhouettes.Size = new System.Drawing.Size(507, 20);
            this.folderSilhouettes.TabIndex = 4;
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
            // folderFlags
            // 
            this.folderFlags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.folderFlags.Location = new System.Drawing.Point(192, 46);
            this.folderFlags.Name = "folderFlags";
            this.folderFlags.Size = new System.Drawing.Size(507, 20);
            this.folderFlags.TabIndex = 2;
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
            // fileDatabaseFileName
            // 
            this.fileDatabaseFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileDatabaseFileName.Location = new System.Drawing.Point(192, 19);
            this.fileDatabaseFileName.Name = "fileDatabaseFileName";
            this.fileDatabaseFileName.Size = new System.Drawing.Size(507, 20);
            this.fileDatabaseFileName.TabIndex = 0;
            // 
            // PageDataSources
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.groupBox1);
            this.Name = "PageDataSources";
            this.Size = new System.Drawing.Size(705, 149);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxSearchPictureSubFolders;
        private System.Windows.Forms.Label label4;
        private Controls.FolderControl folderPictures;
        private System.Windows.Forms.Label label3;
        private Controls.FolderControl folderSilhouettes;
        private System.Windows.Forms.Label label2;
        private Controls.FolderControl folderFlags;
        private System.Windows.Forms.Label label1;
        private Controls.FileNameControl fileDatabaseFileName;
    }
}
