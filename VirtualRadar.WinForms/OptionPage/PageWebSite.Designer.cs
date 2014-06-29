namespace VirtualRadar.WinForms.OptionPage
{
    partial class PageWebSite
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxPreferIataAirportCodes = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxInitialSpeedUnits = new VirtualRadar.WinForms.Controls.ComboBoxPlus();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxInitialHeightUnits = new VirtualRadar.WinForms.Controls.ComboBoxPlus();
            this.label9 = new System.Windows.Forms.Label();
            this.comboBoxInitialDistanceUnits = new VirtualRadar.WinForms.Controls.ComboBoxPlus();
            this.label1 = new System.Windows.Forms.Label();
            this.numericMinimumRefresh = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.numericInitialRefresh = new System.Windows.Forms.NumericUpDown();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBoxEnableCompression = new System.Windows.Forms.CheckBox();
            this.comboBoxProxyType = new VirtualRadar.WinForms.Controls.ComboBoxPlus();
            this.checkBoxEnableMinifying = new System.Windows.Forms.CheckBox();
            this.checkBoxEnableBundling = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericMinimumRefresh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericInitialRefresh)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBoxPreferIataAirportCodes);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.comboBoxInitialSpeedUnits);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.comboBoxInitialHeightUnits);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.comboBoxInitialDistanceUnits);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.numericMinimumRefresh);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.numericInitialRefresh);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(636, 177);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "::OptionsWebSiteSettingsCategory::";
            // 
            // checkBoxPreferIataAirportCodes
            // 
            this.checkBoxPreferIataAirportCodes.AutoSize = true;
            this.checkBoxPreferIataAirportCodes.Location = new System.Drawing.Point(335, 152);
            this.checkBoxPreferIataAirportCodes.Name = "checkBoxPreferIataAirportCodes";
            this.checkBoxPreferIataAirportCodes.Size = new System.Drawing.Size(144, 17);
            this.checkBoxPreferIataAirportCodes.TabIndex = 10;
            this.checkBoxPreferIataAirportCodes.Text = "::PreferIataAirportCodes::";
            this.checkBoxPreferIataAirportCodes.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 126);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "::InitialSpeedUnits:::";
            // 
            // comboBoxInitialSpeedUnits
            // 
            this.comboBoxInitialSpeedUnits.FormattingEnabled = true;
            this.comboBoxInitialSpeedUnits.Location = new System.Drawing.Point(335, 125);
            this.comboBoxInitialSpeedUnits.Name = "comboBoxInitialSpeedUnits";
            this.comboBoxInitialSpeedUnits.Size = new System.Drawing.Size(150, 21);
            this.comboBoxInitialSpeedUnits.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "::InitialHeightUnits:::";
            // 
            // comboBoxInitialHeightUnits
            // 
            this.comboBoxInitialHeightUnits.FormattingEnabled = true;
            this.comboBoxInitialHeightUnits.Location = new System.Drawing.Point(335, 98);
            this.comboBoxInitialHeightUnits.Name = "comboBoxInitialHeightUnits";
            this.comboBoxInitialHeightUnits.Size = new System.Drawing.Size(150, 21);
            this.comboBoxInitialHeightUnits.TabIndex = 7;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 72);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(112, 13);
            this.label9.TabIndex = 4;
            this.label9.Text = "::InitialDistanceUnits:::";
            // 
            // comboBoxInitialDistanceUnits
            // 
            this.comboBoxInitialDistanceUnits.FormattingEnabled = true;
            this.comboBoxInitialDistanceUnits.Location = new System.Drawing.Point(335, 71);
            this.comboBoxInitialDistanceUnits.Name = "comboBoxInitialDistanceUnits";
            this.comboBoxInitialDistanceUnits.Size = new System.Drawing.Size(150, 21);
            this.comboBoxInitialDistanceUnits.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "::MinimumRefresh:::";
            // 
            // numericMinimumRefresh
            // 
            this.numericMinimumRefresh.Location = new System.Drawing.Point(335, 45);
            this.numericMinimumRefresh.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.numericMinimumRefresh.Name = "numericMinimumRefresh";
            this.numericMinimumRefresh.Size = new System.Drawing.Size(77, 20);
            this.numericMinimumRefresh.TabIndex = 3;
            this.numericMinimumRefresh.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 21);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(83, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "::InitialRefresh:::";
            // 
            // numericInitialRefresh
            // 
            this.numericInitialRefresh.Location = new System.Drawing.Point(335, 19);
            this.numericInitialRefresh.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.numericInitialRefresh.Name = "numericInitialRefresh";
            this.numericInitialRefresh.Size = new System.Drawing.Size(77, 20);
            this.numericInitialRefresh.TabIndex = 1;
            this.numericInitialRefresh.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.checkBoxEnableCompression);
            this.groupBox2.Controls.Add(this.comboBoxProxyType);
            this.groupBox2.Controls.Add(this.checkBoxEnableMinifying);
            this.groupBox2.Controls.Add(this.checkBoxEnableBundling);
            this.groupBox2.Location = new System.Drawing.Point(0, 183);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(636, 71);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "::OptionsWebSiteCustomisationCategory::";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 43);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "::ProxyType:::";
            // 
            // checkBoxEnableCompression
            // 
            this.checkBoxEnableCompression.AutoSize = true;
            this.checkBoxEnableCompression.Location = new System.Drawing.Point(335, 42);
            this.checkBoxEnableCompression.Name = "checkBoxEnableCompression";
            this.checkBoxEnableCompression.Size = new System.Drawing.Size(131, 17);
            this.checkBoxEnableCompression.TabIndex = 4;
            this.checkBoxEnableCompression.Text = "::EnableCompression::";
            this.checkBoxEnableCompression.UseVisualStyleBackColor = true;
            // 
            // comboBoxProxyType
            // 
            this.comboBoxProxyType.FormattingEnabled = true;
            this.comboBoxProxyType.Location = new System.Drawing.Point(160, 40);
            this.comboBoxProxyType.Name = "comboBoxProxyType";
            this.comboBoxProxyType.Size = new System.Drawing.Size(150, 21);
            this.comboBoxProxyType.TabIndex = 3;
            // 
            // checkBoxEnableMinifying
            // 
            this.checkBoxEnableMinifying.AutoSize = true;
            this.checkBoxEnableMinifying.Location = new System.Drawing.Point(335, 19);
            this.checkBoxEnableMinifying.Name = "checkBoxEnableMinifying";
            this.checkBoxEnableMinifying.Size = new System.Drawing.Size(112, 17);
            this.checkBoxEnableMinifying.TabIndex = 1;
            this.checkBoxEnableMinifying.Text = "::EnableMinifying::";
            this.checkBoxEnableMinifying.UseVisualStyleBackColor = true;
            // 
            // checkBoxEnableBundling
            // 
            this.checkBoxEnableBundling.AutoSize = true;
            this.checkBoxEnableBundling.Location = new System.Drawing.Point(9, 19);
            this.checkBoxEnableBundling.Name = "checkBoxEnableBundling";
            this.checkBoxEnableBundling.Size = new System.Drawing.Size(112, 17);
            this.checkBoxEnableBundling.TabIndex = 0;
            this.checkBoxEnableBundling.Text = "::EnableBundling::";
            this.checkBoxEnableBundling.UseVisualStyleBackColor = true;
            // 
            // PageWebSite
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "PageWebSite";
            this.Size = new System.Drawing.Size(636, 260);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericMinimumRefresh)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericInitialRefresh)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericMinimumRefresh;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericInitialRefresh;
        private System.Windows.Forms.CheckBox checkBoxPreferIataAirportCodes;
        private System.Windows.Forms.Label label3;
        private Controls.ComboBoxPlus comboBoxInitialSpeedUnits;
        private System.Windows.Forms.Label label2;
        private Controls.ComboBoxPlus comboBoxInitialHeightUnits;
        private System.Windows.Forms.Label label9;
        private Controls.ComboBoxPlus comboBoxInitialDistanceUnits;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBoxEnableCompression;
        private Controls.ComboBoxPlus comboBoxProxyType;
        private System.Windows.Forms.CheckBox checkBoxEnableMinifying;
        private System.Windows.Forms.CheckBox checkBoxEnableBundling;
    }
}
