namespace VirtualRadar.WinForms
{
    partial class OptionsPropertySheetView
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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.treeView = new System.Windows.Forms.TreeView();
            this.sheetHostControl = new VirtualRadar.WinForms.Controls.SheetHostControl();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.linkLabelUseRecommendedSettings = new System.Windows.Forms.LinkLabel();
            this.linkLabelUseIcaoSettings = new System.Windows.Forms.LinkLabel();
            this.buttonSheetButton = new System.Windows.Forms.Button();
            this.labelValidationMessages = new System.Windows.Forms.Label();
            this.linkLabelResetToDefaults = new System.Windows.Forms.LinkLabel();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(12, 12);
            this.splitContainer.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.treeView);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.sheetHostControl);
            this.splitContainer.Size = new System.Drawing.Size(860, 417);
            this.splitContainer.SplitterDistance = 193;
            this.splitContainer.TabIndex = 0;
            // 
            // treeView
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.FullRowSelect = true;
            this.treeView.HideSelection = false;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(193, 417);
            this.treeView.TabIndex = 1;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            // 
            // sheetHostControl
            // 
            this.sheetHostControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sheetHostControl.Location = new System.Drawing.Point(0, 0);
            this.sheetHostControl.Name = "sheetHostControl";
            this.sheetHostControl.Size = new System.Drawing.Size(663, 417);
            this.sheetHostControl.TabIndex = 0;
            this.sheetHostControl.PropertyValueChanged += new System.EventHandler<VirtualRadar.WinForms.Options.SheetEventArgs>(this.sheetHostControl_PropertyValueChanged);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(710, 527);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "::OK::";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(791, 527);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "::Cancel::";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.buttonDelete);
            this.groupBox1.Controls.Add(this.linkLabelUseRecommendedSettings);
            this.groupBox1.Controls.Add(this.linkLabelUseIcaoSettings);
            this.groupBox1.Controls.Add(this.buttonSheetButton);
            this.groupBox1.Controls.Add(this.labelValidationMessages);
            this.groupBox1.Location = new System.Drawing.Point(13, 432);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(859, 78);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            // 
            // buttonDelete
            // 
            this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDelete.Location = new System.Drawing.Point(778, 49);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(75, 23);
            this.buttonDelete.TabIndex = 2;
            this.buttonDelete.Text = "::Delete::";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // linkLabelUseRecommendedSettings
            // 
            this.linkLabelUseRecommendedSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelUseRecommendedSettings.AutoSize = true;
            this.linkLabelUseRecommendedSettings.Location = new System.Drawing.Point(505, 29);
            this.linkLabelUseRecommendedSettings.Name = "linkLabelUseRecommendedSettings";
            this.linkLabelUseRecommendedSettings.Size = new System.Drawing.Size(148, 13);
            this.linkLabelUseRecommendedSettings.TabIndex = 3;
            this.linkLabelUseRecommendedSettings.TabStop = true;
            this.linkLabelUseRecommendedSettings.Text = "::UseRecommendedSettings::";
            this.linkLabelUseRecommendedSettings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelUseRecommendedSettings_LinkClicked);
            // 
            // linkLabelUseIcaoSettings
            // 
            this.linkLabelUseIcaoSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelUseIcaoSettings.AutoSize = true;
            this.linkLabelUseIcaoSettings.Location = new System.Drawing.Point(505, 12);
            this.linkLabelUseIcaoSettings.Name = "linkLabelUseIcaoSettings";
            this.linkLabelUseIcaoSettings.Size = new System.Drawing.Size(158, 13);
            this.linkLabelUseIcaoSettings.TabIndex = 2;
            this.linkLabelUseIcaoSettings.TabStop = true;
            this.linkLabelUseIcaoSettings.Text = "::UseIcaoSpecificationSettings::";
            this.linkLabelUseIcaoSettings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelUseIcaoSettings_LinkClicked);
            // 
            // buttonSheetButton
            // 
            this.buttonSheetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSheetButton.Location = new System.Drawing.Point(622, 49);
            this.buttonSheetButton.Name = "buttonSheetButton";
            this.buttonSheetButton.Size = new System.Drawing.Size(150, 23);
            this.buttonSheetButton.TabIndex = 1;
            this.buttonSheetButton.Text = "buttonSheetButton";
            this.buttonSheetButton.UseVisualStyleBackColor = true;
            this.buttonSheetButton.Click += new System.EventHandler(this.buttonSheetButton_Click);
            // 
            // labelValidationMessages
            // 
            this.labelValidationMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelValidationMessages.ForeColor = System.Drawing.Color.Red;
            this.labelValidationMessages.Location = new System.Drawing.Point(7, 12);
            this.labelValidationMessages.Name = "labelValidationMessages";
            this.labelValidationMessages.Size = new System.Drawing.Size(491, 60);
            this.labelValidationMessages.TabIndex = 0;
            this.labelValidationMessages.Text = "validation message 1\r\nvalidation message 2\r\nvalidation message 3\r\n";
            // 
            // linkLabelResetToDefaults
            // 
            this.linkLabelResetToDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelResetToDefaults.AutoSize = true;
            this.linkLabelResetToDefaults.Location = new System.Drawing.Point(12, 532);
            this.linkLabelResetToDefaults.Name = "linkLabelResetToDefaults";
            this.linkLabelResetToDefaults.Size = new System.Drawing.Size(132, 13);
            this.linkLabelResetToDefaults.TabIndex = 4;
            this.linkLabelResetToDefaults.TabStop = true;
            this.linkLabelResetToDefaults.Text = "::ResetSettingsToDefault::";
            this.linkLabelResetToDefaults.VisitedLinkColor = System.Drawing.Color.Blue;
            this.linkLabelResetToDefaults.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelResetToDefaults_LinkClicked);
            // 
            // OptionsPropertySheetView
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(884, 562);
            this.Controls.Add(this.linkLabelResetToDefaults);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.splitContainer);
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsPropertySheetView";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "::Options::";
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private VirtualRadar.WinForms.Controls.SheetHostControl sheetHostControl;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelValidationMessages;
        private System.Windows.Forms.Button buttonSheetButton;
        private System.Windows.Forms.LinkLabel linkLabelResetToDefaults;
        private System.Windows.Forms.LinkLabel linkLabelUseRecommendedSettings;
        private System.Windows.Forms.LinkLabel linkLabelUseIcaoSettings;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.Button buttonDelete;

    }
}