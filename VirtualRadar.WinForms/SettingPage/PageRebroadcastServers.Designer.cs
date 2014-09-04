namespace VirtualRadar.WinForms.SettingPage
{
    partial class PageRebroadcastServers
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
            this.listRebroadcastServers = new VirtualRadar.WinForms.Controls.BindingListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderReceiver = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFormat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderUNC = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDefaultAccess = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listRebroadcastServers
            // 
            this.listRebroadcastServers.CheckBoxes = true;
            this.listRebroadcastServers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderReceiver,
            this.columnHeaderFormat,
            this.columnHeaderUNC,
            this.columnHeaderDefaultAccess});
            this.listRebroadcastServers.DataSource = null;
            this.listRebroadcastServers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listRebroadcastServers.Location = new System.Drawing.Point(0, 0);
            this.listRebroadcastServers.Name = "listRebroadcastServers";
            this.listRebroadcastServers.Size = new System.Drawing.Size(636, 354);
            this.listRebroadcastServers.TabIndex = 8;
            this.listRebroadcastServers.FetchRecordContent += new System.EventHandler<VirtualRadar.WinForms.Controls.BindingListView.RecordContentEventArgs>(this.listRebroadcastServers_FetchRecordContent);
            this.listRebroadcastServers.AddClicked += new System.EventHandler(this.listRebroadcastServers_AddClicked);
            this.listRebroadcastServers.DeleteClicked += new System.EventHandler(this.listRebroadcastServers_DeleteClicked);
            this.listRebroadcastServers.EditClicked += new System.EventHandler(this.listRebroadcastServers_EditClicked);
            this.listRebroadcastServers.CheckedChanged += new System.EventHandler<VirtualRadar.WinForms.Controls.BindingListView.RecordCheckedEventArgs>(this.listRebroadcastServers_CheckedChanged);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "::Name::";
            this.columnHeaderName.Width = 140;
            // 
            // columnHeaderReceiver
            // 
            this.columnHeaderReceiver.Text = "::Receiver::";
            this.columnHeaderReceiver.Width = 140;
            // 
            // columnHeaderFormat
            // 
            this.columnHeaderFormat.Text = "::Format::";
            this.columnHeaderFormat.Width = 100;
            // 
            // columnHeaderUNC
            // 
            this.columnHeaderUNC.Text = "::UNC::";
            this.columnHeaderUNC.Width = 140;
            // 
            // columnHeaderDefaultAccess
            // 
            this.columnHeaderDefaultAccess.Text = "::Access::";
            this.columnHeaderDefaultAccess.Width = 90;
            // 
            // PageRebroadcastServers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.listRebroadcastServers);
            this.Name = "PageRebroadcastServers";
            this.Size = new System.Drawing.Size(636, 354);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.BindingListView listRebroadcastServers;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderReceiver;
        private System.Windows.Forms.ColumnHeader columnHeaderFormat;
        private System.Windows.Forms.ColumnHeader columnHeaderUNC;
        private System.Windows.Forms.ColumnHeader columnHeaderDefaultAccess;
    }
}
