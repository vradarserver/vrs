namespace VirtualRadar.WinForms.SettingPage
{
    partial class PageRebroadcastServer
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
            this.label3 = new System.Windows.Forms.Label();
            this.numericStaleSeconds = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.numericPort = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxFormat = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxReceiver = new System.Windows.Forms.ComboBox();
            this.checkBoxEnabled = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericStaleSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPort)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 131);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 13);
            this.label3.TabIndex = 23;
            this.label3.Text = "::StaleSeconds:::";
            // 
            // numericStaleSeconds
            // 
            this.numericStaleSeconds.Location = new System.Drawing.Point(200, 129);
            this.numericStaleSeconds.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.numericStaleSeconds.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericStaleSeconds.Name = "numericStaleSeconds";
            this.numericStaleSeconds.Size = new System.Drawing.Size(77, 20);
            this.numericStaleSeconds.TabIndex = 24;
            this.numericStaleSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericStaleSeconds.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(0, 105);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "::Port:::";
            // 
            // numericPort
            // 
            this.numericPort.Location = new System.Drawing.Point(200, 103);
            this.numericPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericPort.Name = "numericPort";
            this.numericPort.Size = new System.Drawing.Size(77, 20);
            this.numericPort.TabIndex = 22;
            this.numericPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 79);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "::Format:::";
            // 
            // comboBoxFormat
            // 
            this.comboBoxFormat.DisplayMember = "Name";
            this.comboBoxFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFormat.FormattingEnabled = true;
            this.comboBoxFormat.Location = new System.Drawing.Point(200, 76);
            this.comboBoxFormat.Name = "comboBoxFormat";
            this.comboBoxFormat.Size = new System.Drawing.Size(150, 21);
            this.comboBoxFormat.TabIndex = 20;
            this.comboBoxFormat.ValueMember = "Value";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(0, 52);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "::Receiver:::";
            // 
            // comboBoxReceiver
            // 
            this.comboBoxReceiver.DisplayMember = "Name";
            this.comboBoxReceiver.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxReceiver.FormattingEnabled = true;
            this.comboBoxReceiver.Location = new System.Drawing.Point(200, 49);
            this.comboBoxReceiver.Name = "comboBoxReceiver";
            this.comboBoxReceiver.Size = new System.Drawing.Size(150, 21);
            this.comboBoxReceiver.TabIndex = 18;
            this.comboBoxReceiver.ValueMember = "UniqueId";
            // 
            // checkBoxEnabled
            // 
            this.checkBoxEnabled.AutoSize = true;
            this.checkBoxEnabled.Location = new System.Drawing.Point(200, 0);
            this.checkBoxEnabled.Name = "checkBoxEnabled";
            this.checkBoxEnabled.Size = new System.Drawing.Size(77, 17);
            this.checkBoxEnabled.TabIndex = 14;
            this.checkBoxEnabled.Text = "::Enabled::";
            this.checkBoxEnabled.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "::Name:::";
            // 
            // textBoxName
            // 
            this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxName.Location = new System.Drawing.Point(200, 23);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(436, 20);
            this.textBoxName.TabIndex = 16;
            // 
            // PageRebroadcastServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numericStaleSeconds);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.numericPort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxFormat);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBoxReceiver);
            this.Controls.Add(this.checkBoxEnabled);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxName);
            this.Name = "PageRebroadcastServer";
            this.Size = new System.Drawing.Size(636, 159);
            ((System.ComponentModel.ISupportInitialize)(this.numericStaleSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericStaleSeconds;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxFormat;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxReceiver;
        private System.Windows.Forms.CheckBox checkBoxEnabled;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxName;
    }
}
