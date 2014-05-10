namespace VirtualRadar.WinForms
{
    partial class ConnectionClientLogView
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
            this.splitContainerClientSessions = new System.Windows.Forms.SplitContainer();
            this.connectionClientListControl = new VirtualRadar.WinForms.Controls.ConnectionClientListControl();
            this.connectionSessionListControl = new VirtualRadar.WinForms.Controls.ConnectionSessionListControl();
            this.buttonClose = new System.Windows.Forms.Button();
            this.splitContainerClientSessions.Panel1.SuspendLayout();
            this.splitContainerClientSessions.Panel2.SuspendLayout();
            this.splitContainerClientSessions.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerClientSessions
            // 
            this.splitContainerClientSessions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerClientSessions.Location = new System.Drawing.Point(12, 12);
            this.splitContainerClientSessions.Name = "splitContainerClientSessions";
            this.splitContainerClientSessions.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerClientSessions.Panel1
            // 
            this.splitContainerClientSessions.Panel1.Controls.Add(this.connectionClientListControl);
            // 
            // splitContainerClientSessions.Panel2
            // 
            this.splitContainerClientSessions.Panel2.Controls.Add(this.connectionSessionListControl);
            this.splitContainerClientSessions.Size = new System.Drawing.Size(823, 532);
            this.splitContainerClientSessions.SplitterDistance = 233;
            this.splitContainerClientSessions.TabIndex = 0;
            // 
            // connectionClientListControl
            // 
            this.connectionClientListControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.connectionClientListControl.Location = new System.Drawing.Point(0, 0);
            this.connectionClientListControl.Name = "connectionClientListControl";
            this.connectionClientListControl.Size = new System.Drawing.Size(823, 233);
            this.connectionClientListControl.TabIndex = 0;
            this.connectionClientListControl.SelectionChanged += new System.EventHandler(this.connectionClientListControl_SelectionChanged);
            // 
            // connectionSessionListControl
            // 
            this.connectionSessionListControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.connectionSessionListControl.Location = new System.Drawing.Point(0, 0);
            this.connectionSessionListControl.Name = "connectionSessionListControl";
            this.connectionSessionListControl.ShowClientDetails = false;
            this.connectionSessionListControl.Size = new System.Drawing.Size(823, 295);
            this.connectionSessionListControl.TabIndex = 0;
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(760, 561);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 2;
            this.buttonClose.Text = "::Close::";
            this.buttonClose.UseVisualStyleBackColor = true;
            // 
            // ConnectionClientLogView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(847, 596);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.splitContainerClientSessions);
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConnectionClientLogView";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "::ConnectionClientLog::";
            this.splitContainerClientSessions.Panel1.ResumeLayout(false);
            this.splitContainerClientSessions.Panel2.ResumeLayout(false);
            this.splitContainerClientSessions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerClientSessions;
        private System.Windows.Forms.Button buttonClose;
        private Controls.ConnectionClientListControl connectionClientListControl;
        private Controls.ConnectionSessionListControl connectionSessionListControl;

    }
}