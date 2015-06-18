namespace VirtualRadar.WinForms.Controls
{
    partial class WebServerUserListControl
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
            this.listView = new VirtualRadar.WinForms.Controls.ListViewPlus();
            this.columnHeaderAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLastRequest = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBytesSent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLastUrl = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderUser = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listView
            // 
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderAddress,
            this.columnHeaderUser,
            this.columnHeaderLastRequest,
            this.columnHeaderBytesSent,
            this.columnHeaderLastUrl});
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.Location = new System.Drawing.Point(0, 0);
            this.listView.MultiSelect = false;
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(644, 279);
            this.listView.TabIndex = 0;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderAddress
            // 
            this.columnHeaderAddress.Text = "::IPAddress::";
            this.columnHeaderAddress.Width = 105;
            // 
            // columnHeaderLastRequest
            // 
            this.columnHeaderLastRequest.Text = "::LastRequest::";
            this.columnHeaderLastRequest.Width = 118;
            // 
            // columnHeaderBytesSent
            // 
            this.columnHeaderBytesSent.Text = "::BytesSent::";
            this.columnHeaderBytesSent.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderBytesSent.Width = 88;
            // 
            // columnHeaderLastUrl
            // 
            this.columnHeaderLastUrl.Text = "::LastURL::";
            this.columnHeaderLastUrl.Width = 226;
            // 
            // columnHeaderUser
            // 
            this.columnHeaderUser.Text = "::User::";
            this.columnHeaderUser.Width = 79;
            // 
            // WebServerUserListControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listView);
            this.Name = "WebServerUserListControl";
            this.Size = new System.Drawing.Size(644, 279);
            this.ResumeLayout(false);

        }

        #endregion

        private VirtualRadar.WinForms.Controls.ListViewPlus listView;
        private System.Windows.Forms.ColumnHeader columnHeaderAddress;
        private System.Windows.Forms.ColumnHeader columnHeaderLastRequest;
        private System.Windows.Forms.ColumnHeader columnHeaderBytesSent;
        private System.Windows.Forms.ColumnHeader columnHeaderLastUrl;
        private System.Windows.Forms.ColumnHeader columnHeaderUser;

    }
}
