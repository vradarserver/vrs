namespace VirtualRadar.WinForms.OptionPage
{
    partial class PageReceivers
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
            this.listReceivers = new VirtualRadar.WinForms.Controls.BindingListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFormat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLocation = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderConnection = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderConnectionParameters = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listReceivers
            // 
            this.listReceivers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listReceivers.CheckBoxes = true;
            this.listReceivers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderFormat,
            this.columnHeaderLocation,
            this.columnHeaderConnection,
            this.columnHeaderConnectionParameters});
            this.listReceivers.Location = new System.Drawing.Point(0, 0);
            this.listReceivers.Name = "listReceivers";
            this.listReceivers.Size = new System.Drawing.Size(688, 212);
            this.listReceivers.TabIndex = 0;
            this.listReceivers.FetchRecordContent += new System.EventHandler<VirtualRadar.WinForms.Controls.BindingListView.RecordContentEventArgs>(this.listReceivers_FetchRecordContent);
            this.listReceivers.AddClicked += new System.EventHandler(this.listReceivers_AddClicked);
            this.listReceivers.DeleteClicked += new System.EventHandler(this.listReceivers_DeleteClicked);
            this.listReceivers.EditClicked += new System.EventHandler(this.listReceivers_EditClicked);
            this.listReceivers.CheckedChanged += new System.EventHandler<VirtualRadar.WinForms.Controls.BindingListView.RecordCheckedEventArgs>(this.listReceivers_CheckedChanged);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "::Name::";
            this.columnHeaderName.Width = 200;
            // 
            // columnHeaderFormat
            // 
            this.columnHeaderFormat.Text = "::Format::";
            this.columnHeaderFormat.Width = 115;
            // 
            // columnHeaderLocation
            // 
            this.columnHeaderLocation.Text = "::Location::";
            this.columnHeaderLocation.Width = 100;
            // 
            // columnHeaderConnection
            // 
            this.columnHeaderConnection.Text = "::Connection::";
            this.columnHeaderConnection.Width = 100;
            // 
            // columnHeaderConnectionParameters
            // 
            this.columnHeaderConnectionParameters.Text = "::ConnectionParameters::";
            this.columnHeaderConnectionParameters.Width = 210;
            // 
            // PageReceivers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.listReceivers);
            this.Name = "PageReceivers";
            this.Size = new System.Drawing.Size(688, 212);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.BindingListView listReceivers;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderFormat;
        private System.Windows.Forms.ColumnHeader columnHeaderLocation;
        private System.Windows.Forms.ColumnHeader columnHeaderConnection;
        private System.Windows.Forms.ColumnHeader columnHeaderConnectionParameters;
    }
}
