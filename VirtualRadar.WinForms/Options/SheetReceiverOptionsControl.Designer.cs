namespace VirtualRadar.WinForms.Options
{
    partial class SheetReceiverOptionsControl
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
            this.comboBoxConnectionType = new VirtualRadar.WinForms.Controls.ComboBoxPlus();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxEnabled = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.receiverLocationIdControl = new VirtualRadar.WinForms.Options.OptionsReceiverLocationSelectControl();
            this.groupBoxNetwork = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.numericPort = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxAddress = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxDataSource = new VirtualRadar.WinForms.Controls.ComboBoxPlus();
            this.checkBoxAutoReconnectAtStartup = new System.Windows.Forms.CheckBox();
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
            this.splitContainerControlsDescription.Panel1.SuspendLayout();
            this.splitContainerControlsDescription.SuspendLayout();
            this.panelContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningProvider)).BeginInit();
            this.groupBoxNetwork.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericPort)).BeginInit();
            this.groupBoxSerial.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerControlsDescription
            // 
            this.splitContainerControlsDescription.Size = new System.Drawing.Size(683, 448);
            this.splitContainerControlsDescription.SplitterDistance = 380;
            // 
            // panelContent
            // 
            this.panelContent.Controls.Add(this.groupBoxSerial);
            this.panelContent.Controls.Add(this.checkBoxAutoReconnectAtStartup);
            this.panelContent.Controls.Add(this.label4);
            this.panelContent.Controls.Add(this.comboBoxDataSource);
            this.panelContent.Controls.Add(this.groupBoxNetwork);
            this.panelContent.Controls.Add(this.receiverLocationIdControl);
            this.panelContent.Controls.Add(this.label3);
            this.panelContent.Controls.Add(this.checkBoxEnabled);
            this.panelContent.Controls.Add(this.label2);
            this.panelContent.Controls.Add(this.textBoxName);
            this.panelContent.Controls.Add(this.label1);
            this.panelContent.Controls.Add(this.comboBoxConnectionType);
            this.panelContent.Size = new System.Drawing.Size(683, 380);
            // 
            // comboBoxConnectionType
            // 
            this.comboBoxConnectionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxConnectionType.FormattingEnabled = true;
            this.warningProvider.SetIconAlignment(this.comboBoxConnectionType, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.errorProvider.SetIconAlignment(this.comboBoxConnectionType, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.comboBoxConnectionType.Location = new System.Drawing.Point(157, 106);
            this.comboBoxConnectionType.Name = "comboBoxConnectionType";
            this.comboBoxConnectionType.Size = new System.Drawing.Size(150, 21);
            this.comboBoxConnectionType.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 109);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "::ConnectionType:::";
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(157, 26);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(521, 20);
            this.textBoxName.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "::Name:::";
            // 
            // checkBoxEnabled
            // 
            this.checkBoxEnabled.AutoSize = true;
            this.checkBoxEnabled.Location = new System.Drawing.Point(157, 3);
            this.checkBoxEnabled.Name = "checkBoxEnabled";
            this.checkBoxEnabled.Size = new System.Drawing.Size(77, 17);
            this.checkBoxEnabled.TabIndex = 0;
            this.checkBoxEnabled.Text = "::Enabled::";
            this.checkBoxEnabled.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "::Location:::";
            // 
            // receiverLocationIdControl
            // 
            this.receiverLocationIdControl.Location = new System.Drawing.Point(157, 79);
            this.receiverLocationIdControl.Name = "receiverLocationIdControl";
            this.receiverLocationIdControl.SelectedLocationId = 0;
            this.receiverLocationIdControl.ShowNoneOption = true;
            this.receiverLocationIdControl.Size = new System.Drawing.Size(150, 21);
            this.receiverLocationIdControl.TabIndex = 7;
            // 
            // groupBoxNetwork
            // 
            this.groupBoxNetwork.Controls.Add(this.label6);
            this.groupBoxNetwork.Controls.Add(this.numericPort);
            this.groupBoxNetwork.Controls.Add(this.label5);
            this.groupBoxNetwork.Controls.Add(this.textBoxAddress);
            this.groupBoxNetwork.Location = new System.Drawing.Point(6, 133);
            this.groupBoxNetwork.Name = "groupBoxNetwork";
            this.groupBoxNetwork.Size = new System.Drawing.Size(672, 80);
            this.groupBoxNetwork.TabIndex = 10;
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
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 55);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "::Format:::";
            // 
            // comboBoxDataSource
            // 
            this.comboBoxDataSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDataSource.FormattingEnabled = true;
            this.warningProvider.SetIconAlignment(this.comboBoxDataSource, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.errorProvider.SetIconAlignment(this.comboBoxDataSource, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.comboBoxDataSource.Location = new System.Drawing.Point(157, 52);
            this.comboBoxDataSource.Name = "comboBoxDataSource";
            this.comboBoxDataSource.Size = new System.Drawing.Size(150, 21);
            this.comboBoxDataSource.TabIndex = 4;
            // 
            // checkBoxAutoReconnectAtStartup
            // 
            this.checkBoxAutoReconnectAtStartup.AutoSize = true;
            this.checkBoxAutoReconnectAtStartup.Location = new System.Drawing.Point(334, 54);
            this.checkBoxAutoReconnectAtStartup.Name = "checkBoxAutoReconnectAtStartup";
            this.checkBoxAutoReconnectAtStartup.Size = new System.Drawing.Size(157, 17);
            this.checkBoxAutoReconnectAtStartup.TabIndex = 5;
            this.checkBoxAutoReconnectAtStartup.Text = "::AutoReconnectAtStartup::";
            this.checkBoxAutoReconnectAtStartup.UseVisualStyleBackColor = true;
            // 
            // groupBoxSerial
            // 
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
            this.groupBoxSerial.Location = new System.Drawing.Point(4, 220);
            this.groupBoxSerial.Name = "groupBoxSerial";
            this.groupBoxSerial.Size = new System.Drawing.Size(674, 157);
            this.groupBoxSerial.TabIndex = 11;
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
            this.textBoxSerialShutdownText.Location = new System.Drawing.Point(153, 126);
            this.textBoxSerialShutdownText.Name = "textBoxSerialShutdownText";
            this.textBoxSerialShutdownText.Size = new System.Drawing.Size(515, 20);
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
            this.textBoxSerialStartupText.Location = new System.Drawing.Point(153, 100);
            this.textBoxSerialStartupText.Name = "textBoxSerialStartupText";
            this.textBoxSerialStartupText.Size = new System.Drawing.Size(515, 20);
            this.textBoxSerialStartupText.TabIndex = 13;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(327, 76);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(103, 13);
            this.label12.TabIndex = 10;
            this.label12.Text = "::SerialHandshake:::";
            // 
            // comboBoxSerialHandshake
            // 
            this.comboBoxSerialHandshake.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSerialHandshake.FormattingEnabled = true;
            this.warningProvider.SetIconAlignment(this.comboBoxSerialHandshake, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.errorProvider.SetIconAlignment(this.comboBoxSerialHandshake, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.comboBoxSerialHandshake.Location = new System.Drawing.Point(474, 73);
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
            this.comboBoxSerialParity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSerialParity.FormattingEnabled = true;
            this.warningProvider.SetIconAlignment(this.comboBoxSerialParity, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.errorProvider.SetIconAlignment(this.comboBoxSerialParity, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.comboBoxSerialParity.Location = new System.Drawing.Point(153, 73);
            this.comboBoxSerialParity.Name = "comboBoxSerialParity";
            this.comboBoxSerialParity.Size = new System.Drawing.Size(150, 21);
            this.comboBoxSerialParity.TabIndex = 9;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(327, 49);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(87, 13);
            this.label10.TabIndex = 6;
            this.label10.Text = "::SerialStopBits:::";
            // 
            // comboBoxSerialStopBits
            // 
            this.comboBoxSerialStopBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSerialStopBits.FormattingEnabled = true;
            this.warningProvider.SetIconAlignment(this.comboBoxSerialStopBits, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.errorProvider.SetIconAlignment(this.comboBoxSerialStopBits, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.comboBoxSerialStopBits.Location = new System.Drawing.Point(474, 46);
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
            this.comboBoxSerialDataBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSerialDataBits.FormattingEnabled = true;
            this.warningProvider.SetIconAlignment(this.comboBoxSerialDataBits, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.errorProvider.SetIconAlignment(this.comboBoxSerialDataBits, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.comboBoxSerialDataBits.Location = new System.Drawing.Point(153, 46);
            this.comboBoxSerialDataBits.Name = "comboBoxSerialDataBits";
            this.comboBoxSerialDataBits.Size = new System.Drawing.Size(150, 21);
            this.comboBoxSerialDataBits.TabIndex = 5;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(327, 22);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(96, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "::SerialBaudRate:::";
            // 
            // comboBoxSerialBaudRate
            // 
            this.comboBoxSerialBaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSerialBaudRate.FormattingEnabled = true;
            this.warningProvider.SetIconAlignment(this.comboBoxSerialBaudRate, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.errorProvider.SetIconAlignment(this.comboBoxSerialBaudRate, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.comboBoxSerialBaudRate.Location = new System.Drawing.Point(474, 19);
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
            this.comboBoxSerialComPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSerialComPort.FormattingEnabled = true;
            this.warningProvider.SetIconAlignment(this.comboBoxSerialComPort, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.errorProvider.SetIconAlignment(this.comboBoxSerialComPort, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.comboBoxSerialComPort.Location = new System.Drawing.Point(153, 19);
            this.comboBoxSerialComPort.Name = "comboBoxSerialComPort";
            this.comboBoxSerialComPort.Size = new System.Drawing.Size(150, 21);
            this.comboBoxSerialComPort.TabIndex = 1;
            // 
            // SheetReceiverOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "SheetReceiverOptionsControl";
            this.Size = new System.Drawing.Size(683, 448);
            this.splitContainerControlsDescription.Panel1.ResumeLayout(false);
            this.splitContainerControlsDescription.ResumeLayout(false);
            this.panelContent.ResumeLayout(false);
            this.panelContent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningProvider)).EndInit();
            this.groupBoxNetwork.ResumeLayout(false);
            this.groupBoxNetwork.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericPort)).EndInit();
            this.groupBoxSerial.ResumeLayout(false);
            this.groupBoxSerial.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private VirtualRadar.WinForms.Controls.ComboBoxPlus comboBoxConnectionType;
        private System.Windows.Forms.CheckBox checkBoxEnabled;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxName;
        private OptionsReceiverLocationSelectControl receiverLocationIdControl;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBoxNetwork;
        private System.Windows.Forms.Label label4;
        private VirtualRadar.WinForms.Controls.ComboBoxPlus comboBoxDataSource;
        private System.Windows.Forms.CheckBox checkBoxAutoReconnectAtStartup;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxAddress;
        private System.Windows.Forms.NumericUpDown numericPort;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBoxSerial;
        private System.Windows.Forms.Label label7;
        private VirtualRadar.WinForms.Controls.ComboBoxPlus comboBoxSerialComPort;
        private System.Windows.Forms.Label label8;
        private VirtualRadar.WinForms.Controls.ComboBoxPlus comboBoxSerialBaudRate;
        private System.Windows.Forms.Label label9;
        private VirtualRadar.WinForms.Controls.ComboBoxPlus comboBoxSerialDataBits;
        private System.Windows.Forms.Label label10;
        private VirtualRadar.WinForms.Controls.ComboBoxPlus comboBoxSerialStopBits;
        private System.Windows.Forms.Label label11;
        private VirtualRadar.WinForms.Controls.ComboBoxPlus comboBoxSerialParity;
        private System.Windows.Forms.Label label12;
        private VirtualRadar.WinForms.Controls.ComboBoxPlus comboBoxSerialHandshake;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox textBoxSerialStartupText;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox textBoxSerialShutdownText;

    }
}
