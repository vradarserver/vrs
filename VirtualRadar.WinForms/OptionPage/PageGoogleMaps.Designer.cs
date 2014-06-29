namespace VirtualRadar.WinForms.OptionPage
{
    partial class PageGoogleMaps
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
            this.locationMap = new VirtualRadar.WinForms.Controls.LocationMapControl();
            this.label1 = new System.Windows.Forms.Label();
            this.numericInitialZoom = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxInitialMapType = new VirtualRadar.WinForms.Controls.ComboBoxPlus();
            ((System.ComponentModel.ISupportInitialize)(this.numericInitialZoom)).BeginInit();
            this.SuspendLayout();
            // 
            // locationMap
            // 
            this.locationMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.locationMap.LatitudeLabel = "::InitialLatitude:::";
            this.locationMap.Location = new System.Drawing.Point(0, 53);
            this.locationMap.LongitudeLabel = "::InitialLongitude:::";
            this.locationMap.Name = "locationMap";
            this.locationMap.Size = new System.Drawing.Size(636, 394);
            this.locationMap.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "::InitialMapType:::";
            // 
            // numericInitialZoom
            // 
            this.numericInitialZoom.Location = new System.Drawing.Point(155, 27);
            this.numericInitialZoom.Maximum = new decimal(new int[] {
            19,
            0,
            0,
            0});
            this.numericInitialZoom.Name = "numericInitialZoom";
            this.numericInitialZoom.Size = new System.Drawing.Size(120, 20);
            this.numericInitialZoom.TabIndex = 3;
            this.numericInitialZoom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "::InitialZoom:::";
            // 
            // comboBoxInitialMapType
            // 
            this.comboBoxInitialMapType.FormattingEnabled = true;
            this.comboBoxInitialMapType.Location = new System.Drawing.Point(155, 0);
            this.comboBoxInitialMapType.Name = "comboBoxInitialMapType";
            this.comboBoxInitialMapType.Size = new System.Drawing.Size(120, 21);
            this.comboBoxInitialMapType.TabIndex = 1;
            // 
            // PageGoogleMaps
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.comboBoxInitialMapType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numericInitialZoom);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.locationMap);
            this.Name = "PageGoogleMaps";
            this.Size = new System.Drawing.Size(636, 447);
            ((System.ComponentModel.ISupportInitialize)(this.numericInitialZoom)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.LocationMapControl locationMap;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericInitialZoom;
        private System.Windows.Forms.Label label2;
        private Controls.ComboBoxPlus comboBoxInitialMapType;
    }
}
