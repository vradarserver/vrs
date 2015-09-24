namespace VirtualRadar.Plugin.FeedFilter.WinForms
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
            this.checkBoxEnabled = new System.Windows.Forms.CheckBox();
            this.checkBoxProhibitUnfilterableFeeds = new System.Windows.Forms.CheckBox();
            this.accessControl = new VirtualRadar.WinForms.Controls.AccessControl();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.linkLabelFilterSettingsLink = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // checkBoxEnabled
            // 
            this.checkBoxEnabled.AutoSize = true;
            this.checkBoxEnabled.Location = new System.Drawing.Point(212, 12);
            this.checkBoxEnabled.Name = "checkBoxEnabled";
            this.checkBoxEnabled.Size = new System.Drawing.Size(77, 17);
            this.checkBoxEnabled.TabIndex = 0;
            this.checkBoxEnabled.Text = "::Enabled::";
            this.checkBoxEnabled.UseVisualStyleBackColor = true;
            // 
            // checkBoxProhibitUnfilterableFeeds
            // 
            this.checkBoxProhibitUnfilterableFeeds.AutoSize = true;
            this.checkBoxProhibitUnfilterableFeeds.Location = new System.Drawing.Point(212, 35);
            this.checkBoxProhibitUnfilterableFeeds.Name = "checkBoxProhibitUnfilterableFeeds";
            this.checkBoxProhibitUnfilterableFeeds.Size = new System.Drawing.Size(155, 17);
            this.checkBoxProhibitUnfilterableFeeds.TabIndex = 1;
            this.checkBoxProhibitUnfilterableFeeds.Text = "::ProhibitUnfilterableFeeds::";
            this.checkBoxProhibitUnfilterableFeeds.UseVisualStyleBackColor = true;
            // 
            // accessControl
            // 
            this.accessControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.accessControl.Location = new System.Drawing.Point(12, 58);
            this.accessControl.Name = "accessControl";
            this.accessControl.Size = new System.Drawing.Size(595, 233);
            this.accessControl.TabIndex = 2;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(532, 324);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "::Cancel::";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(451, 324);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "::OK::";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // linkLabelFilterSettingsLink
            // 
            this.linkLabelFilterSettingsLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelFilterSettingsLink.Location = new System.Drawing.Point(209, 294);
            this.linkLabelFilterSettingsLink.Name = "linkLabelFilterSettingsLink";
            this.linkLabelFilterSettingsLink.Size = new System.Drawing.Size(398, 17);
            this.linkLabelFilterSettingsLink.TabIndex = 3;
            this.linkLabelFilterSettingsLink.TabStop = true;
            this.linkLabelFilterSettingsLink.Text = "Filter settings";
            this.linkLabelFilterSettingsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelFilterSettingsLink_LinkClicked);
            // 
            // OptionsView
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(619, 359);
            this.Controls.Add(this.linkLabelFilterSettingsLink);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.accessControl);
            this.Controls.Add(this.checkBoxProhibitUnfilterableFeeds);
            this.Controls.Add(this.checkBoxEnabled);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsView";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "::PluginName::";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxEnabled;
        private System.Windows.Forms.CheckBox checkBoxProhibitUnfilterableFeeds;
        private VirtualRadar.WinForms.Controls.AccessControl accessControl;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.LinkLabel linkLabelFilterSettingsLink;
    }
}