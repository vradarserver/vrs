namespace VirtualRadar.WinForms.Controls
{
    partial class LocationMapControl
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
            this.numericLatitude = new System.Windows.Forms.NumericUpDown();
            this.numericLongitude = new System.Windows.Forms.NumericUpDown();
            this.labelLatitude = new System.Windows.Forms.Label();
            this.labelLongitude = new System.Windows.Forms.Label();
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.panelBorder = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.numericLatitude)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLongitude)).BeginInit();
            this.panelBorder.SuspendLayout();
            this.SuspendLayout();
            // 
            // numericLatitude
            // 
            this.numericLatitude.DecimalPlaces = 6;
            this.numericLatitude.Location = new System.Drawing.Point(155, 0);
            this.numericLatitude.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.numericLatitude.Minimum = new decimal(new int[] {
            90,
            0,
            0,
            -2147483648});
            this.numericLatitude.Name = "numericLatitude";
            this.numericLatitude.Size = new System.Drawing.Size(120, 20);
            this.numericLatitude.TabIndex = 0;
            this.numericLatitude.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericLatitude.ValueChanged += new System.EventHandler(this.numeric_ValueChanged);
            // 
            // numericLongitude
            // 
            this.numericLongitude.DecimalPlaces = 6;
            this.numericLongitude.Location = new System.Drawing.Point(155, 26);
            this.numericLongitude.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.numericLongitude.Minimum = new decimal(new int[] {
            180,
            0,
            0,
            -2147483648});
            this.numericLongitude.Name = "numericLongitude";
            this.numericLongitude.Size = new System.Drawing.Size(120, 20);
            this.numericLongitude.TabIndex = 1;
            this.numericLongitude.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericLongitude.ValueChanged += new System.EventHandler(this.numeric_ValueChanged);
            // 
            // labelLatitude
            // 
            this.labelLatitude.AutoSize = true;
            this.labelLatitude.Location = new System.Drawing.Point(0, 2);
            this.labelLatitude.Name = "labelLatitude";
            this.labelLatitude.Size = new System.Drawing.Size(60, 13);
            this.labelLatitude.TabIndex = 2;
            this.labelLatitude.Text = "::Latitude:::";
            // 
            // labelLongitude
            // 
            this.labelLongitude.AutoSize = true;
            this.labelLongitude.Location = new System.Drawing.Point(0, 28);
            this.labelLongitude.Name = "labelLongitude";
            this.labelLongitude.Size = new System.Drawing.Size(69, 13);
            this.labelLongitude.TabIndex = 3;
            this.labelLongitude.Text = "::Longitude:::";
            // 
            // webBrowser
            // 
            this.webBrowser.AllowNavigation = false;
            this.webBrowser.AllowWebBrowserDrop = false;
            this.webBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser.Location = new System.Drawing.Point(1, 1);
            this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.ScrollBarsEnabled = false;
            this.webBrowser.Size = new System.Drawing.Size(349, 279);
            this.webBrowser.TabIndex = 4;
            this.webBrowser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser_DocumentCompleted);
            // 
            // panelBorder
            // 
            this.panelBorder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBorder.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panelBorder.Controls.Add(this.webBrowser);
            this.panelBorder.Location = new System.Drawing.Point(0, 52);
            this.panelBorder.Name = "panelBorder";
            this.panelBorder.Size = new System.Drawing.Size(351, 281);
            this.panelBorder.TabIndex = 5;
            // 
            // LocationMapControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelBorder);
            this.Controls.Add(this.labelLongitude);
            this.Controls.Add(this.labelLatitude);
            this.Controls.Add(this.numericLongitude);
            this.Controls.Add(this.numericLatitude);
            this.Name = "LocationMapControl";
            this.Size = new System.Drawing.Size(351, 333);
            ((System.ComponentModel.ISupportInitialize)(this.numericLatitude)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLongitude)).EndInit();
            this.panelBorder.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown numericLatitude;
        private System.Windows.Forms.NumericUpDown numericLongitude;
        private System.Windows.Forms.Label labelLatitude;
        private System.Windows.Forms.Label labelLongitude;
        private System.Windows.Forms.WebBrowser webBrowser;
        private System.Windows.Forms.Panel panelBorder;
    }
}
