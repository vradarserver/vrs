namespace VirtualRadar.WinForms.SettingPage
{
    partial class PageReceiver
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PageReceiver));
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.buttonTestConnection = new System.Windows.Forms.Button();
            this.checkBoxEnabled = new System.Windows.Forms.CheckBox();
            this.textBoxSerialShutdownText = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxAddress = new System.Windows.Forms.TextBox();
            this.textBoxSerialStartupText = new System.Windows.Forms.TextBox();
            this.numericPort = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.comboBoxSerialHandshake = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxConnectionType = new System.Windows.Forms.ComboBox();
            this.comboBoxSerialParity = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxSerialStopBits = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.comboBoxSerialDataBits = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.buttonClearLocationId = new System.Windows.Forms.Button();
            this.comboBoxLocationId = new System.Windows.Forms.ComboBox();
            this.comboBoxSerialBaudRate = new System.Windows.Forms.ComboBox();
            this.groupBoxSerial = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBoxSerialComPort = new System.Windows.Forms.ComboBox();
            this.groupBoxNetwork = new System.Windows.Forms.GroupBox();
            this.buttonWizard = new System.Windows.Forms.Button();
            this.checkBoxAutoReconnectAtStartup = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxDataSource = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericPort)).BeginInit();
            this.groupBoxSerial.SuspendLayout();
            this.groupBoxNetwork.SuspendLayout();
            this.SuspendLayout();
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 129);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(117, 13);
            this.label14.TabIndex = 14;
            this.label14.Text = "::SerialShutdownText:::";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 103);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(103, 13);
            this.label13.TabIndex = 12;
            this.label13.Text = "::SerialStartupText:::";
            // 
            // buttonTestConnection
            // 
            this.buttonTestConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTestConnection.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonTestConnection.Location = new System.Drawing.Point(480, 107);
            this.buttonTestConnection.Name = "buttonTestConnection";
            this.buttonTestConnection.Size = new System.Drawing.Size(156, 23);
            this.buttonTestConnection.TabIndex = 27;
            this.buttonTestConnection.Text = "::TestConnection::";
            this.buttonTestConnection.UseVisualStyleBackColor = true;
            this.buttonTestConnection.Click += new System.EventHandler(this.buttonTestConnection_Click);
            // 
            // checkBoxEnabled
            // 
            this.checkBoxEnabled.AutoSize = true;
            this.checkBoxEnabled.Location = new System.Drawing.Point(155, 4);
            this.checkBoxEnabled.Name = "checkBoxEnabled";
            this.checkBoxEnabled.Size = new System.Drawing.Size(77, 17);
            this.checkBoxEnabled.TabIndex = 15;
            this.checkBoxEnabled.Text = "::Enabled::";
            this.checkBoxEnabled.UseVisualStyleBackColor = true;
            // 
            // textBoxSerialShutdownText
            // 
            this.textBoxSerialShutdownText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSerialShutdownText.Location = new System.Drawing.Point(153, 126);
            this.textBoxSerialShutdownText.Name = "textBoxSerialShutdownText";
            this.textBoxSerialShutdownText.Size = new System.Drawing.Size(477, 20);
            this.textBoxSerialShutdownText.TabIndex = 15;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 86);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "::Location:::";
            // 
            // textBoxAddress
            // 
            this.textBoxAddress.Location = new System.Drawing.Point(151, 20);
            this.textBoxAddress.Name = "textBoxAddress";
            this.textBoxAddress.Size = new System.Drawing.Size(150, 20);
            this.textBoxAddress.TabIndex = 1;
            // 
            // textBoxSerialStartupText
            // 
            this.textBoxSerialStartupText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSerialStartupText.Location = new System.Drawing.Point(153, 100);
            this.textBoxSerialStartupText.Name = "textBoxSerialStartupText";
            this.textBoxSerialStartupText.Size = new System.Drawing.Size(477, 20);
            this.textBoxSerialStartupText.TabIndex = 13;
            // 
            // numericPort
            // 
            this.numericPort.Location = new System.Drawing.Point(151, 47);
            this.numericPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericPort.Name = "numericPort";
            this.numericPort.Size = new System.Drawing.Size(77, 20);
            this.numericPort.TabIndex = 3;
            this.numericPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 23);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "::UNC:::";
            // 
            // label12
            // 
            this.label12.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(333, 76);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(103, 13);
            this.label12.TabIndex = 10;
            this.label12.Text = "::SerialHandshake:::";
            // 
            // comboBoxSerialHandshake
            // 
            this.comboBoxSerialHandshake.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxSerialHandshake.DisplayMember = "Name";
            this.comboBoxSerialHandshake.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSerialHandshake.FormattingEnabled = true;
            this.comboBoxSerialHandshake.Location = new System.Drawing.Point(480, 73);
            this.comboBoxSerialHandshake.Name = "comboBoxSerialHandshake";
            this.comboBoxSerialHandshake.Size = new System.Drawing.Size(150, 21);
            this.comboBoxSerialHandshake.TabIndex = 11;
            this.comboBoxSerialHandshake.ValueMember = "Value";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 76);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(74, 13);
            this.label11.TabIndex = 8;
            this.label11.Text = "::SerialParity:::";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "::Name:::";
            // 
            // textBoxName
            // 
            this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxName.Location = new System.Drawing.Point(155, 29);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(481, 20);
            this.textBoxName.TabIndex = 18;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 112);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 13);
            this.label1.TabIndex = 25;
            this.label1.Text = "::ConnectionType:::";
            // 
            // comboBoxConnectionType
            // 
            this.comboBoxConnectionType.DisplayMember = "Name";
            this.comboBoxConnectionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxConnectionType.FormattingEnabled = true;
            this.comboBoxConnectionType.Location = new System.Drawing.Point(155, 109);
            this.comboBoxConnectionType.Name = "comboBoxConnectionType";
            this.comboBoxConnectionType.Size = new System.Drawing.Size(150, 21);
            this.comboBoxConnectionType.TabIndex = 26;
            this.comboBoxConnectionType.ValueMember = "Value";
            // 
            // comboBoxSerialParity
            // 
            this.comboBoxSerialParity.DisplayMember = "Name";
            this.comboBoxSerialParity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSerialParity.FormattingEnabled = true;
            this.comboBoxSerialParity.Location = new System.Drawing.Point(153, 73);
            this.comboBoxSerialParity.Name = "comboBoxSerialParity";
            this.comboBoxSerialParity.Size = new System.Drawing.Size(150, 21);
            this.comboBoxSerialParity.TabIndex = 9;
            this.comboBoxSerialParity.ValueMember = "Value";
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(333, 49);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(87, 13);
            this.label10.TabIndex = 6;
            this.label10.Text = "::SerialStopBits:::";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 49);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "::Port:::";
            // 
            // comboBoxSerialStopBits
            // 
            this.comboBoxSerialStopBits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxSerialStopBits.DisplayMember = "Name";
            this.comboBoxSerialStopBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSerialStopBits.FormattingEnabled = true;
            this.comboBoxSerialStopBits.Location = new System.Drawing.Point(480, 46);
            this.comboBoxSerialStopBits.Name = "comboBoxSerialStopBits";
            this.comboBoxSerialStopBits.Size = new System.Drawing.Size(150, 21);
            this.comboBoxSerialStopBits.TabIndex = 7;
            this.comboBoxSerialStopBits.ValueMember = "Value";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 49);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(88, 13);
            this.label9.TabIndex = 4;
            this.label9.Text = "::SerialDataBits:::";
            // 
            // comboBoxSerialDataBits
            // 
            this.comboBoxSerialDataBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSerialDataBits.FormattingEnabled = true;
            this.comboBoxSerialDataBits.Location = new System.Drawing.Point(153, 46);
            this.comboBoxSerialDataBits.Name = "comboBoxSerialDataBits";
            this.comboBoxSerialDataBits.Size = new System.Drawing.Size(150, 21);
            this.comboBoxSerialDataBits.TabIndex = 5;
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(333, 22);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(96, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "::SerialBaudRate:::";
            // 
            // buttonClearLocationId
            // 
            this.buttonClearLocationId.FlatAppearance.BorderSize = 0;
            this.buttonClearLocationId.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonClearLocationId.Image = ((System.Drawing.Image)(resources.GetObject("buttonClearLocationId.Image")));
            this.buttonClearLocationId.Location = new System.Drawing.Point(308, 83);
            this.buttonClearLocationId.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.buttonClearLocationId.Name = "buttonClearLocationId";
            this.buttonClearLocationId.Size = new System.Drawing.Size(21, 21);
            this.buttonClearLocationId.TabIndex = 24;
            this.buttonClearLocationId.UseVisualStyleBackColor = true;
            this.buttonClearLocationId.Click += new System.EventHandler(this.buttonClearLocationId_Click);
            // 
            // comboBoxLocationId
            // 
            this.comboBoxLocationId.DisplayMember = "Name";
            this.comboBoxLocationId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLocationId.FormattingEnabled = true;
            this.comboBoxLocationId.Location = new System.Drawing.Point(155, 83);
            this.comboBoxLocationId.Name = "comboBoxLocationId";
            this.comboBoxLocationId.Size = new System.Drawing.Size(150, 21);
            this.comboBoxLocationId.TabIndex = 23;
            this.comboBoxLocationId.ValueMember = "UniqueId";
            // 
            // comboBoxSerialBaudRate
            // 
            this.comboBoxSerialBaudRate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxSerialBaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSerialBaudRate.FormattingEnabled = true;
            this.comboBoxSerialBaudRate.Location = new System.Drawing.Point(480, 19);
            this.comboBoxSerialBaudRate.Name = "comboBoxSerialBaudRate";
            this.comboBoxSerialBaudRate.Size = new System.Drawing.Size(150, 21);
            this.comboBoxSerialBaudRate.TabIndex = 3;
            // 
            // groupBoxSerial
            // 
            this.groupBoxSerial.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSerial.Controls.Add(this.label14);
            this.groupBoxSerial.Controls.Add(this.textBoxSerialShutdownText);
            this.groupBoxSerial.Controls.Add(this.label13);
            this.groupBoxSerial.Controls.Add(this.textBoxSerialStartupText);
            this.groupBoxSerial.Controls.Add(this.label12);
            this.groupBoxSerial.Controls.Add(this.comboBoxSerialHandshake);
            this.groupBoxSerial.Controls.Add(this.label11);
            this.groupBoxSerial.Controls.Add(this.comboBoxSerialParity);
            this.groupBoxSerial.Controls.Add(this.label10);
            this.groupBoxSerial.Controls.Add(this.comboBoxSerialStopBits);
            this.groupBoxSerial.Controls.Add(this.label9);
            this.groupBoxSerial.Controls.Add(this.comboBoxSerialDataBits);
            this.groupBoxSerial.Controls.Add(this.label8);
            this.groupBoxSerial.Controls.Add(this.comboBoxSerialBaudRate);
            this.groupBoxSerial.Controls.Add(this.label7);
            this.groupBoxSerial.Controls.Add(this.comboBoxSerialComPort);
            this.groupBoxSerial.Location = new System.Drawing.Point(0, 222);
            this.groupBoxSerial.Name = "groupBoxSerial";
            this.groupBoxSerial.Size = new System.Drawing.Size(636, 157);
            this.groupBoxSerial.TabIndex = 29;
            this.groupBoxSerial.TabStop = false;
            this.groupBoxSerial.Text = "::USBOverCOM::";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 22);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(88, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "::SerialComPort:::";
            // 
            // comboBoxSerialComPort
            // 
            this.comboBoxSerialComPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSerialComPort.FormattingEnabled = true;
            this.comboBoxSerialComPort.Location = new System.Drawing.Point(153, 19);
            this.comboBoxSerialComPort.Name = "comboBoxSerialComPort";
            this.comboBoxSerialComPort.Size = new System.Drawing.Size(150, 21);
            this.comboBoxSerialComPort.TabIndex = 1;
            // 
            // groupBoxNetwork
            // 
            this.groupBoxNetwork.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxNetwork.Controls.Add(this.label6);
            this.groupBoxNetwork.Controls.Add(this.numericPort);
            this.groupBoxNetwork.Controls.Add(this.label5);
            this.groupBoxNetwork.Controls.Add(this.textBoxAddress);
            this.groupBoxNetwork.Location = new System.Drawing.Point(0, 136);
            this.groupBoxNetwork.Name = "groupBoxNetwork";
            this.groupBoxNetwork.Size = new System.Drawing.Size(636, 80);
            this.groupBoxNetwork.TabIndex = 28;
            this.groupBoxNetwork.TabStop = false;
            this.groupBoxNetwork.Text = "::Network::";
            // 
            // buttonWizard
            // 
            this.buttonWizard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonWizard.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonWizard.Location = new System.Drawing.Point(480, 0);
            this.buttonWizard.Name = "buttonWizard";
            this.buttonWizard.Size = new System.Drawing.Size(156, 23);
            this.buttonWizard.TabIndex = 16;
            this.buttonWizard.Text = "::Wizard::";
            this.buttonWizard.UseVisualStyleBackColor = true;
            this.buttonWizard.Click += new System.EventHandler(this.buttonWizard_Click);
            // 
            // checkBoxAutoReconnectAtStartup
            // 
            this.checkBoxAutoReconnectAtStartup.AutoSize = true;
            this.checkBoxAutoReconnectAtStartup.Location = new System.Drawing.Point(336, 57);
            this.checkBoxAutoReconnectAtStartup.Name = "checkBoxAutoReconnectAtStartup";
            this.checkBoxAutoReconnectAtStartup.Size = new System.Drawing.Size(157, 17);
            this.checkBoxAutoReconnectAtStartup.TabIndex = 21;
            this.checkBoxAutoReconnectAtStartup.Text = "::AutoReconnectAtStartup::";
            this.checkBoxAutoReconnectAtStartup.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(0, 58);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "::Format:::";
            // 
            // comboBoxDataSource
            // 
            this.comboBoxDataSource.DisplayMember = "Name";
            this.comboBoxDataSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDataSource.FormattingEnabled = true;
            this.comboBoxDataSource.Location = new System.Drawing.Point(155, 55);
            this.comboBoxDataSource.Name = "comboBoxDataSource";
            this.comboBoxDataSource.Size = new System.Drawing.Size(150, 21);
            this.comboBoxDataSource.TabIndex = 20;
            this.comboBoxDataSource.ValueMember = "Value";
            // 
            // PageReceiver
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.buttonTestConnection);
            this.Controls.Add(this.checkBoxEnabled);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxConnectionType);
            this.Controls.Add(this.buttonClearLocationId);
            this.Controls.Add(this.comboBoxLocationId);
            this.Controls.Add(this.groupBoxSerial);
            this.Controls.Add(this.groupBoxNetwork);
            this.Controls.Add(this.buttonWizard);
            this.Controls.Add(this.checkBoxAutoReconnectAtStartup);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBoxDataSource);
            this.Name = "PageReceiver";
            this.Size = new System.Drawing.Size(636, 384);
            ((System.ComponentModel.ISupportInitialize)(this.numericPort)).EndInit();
            this.groupBoxSerial.ResumeLayout(false);
            this.groupBoxSerial.PerformLayout();
            this.groupBoxNetwork.ResumeLayout(false);
            this.groupBoxNetwork.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button buttonTestConnection;
        private System.Windows.Forms.CheckBox checkBoxEnabled;
        private System.Windows.Forms.TextBox textBoxSerialShutdownText;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxAddress;
        private System.Windows.Forms.TextBox textBoxSerialStartupText;
        private System.Windows.Forms.NumericUpDown numericPort;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox comboBoxSerialHandshake;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxConnectionType;
        private System.Windows.Forms.ComboBox comboBoxSerialParity;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBoxSerialStopBits;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox comboBoxSerialDataBits;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button buttonClearLocationId;
        private System.Windows.Forms.ComboBox comboBoxLocationId;
        private System.Windows.Forms.ComboBox comboBoxSerialBaudRate;
        private System.Windows.Forms.GroupBox groupBoxSerial;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBoxSerialComPort;
        private System.Windows.Forms.GroupBox groupBoxNetwork;
        private System.Windows.Forms.Button buttonWizard;
        private System.Windows.Forms.CheckBox checkBoxAutoReconnectAtStartup;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxDataSource;
    }
}
