namespace VirtualRadar.WinForms.SettingPage
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
            this.comboBoxClosestAircraftReceiverId = new System.Windows.Forms.ComboBox();
            this.comboBoxWebSiteReceiverId = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxFsxReceiverId = new System.Windows.Forms.ComboBox();
            this.listReceivers = new VirtualRadar.WinForms.Controls.MasterListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFormat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLocation = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderConnection = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderConnectionParameters = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderReceiverUsage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // comboBoxClosestAircraftReceiverId
            // 
            this.comboBoxClosestAircraftReceiverId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxClosestAircraftReceiverId.Location = new System.Drawing.Point(200, 27);
            this.comboBoxClosestAircraftReceiverId.Name = "comboBoxClosestAircraftReceiverId";
            this.comboBoxClosestAircraftReceiverId.Size = new System.Drawing.Size(150, 21);
            this.comboBoxClosestAircraftReceiverId.TabIndex = 10;
            // 
            // comboBoxWebSiteReceiverId
            // 
            this.comboBoxWebSiteReceiverId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxWebSiteReceiverId.Location = new System.Drawing.Point(200, 0);
            this.comboBoxWebSiteReceiverId.Name = "comboBoxWebSiteReceiverId";
            this.comboBoxWebSiteReceiverId.Size = new System.Drawing.Size(150, 21);
            this.comboBoxWebSiteReceiverId.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "::WebSiteReceiverId:::";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(141, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "::ClosestAircraftReceiverId:::";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 57);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(149, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "::FlightSimulatorXReceiverId:::";
            // 
            // comboBoxFsxReceiverId
            // 
            this.comboBoxFsxReceiverId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFsxReceiverId.Location = new System.Drawing.Point(200, 54);
            this.comboBoxFsxReceiverId.Name = "comboBoxFsxReceiverId";
            this.comboBoxFsxReceiverId.Size = new System.Drawing.Size(150, 21);
            this.comboBoxFsxReceiverId.TabIndex = 12;
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
            this.columnHeaderReceiverUsage,
            this.columnHeaderLocation,
            this.columnHeaderConnection,
            this.columnHeaderConnectionParameters});
            this.listReceivers.Location = new System.Drawing.Point(0, 81);
            this.listReceivers.Name = "listReceivers";
            this.listReceivers.Size = new System.Drawing.Size(636, 273);
            this.listReceivers.TabIndex = 13;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "::Name::";
            this.columnHeaderName.Width = 145;
            // 
            // columnHeaderFormat
            // 
            this.columnHeaderFormat.Text = "::Format::";
            this.columnHeaderFormat.Width = 100;
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
            this.columnHeaderConnectionParameters.Width = 165;
            // 
            // columnHeaderReceiverUsage
            // 
            this.columnHeaderReceiverUsage.Text = "::Hidden::";
            this.columnHeaderReceiverUsage.Width = 80;
            // 
            // PageReceivers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.comboBoxClosestAircraftReceiverId);
            this.Controls.Add(this.comboBoxWebSiteReceiverId);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.listReceivers);
            this.Controls.Add(this.comboBoxFsxReceiverId);
            this.Name = "PageReceivers";
            this.Size = new System.Drawing.Size(636, 354);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxClosestAircraftReceiverId;
        private System.Windows.Forms.ComboBox comboBoxWebSiteReceiverId;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private Controls.MasterListView listReceivers;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderFormat;
        private System.Windows.Forms.ColumnHeader columnHeaderLocation;
        private System.Windows.Forms.ColumnHeader columnHeaderConnection;
        private System.Windows.Forms.ColumnHeader columnHeaderConnectionParameters;
        private System.Windows.Forms.ComboBox comboBoxFsxReceiverId;
        private System.Windows.Forms.ColumnHeader columnHeaderReceiverUsage;
    }
}
