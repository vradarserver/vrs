namespace VirtualRadar.WinForms.Controls
{
    partial class FeedStatusControl
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
            this.listView = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderConnectionStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderTotalMessages = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderTotalBadMessages = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderAircraftCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuStatisticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reconnectDataFeedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuResetReceiverRangeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.listView);
            this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox.Location = new System.Drawing.Point(0, 0);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(726, 124);
            this.groupBox.TabIndex = 0;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "::FeedStatus:::";
            // 
            // listView
            // 
            this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderConnectionStatus,
            this.columnHeaderTotalMessages,
            this.columnHeaderTotalBadMessages,
            this.columnHeaderAircraftCount});
            this.listView.ContextMenuStrip = this.contextMenuStrip;
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.Location = new System.Drawing.Point(7, 20);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(713, 98);
            this.listView.TabIndex = 0;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.DoubleClick += new System.EventHandler(this.listView_DoubleClick);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "::NameTitle::";
            this.columnHeaderName.Width = 132;
            // 
            // columnHeaderConnectionStatus
            // 
            this.columnHeaderConnectionStatus.Text = "::ConnectionStatusTitle::";
            this.columnHeaderConnectionStatus.Width = 132;
            // 
            // columnHeaderTotalMessages
            // 
            this.columnHeaderTotalMessages.Text = "::TotalMessagesTitle::";
            this.columnHeaderTotalMessages.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderTotalMessages.Width = 135;
            // 
            // columnHeaderTotalBadMessages
            // 
            this.columnHeaderTotalBadMessages.Text = "::TotalBadMessagesTitle::";
            this.columnHeaderTotalBadMessages.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderTotalBadMessages.Width = 135;
            // 
            // columnHeaderAircraftCount
            // 
            this.columnHeaderAircraftCount.Text = "::AircraftCountTitle::";
            this.columnHeaderAircraftCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderAircraftCount.Width = 107;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuStatisticsToolStripMenuItem,
            this.reconnectDataFeedToolStripMenuItem,
            this.menuResetReceiverRangeToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(237, 92);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // menuStatisticsToolStripMenuItem
            // 
            this.menuStatisticsToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.menuStatisticsToolStripMenuItem.Name = "menuStatisticsToolStripMenuItem";
            this.menuStatisticsToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.menuStatisticsToolStripMenuItem.Text = "::menuStatistics::";
            this.menuStatisticsToolStripMenuItem.Click += new System.EventHandler(this.menuStatisticsToolStripMenuItem_Click);
            // 
            // reconnectDataFeedToolStripMenuItem
            // 
            this.reconnectDataFeedToolStripMenuItem.Name = "reconnectDataFeedToolStripMenuItem";
            this.reconnectDataFeedToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.reconnectDataFeedToolStripMenuItem.Text = "::menuReconnectToDataFeed::";
            this.reconnectDataFeedToolStripMenuItem.Click += new System.EventHandler(this.reconnectDataFeedToolStripMenuItem_Click);
            // 
            // menuResetReceiverRangeToolStripMenuItem
            // 
            this.menuResetReceiverRangeToolStripMenuItem.Name = "menuResetReceiverRangeToolStripMenuItem";
            this.menuResetReceiverRangeToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.menuResetReceiverRangeToolStripMenuItem.Text = "::menuResetReceiverRange::";
            this.menuResetReceiverRangeToolStripMenuItem.Click += new System.EventHandler(this.menuResetReceiverRangeToolStripMenuItem_Click);
            // 
            // FeedStatusControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox);
            this.Name = "FeedStatusControl";
            this.Size = new System.Drawing.Size(726, 124);
            this.groupBox.ResumeLayout(false);
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderConnectionStatus;
        private System.Windows.Forms.ColumnHeader columnHeaderTotalMessages;
        private System.Windows.Forms.ColumnHeader columnHeaderTotalBadMessages;
        private System.Windows.Forms.ColumnHeader columnHeaderAircraftCount;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem reconnectDataFeedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuStatisticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuResetReceiverRangeToolStripMenuItem;
    }
}
