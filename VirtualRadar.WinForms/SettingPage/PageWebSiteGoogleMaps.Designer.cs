namespace VirtualRadar.WinForms.SettingPage
{
    partial class PageWebSiteGoogleMaps
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
            this.comboBoxInitialMapType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.numericInitialZoom = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.bindingMap = new VirtualRadar.WinForms.Controls.BindingMapControl();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.numericInitialLongitude = new System.Windows.Forms.NumericUpDown();
            this.numericInitialLatitude = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericInitialZoom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericInitialLongitude)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericInitialLatitude)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxInitialMapType
            // 
            this.comboBoxInitialMapType.DisplayMember = "Name";
            this.comboBoxInitialMapType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxInitialMapType.FormattingEnabled = true;
            this.comboBoxInitialMapType.Location = new System.Drawing.Point(200, 0);
            this.comboBoxInitialMapType.Name = "comboBoxInitialMapType";
            this.comboBoxInitialMapType.Size = new System.Drawing.Size(120, 21);
            this.comboBoxInitialMapType.TabIndex = 5;
            this.comboBoxInitialMapType.ValueMember = "Value";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "::InitialZoom:::";
            // 
            // numericInitialZoom
            // 
            this.numericInitialZoom.Location = new System.Drawing.Point(200, 27);
            this.numericInitialZoom.Maximum = new decimal(new int[] {
            19,
            0,
            0,
            0});
            this.numericInitialZoom.Name = "numericInitialZoom";
            this.numericInitialZoom.Size = new System.Drawing.Size(120, 20);
            this.numericInitialZoom.TabIndex = 7;
            this.numericInitialZoom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "::InitialMapType:::";
            // 
            // bindingMap
            // 
            this.bindingMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bindingMap.BindMapType = true;
            this.bindingMap.BindZoomLevel = true;
            this.bindingMap.LatitudeMember = null;
            this.bindingMap.Location = new System.Drawing.Point(0, 105);
            this.bindingMap.LongitudeMember = null;
            this.bindingMap.MapTypeMember = null;
            this.bindingMap.Name = "bindingMap";
            this.bindingMap.Size = new System.Drawing.Size(636, 342);
            this.bindingMap.TabIndex = 12;
            this.bindingMap.ZoomLevelMember = null;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "::Longitude:::";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(0, 55);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "::Latitude:::";
            // 
            // numericInitialLongitude
            // 
            this.numericInitialLongitude.DecimalPlaces = 6;
            this.numericInitialLongitude.Location = new System.Drawing.Point(200, 79);
            this.numericInitialLongitude.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.numericInitialLongitude.Minimum = new decimal(new int[] {
            180,
            0,
            0,
            -2147483648});
            this.numericInitialLongitude.Name = "numericInitialLongitude";
            this.numericInitialLongitude.Size = new System.Drawing.Size(100, 20);
            this.numericInitialLongitude.TabIndex = 11;
            this.numericInitialLongitude.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // numericInitialLatitude
            // 
            this.numericInitialLatitude.DecimalPlaces = 6;
            this.numericInitialLatitude.Location = new System.Drawing.Point(200, 53);
            this.numericInitialLatitude.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.numericInitialLatitude.Minimum = new decimal(new int[] {
            90,
            0,
            0,
            -2147483648});
            this.numericInitialLatitude.Name = "numericInitialLatitude";
            this.numericInitialLatitude.Size = new System.Drawing.Size(100, 20);
            this.numericInitialLatitude.TabIndex = 9;
            this.numericInitialLatitude.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // PageWebSiteGoogleMaps
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.bindingMap);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numericInitialLongitude);
            this.Controls.Add(this.numericInitialLatitude);
            this.Controls.Add(this.comboBoxInitialMapType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numericInitialZoom);
            this.Controls.Add(this.label1);
            this.Name = "PageWebSiteGoogleMaps";
            this.Size = new System.Drawing.Size(636, 447);
            ((System.ComponentModel.ISupportInitialize)(this.numericInitialZoom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericInitialLongitude)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericInitialLatitude)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxInitialMapType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericInitialZoom;
        private System.Windows.Forms.Label label1;
        private Controls.BindingMapControl bindingMap;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericInitialLongitude;
        private System.Windows.Forms.NumericUpDown numericInitialLatitude;
    }
}
