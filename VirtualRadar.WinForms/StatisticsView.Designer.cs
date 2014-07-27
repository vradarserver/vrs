namespace VirtualRadar.WinForms
{
    partial class StatisticsView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StatisticsView));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelThroughput = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.labelBytesReceived = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labelDuration = new System.Windows.Forms.Label();
            this.labelBadChecksum = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.labelBaseStationBadlyFormatted = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.labelBaseStationMessages = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.labelPIPresent = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.labelNoAdsbPayload = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.listViewModeSDFCounts = new System.Windows.Forms.ListView();
            this.columnHeaderDF = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.labelPIBadParity = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.labelShortFrameUnusable = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.labelLongFrame = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.labelShortFrame = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.labelModeSMessages = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.labelAdsbRejected = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.splitContainerAdsbMessageCounts = new System.Windows.Forms.SplitContainer();
            this.listViewAdsbTypeCounts = new System.Windows.Forms.ListView();
            this.columnHeaderType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderTypeCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listViewAdsbMessageFormatCounts = new System.Windows.Forms.ListView();
            this.columnHeaderFormat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFormatCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.labelPositionsOutOfRange = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.labelPositionResets = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.labelSpeedChecksExceeded = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.labelAdsbMessages = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonResetCounters = new System.Windows.Forms.Button();
            this.splitContainerEverythingVsAdsb = new System.Windows.Forms.SplitContainer();
            this.label5 = new System.Windows.Forms.Label();
            this.labelCurrentBufferSize = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.splitContainerAdsbMessageCounts.Panel1.SuspendLayout();
            this.splitContainerAdsbMessageCounts.Panel2.SuspendLayout();
            this.splitContainerAdsbMessageCounts.SuspendLayout();
            this.splitContainerEverythingVsAdsb.Panel1.SuspendLayout();
            this.splitContainerEverythingVsAdsb.Panel2.SuspendLayout();
            this.splitContainerEverythingVsAdsb.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.labelCurrentBufferSize);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.labelThroughput);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.labelBytesReceived);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.labelDuration);
            this.groupBox1.Controls.Add(this.labelBadChecksum);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(683, 78);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "::Connection::";
            // 
            // labelThroughput
            // 
            this.labelThroughput.AutoSize = true;
            this.labelThroughput.Location = new System.Drawing.Point(522, 39);
            this.labelThroughput.Margin = new System.Windows.Forms.Padding(3);
            this.labelThroughput.Name = "labelThroughput";
            this.labelThroughput.Size = new System.Drawing.Size(84, 13);
            this.labelThroughput.TabIndex = 5;
            this.labelThroughput.Text = "labelThroughput";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(339, 39);
            this.label3.Margin = new System.Windows.Forms.Padding(3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "::Throughput:::";
            // 
            // labelBytesReceived
            // 
            this.labelBytesReceived.AutoSize = true;
            this.labelBytesReceived.Location = new System.Drawing.Point(186, 39);
            this.labelBytesReceived.Margin = new System.Windows.Forms.Padding(3);
            this.labelBytesReceived.Name = "labelBytesReceived";
            this.labelBytesReceived.Size = new System.Drawing.Size(101, 13);
            this.labelBytesReceived.TabIndex = 3;
            this.labelBytesReceived.Text = "labelBytesReceived";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 39);
            this.label2.Margin = new System.Windows.Forms.Padding(3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "::BytesReceived:::";
            // 
            // labelDuration
            // 
            this.labelDuration.AutoSize = true;
            this.labelDuration.Location = new System.Drawing.Point(186, 20);
            this.labelDuration.Margin = new System.Windows.Forms.Padding(3);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(69, 13);
            this.labelDuration.TabIndex = 1;
            this.labelDuration.Text = "labelDuration";
            // 
            // labelBadChecksum
            // 
            this.labelBadChecksum.AutoSize = true;
            this.labelBadChecksum.Location = new System.Drawing.Point(522, 19);
            this.labelBadChecksum.Margin = new System.Windows.Forms.Padding(3);
            this.labelBadChecksum.Name = "labelBadChecksum";
            this.labelBadChecksum.Size = new System.Drawing.Size(98, 13);
            this.labelBadChecksum.TabIndex = 13;
            this.labelBadChecksum.Text = "labelBadChecksum";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "::Duration:::";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(339, 19);
            this.label12.Margin = new System.Windows.Forms.Padding(3);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(91, 13);
            this.label12.TabIndex = 12;
            this.label12.Text = "::BadChecksum:::";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.labelBaseStationBadlyFormatted);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.labelBaseStationMessages);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(0, 84);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(683, 47);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "::BaseStation::";
            // 
            // labelBaseStationBadlyFormatted
            // 
            this.labelBaseStationBadlyFormatted.AutoSize = true;
            this.labelBaseStationBadlyFormatted.Location = new System.Drawing.Point(522, 20);
            this.labelBaseStationBadlyFormatted.Margin = new System.Windows.Forms.Padding(3);
            this.labelBaseStationBadlyFormatted.Name = "labelBaseStationBadlyFormatted";
            this.labelBaseStationBadlyFormatted.Size = new System.Drawing.Size(159, 13);
            this.labelBaseStationBadlyFormatted.TabIndex = 3;
            this.labelBaseStationBadlyFormatted.Text = "labelBaseStationBadlyFormatted";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(339, 20);
            this.label6.Margin = new System.Windows.Forms.Padding(3);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(95, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "::BadlyFormatted:::";
            // 
            // labelBaseStationMessages
            // 
            this.labelBaseStationMessages.AutoSize = true;
            this.labelBaseStationMessages.Location = new System.Drawing.Point(186, 20);
            this.labelBaseStationMessages.Margin = new System.Windows.Forms.Padding(3);
            this.labelBaseStationMessages.Name = "labelBaseStationMessages";
            this.labelBaseStationMessages.Size = new System.Drawing.Size(134, 13);
            this.labelBaseStationMessages.TabIndex = 1;
            this.labelBaseStationMessages.Text = "labelBaseStationMessages";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 20);
            this.label4.Margin = new System.Windows.Forms.Padding(3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(116, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "::MessagesReceived:::";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.labelPIPresent);
            this.groupBox3.Controls.Add(this.label15);
            this.groupBox3.Controls.Add(this.labelNoAdsbPayload);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.listViewModeSDFCounts);
            this.groupBox3.Controls.Add(this.labelPIBadParity);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.labelShortFrameUnusable);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.labelLongFrame);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.labelShortFrame);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.labelModeSMessages);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Location = new System.Drawing.Point(0, 137);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(683, 152);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "::ModeS::";
            // 
            // labelPIPresent
            // 
            this.labelPIPresent.AutoSize = true;
            this.labelPIPresent.Location = new System.Drawing.Point(186, 114);
            this.labelPIPresent.Margin = new System.Windows.Forms.Padding(3);
            this.labelPIPresent.Name = "labelPIPresent";
            this.labelPIPresent.Size = new System.Drawing.Size(75, 13);
            this.labelPIPresent.TabIndex = 18;
            this.labelPIPresent.Text = "labelPIPresent";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(7, 114);
            this.label15.Margin = new System.Windows.Forms.Padding(3);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(68, 13);
            this.label15.TabIndex = 17;
            this.label15.Text = "::PIPresent:::";
            // 
            // labelNoAdsbPayload
            // 
            this.labelNoAdsbPayload.AutoSize = true;
            this.labelNoAdsbPayload.Location = new System.Drawing.Point(186, 38);
            this.labelNoAdsbPayload.Margin = new System.Windows.Forms.Padding(3);
            this.labelNoAdsbPayload.Name = "labelNoAdsbPayload";
            this.labelNoAdsbPayload.Size = new System.Drawing.Size(105, 13);
            this.labelNoAdsbPayload.TabIndex = 16;
            this.labelNoAdsbPayload.Text = "labelNoAdsbPayload";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(7, 38);
            this.label13.Margin = new System.Windows.Forms.Padding(3);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(98, 13);
            this.label13.TabIndex = 15;
            this.label13.Text = "::NoAdsbPayload:::";
            // 
            // listViewModeSDFCounts
            // 
            this.listViewModeSDFCounts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewModeSDFCounts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderDF,
            this.columnHeaderCount});
            this.listViewModeSDFCounts.FullRowSelect = true;
            this.listViewModeSDFCounts.GridLines = true;
            this.listViewModeSDFCounts.HideSelection = false;
            this.listViewModeSDFCounts.Location = new System.Drawing.Point(338, 19);
            this.listViewModeSDFCounts.Name = "listViewModeSDFCounts";
            this.listViewModeSDFCounts.Size = new System.Drawing.Size(339, 127);
            this.listViewModeSDFCounts.TabIndex = 0;
            this.listViewModeSDFCounts.UseCompatibleStateImageBehavior = false;
            this.listViewModeSDFCounts.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderDF
            // 
            this.columnHeaderDF.Text = "::DF::";
            this.columnHeaderDF.Width = 169;
            // 
            // columnHeaderCount
            // 
            this.columnHeaderCount.Text = "::Count::";
            this.columnHeaderCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderCount.Width = 130;
            // 
            // labelPIBadParity
            // 
            this.labelPIBadParity.AutoSize = true;
            this.labelPIBadParity.Location = new System.Drawing.Point(186, 133);
            this.labelPIBadParity.Margin = new System.Windows.Forms.Padding(3);
            this.labelPIBadParity.Name = "labelPIBadParity";
            this.labelPIBadParity.Size = new System.Drawing.Size(84, 13);
            this.labelPIBadParity.TabIndex = 11;
            this.labelPIBadParity.Text = "labelPIBadParity";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(7, 133);
            this.label11.Margin = new System.Windows.Forms.Padding(3);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(77, 13);
            this.label11.TabIndex = 10;
            this.label11.Text = "::PIBadParity:::";
            // 
            // labelShortFrameUnusable
            // 
            this.labelShortFrameUnusable.AutoSize = true;
            this.labelShortFrameUnusable.Location = new System.Drawing.Point(186, 76);
            this.labelShortFrameUnusable.Margin = new System.Windows.Forms.Padding(3);
            this.labelShortFrameUnusable.Name = "labelShortFrameUnusable";
            this.labelShortFrameUnusable.Size = new System.Drawing.Size(128, 13);
            this.labelShortFrameUnusable.TabIndex = 9;
            this.labelShortFrameUnusable.Text = "labelShortFrameUnusable";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 76);
            this.label9.Margin = new System.Windows.Forms.Padding(3);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(121, 13);
            this.label9.TabIndex = 8;
            this.label9.Text = "::ShortFrameUnusable:::";
            // 
            // labelLongFrame
            // 
            this.labelLongFrame.AutoSize = true;
            this.labelLongFrame.Location = new System.Drawing.Point(186, 95);
            this.labelLongFrame.Margin = new System.Windows.Forms.Padding(3);
            this.labelLongFrame.Name = "labelLongFrame";
            this.labelLongFrame.Size = new System.Drawing.Size(82, 13);
            this.labelLongFrame.TabIndex = 7;
            this.labelLongFrame.Text = "labelLongFrame";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 95);
            this.label10.Margin = new System.Windows.Forms.Padding(3);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(75, 13);
            this.label10.TabIndex = 6;
            this.label10.Text = "::LongFrame:::";
            // 
            // labelShortFrame
            // 
            this.labelShortFrame.AutoSize = true;
            this.labelShortFrame.Location = new System.Drawing.Point(186, 57);
            this.labelShortFrame.Margin = new System.Windows.Forms.Padding(3);
            this.labelShortFrame.Name = "labelShortFrame";
            this.labelShortFrame.Size = new System.Drawing.Size(83, 13);
            this.labelShortFrame.TabIndex = 5;
            this.labelShortFrame.Text = "labelShortFrame";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 57);
            this.label8.Margin = new System.Windows.Forms.Padding(3);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(76, 13);
            this.label8.TabIndex = 4;
            this.label8.Text = "::ShortFrame:::";
            // 
            // labelModeSMessages
            // 
            this.labelModeSMessages.AutoSize = true;
            this.labelModeSMessages.Location = new System.Drawing.Point(186, 19);
            this.labelModeSMessages.Margin = new System.Windows.Forms.Padding(3);
            this.labelModeSMessages.Name = "labelModeSMessages";
            this.labelModeSMessages.Size = new System.Drawing.Size(111, 13);
            this.labelModeSMessages.TabIndex = 3;
            this.labelModeSMessages.Text = "labelModeSMessages";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 19);
            this.label7.Margin = new System.Windows.Forms.Padding(3);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(116, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "::MessagesReceived:::";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.labelAdsbRejected);
            this.groupBox4.Controls.Add(this.label18);
            this.groupBox4.Controls.Add(this.splitContainerAdsbMessageCounts);
            this.groupBox4.Controls.Add(this.labelPositionsOutOfRange);
            this.groupBox4.Controls.Add(this.label17);
            this.groupBox4.Controls.Add(this.labelPositionResets);
            this.groupBox4.Controls.Add(this.label19);
            this.groupBox4.Controls.Add(this.labelSpeedChecksExceeded);
            this.groupBox4.Controls.Add(this.label16);
            this.groupBox4.Controls.Add(this.labelAdsbMessages);
            this.groupBox4.Controls.Add(this.label14);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(0, 0);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(683, 246);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "::ADSB::";
            // 
            // labelAdsbRejected
            // 
            this.labelAdsbRejected.AutoSize = true;
            this.labelAdsbRejected.Location = new System.Drawing.Point(186, 38);
            this.labelAdsbRejected.Margin = new System.Windows.Forms.Padding(3);
            this.labelAdsbRejected.Name = "labelAdsbRejected";
            this.labelAdsbRejected.Size = new System.Drawing.Size(96, 13);
            this.labelAdsbRejected.TabIndex = 14;
            this.labelAdsbRejected.Text = "labelAdsbRejected";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(7, 38);
            this.label18.Margin = new System.Windows.Forms.Padding(3);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(89, 13);
            this.label18.TabIndex = 13;
            this.label18.Text = "::AdsbRejected:::";
            // 
            // splitContainerAdsbMessageCounts
            // 
            this.splitContainerAdsbMessageCounts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerAdsbMessageCounts.Location = new System.Drawing.Point(6, 76);
            this.splitContainerAdsbMessageCounts.Name = "splitContainerAdsbMessageCounts";
            // 
            // splitContainerAdsbMessageCounts.Panel1
            // 
            this.splitContainerAdsbMessageCounts.Panel1.Controls.Add(this.listViewAdsbTypeCounts);
            // 
            // splitContainerAdsbMessageCounts.Panel2
            // 
            this.splitContainerAdsbMessageCounts.Panel2.Controls.Add(this.listViewAdsbMessageFormatCounts);
            this.splitContainerAdsbMessageCounts.Size = new System.Drawing.Size(671, 164);
            this.splitContainerAdsbMessageCounts.SplitterDistance = 328;
            this.splitContainerAdsbMessageCounts.TabIndex = 12;
            // 
            // listViewAdsbTypeCounts
            // 
            this.listViewAdsbTypeCounts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderType,
            this.columnHeaderTypeCount});
            this.listViewAdsbTypeCounts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewAdsbTypeCounts.FullRowSelect = true;
            this.listViewAdsbTypeCounts.GridLines = true;
            this.listViewAdsbTypeCounts.HideSelection = false;
            this.listViewAdsbTypeCounts.Location = new System.Drawing.Point(0, 0);
            this.listViewAdsbTypeCounts.Name = "listViewAdsbTypeCounts";
            this.listViewAdsbTypeCounts.Size = new System.Drawing.Size(328, 164);
            this.listViewAdsbTypeCounts.TabIndex = 0;
            this.listViewAdsbTypeCounts.UseCompatibleStateImageBehavior = false;
            this.listViewAdsbTypeCounts.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Text = "::Type::";
            this.columnHeaderType.Width = 167;
            // 
            // columnHeaderTypeCount
            // 
            this.columnHeaderTypeCount.Text = "::Count::";
            this.columnHeaderTypeCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderTypeCount.Width = 130;
            // 
            // listViewAdsbMessageFormatCounts
            // 
            this.listViewAdsbMessageFormatCounts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFormat,
            this.columnHeaderFormatCount});
            this.listViewAdsbMessageFormatCounts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewAdsbMessageFormatCounts.FullRowSelect = true;
            this.listViewAdsbMessageFormatCounts.GridLines = true;
            this.listViewAdsbMessageFormatCounts.HideSelection = false;
            this.listViewAdsbMessageFormatCounts.Location = new System.Drawing.Point(0, 0);
            this.listViewAdsbMessageFormatCounts.Name = "listViewAdsbMessageFormatCounts";
            this.listViewAdsbMessageFormatCounts.Size = new System.Drawing.Size(339, 164);
            this.listViewAdsbMessageFormatCounts.TabIndex = 0;
            this.listViewAdsbMessageFormatCounts.UseCompatibleStateImageBehavior = false;
            this.listViewAdsbMessageFormatCounts.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderFormat
            // 
            this.columnHeaderFormat.Text = "::Format::";
            this.columnHeaderFormat.Width = 161;
            // 
            // columnHeaderFormatCount
            // 
            this.columnHeaderFormatCount.Text = "::Count::";
            this.columnHeaderFormatCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderFormatCount.Width = 130;
            // 
            // labelPositionsOutOfRange
            // 
            this.labelPositionsOutOfRange.AutoSize = true;
            this.labelPositionsOutOfRange.Location = new System.Drawing.Point(522, 57);
            this.labelPositionsOutOfRange.Margin = new System.Windows.Forms.Padding(3);
            this.labelPositionsOutOfRange.Name = "labelPositionsOutOfRange";
            this.labelPositionsOutOfRange.Size = new System.Drawing.Size(131, 13);
            this.labelPositionsOutOfRange.TabIndex = 11;
            this.labelPositionsOutOfRange.Text = "labelPositionsOutOfRange";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(339, 57);
            this.label17.Margin = new System.Windows.Forms.Padding(3);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(124, 13);
            this.label17.TabIndex = 10;
            this.label17.Text = "::PositionsOutOfRange:::";
            // 
            // labelPositionResets
            // 
            this.labelPositionResets.AutoSize = true;
            this.labelPositionResets.Location = new System.Drawing.Point(522, 38);
            this.labelPositionResets.Margin = new System.Windows.Forms.Padding(3);
            this.labelPositionResets.Name = "labelPositionResets";
            this.labelPositionResets.Size = new System.Drawing.Size(99, 13);
            this.labelPositionResets.TabIndex = 9;
            this.labelPositionResets.Text = "labelPositionResets";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(339, 38);
            this.label19.Margin = new System.Windows.Forms.Padding(3);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(92, 13);
            this.label19.TabIndex = 8;
            this.label19.Text = "::PositionResets:::";
            // 
            // labelSpeedChecksExceeded
            // 
            this.labelSpeedChecksExceeded.AutoSize = true;
            this.labelSpeedChecksExceeded.Location = new System.Drawing.Point(522, 19);
            this.labelSpeedChecksExceeded.Margin = new System.Windows.Forms.Padding(3);
            this.labelSpeedChecksExceeded.Name = "labelSpeedChecksExceeded";
            this.labelSpeedChecksExceeded.Size = new System.Drawing.Size(144, 13);
            this.labelSpeedChecksExceeded.TabIndex = 7;
            this.labelSpeedChecksExceeded.Text = "labelSpeedChecksExceeded";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(339, 19);
            this.label16.Margin = new System.Windows.Forms.Padding(3);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(137, 13);
            this.label16.TabIndex = 6;
            this.label16.Text = "::SpeedChecksExceeded:::";
            // 
            // labelAdsbMessages
            // 
            this.labelAdsbMessages.AutoSize = true;
            this.labelAdsbMessages.Location = new System.Drawing.Point(186, 19);
            this.labelAdsbMessages.Margin = new System.Windows.Forms.Padding(3);
            this.labelAdsbMessages.Name = "labelAdsbMessages";
            this.labelAdsbMessages.Size = new System.Drawing.Size(101, 13);
            this.labelAdsbMessages.TabIndex = 5;
            this.labelAdsbMessages.Text = "labelAdsbMessages";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(7, 19);
            this.label14.Margin = new System.Windows.Forms.Padding(3);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(116, 13);
            this.label14.TabIndex = 4;
            this.label14.Text = "::MessagesReceived:::";
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(622, 568);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 5;
            this.buttonClose.Text = "::Close::";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonResetCounters
            // 
            this.buttonResetCounters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonResetCounters.Location = new System.Drawing.Point(12, 568);
            this.buttonResetCounters.Name = "buttonResetCounters";
            this.buttonResetCounters.Size = new System.Drawing.Size(153, 23);
            this.buttonResetCounters.TabIndex = 4;
            this.buttonResetCounters.Text = "::ResetCounters::";
            this.buttonResetCounters.UseVisualStyleBackColor = true;
            this.buttonResetCounters.Click += new System.EventHandler(this.buttonResetCounters_Click);
            // 
            // splitContainerEverythingVsAdsb
            // 
            this.splitContainerEverythingVsAdsb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerEverythingVsAdsb.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerEverythingVsAdsb.Location = new System.Drawing.Point(12, 12);
            this.splitContainerEverythingVsAdsb.Name = "splitContainerEverythingVsAdsb";
            this.splitContainerEverythingVsAdsb.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerEverythingVsAdsb.Panel1
            // 
            this.splitContainerEverythingVsAdsb.Panel1.Controls.Add(this.groupBox1);
            this.splitContainerEverythingVsAdsb.Panel1.Controls.Add(this.groupBox2);
            this.splitContainerEverythingVsAdsb.Panel1.Controls.Add(this.groupBox3);
            // 
            // splitContainerEverythingVsAdsb.Panel2
            // 
            this.splitContainerEverythingVsAdsb.Panel2.Controls.Add(this.groupBox4);
            this.splitContainerEverythingVsAdsb.Size = new System.Drawing.Size(683, 550);
            this.splitContainerEverythingVsAdsb.SplitterDistance = 300;
            this.splitContainerEverythingVsAdsb.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 58);
            this.label5.Margin = new System.Windows.Forms.Padding(3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "::ReadBufferSize:::";
            // 
            // labelCurrentBufferSize
            // 
            this.labelCurrentBufferSize.AutoSize = true;
            this.labelCurrentBufferSize.Location = new System.Drawing.Point(186, 58);
            this.labelCurrentBufferSize.Margin = new System.Windows.Forms.Padding(3);
            this.labelCurrentBufferSize.Name = "labelCurrentBufferSize";
            this.labelCurrentBufferSize.Size = new System.Drawing.Size(111, 13);
            this.labelCurrentBufferSize.TabIndex = 15;
            this.labelCurrentBufferSize.Text = "labelCurrentBufferSize";
            // 
            // StatisticsView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(709, 603);
            this.Controls.Add(this.splitContainerEverythingVsAdsb);
            this.Controls.Add(this.buttonResetCounters);
            this.Controls.Add(this.buttonClose);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "StatisticsView";
            this.Text = "::StatisticsTitle::";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.splitContainerAdsbMessageCounts.Panel1.ResumeLayout(false);
            this.splitContainerAdsbMessageCounts.Panel2.ResumeLayout(false);
            this.splitContainerAdsbMessageCounts.ResumeLayout(false);
            this.splitContainerEverythingVsAdsb.Panel1.ResumeLayout(false);
            this.splitContainerEverythingVsAdsb.Panel2.ResumeLayout(false);
            this.splitContainerEverythingVsAdsb.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelDuration;
        private System.Windows.Forms.Label labelBytesReceived;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelThroughput;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelBaseStationMessages;
        private System.Windows.Forms.Label labelBaseStationBadlyFormatted;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label labelShortFrame;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label labelModeSMessages;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label labelLongFrame;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label labelShortFrameUnusable;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label labelPIBadParity;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label labelBadChecksum;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ListView listViewModeSDFCounts;
        private System.Windows.Forms.ColumnHeader columnHeaderDF;
        private System.Windows.Forms.ColumnHeader columnHeaderCount;
        private System.Windows.Forms.Label labelNoAdsbPayload;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label labelAdsbMessages;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label labelSpeedChecksExceeded;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label labelPositionsOutOfRange;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label labelPositionResets;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.SplitContainer splitContainerAdsbMessageCounts;
        private System.Windows.Forms.ListView listViewAdsbTypeCounts;
        private System.Windows.Forms.ListView listViewAdsbMessageFormatCounts;
        private System.Windows.Forms.ColumnHeader columnHeaderType;
        private System.Windows.Forms.ColumnHeader columnHeaderTypeCount;
        private System.Windows.Forms.ColumnHeader columnHeaderFormat;
        private System.Windows.Forms.ColumnHeader columnHeaderFormatCount;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonResetCounters;
        private System.Windows.Forms.Label labelPIPresent;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label labelAdsbRejected;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.SplitContainer splitContainerEverythingVsAdsb;
        private System.Windows.Forms.Label labelCurrentBufferSize;
        private System.Windows.Forms.Label label5;
    }
}