namespace VirtualRadar.WinForms.OptionPage
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
            this.buttonTestConnection = new System.Windows.Forms.Button();
            this.buttonClearLocationId = new System.Windows.Forms.Button();
            this.comboBoxLocationId = new VirtualRadar.WinForms.Controls.ObservableListComboBox();
            this.groupBoxSerial = new System.Windows.Forms.GroupBox();
            this.label14 = new System.Windows.Forms.Label();
            this.textBoxSerialShutdownText = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.textBoxSerialStartupText = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.comboBoxSerialHandshake = new VirtualRadar.WinForms.Controls.ComboBoxPlus();
            this.label11 = new System.Windows.Forms.Label();
            this.comboBoxSerialParity = new VirtualRadar.WinForms.Controls.ComboBoxPlus();
            this.label10 = new System.Windows.Forms.Label();
            this.comboBoxSerialStopBits = new VirtualRadar.WinForms.Controls.ComboBoxPlus();
            this.label9 = new System.Windows.Forms.Label();
            this.comboBoxSerialDataBits = new VirtualRadar.WinForms.Controls.ComboBoxPlus();
            this.label8 = new System.Windows.Forms.Label();
            this.comboBoxSerialBaudRate = new VirtualRadar.WinForms.Controls.ComboBoxPlus();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBoxSerialComPort = new VirtualRadar.WinForms.Controls.ComboBoxPlus();
            this.checkBoxAutoReconnectAtStartup = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxDataSource = new VirtualRadar.WinForms.Controls.ComboBoxPlus();
            this.groupBoxNetwork = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.numericPort = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxAddress = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBoxEnabled = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxConnectionType = new VirtualRadar.WinForms.Controls.ComboBoxPlus();
            this.groupBoxSerial.SuspendLayout();
            this.groupBoxNetwork.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericPort)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonTestConnection
            // 
            this.buttonTestConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTestConnection.Location = new System.Drawing.Point(480, 101);
            this.buttonTestConnection.Name = "buttonTestConnection";
            this.buttonTestConnection.Size = new System.Drawing.Size(156, 23);
            this.buttonTestConnection.TabIndex = 11;
            this.buttonTestConnection.Text = "::TestConnection::";
            this.buttonTestConnection.UseVisualStyleBackColor = true;
            this.buttonTestConnection.Click += new System.EventHandler(this.buttonTestConnection_Click);
            // 
            // buttonClearLocationId
            // 
            this.buttonClearLocationId.FlatAppearance.BorderSize = 0;
            this.buttonClearLocationId.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonClearLocationId.Image = ((System.Drawing.Image)(resources.GetObject("buttonClearLocationId.Image")));
            this.buttonClearLocationId.Location = new System.Drawing.Point(308, 77);
            this.buttonClearLocationId.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.buttonClearLocationId.Name = "buttonClearLocationId";
            this.buttonClearLocationId.Size = new System.Drawing.Size(21, 21);
            this.buttonClearLocationId.TabIndex = 8;
            this.buttonClearLocationId.UseVisualStyleBackColor = true;
            this.buttonClearLocationId.Click += new System.EventHandler(this.buttonClearLocationId_Click);
            // 
            // comboBoxLocationId
            // 
            this.comboBoxLocationId.DisplayMember = "Name";
            this.comboBoxLocationId.FormattingEnabled = true;
            this.comboBoxLocationId.Location = new System.Drawing.Point(155, 77);
            this.comboBoxLocationId.Name = "comboBoxLocationId";
            this.comboBoxLocationId.Size = new System.Drawing.Size(150, 21);
            this.comboBoxLocationId.TabIndex = 7;
            this.comboBoxLocationId.ValueMember = "UniqueId";
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
            this.groupBoxSerial.Location = new System.Drawing.Point(0, 216);
            this.groupBoxSerial.Name = "groupBoxSerial";
            this.groupBoxSerial.Size = new System.Drawing.Size(636, 157);
            this.groupBoxSerial.TabIndex = 13;
            this.groupBoxSerial.TabStop = false;
            this.groupBoxSerial.Text = "::USBOverCOM::";
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
            // textBoxSerialShutdownText
            // 
            this.textBoxSerialShutdownText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSerialShutdownText.Location = new System.Drawing.Point(153, 126);
            this.textBoxSerialShutdownText.Name = "textBoxSerialShutdownText";
            this.textBoxSerialShutdownText.Size = new System.Drawing.Size(477, 20);
            this.textBoxSerialShutdownText.TabIndex = 15;
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
            // textBoxSerialStartupText
            // 
            this.textBoxSerialStartupText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSerialStartupText.Location = new System.Drawing.Point(153, 100);
            this.textBoxSerialStartupText.Name = "textBoxSerialStartupText";
            this.textBoxSerialStartupText.Size = new System.Drawing.Size(477, 20);
            this.textBoxSerialStartupText.TabIndex = 13;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(333, 76);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(103, 13);
            this.label12.TabIndex = 10;
            this.label12.Text = "::SerialHandshake:::";
            // 
            // comboBoxSerialHandshake
            // 
            this.comboBoxSerialHandshake.FormattingEnabled = true;
            this.comboBoxSerialHandshake.Location = new System.Drawing.Point(480, 73);
            this.comboBoxSerialHandshake.Name = "comboBoxSerialHandshake";
            this.comboBoxSerialHandshake.Size = new System.Drawing.Size(150, 21);
            this.comboBoxSerialHandshake.TabIndex = 11;
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
            // comboBoxSerialParity
            // 
            this.comboBoxSerialParity.FormattingEnabled = true;
            this.comboBoxSerialParity.Location = new System.Drawing.Point(153, 73);
            this.comboBoxSerialParity.Name = "comboBoxSerialParity";
            this.comboBoxSerialParity.Size = new System.Drawing.Size(150, 21);
            this.comboBoxSerialParity.TabIndex = 9;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(333, 49);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(87, 13);
            this.label10.TabIndex = 6;
            this.label10.Text = "::SerialStopBits:::";
            // 
            // comboBoxSerialStopBits
            // 
            this.comboBoxSerialStopBits.FormattingEnabled = true;
            this.comboBoxSerialStopBits.Location = new System.Drawing.Point(480, 46);
            this.comboBoxSerialStopBits.Name = "comboBoxSerialStopBits";
            this.comboBoxSerialStopBits.Size = new System.Drawing.Size(150, 21);
            this.comboBoxSerialStopBits.TabIndex = 7;
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
            this.comboBoxSerialDataBits.FormattingEnabled = true;
            this.comboBoxSerialDataBits.Location = new System.Drawing.Point(153, 46);
            this.comboBoxSerialDataBits.Name = "comboBoxSerialDataBits";
            this.comboBoxSerialDataBits.Size = new System.Drawing.Size(150, 21);
            this.comboBoxSerialDataBits.TabIndex = 5;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(333, 22);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(96, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "::SerialBaudRate:::";
            // 
            // comboBoxSerialBaudRate
            // 
            this.comboBoxSerialBaudRate.FormattingEnabled = true;
            this.comboBoxSerialBaudRate.Location = new System.Drawing.Point(480, 19);
            this.comboBoxSerialBaudRate.Name = "comboBoxSerialBaudRate";
            this.comboBoxSerialBaudRate.Size = new System.Drawing.Size(150, 21);
            this.comboBoxSerialBaudRate.TabIndex = 3;
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
            this.comboBoxSerialComPort.FormattingEnabled = true;
            this.comboBoxSerialComPort.Location = new System.Drawing.Point(153, 19);
            this.comboBoxSerialComPort.Name = "comboBoxSerialComPort";
            this.comboBoxSerialComPort.Size = new System.Drawing.Size(150, 21);
            this.comboBoxSerialComPort.TabIndex = 1;
            // 
            // checkBoxAutoReconnectAtStartup
            // 
            this.checkBoxAutoReconnectAtStartup.AutoSize = true;
            this.checkBoxAutoReconnectAtStartup.Location = new System.Drawing.Point(336, 51);
            this.checkBoxAutoReconnectAtStartup.Name = "checkBoxAutoReconnectAtStartup";
            this.checkBoxAutoReconnectAtStartup.Size = new System.Drawing.Size(157, 17);
            this.checkBoxAutoReconnectAtStartup.TabIndex = 5;
            this.checkBoxAutoReconnectAtStartup.Text = "::AutoReconnectAtStartup::";
            this.checkBoxAutoReconnectAtStartup.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(0, 52);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "::Format:::";
            // 
            // comboBoxDataSource
            // 
            this.comboBoxDataSource.FormattingEnabled = true;
            this.comboBoxDataSource.Location = new System.Drawing.Point(155, 49);
            this.comboBoxDataSource.Name = "comboBoxDataSource";
            this.comboBoxDataSource.Size = new System.Drawing.Size(150, 21);
            this.comboBoxDataSource.TabIndex = 4;
            // 
            // groupBoxNetwork
            // 
            this.groupBoxNetwork.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxNetwork.Controls.Add(this.label6);
            this.groupBoxNetwork.Controls.Add(this.numericPort);
            this.groupBoxNetwork.Controls.Add(this.label5);
            this.groupBoxNetwork.Controls.Add(this.textBoxAddress);
            this.groupBoxNetwork.Location = new System.Drawing.Point(0, 130);
            this.groupBoxNetwork.Name = "groupBoxNetwork";
            this.groupBoxNetwork.Size = new System.Drawing.Size(636, 80);
            this.groupBoxNetwork.TabIndex = 12;
            this.groupBoxNetwork.TabStop = false;
            this.groupBoxNetwork.Text = "::Network::";
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
            // textBoxAddress
            // 
            this.textBoxAddress.Location = new System.Drawing.Point(151, 20);
            this.textBoxAddress.Name = "textBoxAddress";
            this.textBoxAddress.Size = new System.Drawing.Size(150, 20);
            this.textBoxAddress.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 80);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "::Location:::";
            // 
            // checkBoxEnabled
            // 
            this.checkBoxEnabled.AutoSize = true;
            this.checkBoxEnabled.Location = new System.Drawing.Point(155, 0);
            this.checkBoxEnabled.Name = "checkBoxEnabled";
            this.checkBoxEnabled.Size = new System.Drawing.Size(77, 17);
            this.checkBoxEnabled.TabIndex = 0;
            this.checkBoxEnabled.Text = "::Enabled::";
            this.checkBoxEnabled.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "::Name:::";
            // 
            // textBoxName
            // 
            this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxName.Location = new System.Drawing.Point(155, 23);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(481, 20);
            this.textBoxName.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 106);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "::ConnectionType:::";
            // 
            // comboBoxConnectionType
            // 
            this.comboBoxConnectionType.FormattingEnabled = true;
            this.comboBoxConnectionType.Location = new System.Drawing.Point(155, 103);
            this.comboBoxConnectionType.Name = "comboBoxConnectionType";
            this.comboBoxConnectionType.Size = new System.Drawing.Size(150, 21);
            this.comboBoxConnectionType.TabIndex = 10;
            this.comboBoxConnectionType.SelectedIndexChanged += new System.EventHandler(this.comboBoxConnectionType_SelectedIndexChanged);
            // 
            // PageReceiver
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.buttonTestConnection);
            this.Controls.Add(this.buttonClearLocationId);
            this.Controls.Add(this.comboBoxLocationId);
            this.Controls.Add(this.groupBoxSerial);
            this.Controls.Add(this.checkBoxAutoReconnectAtStartup);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBoxDataSource);
            this.Controls.Add(this.groupBoxNetwork);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkBoxEnabled);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxConnectionType);
            this.Name = "PageReceiver";
            this.Size = new System.Drawing.Size(636, 375);
            this.groupBoxSerial.ResumeLayout(false);
            this.groupBoxSerial.PerformLayout();
            this.groupBoxNetwork.ResumeLayout(false);
            this.groupBoxNetwork.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxSerial;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox textBoxSerialShutdownText;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox textBoxSerialStartupText;
        private System.Windows.Forms.Label label12;
        private Controls.ComboBoxPlus comboBoxSerialHandshake;
        private System.Windows.Forms.Label label11;
        private Controls.ComboBoxPlus comboBoxSerialParity;
        private System.Windows.Forms.Label label10;
        private Controls.ComboBoxPlus comboBoxSerialStopBits;
        private System.Windows.Forms.Label label9;
        private Controls.ComboBoxPlus comboBoxSerialDataBits;
        private System.Windows.Forms.Label label8;
        private Controls.ComboBoxPlus comboBoxSerialBaudRate;
        private System.Windows.Forms.Label label7;
        private Controls.ComboBoxPlus comboBoxSerialComPort;
        private System.Windows.Forms.CheckBox checkBoxAutoReconnectAtStartup;
        private System.Windows.Forms.Label label4;
        private Controls.ComboBoxPlus comboBoxDataSource;
        private System.Windows.Forms.GroupBox groupBoxNetwork;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericPort;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxAddress;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBoxEnabled;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label label1;
        private Controls.ComboBoxPlus comboBoxConnectionType;
        private Controls.ObservableListComboBox comboBoxLocationId;
        private System.Windows.Forms.Button buttonClearLocationId;
        private System.Windows.Forms.Button buttonTestConnection;
    }
}
