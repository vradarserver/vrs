namespace VirtualRadar.WinForms
{
    partial class XPlaneView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XPlaneView));
            this.txtXPlaneHost = new System.Windows.Forms.TextBox();
            this.nudXPlanePort = new System.Windows.Forms.NumericUpDown();
            this.nudReplyPort = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblConnectionStatus = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudXPlanePort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudReplyPort)).BeginInit();
            this.SuspendLayout();
            // 
            // txtXPlaneHost
            // 
            this.txtXPlaneHost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtXPlaneHost.Location = new System.Drawing.Point(157, 13);
            this.txtXPlaneHost.Name = "txtXPlaneHost";
            this.txtXPlaneHost.Size = new System.Drawing.Size(353, 20);
            this.txtXPlaneHost.TabIndex = 0;
            // 
            // nudXPlanePort
            // 
            this.nudXPlanePort.Location = new System.Drawing.Point(157, 39);
            this.nudXPlanePort.Maximum = new decimal(new int[] {
            65534,
            0,
            0,
            0});
            this.nudXPlanePort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudXPlanePort.Name = "nudXPlanePort";
            this.nudXPlanePort.Size = new System.Drawing.Size(120, 20);
            this.nudXPlanePort.TabIndex = 1;
            this.nudXPlanePort.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // nudReplyPort
            // 
            this.nudReplyPort.Location = new System.Drawing.Point(157, 65);
            this.nudReplyPort.Maximum = new decimal(new int[] {
            65534,
            0,
            0,
            0});
            this.nudReplyPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudReplyPort.Name = "nudReplyPort";
            this.nudReplyPort.Size = new System.Drawing.Size(120, 20);
            this.nudReplyPort.TabIndex = 2;
            this.nudReplyPort.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "::XPlaneHost:::";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "::XPlanePort:::";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "::ReplyPort:::";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(157, 97);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(196, 23);
            this.btnConnect.TabIndex = 6;
            this.btnConnect.Text = "::ConnectToXPlane::";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblConnectionStatus.Location = new System.Drawing.Point(157, 132);
            this.lblConnectionStatus.Margin = new System.Windows.Forms.Padding(3, 9, 3, 6);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(353, 52);
            this.lblConnectionStatus.TabIndex = 7;
            this.lblConnectionStatus.Text = "-";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 132);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "::Status:::";
            // 
            // XPlaneView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(522, 199);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblConnectionStatus);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nudReplyPort);
            this.Controls.Add(this.nudXPlanePort);
            this.Controls.Add(this.txtXPlaneHost);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "XPlaneView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "XPlane";
            ((System.ComponentModel.ISupportInitialize)(this.nudXPlanePort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudReplyPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtXPlaneHost;
        private System.Windows.Forms.NumericUpDown nudXPlanePort;
        private System.Windows.Forms.NumericUpDown nudReplyPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label lblConnectionStatus;
        private System.Windows.Forms.Label label4;
    }
}