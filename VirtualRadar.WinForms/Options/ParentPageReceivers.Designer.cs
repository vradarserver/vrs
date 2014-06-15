namespace VirtualRadar.WinForms.Options
{
    partial class ParentPageReceivers
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
            this.buttonNew = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.feedWebSiteReceiverId = new VirtualRadar.WinForms.Options.OptionsFeedSelectControl();
            this.feedClosestAircaftReceiverId = new VirtualRadar.WinForms.Options.OptionsFeedSelectControl();
            this.feedFlightSimulatorXReceiverId = new VirtualRadar.WinForms.Options.OptionsFeedSelectControl();
            this.listViewReceivers = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFormat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLocation = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderConnection = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderConnectionParameters = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.splitContainerControlsDescription.Panel1.SuspendLayout();
            this.splitContainerControlsDescription.SuspendLayout();
            this.panelContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainerControlsDescription
            // 
            this.splitContainerControlsDescription.Size = new System.Drawing.Size(737, 393);
            this.splitContainerControlsDescription.SplitterDistance = 325;
            // 
            // panelContent
            // 
            this.panelContent.Controls.Add(this.listViewReceivers);
            this.panelContent.Controls.Add(this.buttonNew);
            this.panelContent.Controls.Add(this.label1);
            this.panelContent.Controls.Add(this.label2);
            this.panelContent.Controls.Add(this.label3);
            this.panelContent.Controls.Add(this.feedWebSiteReceiverId);
            this.panelContent.Controls.Add(this.feedClosestAircaftReceiverId);
            this.panelContent.Controls.Add(this.feedFlightSimulatorXReceiverId);
            this.panelContent.Size = new System.Drawing.Size(737, 325);
            // 
            // buttonNew
            // 
            this.buttonNew.Location = new System.Drawing.Point(659, 299);
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Size = new System.Drawing.Size(75, 23);
            this.buttonNew.TabIndex = 7;
            this.buttonNew.Text = "::New::";
            this.buttonNew.UseVisualStyleBackColor = true;
            this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "::WebSiteReceiverId:::";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(141, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "::ClosestAircraftReceiverId:::";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(149, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "::FlightSimulatorXReceiverId:::";
            // 
            // feedWebSiteReceiverId
            // 
            this.warningProvider.SetIconAlignment(this.feedWebSiteReceiverId, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.errorProvider.SetIconAlignment(this.feedWebSiteReceiverId, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.feedWebSiteReceiverId.Location = new System.Drawing.Point(245, 3);
            this.feedWebSiteReceiverId.Name = "feedWebSiteReceiverId";
            this.feedWebSiteReceiverId.Size = new System.Drawing.Size(200, 21);
            this.feedWebSiteReceiverId.TabIndex = 1;
            // 
            // feedClosestAircaftReceiverId
            // 
            this.warningProvider.SetIconAlignment(this.feedClosestAircaftReceiverId, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.errorProvider.SetIconAlignment(this.feedClosestAircaftReceiverId, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.feedClosestAircaftReceiverId.Location = new System.Drawing.Point(245, 30);
            this.feedClosestAircaftReceiverId.Name = "feedClosestAircaftReceiverId";
            this.feedClosestAircaftReceiverId.Size = new System.Drawing.Size(200, 21);
            this.feedClosestAircaftReceiverId.TabIndex = 3;
            // 
            // feedFlightSimulatorXReceiverId
            // 
            this.warningProvider.SetIconAlignment(this.feedFlightSimulatorXReceiverId, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.errorProvider.SetIconAlignment(this.feedFlightSimulatorXReceiverId, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.feedFlightSimulatorXReceiverId.Location = new System.Drawing.Point(245, 57);
            this.feedFlightSimulatorXReceiverId.Name = "feedFlightSimulatorXReceiverId";
            this.feedFlightSimulatorXReceiverId.Size = new System.Drawing.Size(200, 21);
            this.feedFlightSimulatorXReceiverId.TabIndex = 5;
            // 
            // listViewReceivers
            // 
            this.listViewReceivers.CheckBoxes = true;
            this.listViewReceivers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderFormat,
            this.columnHeaderLocation,
            this.columnHeaderConnection,
            this.columnHeaderConnectionParameters});
            this.listViewReceivers.FullRowSelect = true;
            this.listViewReceivers.GridLines = true;
            this.listViewReceivers.Location = new System.Drawing.Point(6, 84);
            this.listViewReceivers.Name = "listViewReceivers";
            this.listViewReceivers.Size = new System.Drawing.Size(726, 209);
            this.listViewReceivers.TabIndex = 6;
            this.listViewReceivers.UseCompatibleStateImageBehavior = false;
            this.listViewReceivers.View = System.Windows.Forms.View.Details;
            this.listViewReceivers.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewReceivers_ItemChecked);
            this.listViewReceivers.DoubleClick += new System.EventHandler(this.listViewReceivers_DoubleClick);
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
            this.columnHeaderLocation.Width = 98;
            // 
            // columnHeaderConnection
            // 
            this.columnHeaderConnection.Text = "::Connection::";
            this.columnHeaderConnection.Width = 108;
            // 
            // columnHeaderConnectionParameters
            // 
            this.columnHeaderConnectionParameters.Text = "::ConnectionParameters::";
            this.columnHeaderConnectionParameters.Width = 208;
            // 
            // ParentPageReceivers
            // 
            this.Name = "ParentPageReceivers";
            this.Size = new System.Drawing.Size(737, 393);
            this.splitContainerControlsDescription.Panel1.ResumeLayout(false);
            this.splitContainerControlsDescription.ResumeLayout(false);
            this.panelContent.ResumeLayout(false);
            this.panelContent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonNew;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private OptionsFeedSelectControl feedWebSiteReceiverId;
        private OptionsFeedSelectControl feedClosestAircaftReceiverId;
        private OptionsFeedSelectControl feedFlightSimulatorXReceiverId;
        private System.Windows.Forms.ListView listViewReceivers;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderConnection;
        private System.Windows.Forms.ColumnHeader columnHeaderConnectionParameters;
        private System.Windows.Forms.ColumnHeader columnHeaderFormat;
        private System.Windows.Forms.ColumnHeader columnHeaderLocation;
    }
}
