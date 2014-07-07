namespace VirtualRadar.WinForms
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
            if(disposing) {
                if(components != null) components.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsView));
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.panelContent = new System.Windows.Forms.Panel();
            this.splitContainerPagePicker = new System.Windows.Forms.SplitContainer();
            this.treeViewPagePicker = new System.Windows.Forms.TreeView();
            this.splitContainerPageContent = new System.Windows.Forms.SplitContainer();
            this.panelPageContentBorder = new System.Windows.Forms.Panel();
            this.panelPageContent = new System.Windows.Forms.Panel();
            this.panelInlineHelpBorder = new System.Windows.Forms.Panel();
            this.panelInlineHelp = new System.Windows.Forms.Panel();
            this.labelInlineHelp = new System.Windows.Forms.Label();
            this.labelInlineHelpTitle = new System.Windows.Forms.Label();
            this.errorProvider = new VirtualRadar.WinForms.Controls.ErrorProviderPlus(this.components);
            this.warningProvider = new VirtualRadar.WinForms.Controls.ErrorProviderPlus(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.presetConfigurationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.justFlightSimulatorXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.defaultConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.receiverConfigurationWizardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelContent.SuspendLayout();
            this.splitContainerPagePicker.Panel1.SuspendLayout();
            this.splitContainerPagePicker.Panel2.SuspendLayout();
            this.splitContainerPagePicker.SuspendLayout();
            this.splitContainerPageContent.Panel1.SuspendLayout();
            this.splitContainerPageContent.Panel2.SuspendLayout();
            this.splitContainerPageContent.SuspendLayout();
            this.panelPageContentBorder.SuspendLayout();
            this.panelInlineHelpBorder.SuspendLayout();
            this.panelInlineHelp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningProvider)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(716, 527);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "::OK::";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(797, 527);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "::Cancel::";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // panelContent
            // 
            this.panelContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContent.Controls.Add(this.splitContainerPagePicker);
            this.panelContent.Location = new System.Drawing.Point(12, 30);
            this.panelContent.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(860, 484);
            this.panelContent.TabIndex = 2;
            // 
            // splitContainerPagePicker
            // 
            this.splitContainerPagePicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerPagePicker.Location = new System.Drawing.Point(0, 0);
            this.splitContainerPagePicker.Name = "splitContainerPagePicker";
            // 
            // splitContainerPagePicker.Panel1
            // 
            this.splitContainerPagePicker.Panel1.Controls.Add(this.treeViewPagePicker);
            // 
            // splitContainerPagePicker.Panel2
            // 
            this.splitContainerPagePicker.Panel2.Controls.Add(this.splitContainerPageContent);
            this.splitContainerPagePicker.Size = new System.Drawing.Size(860, 484);
            this.splitContainerPagePicker.SplitterDistance = 210;
            this.splitContainerPagePicker.TabIndex = 0;
            // 
            // treeViewPagePicker
            // 
            this.treeViewPagePicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewPagePicker.FullRowSelect = true;
            this.treeViewPagePicker.HideSelection = false;
            this.treeViewPagePicker.Location = new System.Drawing.Point(0, 0);
            this.treeViewPagePicker.Name = "treeViewPagePicker";
            this.treeViewPagePicker.ShowRootLines = false;
            this.treeViewPagePicker.Size = new System.Drawing.Size(210, 484);
            this.treeViewPagePicker.TabIndex = 0;
            this.treeViewPagePicker.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewPagePicker_AfterSelect);
            // 
            // splitContainerPageContent
            // 
            this.splitContainerPageContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerPageContent.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerPageContent.Location = new System.Drawing.Point(0, 0);
            this.splitContainerPageContent.Name = "splitContainerPageContent";
            this.splitContainerPageContent.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerPageContent.Panel1
            // 
            this.splitContainerPageContent.Panel1.Controls.Add(this.panelPageContentBorder);
            // 
            // splitContainerPageContent.Panel2
            // 
            this.splitContainerPageContent.Panel2.Controls.Add(this.panelInlineHelpBorder);
            this.splitContainerPageContent.Size = new System.Drawing.Size(646, 484);
            this.splitContainerPageContent.SplitterDistance = 385;
            this.splitContainerPageContent.TabIndex = 0;
            // 
            // panelPageContentBorder
            // 
            this.panelPageContentBorder.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panelPageContentBorder.Controls.Add(this.panelPageContent);
            this.panelPageContentBorder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPageContentBorder.Location = new System.Drawing.Point(0, 0);
            this.panelPageContentBorder.Name = "panelPageContentBorder";
            this.panelPageContentBorder.Size = new System.Drawing.Size(646, 385);
            this.panelPageContentBorder.TabIndex = 0;
            // 
            // panelPageContent
            // 
            this.panelPageContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPageContent.AutoScroll = true;
            this.panelPageContent.BackColor = System.Drawing.SystemColors.Control;
            this.panelPageContent.Location = new System.Drawing.Point(1, 1);
            this.panelPageContent.Name = "panelPageContent";
            this.panelPageContent.Size = new System.Drawing.Size(644, 383);
            this.panelPageContent.TabIndex = 0;
            // 
            // panelInlineHelpBorder
            // 
            this.panelInlineHelpBorder.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panelInlineHelpBorder.Controls.Add(this.panelInlineHelp);
            this.panelInlineHelpBorder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelInlineHelpBorder.Location = new System.Drawing.Point(0, 0);
            this.panelInlineHelpBorder.Name = "panelInlineHelpBorder";
            this.panelInlineHelpBorder.Size = new System.Drawing.Size(646, 95);
            this.panelInlineHelpBorder.TabIndex = 0;
            // 
            // panelInlineHelp
            // 
            this.panelInlineHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelInlineHelp.AutoScroll = true;
            this.panelInlineHelp.BackColor = System.Drawing.SystemColors.Control;
            this.panelInlineHelp.Controls.Add(this.labelInlineHelp);
            this.panelInlineHelp.Controls.Add(this.labelInlineHelpTitle);
            this.panelInlineHelp.Location = new System.Drawing.Point(1, 1);
            this.panelInlineHelp.Name = "panelInlineHelp";
            this.panelInlineHelp.Size = new System.Drawing.Size(644, 93);
            this.panelInlineHelp.TabIndex = 0;
            // 
            // labelInlineHelp
            // 
            this.labelInlineHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelInlineHelp.Location = new System.Drawing.Point(3, 22);
            this.labelInlineHelp.Name = "labelInlineHelp";
            this.labelInlineHelp.Size = new System.Drawing.Size(634, 65);
            this.labelInlineHelp.TabIndex = 1;
            this.labelInlineHelp.Text = "<Inline Help Goes Here>";
            // 
            // labelInlineHelpTitle
            // 
            this.labelInlineHelpTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelInlineHelpTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelInlineHelpTitle.Location = new System.Drawing.Point(4, 4);
            this.labelInlineHelpTitle.Name = "labelInlineHelpTitle";
            this.labelInlineHelpTitle.Size = new System.Drawing.Size(637, 18);
            this.labelInlineHelpTitle.TabIndex = 0;
            this.labelInlineHelpTitle.Text = "<Heading Goes Here>";
            // 
            // errorProvider
            // 
            this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider.ContainerControl = this;
            // 
            // warningProvider
            // 
            this.warningProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.warningProvider.ContainerControl = this;
            this.warningProvider.Icon = ((System.Drawing.Icon)(resources.GetObject("warningProvider.Icon")));
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.presetConfigurationsToolStripMenuItem,
            this.receiverConfigurationWizardToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(884, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // presetConfigurationsToolStripMenuItem
            // 
            this.presetConfigurationsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.justFlightSimulatorXToolStripMenuItem,
            this.toolStripMenuItem1,
            this.defaultConfigurationToolStripMenuItem});
            this.presetConfigurationsToolStripMenuItem.Name = "presetConfigurationsToolStripMenuItem";
            this.presetConfigurationsToolStripMenuItem.Size = new System.Drawing.Size(142, 20);
            this.presetConfigurationsToolStripMenuItem.Text = "::PresetConfigurations::";
            // 
            // justFlightSimulatorXToolStripMenuItem
            // 
            this.justFlightSimulatorXToolStripMenuItem.Name = "justFlightSimulatorXToolStripMenuItem";
            this.justFlightSimulatorXToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.justFlightSimulatorXToolStripMenuItem.Text = "::JustFlightSimulatorX::";
            this.justFlightSimulatorXToolStripMenuItem.Click += new System.EventHandler(this.justFlightSimulatorXToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(195, 6);
            // 
            // defaultConfigurationToolStripMenuItem
            // 
            this.defaultConfigurationToolStripMenuItem.Name = "defaultConfigurationToolStripMenuItem";
            this.defaultConfigurationToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.defaultConfigurationToolStripMenuItem.Text = "::DefaultConfiguration::";
            this.defaultConfigurationToolStripMenuItem.Click += new System.EventHandler(this.defaultConfigurationToolStripMenuItem_Click);
            // 
            // receiverConfigurationWizardToolStripMenuItem
            // 
            this.receiverConfigurationWizardToolStripMenuItem.Name = "receiverConfigurationWizardToolStripMenuItem";
            this.receiverConfigurationWizardToolStripMenuItem.Size = new System.Drawing.Size(185, 20);
            this.receiverConfigurationWizardToolStripMenuItem.Text = "::ReceiverConfigurationWizard::";
            this.receiverConfigurationWizardToolStripMenuItem.Click += new System.EventHandler(this.receiverConfigurationWizardToolStripMenuItem_Click);
            // 
            // OptionsView
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(884, 562);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsView";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "::Options::";
            this.panelContent.ResumeLayout(false);
            this.splitContainerPagePicker.Panel1.ResumeLayout(false);
            this.splitContainerPagePicker.Panel2.ResumeLayout(false);
            this.splitContainerPagePicker.ResumeLayout(false);
            this.splitContainerPageContent.Panel1.ResumeLayout(false);
            this.splitContainerPageContent.Panel2.ResumeLayout(false);
            this.splitContainerPageContent.ResumeLayout(false);
            this.panelPageContentBorder.ResumeLayout(false);
            this.panelInlineHelpBorder.ResumeLayout(false);
            this.panelInlineHelp.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningProvider)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.SplitContainer splitContainerPagePicker;
        private System.Windows.Forms.TreeView treeViewPagePicker;
        private System.Windows.Forms.SplitContainer splitContainerPageContent;
        private System.Windows.Forms.Panel panelPageContentBorder;
        private System.Windows.Forms.Panel panelPageContent;
        private System.Windows.Forms.Panel panelInlineHelpBorder;
        private System.Windows.Forms.Panel panelInlineHelp;
        private System.Windows.Forms.Label labelInlineHelp;
        private System.Windows.Forms.Label labelInlineHelpTitle;
        private VirtualRadar.WinForms.Controls.ErrorProviderPlus errorProvider;
        private VirtualRadar.WinForms.Controls.ErrorProviderPlus warningProvider;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem presetConfigurationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem justFlightSimulatorXToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem defaultConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem receiverConfigurationWizardToolStripMenuItem;
    }
}