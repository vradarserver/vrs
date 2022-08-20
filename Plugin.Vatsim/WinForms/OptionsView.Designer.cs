
namespace VirtualRadar.Plugin.Vatsim.WinForms
{
    partial class OptionsView
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.chkEnabled = new System.Windows.Forms.CheckBox();
            this.nudRefreshIntervalSeconds = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chkAssumeSlowAircraftAreOnGround = new System.Windows.Forms.CheckBox();
            this.nudSlowAircraftThresholdSpeedKnots = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.chkInferModelFromModelType = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.cmbDistanceUnit = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.nudHeight = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.nudWidth = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.txtAirportCode = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.nudLongitude = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.nudLatitude = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.cmbCentreOn = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtFeedName = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lvwGeofencedFeeds = new System.Windows.Forms.ListView();
            this.colFeedName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colCentredOn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colLatitude = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colLongitude = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAirport = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPilotCid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colWidth = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colHeight = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDistanceUnit = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.nudPilotCid = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nudRefreshIntervalSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSlowAircraftThresholdSpeedKnots)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLongitude)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLatitude)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPilotCid)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(758, 470);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 14;
            this.btnCancel.Text = "::Cancel::";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSave.Location = new System.Drawing.Point(677, 470);
            this.btnSave.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 13;
            this.btnSave.Text = "::Save::";
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // chkEnabled
            // 
            this.chkEnabled.AutoSize = true;
            this.chkEnabled.Location = new System.Drawing.Point(169, 12);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.Size = new System.Drawing.Size(77, 17);
            this.chkEnabled.TabIndex = 15;
            this.chkEnabled.Text = "::Enabled::";
            this.chkEnabled.UseVisualStyleBackColor = true;
            // 
            // nudRefreshIntervalSeconds
            // 
            this.nudRefreshIntervalSeconds.Location = new System.Drawing.Point(609, 13);
            this.nudRefreshIntervalSeconds.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.nudRefreshIntervalSeconds.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nudRefreshIntervalSeconds.Name = "nudRefreshIntervalSeconds";
            this.nudRefreshIntervalSeconds.Size = new System.Drawing.Size(60, 20);
            this.nudRefreshIntervalSeconds.TabIndex = 16;
            this.nudRefreshIntervalSeconds.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(452, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "::RefreshInterval:::";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(675, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 18;
            this.label2.Text = "::Seconds::";
            // 
            // chkAssumeSlowAircraftAreOnGround
            // 
            this.chkAssumeSlowAircraftAreOnGround.AutoSize = true;
            this.chkAssumeSlowAircraftAreOnGround.Location = new System.Drawing.Point(169, 40);
            this.chkAssumeSlowAircraftAreOnGround.Name = "chkAssumeSlowAircraftAreOnGround";
            this.chkAssumeSlowAircraftAreOnGround.Size = new System.Drawing.Size(196, 17);
            this.chkAssumeSlowAircraftAreOnGround.TabIndex = 19;
            this.chkAssumeSlowAircraftAreOnGround.Text = "::AssumeSlowAircraftAreOnGround::";
            this.chkAssumeSlowAircraftAreOnGround.UseVisualStyleBackColor = true;
            // 
            // nudSlowAircraftThresholdSpeedKnots
            // 
            this.nudSlowAircraftThresholdSpeedKnots.Location = new System.Drawing.Point(609, 39);
            this.nudSlowAircraftThresholdSpeedKnots.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.nudSlowAircraftThresholdSpeedKnots.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nudSlowAircraftThresholdSpeedKnots.Name = "nudSlowAircraftThresholdSpeedKnots";
            this.nudSlowAircraftThresholdSpeedKnots.Size = new System.Drawing.Size(60, 20);
            this.nudSlowAircraftThresholdSpeedKnots.TabIndex = 20;
            this.nudSlowAircraftThresholdSpeedKnots.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(675, 41);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 21;
            this.label3.Text = "::Knots::";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(452, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(108, 13);
            this.label4.TabIndex = 22;
            this.label4.Text = "::GroundSpeedMax:::";
            // 
            // chkInferModelFromModelType
            // 
            this.chkInferModelFromModelType.AutoSize = true;
            this.chkInferModelFromModelType.Location = new System.Drawing.Point(169, 68);
            this.chkInferModelFromModelType.Name = "chkInferModelFromModelType";
            this.chkInferModelFromModelType.Size = new System.Drawing.Size(164, 17);
            this.chkInferModelFromModelType.TabIndex = 23;
            this.chkInferModelFromModelType.Text = "::InferModelFromModelType::";
            this.chkInferModelFromModelType.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.nudPilotCid);
            this.groupBox1.Controls.Add(this.btnAdd);
            this.groupBox1.Controls.Add(this.btnDelete);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.cmbDistanceUnit);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.nudHeight);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.nudWidth);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.txtAirportCode);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.nudLongitude);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.nudLatitude);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.cmbCentreOn);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtFeedName);
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Controls.Add(this.lvwGeofencedFeeds);
            this.groupBox1.Location = new System.Drawing.Point(15, 91);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(818, 367);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "::GeofencedFeeds::";
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAdd.Location = new System.Drawing.Point(6, 194);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 22;
            this.btnAdd.Text = "::Add::";
            this.btnAdd.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Location = new System.Drawing.Point(737, 194);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 21;
            this.btnDelete.Text = "::Delete::";
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // label14
            // 
            this.label14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 341);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(106, 13);
            this.label14.TabIndex = 20;
            this.label14.Text = "::DistanceUnitLabel::";
            // 
            // cmbDistanceUnit
            // 
            this.cmbDistanceUnit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbDistanceUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDistanceUnit.FormattingEnabled = true;
            this.cmbDistanceUnit.Location = new System.Drawing.Point(165, 338);
            this.cmbDistanceUnit.Name = "cmbDistanceUnit";
            this.cmbDistanceUnit.Size = new System.Drawing.Size(190, 21);
            this.cmbDistanceUnit.TabIndex = 19;
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 314);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(79, 13);
            this.label13.TabIndex = 17;
            this.label13.Text = "::HeightLabel:::";
            // 
            // nudHeight
            // 
            this.nudHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudHeight.DecimalPlaces = 2;
            this.nudHeight.Location = new System.Drawing.Point(165, 312);
            this.nudHeight.Maximum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudHeight.Name = "nudHeight";
            this.nudHeight.Size = new System.Drawing.Size(100, 20);
            this.nudHeight.TabIndex = 18;
            this.nudHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 288);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(76, 13);
            this.label12.TabIndex = 15;
            this.label12.Text = "::WidthLabel:::";
            // 
            // nudWidth
            // 
            this.nudWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudWidth.DecimalPlaces = 2;
            this.nudWidth.Location = new System.Drawing.Point(165, 286);
            this.nudWidth.Maximum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudWidth.Name = "nudWidth";
            this.nudWidth.Size = new System.Drawing.Size(100, 20);
            this.nudWidth.TabIndex = 16;
            this.nudWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(437, 341);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(60, 13);
            this.label11.TabIndex = 14;
            this.label11.Text = "::PilotCID:::";
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(437, 315);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(74, 13);
            this.label10.TabIndex = 12;
            this.label10.Text = "::AirportCode::";
            // 
            // txtAirportCode
            // 
            this.txtAirportCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtAirportCode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtAirportCode.Location = new System.Drawing.Point(596, 312);
            this.txtAirportCode.MaxLength = 4;
            this.txtAirportCode.Name = "txtAirportCode";
            this.txtAirportCode.Size = new System.Drawing.Size(100, 20);
            this.txtAirportCode.TabIndex = 11;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(437, 288);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(95, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "::LongitudeLabel:::";
            // 
            // nudLongitude
            // 
            this.nudLongitude.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudLongitude.DecimalPlaces = 6;
            this.nudLongitude.Location = new System.Drawing.Point(596, 286);
            this.nudLongitude.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.nudLongitude.Minimum = new decimal(new int[] {
            180,
            0,
            0,
            -2147483648});
            this.nudLongitude.Name = "nudLongitude";
            this.nudLongitude.Size = new System.Drawing.Size(100, 20);
            this.nudLongitude.TabIndex = 10;
            this.nudLongitude.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(437, 262);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(86, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "::LatitudeLabel:::";
            // 
            // nudLatitude
            // 
            this.nudLatitude.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudLatitude.DecimalPlaces = 6;
            this.nudLatitude.Location = new System.Drawing.Point(596, 260);
            this.nudLatitude.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.nudLatitude.Minimum = new decimal(new int[] {
            90,
            0,
            0,
            -2147483648});
            this.nudLatitude.Name = "nudLatitude";
            this.nudLatitude.Size = new System.Drawing.Size(100, 20);
            this.nudLatitude.TabIndex = 8;
            this.nudLatitude.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 260);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(99, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "::CentredOnLabel:::";
            // 
            // cmbCentreOn
            // 
            this.cmbCentreOn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbCentreOn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCentreOn.FormattingEnabled = true;
            this.cmbCentreOn.Location = new System.Drawing.Point(165, 257);
            this.cmbCentreOn.Name = "cmbCentreOn";
            this.cmbCentreOn.Size = new System.Drawing.Size(190, 21);
            this.cmbCentreOn.TabIndex = 5;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label6.Location = new System.Drawing.Point(162, 234);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "VATSIM:";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 234);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "::FeedNameLabel:::";
            // 
            // txtFeedName
            // 
            this.txtFeedName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtFeedName.Location = new System.Drawing.Point(218, 231);
            this.txtFeedName.Name = "txtFeedName";
            this.txtFeedName.Size = new System.Drawing.Size(137, 20);
            this.txtFeedName.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel1.Location = new System.Drawing.Point(6, 223);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(806, 1);
            this.panel1.TabIndex = 1;
            // 
            // lvwGeofencedFeeds
            // 
            this.lvwGeofencedFeeds.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvwGeofencedFeeds.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colFeedName,
            this.colCentredOn,
            this.colLatitude,
            this.colLongitude,
            this.colAirport,
            this.colPilotCid,
            this.colWidth,
            this.colHeight,
            this.colDistanceUnit});
            this.lvwGeofencedFeeds.FullRowSelect = true;
            this.lvwGeofencedFeeds.GridLines = true;
            this.lvwGeofencedFeeds.HideSelection = false;
            this.lvwGeofencedFeeds.Location = new System.Drawing.Point(6, 19);
            this.lvwGeofencedFeeds.Name = "lvwGeofencedFeeds";
            this.lvwGeofencedFeeds.Size = new System.Drawing.Size(806, 169);
            this.lvwGeofencedFeeds.TabIndex = 0;
            this.lvwGeofencedFeeds.UseCompatibleStateImageBehavior = false;
            this.lvwGeofencedFeeds.View = System.Windows.Forms.View.Details;
            // 
            // colFeedName
            // 
            this.colFeedName.Text = "::FeedNameHeading::";
            this.colFeedName.Width = 120;
            // 
            // colCentredOn
            // 
            this.colCentredOn.Text = "::CentredOnHeading::";
            this.colCentredOn.Width = 100;
            // 
            // colLatitude
            // 
            this.colLatitude.Text = "::LatitudeHeading::";
            this.colLatitude.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.colLatitude.Width = 80;
            // 
            // colLongitude
            // 
            this.colLongitude.Text = "::LongitudeHeading::";
            this.colLongitude.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.colLongitude.Width = 80;
            // 
            // colAirport
            // 
            this.colAirport.Text = "::AirportHeading::";
            this.colAirport.Width = 80;
            // 
            // colPilotCid
            // 
            this.colPilotCid.Text = "::PilotHeading::";
            this.colPilotCid.Width = 80;
            // 
            // colWidth
            // 
            this.colWidth.Text = "::WidthHeading::";
            this.colWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.colWidth.Width = 80;
            // 
            // colHeight
            // 
            this.colHeight.Text = "::HeightHeading::";
            this.colHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.colHeight.Width = 80;
            // 
            // colDistanceUnit
            // 
            this.colDistanceUnit.Text = "::DistanceUnitHeading::";
            this.colDistanceUnit.Width = 80;
            // 
            // nudPilotCid
            // 
            this.nudPilotCid.Location = new System.Drawing.Point(596, 339);
            this.nudPilotCid.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.nudPilotCid.Name = "nudPilotCid";
            this.nudPilotCid.Size = new System.Drawing.Size(100, 20);
            this.nudPilotCid.TabIndex = 23;
            // 
            // OptionsView
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(845, 505);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.chkInferModelFromModelType);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.nudSlowAircraftThresholdSpeedKnots);
            this.Controls.Add(this.chkAssumeSlowAircraftAreOnGround);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nudRefreshIntervalSeconds);
            this.Controls.Add(this.chkEnabled);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsView";
            this.ShowIcon = false;
            this.Text = "::OptionsDialogTitle::";
            ((System.ComponentModel.ISupportInitialize)(this.nudRefreshIntervalSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSlowAircraftThresholdSpeedKnots)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLongitude)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLatitude)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPilotCid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.NumericUpDown nudRefreshIntervalSeconds;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkAssumeSlowAircraftAreOnGround;
        private System.Windows.Forms.NumericUpDown nudSlowAircraftThresholdSpeedKnots;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkInferModelFromModelType;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView lvwGeofencedFeeds;
        private System.Windows.Forms.ColumnHeader colFeedName;
        private System.Windows.Forms.ColumnHeader colCentredOn;
        private System.Windows.Forms.ColumnHeader colLatitude;
        private System.Windows.Forms.ColumnHeader colLongitude;
        private System.Windows.Forms.ColumnHeader colAirport;
        private System.Windows.Forms.ColumnHeader colPilotCid;
        private System.Windows.Forms.TextBox txtFeedName;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbCentreOn;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown nudLongitude;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown nudLatitude;
        private System.Windows.Forms.TextBox txtAirportCode;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ComboBox cmbDistanceUnit;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.NumericUpDown nudHeight;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown nudWidth;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ColumnHeader colWidth;
        private System.Windows.Forms.ColumnHeader colHeight;
        private System.Windows.Forms.ColumnHeader colDistanceUnit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.NumericUpDown nudPilotCid;
    }
}