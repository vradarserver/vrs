namespace VirtualRadar.WinForms.Controls
{
    partial class ConnectionSessionListControl
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
            this.columnHeaderStart = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderIpAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSource = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDuration = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderRequests = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBytesSent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderHtmlBytes = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderJsonBytes = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderImageBytes = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderAudioBytes = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderOtherBytes = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listView
            // 
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderStart,
            this.columnHeaderIpAddress,
            this.columnHeaderSource,
            this.columnHeaderDuration,
            this.columnHeaderRequests,
            this.columnHeaderBytesSent,
            this.columnHeaderHtmlBytes,
            this.columnHeaderJsonBytes,
            this.columnHeaderImageBytes,
            this.columnHeaderAudioBytes,
            this.columnHeaderOtherBytes});
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(0, 0);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(967, 255);
            this.listView.TabIndex = 0;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_ColumnClick);
            // 
            // columnHeaderStart
            // 
            this.columnHeaderStart.Text = "::Start::";
            this.columnHeaderStart.Width = 120;
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
            // columnHeaderDuration
            // 
            this.columnHeaderDuration.Text = "::Duration::";
            this.columnHeaderDuration.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderDuration.Width = 70;
            // 
            // columnHeaderRequests
            // 
            this.columnHeaderRequests.Text = "::Requests::";
            this.columnHeaderRequests.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderRequests.Width = 87;
            // 
            // columnHeaderBytesSent
            // 
            this.columnHeaderBytesSent.Text = "::BytesSent::";
            this.columnHeaderBytesSent.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderBytesSent.Width = 99;
            // 
            // columnHeaderHtmlBytes
            // 
            this.columnHeaderHtmlBytes.Text = "HTML/JS";
            this.columnHeaderHtmlBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderHtmlBytes.Width = 75;
            // 
            // columnHeaderJsonBytes
            // 
            this.columnHeaderJsonBytes.Text = "JSON";
            this.columnHeaderJsonBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderJsonBytes.Width = 75;
            // 
            // columnHeaderImageBytes
            // 
            this.columnHeaderImageBytes.Text = "::Images::";
            this.columnHeaderImageBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderImageBytes.Width = 75;
            // 
            // columnHeaderAudioBytes
            // 
            this.columnHeaderAudioBytes.Text = "::Audio::";
            this.columnHeaderAudioBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderAudioBytes.Width = 75;
            // 
            // columnHeaderOtherBytes
            // 
            this.columnHeaderOtherBytes.Text = "::Other::";
            this.columnHeaderOtherBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderOtherBytes.Width = 75;
            // 
            // ConnectionSessionListControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listView);
            this.Name = "ConnectionSessionListControl";
            this.Size = new System.Drawing.Size(967, 255);
            this.ResumeLayout(false);

        }

        #endregion

        private VirtualRadar.WinForms.Controls.ListViewPlus listView;
        private System.Windows.Forms.ColumnHeader columnHeaderStart;
        private System.Windows.Forms.ColumnHeader columnHeaderIpAddress;
        private System.Windows.Forms.ColumnHeader columnHeaderSource;
        private System.Windows.Forms.ColumnHeader columnHeaderDuration;
        private System.Windows.Forms.ColumnHeader columnHeaderRequests;
        private System.Windows.Forms.ColumnHeader columnHeaderBytesSent;
        private System.Windows.Forms.ColumnHeader columnHeaderHtmlBytes;
        private System.Windows.Forms.ColumnHeader columnHeaderJsonBytes;
        private System.Windows.Forms.ColumnHeader columnHeaderImageBytes;
        private System.Windows.Forms.ColumnHeader columnHeaderAudioBytes;
        private System.Windows.Forms.ColumnHeader columnHeaderOtherBytes;
    }
}
