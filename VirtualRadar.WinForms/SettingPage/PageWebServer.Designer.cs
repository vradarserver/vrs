namespace VirtualRadar.WinForms.SettingPage
{
    partial class PageWebServer
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
            if (disposing && (components != null))
            {
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
            this.label6 = new System.Windows.Forms.Label();
            this.numericUPnpPort = new System.Windows.Forms.NumericUpDown();
            this.checkBoxAutoStartUPnP = new System.Windows.Forms.CheckBox();
            this.checkBoxResetPortAssignmentsOnStartup = new System.Windows.Forms.CheckBox();
            this.checkBoxEnableUPnp = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxInternetClientCanShowPolarPlots = new System.Windows.Forms.CheckBox();
            this.checkBoxAllowInternetProximityGadgets = new System.Windows.Forms.CheckBox();
            this.checkBoxInternetClientCanSubmitRoutes = new System.Windows.Forms.CheckBox();
            this.checkBoxInternetUsersCanViewPictures = new System.Windows.Forms.CheckBox();
            this.checkBoxInternetUserCanSeeLabels = new System.Windows.Forms.CheckBox();
            this.checkBoxInternetUsersCanRunReports = new System.Windows.Forms.CheckBox();
            this.checkBoxInternetUsersCanListenToAudio = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numericInternetUserIdleTimeout = new System.Windows.Forms.NumericUpDown();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUPnpPort)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericInternetUserIdleTimeout)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.numericUPnpPort);
            this.groupBox1.Controls.Add(this.checkBoxAutoStartUPnP);
            this.groupBox1.Controls.Add(this.checkBoxResetPortAssignmentsOnStartup);
            this.groupBox1.Controls.Add(this.checkBoxEnableUPnp);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(636, 119);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "::OptionsUPnPCategory::";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 90);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(68, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "::UPnpPort:::";
            // 
            // numericUPnpPort
            // 
            this.numericUPnpPort.Location = new System.Drawing.Point(199, 88);
            this.numericUPnpPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUPnpPort.Name = "numericUPnpPort";
            this.numericUPnpPort.Size = new System.Drawing.Size(77, 20);
            this.numericUPnpPort.TabIndex = 4;
            this.numericUPnpPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // checkBoxAutoStartUPnP
            // 
            this.checkBoxAutoStartUPnP.AutoSize = true;
            this.checkBoxAutoStartUPnP.Location = new System.Drawing.Point(199, 65);
            this.checkBoxAutoStartUPnP.Name = "checkBoxAutoStartUPnP";
            this.checkBoxAutoStartUPnP.Size = new System.Drawing.Size(110, 17);
            this.checkBoxAutoStartUPnP.TabIndex = 2;
            this.checkBoxAutoStartUPnP.Text = "::AutoStartUPnP::";
            this.checkBoxAutoStartUPnP.UseVisualStyleBackColor = true;
            // 
            // checkBoxResetPortAssignmentsOnStartup
            // 
            this.checkBoxResetPortAssignmentsOnStartup.AutoSize = true;
            this.checkBoxResetPortAssignmentsOnStartup.Location = new System.Drawing.Point(199, 42);
            this.checkBoxResetPortAssignmentsOnStartup.Name = "checkBoxResetPortAssignmentsOnStartup";
            this.checkBoxResetPortAssignmentsOnStartup.Size = new System.Drawing.Size(192, 17);
            this.checkBoxResetPortAssignmentsOnStartup.TabIndex = 1;
            this.checkBoxResetPortAssignmentsOnStartup.Text = "::ResetPortAssignmentsOnStartup::";
            this.checkBoxResetPortAssignmentsOnStartup.UseVisualStyleBackColor = true;
            // 
            // checkBoxEnableUPnp
            // 
            this.checkBoxEnableUPnp.AutoSize = true;
            this.checkBoxEnableUPnp.Location = new System.Drawing.Point(199, 19);
            this.checkBoxEnableUPnp.Name = "checkBoxEnableUPnp";
            this.checkBoxEnableUPnp.Size = new System.Drawing.Size(98, 17);
            this.checkBoxEnableUPnp.TabIndex = 0;
            this.checkBoxEnableUPnp.Text = "::EnableUPnp::";
            this.checkBoxEnableUPnp.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.checkBoxInternetClientCanShowPolarPlots);
            this.groupBox2.Controls.Add(this.checkBoxAllowInternetProximityGadgets);
            this.groupBox2.Controls.Add(this.checkBoxInternetClientCanSubmitRoutes);
            this.groupBox2.Controls.Add(this.checkBoxInternetUsersCanViewPictures);
            this.groupBox2.Controls.Add(this.checkBoxInternetUserCanSeeLabels);
            this.groupBox2.Controls.Add(this.checkBoxInternetUsersCanRunReports);
            this.groupBox2.Controls.Add(this.checkBoxInternetUsersCanListenToAudio);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.numericInternetUserIdleTimeout);
            this.groupBox2.Location = new System.Drawing.Point(0, 125);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(636, 211);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "::OptionsWebServerInternetClientsCategory::";
            // 
            // checkBoxInternetClientCanShowPolarPlots
            // 
            this.checkBoxInternetClientCanShowPolarPlots.AutoSize = true;
            this.checkBoxInternetClientCanShowPolarPlots.Location = new System.Drawing.Point(199, 160);
            this.checkBoxInternetClientCanShowPolarPlots.Name = "checkBoxInternetClientCanShowPolarPlots";
            this.checkBoxInternetClientCanShowPolarPlots.Size = new System.Drawing.Size(193, 17);
            this.checkBoxInternetClientCanShowPolarPlots.TabIndex = 7;
            this.checkBoxInternetClientCanShowPolarPlots.Text = "::InternetClientCanShowPolarPlots::";
            this.checkBoxInternetClientCanShowPolarPlots.UseVisualStyleBackColor = true;
            // 
            // checkBoxAllowInternetProximityGadgets
            // 
            this.checkBoxAllowInternetProximityGadgets.AutoSize = true;
            this.checkBoxAllowInternetProximityGadgets.Location = new System.Drawing.Point(199, 183);
            this.checkBoxAllowInternetProximityGadgets.Name = "checkBoxAllowInternetProximityGadgets";
            this.checkBoxAllowInternetProximityGadgets.Size = new System.Drawing.Size(180, 17);
            this.checkBoxAllowInternetProximityGadgets.TabIndex = 8;
            this.checkBoxAllowInternetProximityGadgets.Text = "::AllowInternetProximityGadgets::";
            this.checkBoxAllowInternetProximityGadgets.UseVisualStyleBackColor = true;
            // 
            // checkBoxInternetClientCanSubmitRoutes
            // 
            this.checkBoxInternetClientCanSubmitRoutes.AutoSize = true;
            this.checkBoxInternetClientCanSubmitRoutes.Location = new System.Drawing.Point(199, 137);
            this.checkBoxInternetClientCanSubmitRoutes.Name = "checkBoxInternetClientCanSubmitRoutes";
            this.checkBoxInternetClientCanSubmitRoutes.Size = new System.Drawing.Size(185, 17);
            this.checkBoxInternetClientCanSubmitRoutes.TabIndex = 6;
            this.checkBoxInternetClientCanSubmitRoutes.Text = "::InternetClientCanSubmitRoutes::";
            this.checkBoxInternetClientCanSubmitRoutes.UseVisualStyleBackColor = true;
            // 
            // checkBoxInternetUsersCanViewPictures
            // 
            this.checkBoxInternetUsersCanViewPictures.AutoSize = true;
            this.checkBoxInternetUsersCanViewPictures.Location = new System.Drawing.Point(199, 91);
            this.checkBoxInternetUsersCanViewPictures.Name = "checkBoxInternetUsersCanViewPictures";
            this.checkBoxInternetUsersCanViewPictures.Size = new System.Drawing.Size(181, 17);
            this.checkBoxInternetUsersCanViewPictures.TabIndex = 4;
            this.checkBoxInternetUsersCanViewPictures.Text = "::InternetUsersCanViewPictures::";
            this.checkBoxInternetUsersCanViewPictures.UseVisualStyleBackColor = true;
            // 
            // checkBoxInternetUserCanSeeLabels
            // 
            this.checkBoxInternetUserCanSeeLabels.AutoSize = true;
            this.checkBoxInternetUserCanSeeLabels.Location = new System.Drawing.Point(199, 114);
            this.checkBoxInternetUserCanSeeLabels.Name = "checkBoxInternetUserCanSeeLabels";
            this.checkBoxInternetUserCanSeeLabels.Size = new System.Drawing.Size(165, 17);
            this.checkBoxInternetUserCanSeeLabels.TabIndex = 5;
            this.checkBoxInternetUserCanSeeLabels.Text = "::InternetUserCanSeeLabels::";
            this.checkBoxInternetUserCanSeeLabels.UseVisualStyleBackColor = true;
            // 
            // checkBoxInternetUsersCanRunReports
            // 
            this.checkBoxInternetUsersCanRunReports.AutoSize = true;
            this.checkBoxInternetUsersCanRunReports.Location = new System.Drawing.Point(199, 45);
            this.checkBoxInternetUsersCanRunReports.Name = "checkBoxInternetUsersCanRunReports";
            this.checkBoxInternetUsersCanRunReports.Size = new System.Drawing.Size(177, 17);
            this.checkBoxInternetUsersCanRunReports.TabIndex = 2;
            this.checkBoxInternetUsersCanRunReports.Text = "::InternetUsersCanRunReports::";
            this.checkBoxInternetUsersCanRunReports.UseVisualStyleBackColor = true;
            // 
            // checkBoxInternetUsersCanListenToAudio
            // 
            this.checkBoxInternetUsersCanListenToAudio.AutoSize = true;
            this.checkBoxInternetUsersCanListenToAudio.Location = new System.Drawing.Point(199, 68);
            this.checkBoxInternetUsersCanListenToAudio.Name = "checkBoxInternetUsersCanListenToAudio";
            this.checkBoxInternetUsersCanListenToAudio.Size = new System.Drawing.Size(188, 17);
            this.checkBoxInternetUsersCanListenToAudio.TabIndex = 3;
            this.checkBoxInternetUsersCanListenToAudio.Text = "::InternetUsersCanListenToAudio::";
            this.checkBoxInternetUsersCanListenToAudio.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "::IdleTimeout:::";
            // 
            // numericInternetUserIdleTimeout
            // 
            this.numericInternetUserIdleTimeout.Location = new System.Drawing.Point(199, 19);
            this.numericInternetUserIdleTimeout.Maximum = new decimal(new int[] {
            1440,
            0,
            0,
            0});
            this.numericInternetUserIdleTimeout.Name = "numericInternetUserIdleTimeout";
            this.numericInternetUserIdleTimeout.Size = new System.Drawing.Size(77, 20);
            this.numericInternetUserIdleTimeout.TabIndex = 1;
            this.numericInternetUserIdleTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // PageWebServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Name = "PageWebServer";
            this.Size = new System.Drawing.Size(636, 339);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUPnpPort)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericInternetUserIdleTimeout)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericUPnpPort;
        private System.Windows.Forms.CheckBox checkBoxAutoStartUPnP;
        private System.Windows.Forms.CheckBox checkBoxResetPortAssignmentsOnStartup;
        private System.Windows.Forms.CheckBox checkBoxEnableUPnp;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBoxInternetClientCanShowPolarPlots;
        private System.Windows.Forms.CheckBox checkBoxAllowInternetProximityGadgets;
        private System.Windows.Forms.CheckBox checkBoxInternetClientCanSubmitRoutes;
        private System.Windows.Forms.CheckBox checkBoxInternetUsersCanViewPictures;
        private System.Windows.Forms.CheckBox checkBoxInternetUserCanSeeLabels;
        private System.Windows.Forms.CheckBox checkBoxInternetUsersCanRunReports;
        private System.Windows.Forms.CheckBox checkBoxInternetUsersCanListenToAudio;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericInternetUserIdleTimeout;
    }
}
