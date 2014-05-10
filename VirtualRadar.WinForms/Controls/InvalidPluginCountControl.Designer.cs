namespace VirtualRadar.WinForms.Controls
{
    partial class InvalidPluginCountControl
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
            this.linkLabelInvalidPluginCount = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // linkLabelInvalidPluginCount
            // 
            this.linkLabelInvalidPluginCount.AutoSize = true;
            this.linkLabelInvalidPluginCount.Location = new System.Drawing.Point(0, 0);
            this.linkLabelInvalidPluginCount.Name = "linkLabelInvalidPluginCount";
            this.linkLabelInvalidPluginCount.Size = new System.Drawing.Size(167, 13);
            this.linkLabelInvalidPluginCount.TabIndex = 5;
            this.linkLabelInvalidPluginCount.TabStop = true;
            this.linkLabelInvalidPluginCount.Text = "<Invalid Plugin Count Goes Here>";
            this.linkLabelInvalidPluginCount.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelInvalidPluginCount_LinkClicked);
            // 
            // InvalidPluginCountControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.linkLabelInvalidPluginCount);
            this.Name = "InvalidPluginCountControl";
            this.Size = new System.Drawing.Size(173, 13);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel linkLabelInvalidPluginCount;
    }
}
