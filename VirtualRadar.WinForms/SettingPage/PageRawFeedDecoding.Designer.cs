namespace VirtualRadar.WinForms.SettingPage
{
    partial class PageRawFeedDecoding
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
            this.label18 = new System.Windows.Forms.Label();
            this.checkBoxSuppressReceiverRangeCheck = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.numericReceiverRange = new System.Windows.Forms.NumericUpDown();
            this.numericAcceptIcaoInNonPISeconds = new System.Windows.Forms.NumericUpDown();
            this.numericAcceptableSurfaceSpeed = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.numericAcceptIcaoInNonPICount = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.checkBoxIgnoreCallsignsInBds20 = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.numericAcceptableAirSurfaceTransitionSpeed = new System.Windows.Forms.NumericUpDown();
            this.numericAcceptIcaoInPI0Seconds = new System.Windows.Forms.NumericUpDown();
            this.linkLabelUseIcaoSettings = new System.Windows.Forms.LinkLabel();
            this.label8 = new System.Windows.Forms.Label();
            this.numericAcceptIcaoInPI0Count = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numericAcceptableAirborneSpeed = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numericSlowSurfaceGlobalPositionLimit = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBoxIgnoreBadCodeblockNonPI0 = new System.Windows.Forms.CheckBox();
            this.checkBoxIgnoreBadCodeblockPI0 = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxSuppressIcao0 = new System.Windows.Forms.CheckBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.numericFastSurfaceGlobalPositionLimit = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxUseLocalDecodeForInitialPosition = new System.Windows.Forms.CheckBox();
            this.numericAirborneGlobalPositionLimit = new System.Windows.Forms.NumericUpDown();
            this.checkBoxIgnoreMilitaryExtendedSquitter = new System.Windows.Forms.CheckBox();
            this.linkLabelUseRecommendedSettings = new System.Windows.Forms.LinkLabel();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericReceiverRange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAcceptIcaoInNonPISeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAcceptableSurfaceSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAcceptIcaoInNonPICount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAcceptableAirSurfaceTransitionSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAcceptIcaoInPI0Seconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAcceptIcaoInPI0Count)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAcceptableAirborneSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericSlowSurfaceGlobalPositionLimit)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericFastSurfaceGlobalPositionLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAirborneGlobalPositionLimit)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label18);
            this.groupBox1.Controls.Add(this.checkBoxSuppressReceiverRangeCheck);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.numericReceiverRange);
            this.groupBox1.Location = new System.Drawing.Point(0, 22);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(636, 72);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "::Range::";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(283, 21);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(74, 13);
            this.label18.TabIndex = 3;
            this.label18.Text = "::PKilometres::";
            // 
            // checkBoxSuppressReceiverRangeCheck
            // 
            this.checkBoxSuppressReceiverRangeCheck.AutoSize = true;
            this.checkBoxSuppressReceiverRangeCheck.Location = new System.Drawing.Point(200, 45);
            this.checkBoxSuppressReceiverRangeCheck.Name = "checkBoxSuppressReceiverRangeCheck";
            this.checkBoxSuppressReceiverRangeCheck.Size = new System.Drawing.Size(188, 17);
            this.checkBoxSuppressReceiverRangeCheck.TabIndex = 2;
            this.checkBoxSuppressReceiverRangeCheck.Text = "::SuppressReceiverRangeCheck::";
            this.checkBoxSuppressReceiverRangeCheck.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 21);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(97, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "::ReceiverRange:::";
            // 
            // numericReceiverRange
            // 
            this.numericReceiverRange.Location = new System.Drawing.Point(200, 19);
            this.numericReceiverRange.Maximum = new decimal(new int[] {
            20037,
            0,
            0,
            0});
            this.numericReceiverRange.Name = "numericReceiverRange";
            this.numericReceiverRange.Size = new System.Drawing.Size(77, 20);
            this.numericReceiverRange.TabIndex = 1;
            this.numericReceiverRange.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // numericAcceptIcaoInNonPISeconds
            // 
            this.numericAcceptIcaoInNonPISeconds.Location = new System.Drawing.Point(301, 45);
            this.numericAcceptIcaoInNonPISeconds.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.numericAcceptIcaoInNonPISeconds.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericAcceptIcaoInNonPISeconds.Name = "numericAcceptIcaoInNonPISeconds";
            this.numericAcceptIcaoInNonPISeconds.Size = new System.Drawing.Size(77, 20);
            this.numericAcceptIcaoInNonPISeconds.TabIndex = 7;
            this.numericAcceptIcaoInNonPISeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericAcceptIcaoInNonPISeconds.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // numericAcceptableSurfaceSpeed
            // 
            this.numericAcceptableSurfaceSpeed.DecimalPlaces = 3;
            this.numericAcceptableSurfaceSpeed.Location = new System.Drawing.Point(200, 195);
            this.numericAcceptableSurfaceSpeed.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericAcceptableSurfaceSpeed.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numericAcceptableSurfaceSpeed.Name = "numericAcceptableSurfaceSpeed";
            this.numericAcceptableSurfaceSpeed.Size = new System.Drawing.Size(77, 20);
            this.numericAcceptableSurfaceSpeed.TabIndex = 13;
            this.numericAcceptableSurfaceSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericAcceptableSurfaceSpeed.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 47);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(144, 13);
            this.label10.TabIndex = 4;
            this.label10.Text = "::AcceptIcaoInNonPICount:::";
            // 
            // numericAcceptIcaoInNonPICount
            // 
            this.numericAcceptIcaoInNonPICount.Location = new System.Drawing.Point(200, 45);
            this.numericAcceptIcaoInNonPICount.Name = "numericAcceptIcaoInNonPICount";
            this.numericAcceptIcaoInNonPICount.Size = new System.Drawing.Size(77, 20);
            this.numericAcceptIcaoInNonPICount.TabIndex = 5;
            this.numericAcceptIcaoInNonPICount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericAcceptIcaoInNonPICount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 197);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(110, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "::MaxSurfaceSpeed:::";
            // 
            // checkBoxIgnoreCallsignsInBds20
            // 
            this.checkBoxIgnoreCallsignsInBds20.AutoSize = true;
            this.checkBoxIgnoreCallsignsInBds20.Location = new System.Drawing.Point(200, 221);
            this.checkBoxIgnoreCallsignsInBds20.Name = "checkBoxIgnoreCallsignsInBds20";
            this.checkBoxIgnoreCallsignsInBds20.Size = new System.Drawing.Size(148, 17);
            this.checkBoxIgnoreCallsignsInBds20.TabIndex = 14;
            this.checkBoxIgnoreCallsignsInBds20.Text = "::IgnoreCallsignsInBds20::";
            this.checkBoxIgnoreCallsignsInBds20.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 171);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(119, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "::MaxTransitionSpeed:::";
            // 
            // numericAcceptableAirSurfaceTransitionSpeed
            // 
            this.numericAcceptableAirSurfaceTransitionSpeed.DecimalPlaces = 3;
            this.numericAcceptableAirSurfaceTransitionSpeed.Location = new System.Drawing.Point(200, 169);
            this.numericAcceptableAirSurfaceTransitionSpeed.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericAcceptableAirSurfaceTransitionSpeed.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            196608});
            this.numericAcceptableAirSurfaceTransitionSpeed.Name = "numericAcceptableAirSurfaceTransitionSpeed";
            this.numericAcceptableAirSurfaceTransitionSpeed.Size = new System.Drawing.Size(77, 20);
            this.numericAcceptableAirSurfaceTransitionSpeed.TabIndex = 11;
            this.numericAcceptableAirSurfaceTransitionSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericAcceptableAirSurfaceTransitionSpeed.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // numericAcceptIcaoInPI0Seconds
            // 
            this.numericAcceptIcaoInPI0Seconds.Location = new System.Drawing.Point(301, 19);
            this.numericAcceptIcaoInPI0Seconds.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.numericAcceptIcaoInPI0Seconds.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericAcceptIcaoInPI0Seconds.Name = "numericAcceptIcaoInPI0Seconds";
            this.numericAcceptIcaoInPI0Seconds.Size = new System.Drawing.Size(77, 20);
            this.numericAcceptIcaoInPI0Seconds.TabIndex = 3;
            this.numericAcceptIcaoInPI0Seconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericAcceptIcaoInPI0Seconds.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // linkLabelUseIcaoSettings
            // 
            this.linkLabelUseIcaoSettings.AutoSize = true;
            this.linkLabelUseIcaoSettings.Location = new System.Drawing.Point(6, 0);
            this.linkLabelUseIcaoSettings.Name = "linkLabelUseIcaoSettings";
            this.linkLabelUseIcaoSettings.Size = new System.Drawing.Size(158, 13);
            this.linkLabelUseIcaoSettings.TabIndex = 0;
            this.linkLabelUseIcaoSettings.TabStop = true;
            this.linkLabelUseIcaoSettings.Text = "::UseIcaoSpecificationSettings::";
            this.linkLabelUseIcaoSettings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelUseIcaoSettings_LinkClicked);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 21);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(130, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "::AcceptIcaoInPI0Count:::";
            // 
            // numericAcceptIcaoInPI0Count
            // 
            this.numericAcceptIcaoInPI0Count.Location = new System.Drawing.Point(200, 19);
            this.numericAcceptIcaoInPI0Count.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericAcceptIcaoInPI0Count.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericAcceptIcaoInPI0Count.Name = "numericAcceptIcaoInPI0Count";
            this.numericAcceptIcaoInPI0Count.Size = new System.Drawing.Size(77, 20);
            this.numericAcceptIcaoInPI0Count.TabIndex = 1;
            this.numericAcceptIcaoInPI0Count.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericAcceptIcaoInPI0Count.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 145);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(112, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "::MaxAirborneSpeed:::";
            // 
            // numericAcceptableAirborneSpeed
            // 
            this.numericAcceptableAirborneSpeed.DecimalPlaces = 3;
            this.numericAcceptableAirborneSpeed.Location = new System.Drawing.Point(200, 143);
            this.numericAcceptableAirborneSpeed.Maximum = new decimal(new int[] {
            45,
            0,
            0,
            0});
            this.numericAcceptableAirborneSpeed.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            196608});
            this.numericAcceptableAirborneSpeed.Name = "numericAcceptableAirborneSpeed";
            this.numericAcceptableAirborneSpeed.Size = new System.Drawing.Size(77, 20);
            this.numericAcceptableAirborneSpeed.TabIndex = 9;
            this.numericAcceptableAirborneSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericAcceptableAirborneSpeed.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 119);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(112, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "::SlowSurfaceGlobal:::";
            // 
            // numericSlowSurfaceGlobalPositionLimit
            // 
            this.numericSlowSurfaceGlobalPositionLimit.Location = new System.Drawing.Point(200, 117);
            this.numericSlowSurfaceGlobalPositionLimit.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.numericSlowSurfaceGlobalPositionLimit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericSlowSurfaceGlobalPositionLimit.Name = "numericSlowSurfaceGlobalPositionLimit";
            this.numericSlowSurfaceGlobalPositionLimit.Size = new System.Drawing.Size(77, 20);
            this.numericSlowSurfaceGlobalPositionLimit.TabIndex = 7;
            this.numericSlowSurfaceGlobalPositionLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericSlowSurfaceGlobalPositionLimit.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 93);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(109, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "::FastSurfaceGlobal:::";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.checkBoxIgnoreBadCodeblockNonPI0);
            this.groupBox3.Controls.Add(this.checkBoxIgnoreBadCodeblockPI0);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.label21);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.label20);
            this.groupBox3.Controls.Add(this.label19);
            this.groupBox3.Controls.Add(this.numericAcceptIcaoInNonPISeconds);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.numericAcceptIcaoInNonPICount);
            this.groupBox3.Controls.Add(this.numericAcceptIcaoInPI0Seconds);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.numericAcceptIcaoInPI0Count);
            this.groupBox3.Location = new System.Drawing.Point(0, 375);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(636, 96);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "::OptionsRawFeedAcceptIcaoAsValidCategory::";
            // 
            // checkBoxIgnoreBadCodeblockNonPI0
            // 
            this.checkBoxIgnoreBadCodeblockNonPI0.AutoSize = true;
            this.checkBoxIgnoreBadCodeblockNonPI0.Location = new System.Drawing.Point(389, 72);
            this.checkBoxIgnoreBadCodeblockNonPI0.Name = "checkBoxIgnoreBadCodeblockNonPI0";
            this.checkBoxIgnoreBadCodeblockNonPI0.Size = new System.Drawing.Size(131, 17);
            this.checkBoxIgnoreBadCodeblockNonPI0.TabIndex = 14;
            this.checkBoxIgnoreBadCodeblockNonPI0.Text = "::InNonPI0Messages::";
            this.checkBoxIgnoreBadCodeblockNonPI0.UseVisualStyleBackColor = true;
            // 
            // checkBoxIgnoreBadCodeblockPI0
            // 
            this.checkBoxIgnoreBadCodeblockPI0.AutoSize = true;
            this.checkBoxIgnoreBadCodeblockPI0.Location = new System.Drawing.Point(200, 72);
            this.checkBoxIgnoreBadCodeblockPI0.Name = "checkBoxIgnoreBadCodeblockPI0";
            this.checkBoxIgnoreBadCodeblockPI0.Size = new System.Drawing.Size(111, 17);
            this.checkBoxIgnoreBadCodeblockPI0.TabIndex = 13;
            this.checkBoxIgnoreBadCodeblockPI0.Text = "::InPI0Messages::";
            this.checkBoxIgnoreBadCodeblockPI0.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 73);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(129, 13);
            this.label11.TabIndex = 12;
            this.label11.Text = "::IgnoreUnassignedIcao:::";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(386, 47);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(61, 13);
            this.label21.TabIndex = 11;
            this.label21.Text = "::Seconds::";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(283, 47);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(12, 13);
            this.label9.TabIndex = 10;
            this.label9.Text = "/";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(386, 21);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(61, 13);
            this.label20.TabIndex = 9;
            this.label20.Text = "::Seconds::";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(283, 21);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(12, 13);
            this.label19.TabIndex = 8;
            this.label19.Text = "/";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.checkBoxSuppressIcao0);
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.label14);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.checkBoxIgnoreCallsignsInBds20);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.numericAcceptableSurfaceSpeed);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.numericAcceptableAirSurfaceTransitionSpeed);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.numericAcceptableAirborneSpeed);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.numericSlowSurfaceGlobalPositionLimit);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.numericFastSurfaceGlobalPositionLimit);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.checkBoxUseLocalDecodeForInitialPosition);
            this.groupBox2.Controls.Add(this.numericAirborneGlobalPositionLimit);
            this.groupBox2.Controls.Add(this.checkBoxIgnoreMilitaryExtendedSquitter);
            this.groupBox2.Location = new System.Drawing.Point(0, 100);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(636, 269);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "::OptionsRawFeedDecoderParametersCategory::";
            // 
            // checkBoxSuppressIcao0
            // 
            this.checkBoxSuppressIcao0.AutoSize = true;
            this.checkBoxSuppressIcao0.Location = new System.Drawing.Point(200, 244);
            this.checkBoxSuppressIcao0.Name = "checkBoxSuppressIcao0";
            this.checkBoxSuppressIcao0.Size = new System.Drawing.Size(113, 17);
            this.checkBoxSuppressIcao0.TabIndex = 21;
            this.checkBoxSuppressIcao0.Text = "::SuppressICAO0::";
            this.checkBoxSuppressIcao0.UseVisualStyleBackColor = true;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(283, 197);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(151, 13);
            this.label17.TabIndex = 20;
            this.label17.Text = "::PKilometresOver30Seconds::";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(283, 171);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(151, 13);
            this.label16.TabIndex = 19;
            this.label16.Text = "::PKilometresOver30Seconds::";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(283, 145);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(151, 13);
            this.label15.TabIndex = 18;
            this.label15.Text = "::PKilometresOver30Seconds::";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(283, 119);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(68, 13);
            this.label14.TabIndex = 17;
            this.label14.Text = "::PSeconds::";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(283, 93);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(68, 13);
            this.label13.TabIndex = 16;
            this.label13.Text = "::PSeconds::";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(283, 67);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(68, 13);
            this.label12.TabIndex = 15;
            this.label12.Text = "::PSeconds::";
            // 
            // numericFastSurfaceGlobalPositionLimit
            // 
            this.numericFastSurfaceGlobalPositionLimit.Location = new System.Drawing.Point(200, 91);
            this.numericFastSurfaceGlobalPositionLimit.Maximum = new decimal(new int[] {
            75,
            0,
            0,
            0});
            this.numericFastSurfaceGlobalPositionLimit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericFastSurfaceGlobalPositionLimit.Name = "numericFastSurfaceGlobalPositionLimit";
            this.numericFastSurfaceGlobalPositionLimit.Size = new System.Drawing.Size(77, 20);
            this.numericFastSurfaceGlobalPositionLimit.TabIndex = 5;
            this.numericFastSurfaceGlobalPositionLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericFastSurfaceGlobalPositionLimit.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "::AirborneGlobal:::";
            // 
            // checkBoxUseLocalDecodeForInitialPosition
            // 
            this.checkBoxUseLocalDecodeForInitialPosition.AutoSize = true;
            this.checkBoxUseLocalDecodeForInitialPosition.Location = new System.Drawing.Point(200, 42);
            this.checkBoxUseLocalDecodeForInitialPosition.Name = "checkBoxUseLocalDecodeForInitialPosition";
            this.checkBoxUseLocalDecodeForInitialPosition.Size = new System.Drawing.Size(197, 17);
            this.checkBoxUseLocalDecodeForInitialPosition.TabIndex = 1;
            this.checkBoxUseLocalDecodeForInitialPosition.Text = "::UseLocalDecodeForInitialPosition::";
            this.checkBoxUseLocalDecodeForInitialPosition.UseVisualStyleBackColor = true;
            // 
            // numericAirborneGlobalPositionLimit
            // 
            this.numericAirborneGlobalPositionLimit.Location = new System.Drawing.Point(200, 65);
            this.numericAirborneGlobalPositionLimit.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.numericAirborneGlobalPositionLimit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericAirborneGlobalPositionLimit.Name = "numericAirborneGlobalPositionLimit";
            this.numericAirborneGlobalPositionLimit.Size = new System.Drawing.Size(77, 20);
            this.numericAirborneGlobalPositionLimit.TabIndex = 3;
            this.numericAirborneGlobalPositionLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericAirborneGlobalPositionLimit.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // checkBoxIgnoreMilitaryExtendedSquitter
            // 
            this.checkBoxIgnoreMilitaryExtendedSquitter.AutoSize = true;
            this.checkBoxIgnoreMilitaryExtendedSquitter.Location = new System.Drawing.Point(200, 19);
            this.checkBoxIgnoreMilitaryExtendedSquitter.Name = "checkBoxIgnoreMilitaryExtendedSquitter";
            this.checkBoxIgnoreMilitaryExtendedSquitter.Size = new System.Drawing.Size(181, 17);
            this.checkBoxIgnoreMilitaryExtendedSquitter.TabIndex = 0;
            this.checkBoxIgnoreMilitaryExtendedSquitter.Text = "::IgnoreMilitaryExtendedSquitter::";
            this.checkBoxIgnoreMilitaryExtendedSquitter.UseVisualStyleBackColor = true;
            // 
            // linkLabelUseRecommendedSettings
            // 
            this.linkLabelUseRecommendedSettings.AutoSize = true;
            this.linkLabelUseRecommendedSettings.Location = new System.Drawing.Point(297, 0);
            this.linkLabelUseRecommendedSettings.Name = "linkLabelUseRecommendedSettings";
            this.linkLabelUseRecommendedSettings.Size = new System.Drawing.Size(148, 13);
            this.linkLabelUseRecommendedSettings.TabIndex = 1;
            this.linkLabelUseRecommendedSettings.TabStop = true;
            this.linkLabelUseRecommendedSettings.Text = "::UseRecommendedSettings::";
            this.linkLabelUseRecommendedSettings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelUseRecommendedSettings_LinkClicked);
            // 
            // PageRawFeedDecoding
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.linkLabelUseIcaoSettings);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.linkLabelUseRecommendedSettings);
            this.Name = "PageRawFeedDecoding";
            this.Size = new System.Drawing.Size(636, 475);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericReceiverRange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAcceptIcaoInNonPISeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAcceptableSurfaceSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAcceptIcaoInNonPICount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAcceptableAirSurfaceTransitionSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAcceptIcaoInPI0Seconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAcceptIcaoInPI0Count)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAcceptableAirborneSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericSlowSurfaceGlobalPositionLimit)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericFastSurfaceGlobalPositionLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericAirborneGlobalPositionLimit)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxSuppressReceiverRangeCheck;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericReceiverRange;
        private System.Windows.Forms.NumericUpDown numericAcceptIcaoInNonPISeconds;
        private System.Windows.Forms.NumericUpDown numericAcceptableSurfaceSpeed;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown numericAcceptIcaoInNonPICount;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox checkBoxIgnoreCallsignsInBds20;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericAcceptableAirSurfaceTransitionSpeed;
        private System.Windows.Forms.NumericUpDown numericAcceptIcaoInPI0Seconds;
        private System.Windows.Forms.LinkLabel linkLabelUseIcaoSettings;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numericAcceptIcaoInPI0Count;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericAcceptableAirborneSpeed;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericSlowSurfaceGlobalPositionLimit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown numericFastSurfaceGlobalPositionLimit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxUseLocalDecodeForInitialPosition;
        private System.Windows.Forms.NumericUpDown numericAirborneGlobalPositionLimit;
        private System.Windows.Forms.CheckBox checkBoxIgnoreMilitaryExtendedSquitter;
        private System.Windows.Forms.LinkLabel linkLabelUseRecommendedSettings;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.CheckBox checkBoxSuppressIcao0;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox checkBoxIgnoreBadCodeblockNonPI0;
        private System.Windows.Forms.CheckBox checkBoxIgnoreBadCodeblockPI0;
        private System.Windows.Forms.Label label11;
    }
}
