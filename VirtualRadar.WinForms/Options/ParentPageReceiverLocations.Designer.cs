namespace VirtualRadar.WinForms.Options
{
    partial class ParentPageReceiverLocations
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
            this.buttonNew = new System.Windows.Forms.Button();
            this.linkLabelUpdateFromDatabase = new System.Windows.Forms.LinkLabel();
            this.splitContainerControlsDescription.Panel1.SuspendLayout();
            this.splitContainerControlsDescription.SuspendLayout();
            this.panelContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainerControlsDescription
            // 
            this.splitContainerControlsDescription.Size = new System.Drawing.Size(254, 189);
            this.splitContainerControlsDescription.SplitterDistance = 121;
            // 
            // panelContent
            // 
            this.panelContent.Controls.Add(this.buttonNew);
            this.panelContent.Controls.Add(this.linkLabelUpdateFromDatabase);
            this.panelContent.Size = new System.Drawing.Size(254, 121);
            // 
            // buttonNew
            // 
            this.buttonNew.Location = new System.Drawing.Point(4, 4);
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Size = new System.Drawing.Size(75, 23);
            this.buttonNew.TabIndex = 0;
            this.buttonNew.Text = "::New::";
            this.buttonNew.UseVisualStyleBackColor = true;
            this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
            // 
            // linkLabelUpdateFromDatabase
            // 
            this.linkLabelUpdateFromDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelUpdateFromDatabase.AutoSize = true;
            this.linkLabelUpdateFromDatabase.Location = new System.Drawing.Point(5, 105);
            this.linkLabelUpdateFromDatabase.Name = "linkLabelUpdateFromDatabase";
            this.linkLabelUpdateFromDatabase.Size = new System.Drawing.Size(180, 13);
            this.linkLabelUpdateFromDatabase.TabIndex = 7;
            this.linkLabelUpdateFromDatabase.TabStop = true;
            this.linkLabelUpdateFromDatabase.Text = "::UpdateFromBaseStationDatabase::";
            this.linkLabelUpdateFromDatabase.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelUpdateFromDatabase_LinkClicked);
            // 
            // ParentPageReceiverLocations
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Name = "ParentPageReceiverLocations";
            this.Size = new System.Drawing.Size(254, 189);
            this.splitContainerControlsDescription.Panel1.ResumeLayout(false);
            this.splitContainerControlsDescription.ResumeLayout(false);
            this.panelContent.ResumeLayout(false);
            this.panelContent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonNew;
        private System.Windows.Forms.LinkLabel linkLabelUpdateFromDatabase;
    }
}
