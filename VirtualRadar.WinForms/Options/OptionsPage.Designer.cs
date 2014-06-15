namespace VirtualRadar.WinForms.Options
{
    partial class OptionsPage
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsPage));
            this.splitContainerControlsDescription = new System.Windows.Forms.SplitContainer();
            this.panelContent = new System.Windows.Forms.Panel();
            this.panelDescriptionBorder = new System.Windows.Forms.Panel();
            this.panelDescription = new System.Windows.Forms.Panel();
            this.labelDescriptionTitle = new System.Windows.Forms.Label();
            this.labelDescription = new System.Windows.Forms.Label();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.warningProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.splitContainerControlsDescription.Panel1.SuspendLayout();
            this.splitContainerControlsDescription.Panel2.SuspendLayout();
            this.splitContainerControlsDescription.SuspendLayout();
            this.panelDescriptionBorder.SuspendLayout();
            this.panelDescription.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainerControlsDescription
            // 
            this.splitContainerControlsDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControlsDescription.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerControlsDescription.Location = new System.Drawing.Point(0, 0);
            this.splitContainerControlsDescription.Name = "splitContainerControlsDescription";
            this.splitContainerControlsDescription.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerControlsDescription.Panel1
            // 
            this.splitContainerControlsDescription.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainerControlsDescription.Panel1.Controls.Add(this.panelContent);
            // 
            // splitContainerControlsDescription.Panel2
            // 
            this.splitContainerControlsDescription.Panel2.Controls.Add(this.panelDescriptionBorder);
            this.splitContainerControlsDescription.Size = new System.Drawing.Size(424, 370);
            this.splitContainerControlsDescription.SplitterDistance = 302;
            this.splitContainerControlsDescription.TabIndex = 1;
            // 
            // panelContent
            // 
            this.panelContent.AutoScroll = true;
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Location = new System.Drawing.Point(0, 0);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(424, 302);
            this.panelContent.TabIndex = 0;
            // 
            // panelDescriptionBorder
            // 
            this.panelDescriptionBorder.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panelDescriptionBorder.Controls.Add(this.panelDescription);
            this.panelDescriptionBorder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDescriptionBorder.Location = new System.Drawing.Point(0, 0);
            this.panelDescriptionBorder.Name = "panelDescriptionBorder";
            this.panelDescriptionBorder.Size = new System.Drawing.Size(424, 64);
            this.panelDescriptionBorder.TabIndex = 3;
            // 
            // panelDescription
            // 
            this.panelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelDescription.AutoScroll = true;
            this.panelDescription.BackColor = System.Drawing.SystemColors.Control;
            this.panelDescription.Controls.Add(this.labelDescriptionTitle);
            this.panelDescription.Controls.Add(this.labelDescription);
            this.panelDescription.Location = new System.Drawing.Point(1, 1);
            this.panelDescription.Name = "panelDescription";
            this.panelDescription.Size = new System.Drawing.Size(422, 62);
            this.panelDescription.TabIndex = 2;
            // 
            // labelDescriptionTitle
            // 
            this.labelDescriptionTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDescriptionTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDescriptionTitle.Location = new System.Drawing.Point(4, 2);
            this.labelDescriptionTitle.Name = "labelDescriptionTitle";
            this.labelDescriptionTitle.Size = new System.Drawing.Size(414, 18);
            this.labelDescriptionTitle.TabIndex = 1;
            this.labelDescriptionTitle.Text = "<title goes here>";
            // 
            // labelDescription
            // 
            this.labelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDescription.Location = new System.Drawing.Point(4, 20);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(414, 40);
            this.labelDescription.TabIndex = 0;
            this.labelDescription.Text = "<description goes here>";
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
            // OptionsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.splitContainerControlsDescription);
            this.Name = "OptionsPage";
            this.Size = new System.Drawing.Size(424, 370);
            this.splitContainerControlsDescription.Panel1.ResumeLayout(false);
            this.splitContainerControlsDescription.Panel2.ResumeLayout(false);
            this.splitContainerControlsDescription.ResumeLayout(false);
            this.panelDescriptionBorder.ResumeLayout(false);
            this.panelDescription.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.SplitContainer splitContainerControlsDescription;
        public System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.Panel panelDescriptionBorder;
        private System.Windows.Forms.Panel panelDescription;
        private System.Windows.Forms.Label labelDescriptionTitle;
        private System.Windows.Forms.Label labelDescription;
        public System.Windows.Forms.ErrorProvider errorProvider;
        public System.Windows.Forms.ErrorProvider warningProvider;
    }
}
