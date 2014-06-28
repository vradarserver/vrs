namespace VirtualRadar.WinForms.OptionPage
{
    partial class PageReceiverLocations
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
            this.listReceiverLocations = new VirtualRadar.WinForms.Controls.BindingListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLatitude = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLongitude = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.linkLabelUpdateFromBaseStationDatabase = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // listReceiverLocations
            // 
            this.listReceiverLocations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listReceiverLocations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderLatitude,
            this.columnHeaderLongitude});
            this.listReceiverLocations.Location = new System.Drawing.Point(0, 0);
            this.listReceiverLocations.Name = "listReceiverLocations";
            this.listReceiverLocations.Size = new System.Drawing.Size(784, 405);
            this.listReceiverLocations.TabIndex = 0;
            this.listReceiverLocations.FetchRecordContent += new System.EventHandler<VirtualRadar.WinForms.Controls.BindingListView.RecordContentEventArgs>(this.listReceiverLocations_FetchRecordContent);
            this.listReceiverLocations.AddClicked += new System.EventHandler(this.listReceiverLocations_AddClicked);
            this.listReceiverLocations.DeleteClicked += new System.EventHandler(this.listReceiverLocations_DeleteClicked);
            this.listReceiverLocations.EditClicked += new System.EventHandler(this.listReceiverLocations_EditClicked);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "::Name::";
            this.columnHeaderName.Width = 200;
            // 
            // columnHeaderLatitude
            // 
            this.columnHeaderLatitude.Text = "::Latitude::";
            this.columnHeaderLatitude.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderLatitude.Width = 100;
            // 
            // columnHeaderLongitude
            // 
            this.columnHeaderLongitude.Text = "::Longitude::";
            this.columnHeaderLongitude.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderLongitude.Width = 100;
            // 
            // linkLabelUpdateFromBaseStationDatabase
            // 
            this.linkLabelUpdateFromBaseStationDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelUpdateFromBaseStationDatabase.AutoSize = true;
            this.linkLabelUpdateFromBaseStationDatabase.Location = new System.Drawing.Point(0, 408);
            this.linkLabelUpdateFromBaseStationDatabase.Name = "linkLabelUpdateFromBaseStationDatabase";
            this.linkLabelUpdateFromBaseStationDatabase.Size = new System.Drawing.Size(180, 13);
            this.linkLabelUpdateFromBaseStationDatabase.TabIndex = 1;
            this.linkLabelUpdateFromBaseStationDatabase.TabStop = true;
            this.linkLabelUpdateFromBaseStationDatabase.Text = "::UpdateFromBaseStationDatabase::";
            this.linkLabelUpdateFromBaseStationDatabase.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelUpdateFromBaseStationDatabase_LinkClicked);
            // 
            // PageReceiverLocations
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.linkLabelUpdateFromBaseStationDatabase);
            this.Controls.Add(this.listReceiverLocations);
            this.Name = "PageReceiverLocations";
            this.Size = new System.Drawing.Size(784, 421);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.BindingListView listReceiverLocations;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderLatitude;
        private System.Windows.Forms.ColumnHeader columnHeaderLongitude;
        private System.Windows.Forms.LinkLabel linkLabelUpdateFromBaseStationDatabase;
    }
}
