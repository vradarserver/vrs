namespace VirtualRadar.WinForms.Controls
{
    partial class PluginDetailPanel
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
                if(components != null) {
                    components.Dispose();
                }
                UnhookPluginEvents();
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
            this.labelPluginName = new System.Windows.Forms.Label();
            this.buttonConfigure = new System.Windows.Forms.Button();
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelStatusDescription = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelPluginName
            // 
            this.labelPluginName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPluginName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPluginName.Location = new System.Drawing.Point(4, 4);
            this.labelPluginName.Name = "labelPluginName";
            this.labelPluginName.Size = new System.Drawing.Size(264, 23);
            this.labelPluginName.TabIndex = 0;
            this.labelPluginName.Text = "<Name goes here>";
            // 
            // buttonConfigure
            // 
            this.buttonConfigure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConfigure.Location = new System.Drawing.Point(386, 3);
            this.buttonConfigure.Name = "buttonConfigure";
            this.buttonConfigure.Size = new System.Drawing.Size(75, 23);
            this.buttonConfigure.TabIndex = 1;
            this.buttonConfigure.Text = "::Options::";
            this.buttonConfigure.UseVisualStyleBackColor = true;
            this.buttonConfigure.Click += new System.EventHandler(this.buttonConfigure_Click);
            // 
            // labelVersion
            // 
            this.labelVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelVersion.Location = new System.Drawing.Point(274, 8);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(106, 19);
            this.labelVersion.TabIndex = 2;
            this.labelVersion.Text = "<Version goes here>";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStatus.Location = new System.Drawing.Point(3, 27);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(458, 19);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "<Status goes here>";
            // 
            // labelStatusDescription
            // 
            this.labelStatusDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStatusDescription.Location = new System.Drawing.Point(3, 46);
            this.labelStatusDescription.Name = "labelStatusDescription";
            this.labelStatusDescription.Size = new System.Drawing.Size(458, 19);
            this.labelStatusDescription.TabIndex = 4;
            this.labelStatusDescription.Text = "<Optional status description goes here>";
            // 
            // PluginDetailPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.labelStatusDescription);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.buttonConfigure);
            this.Controls.Add(this.labelPluginName);
            this.Name = "PluginDetailPanel";
            this.Size = new System.Drawing.Size(464, 71);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelPluginName;
        private System.Windows.Forms.Button buttonConfigure;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label labelStatusDescription;
    }
}
