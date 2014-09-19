namespace VirtualRadar.WinForms.Controls
{
    partial class RebroadcastStatusControl
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
            this.components = new System.ComponentModel.Container();
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.listView = new VirtualRadar.WinForms.Controls.ListViewPlus();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderIPAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPort = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBytesBuffered = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBytesSent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBytesStale = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.labelDescribeConfiguration = new System.Windows.Forms.Label();
            this.menuShowExceptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.groupBox.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.listView);
            this.groupBox.Controls.Add(this.label1);
            this.groupBox.Controls.Add(this.labelDescribeConfiguration);
            this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox.Location = new System.Drawing.Point(0, 0);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(708, 218);
            this.groupBox.TabIndex = 0;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "::RebroadcastServersStatus::";
            // 
            // listView
            // 
            this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderIPAddress,
            this.columnHeaderPort,
            this.columnHeaderBytesBuffered,
            this.columnHeaderBytesSent,
            this.columnHeaderBytesStale});
            this.listView.ContextMenuStrip = this.contextMenuStrip;
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.Location = new System.Drawing.Point(10, 37);
            this.listView.MultiSelect = false;
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(692, 175);
            this.listView.TabIndex = 2;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "::Name::";
            this.columnHeaderName.Width = 166;
            // 
            // columnHeaderIPAddress
            // 
            this.columnHeaderIPAddress.Text = "::IPAddress::";
            this.columnHeaderIPAddress.Width = 145;
            // 
            // columnHeaderPort
            // 
            this.columnHeaderPort.Text = "::Port::";
            // 
            // columnHeaderBytesBuffered
            // 
            this.columnHeaderBytesBuffered.Text = "::BytesBuffered::";
            this.columnHeaderBytesBuffered.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderBytesBuffered.Width = 100;
            // 
            // columnHeaderBytesSent
            // 
            this.columnHeaderBytesSent.Text = "::BytesSent::";
            this.columnHeaderBytesSent.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderBytesSent.Width = 100;
            // 
            // columnHeaderBytesStale
            // 
            this.columnHeaderBytesStale.Text = "::BytesStale::";
            this.columnHeaderBytesStale.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderBytesStale.Width = 100;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "::Configuration:::";
            // 
            // labelDescribeConfiguration
            // 
            this.labelDescribeConfiguration.AutoSize = true;
            this.labelDescribeConfiguration.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelDescribeConfiguration.Location = new System.Drawing.Point(136, 20);
            this.labelDescribeConfiguration.Name = "labelDescribeConfiguration";
            this.labelDescribeConfiguration.Size = new System.Drawing.Size(33, 13);
            this.labelDescribeConfiguration.TabIndex = 0;
            this.labelDescribeConfiguration.Text = "None";
            this.labelDescribeConfiguration.Click += new System.EventHandler(this.labelDescribeConfiguration_Click);
            // 
            // menuShowExceptionsToolStripMenuItem
            // 
            this.menuShowExceptionsToolStripMenuItem.Name = "menuShowExceptionsToolStripMenuItem";
            this.menuShowExceptionsToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.menuShowExceptionsToolStripMenuItem.Text = "::menuShowExceptions::";
            this.menuShowExceptionsToolStripMenuItem.Click += new System.EventHandler(this.menuShowExceptionsToolStripMenuItem_Click);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuShowExceptionsToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(203, 48);
            // 
            // RebroadcastStatusControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox);
            this.Name = "RebroadcastStatusControl";
            this.Size = new System.Drawing.Size(708, 218);
            this.groupBox.ResumeLayout(false);
            this.groupBox.PerformLayout();
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelDescribeConfiguration;
        private VirtualRadar.WinForms.Controls.ListViewPlus listView;
        private System.Windows.Forms.ColumnHeader columnHeaderIPAddress;
        private System.Windows.Forms.ColumnHeader columnHeaderPort;
        private System.Windows.Forms.ColumnHeader columnHeaderBytesSent;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderBytesBuffered;
        private System.Windows.Forms.ColumnHeader columnHeaderBytesStale;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuShowExceptionsToolStripMenuItem;
    }
}
