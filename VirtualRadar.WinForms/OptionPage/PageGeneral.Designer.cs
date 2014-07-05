namespace VirtualRadar.WinForms.OptionPage
{
    partial class PageGeneral
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
            this.checkBoxMinimiseToSystemTray = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.numericDurationOfShortTrails = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numericDurationBeforeAircraftRemovedFromTracking = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numericDurationBeforeAircraftRemovedFromMap = new System.Windows.Forms.NumericUpDown();
            this.checkBoxAutomaticallyDownloadNewRoutes = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numericDaysBetweenChecks = new System.Windows.Forms.NumericUpDown();
            this.checkBoxCheckForNewVersions = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonTestAudio = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.numericReadingSpeed = new System.Windows.Forms.NumericUpDown();
            this.comboBoxTextToSpeechVoice = new VirtualRadar.WinForms.Controls.ComboBoxPlus();
            this.checkBoxAudioEnabled = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericDurationOfShortTrails)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericDurationBeforeAircraftRemovedFromTracking)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericDurationBeforeAircraftRemovedFromMap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericDaysBetweenChecks)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericReadingSpeed)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBoxMinimiseToSystemTray);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.numericDurationOfShortTrails);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.numericDurationBeforeAircraftRemovedFromTracking);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.numericDurationBeforeAircraftRemovedFromMap);
            this.groupBox1.Controls.Add(this.checkBoxAutomaticallyDownloadNewRoutes);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.numericDaysBetweenChecks);
            this.groupBox1.Controls.Add(this.checkBoxCheckForNewVersions);
            this.groupBox1.Location = new System.Drawing.Point(4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(629, 195);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "::OptionsGeneralCategory::";
            // 
            // checkBoxMinimiseToSystemTray
            // 
            this.checkBoxMinimiseToSystemTray.AutoSize = true;
            this.checkBoxMinimiseToSystemTray.Location = new System.Drawing.Point(335, 169);
            this.checkBoxMinimiseToSystemTray.Name = "checkBoxMinimiseToSystemTray";
            this.checkBoxMinimiseToSystemTray.Size = new System.Drawing.Size(146, 17);
            this.checkBoxMinimiseToSystemTray.TabIndex = 10;
            this.checkBoxMinimiseToSystemTray.Text = "::MinimiseToSystemTray::";
            this.checkBoxMinimiseToSystemTray.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 145);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(123, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "::DurationOfShortTrails:::";
            // 
            // numericDurationOfShortTrails
            // 
            this.numericDurationOfShortTrails.Location = new System.Drawing.Point(335, 143);
            this.numericDurationOfShortTrails.Maximum = new decimal(new int[] {
            1800,
            0,
            0,
            0});
            this.numericDurationOfShortTrails.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericDurationOfShortTrails.Name = "numericDurationOfShortTrails";
            this.numericDurationOfShortTrails.Size = new System.Drawing.Size(70, 20);
            this.numericDurationOfShortTrails.TabIndex = 9;
            this.numericDurationOfShortTrails.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericDurationOfShortTrails.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 119);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(237, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "::DurationBeforeAircraftRemovedFromTracking:::";
            // 
            // numericDurationBeforeAircraftRemovedFromTracking
            // 
            this.numericDurationBeforeAircraftRemovedFromTracking.Location = new System.Drawing.Point(335, 117);
            this.numericDurationBeforeAircraftRemovedFromTracking.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.numericDurationBeforeAircraftRemovedFromTracking.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericDurationBeforeAircraftRemovedFromTracking.Name = "numericDurationBeforeAircraftRemovedFromTracking";
            this.numericDurationBeforeAircraftRemovedFromTracking.Size = new System.Drawing.Size(70, 20);
            this.numericDurationBeforeAircraftRemovedFromTracking.TabIndex = 7;
            this.numericDurationBeforeAircraftRemovedFromTracking.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericDurationBeforeAircraftRemovedFromTracking.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 93);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(216, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "::DurationBeforeAircraftRemovedFromMap:::";
            // 
            // numericDurationBeforeAircraftRemovedFromMap
            // 
            this.numericDurationBeforeAircraftRemovedFromMap.Location = new System.Drawing.Point(335, 91);
            this.numericDurationBeforeAircraftRemovedFromMap.Maximum = new decimal(new int[] {
            540,
            0,
            0,
            0});
            this.numericDurationBeforeAircraftRemovedFromMap.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericDurationBeforeAircraftRemovedFromMap.Name = "numericDurationBeforeAircraftRemovedFromMap";
            this.numericDurationBeforeAircraftRemovedFromMap.Size = new System.Drawing.Size(70, 20);
            this.numericDurationBeforeAircraftRemovedFromMap.TabIndex = 5;
            this.numericDurationBeforeAircraftRemovedFromMap.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericDurationBeforeAircraftRemovedFromMap.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // checkBoxAutomaticallyDownloadNewRoutes
            // 
            this.checkBoxAutomaticallyDownloadNewRoutes.AutoSize = true;
            this.checkBoxAutomaticallyDownloadNewRoutes.Location = new System.Drawing.Point(335, 19);
            this.checkBoxAutomaticallyDownloadNewRoutes.Name = "checkBoxAutomaticallyDownloadNewRoutes";
            this.checkBoxAutomaticallyDownloadNewRoutes.Size = new System.Drawing.Size(204, 17);
            this.checkBoxAutomaticallyDownloadNewRoutes.TabIndex = 0;
            this.checkBoxAutomaticallyDownloadNewRoutes.Text = "::AutomaticallyDownloadNewRoutes::";
            this.checkBoxAutomaticallyDownloadNewRoutes.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "::DaysBetweenChecks:::";
            // 
            // numericDaysBetweenChecks
            // 
            this.numericDaysBetweenChecks.Location = new System.Drawing.Point(335, 65);
            this.numericDaysBetweenChecks.Maximum = new decimal(new int[] {
            365,
            0,
            0,
            0});
            this.numericDaysBetweenChecks.Name = "numericDaysBetweenChecks";
            this.numericDaysBetweenChecks.Size = new System.Drawing.Size(70, 20);
            this.numericDaysBetweenChecks.TabIndex = 3;
            this.numericDaysBetweenChecks.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // checkBoxCheckForNewVersions
            // 
            this.checkBoxCheckForNewVersions.AutoSize = true;
            this.checkBoxCheckForNewVersions.Location = new System.Drawing.Point(335, 42);
            this.checkBoxCheckForNewVersions.Name = "checkBoxCheckForNewVersions";
            this.checkBoxCheckForNewVersions.Size = new System.Drawing.Size(146, 17);
            this.checkBoxCheckForNewVersions.TabIndex = 1;
            this.checkBoxCheckForNewVersions.Text = "::CheckForNewVersions::";
            this.checkBoxCheckForNewVersions.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.buttonTestAudio);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.numericReadingSpeed);
            this.groupBox2.Controls.Add(this.comboBoxTextToSpeechVoice);
            this.groupBox2.Controls.Add(this.checkBoxAudioEnabled);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(4, 206);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(629, 95);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "::Audio::";
            // 
            // buttonTestAudio
            // 
            this.buttonTestAudio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTestAudio.Location = new System.Drawing.Point(467, 65);
            this.buttonTestAudio.Name = "buttonTestAudio";
            this.buttonTestAudio.Size = new System.Drawing.Size(156, 23);
            this.buttonTestAudio.TabIndex = 5;
            this.buttonTestAudio.Text = "::TestAudioSettings::";
            this.buttonTestAudio.UseVisualStyleBackColor = true;
            this.buttonTestAudio.Click += new System.EventHandler(this.buttonTestAudio_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 67);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(93, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "::ReadingSpeed:::";
            // 
            // numericReadingSpeed
            // 
            this.numericReadingSpeed.Location = new System.Drawing.Point(335, 65);
            this.numericReadingSpeed.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericReadingSpeed.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.numericReadingSpeed.Name = "numericReadingSpeed";
            this.numericReadingSpeed.Size = new System.Drawing.Size(70, 20);
            this.numericReadingSpeed.TabIndex = 4;
            this.numericReadingSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericReadingSpeed.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // comboBoxTextToSpeechVoice
            // 
            this.comboBoxTextToSpeechVoice.DisplayMember = "Description";
            this.comboBoxTextToSpeechVoice.FormattingEnabled = true;
            this.comboBoxTextToSpeechVoice.Location = new System.Drawing.Point(335, 38);
            this.comboBoxTextToSpeechVoice.Name = "comboBoxTextToSpeechVoice";
            this.comboBoxTextToSpeechVoice.Size = new System.Drawing.Size(160, 21);
            this.comboBoxTextToSpeechVoice.TabIndex = 2;
            this.comboBoxTextToSpeechVoice.ValueMember = "Value";
            // 
            // checkBoxAudioEnabled
            // 
            this.checkBoxAudioEnabled.AutoSize = true;
            this.checkBoxAudioEnabled.Location = new System.Drawing.Point(335, 14);
            this.checkBoxAudioEnabled.Name = "checkBoxAudioEnabled";
            this.checkBoxAudioEnabled.Size = new System.Drawing.Size(77, 17);
            this.checkBoxAudioEnabled.TabIndex = 0;
            this.checkBoxAudioEnabled.Text = "::Enabled::";
            this.checkBoxAudioEnabled.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 46);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(120, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "::TextToSpeechVoice:::";
            // 
            // PageGeneral
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "PageGeneral";
            this.Size = new System.Drawing.Size(636, 305);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericDurationOfShortTrails)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericDurationBeforeAircraftRemovedFromTracking)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericDurationBeforeAircraftRemovedFromMap)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericDaysBetweenChecks)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericReadingSpeed)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxCheckForNewVersions;
        private System.Windows.Forms.NumericUpDown numericDaysBetweenChecks;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxAutomaticallyDownloadNewRoutes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericDurationBeforeAircraftRemovedFromMap;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericDurationBeforeAircraftRemovedFromTracking;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericDurationOfShortTrails;
        private System.Windows.Forms.CheckBox checkBoxMinimiseToSystemTray;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBoxAudioEnabled;
        private Controls.ComboBoxPlus comboBoxTextToSpeechVoice;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericReadingSpeed;
        private System.Windows.Forms.Button buttonTestAudio;
    }
}
