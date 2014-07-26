namespace VirtualRadar.WinForms.Controls
{
    partial class ConnectionClientListControl
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
            if (disposing && (components != null))
            {
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
            this.listView = new VirtualRadar.WinForms.Controls.ListViewPlus();
            this.columnHeaderIpAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSource = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderReverseDns = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFirstSeen = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLastSeen = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderCountSessions = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDuration = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBytesSent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listView
            // 
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderIpAddress,
            this.columnHeaderSource,
            this.columnHeaderReverseDns,
            this.columnHeaderFirstSeen,
            this.columnHeaderLastSeen,
            this.columnHeaderCountSessions,
            this.columnHeaderDuration,
            this.columnHeaderBytesSent});
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(0, 0);
            this.listView.MultiSelect = false;
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(800, 164);
            this.listView.TabIndex = 0;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            // 
            // columnHeaderIpAddress
            // 
            this.columnHeaderIpAddress.Text = "::IPAddress::";
            this.columnHeaderIpAddress.Width = 90;
            // 
            // columnHeaderSource
            // 
            this.columnHeaderSource.Text = "::Source::";
            // 
            // columnHeaderReverseDns
            // 
            this.columnHeaderReverseDns.Text = "::ReverseDNS::";
            this.columnHeaderReverseDns.Width = 160;
            // 
            // columnHeaderFirstSeen
            // 
            this.columnHeaderFirstSeen.Text = "::FirstSeen::";
            this.columnHeaderFirstSeen.Width = 128;
            // 
            // columnHeaderLastSeen
            // 
            this.columnHeaderLastSeen.Text = "::LastSeen::";
            this.columnHeaderLastSeen.Width = 128;
            // 
            // columnHeaderCountSessions
            // 
            this.columnHeaderCountSessions.Text = "::Sessions::";
            this.columnHeaderCountSessions.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderCountSessions.Width = 70;
            // 
            // columnHeaderDuration
            // 
            this.columnHeaderDuration.Text = "::Duration::";
            this.columnHeaderDuration.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderDuration.Width = 70;
            // 
            // columnHeaderBytesSent
            // 
            this.columnHeaderBytesSent.Text = "::BytesSent::";
            this.columnHeaderBytesSent.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderBytesSent.Width = 93;
            // 
            // ConnectionClientListControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listView);
            this.Name = "ConnectionClientListControl";
            this.Size = new System.Drawing.Size(800, 164);
            this.ResumeLayout(false);

        }

        #endregion

        private VirtualRadar.WinForms.Controls.ListViewPlus listView;
        private System.Windows.Forms.ColumnHeader columnHeaderIpAddress;
        private System.Windows.Forms.ColumnHeader columnHeaderSource;
        private System.Windows.Forms.ColumnHeader columnHeaderFirstSeen;
        private System.Windows.Forms.ColumnHeader columnHeaderLastSeen;
        private System.Windows.Forms.ColumnHeader columnHeaderCountSessions;
        private System.Windows.Forms.ColumnHeader columnHeaderBytesSent;
        private System.Windows.Forms.ColumnHeader columnHeaderDuration;
        private System.Windows.Forms.ColumnHeader columnHeaderReverseDns;
    }
}
