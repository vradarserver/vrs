namespace VirtualRadar.WinForms.SettingPage
{
    partial class PageMono
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
            this.checkBoxUseMarkerLabels = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // checkBoxUseMarkerLabels
            // 
            this.checkBoxUseMarkerLabels.AutoSize = true;
            this.checkBoxUseMarkerLabels.Location = new System.Drawing.Point(3, 3);
            this.checkBoxUseMarkerLabels.Name = "checkBoxUseMarkerLabels";
            this.checkBoxUseMarkerLabels.Size = new System.Drawing.Size(121, 17);
            this.checkBoxUseMarkerLabels.TabIndex = 18;
            this.checkBoxUseMarkerLabels.Text = "::UseMarkerLabels::";
            this.checkBoxUseMarkerLabels.UseVisualStyleBackColor = true;
            // 
            // PageMono
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.checkBoxUseMarkerLabels);
            this.Name = "PageMono";
            this.Size = new System.Drawing.Size(636, 328);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxUseMarkerLabels;
    }
}
