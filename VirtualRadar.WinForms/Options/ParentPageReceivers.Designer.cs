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
            this.splitContainerControlsDescription.Panel1.SuspendLayout();
            this.splitContainerControlsDescription.SuspendLayout();
            this.panelContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainerControlsDescription
            // 
            this.splitContainerControlsDescription.Size = new System.Drawing.Size(450, 190);
            this.splitContainerControlsDescription.SplitterDistance = 122;
            // 
            // panelContent
            // 
            this.panelContent.Controls.Add(this.buttonNew);
            this.panelContent.Controls.Add(this.label1);
            this.panelContent.Controls.Add(this.label2);
            this.panelContent.Controls.Add(this.label3);
            this.panelContent.Controls.Add(this.feedWebSiteReceiverId);
            this.panelContent.Controls.Add(this.feedClosestAircaftReceiverId);
            this.panelContent.Controls.Add(this.feedFlightSimulatorXReceiverId);
            this.panelContent.Size = new System.Drawing.Size(450, 122);
            // 
            // buttonNew
            // 
            this.buttonNew.Location = new System.Drawing.Point(4, 4);
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Size = new System.Drawing.Size(75, 23);
            this.buttonNew.TabIndex = 0;
            this.buttonNew.Text = "::New::";
            this.buttonNew.UseVisualStyleBackColor = true;
            this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "::WebSiteReceiverId:::";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(141, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "::ClosestAircraftReceiverId:::";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(149, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "::FlightSimulatorXReceiverId:::";
            // 
            // feedWebSiteReceiverId
            // 
            this.warningProvider.SetIconAlignment(this.feedWebSiteReceiverId, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.errorProvider.SetIconAlignment(this.feedWebSiteReceiverId, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.feedWebSiteReceiverId.Location = new System.Drawing.Point(245, 33);
            this.feedWebSiteReceiverId.Name = "feedWebSiteReceiverId";
            this.feedWebSiteReceiverId.Size = new System.Drawing.Size(200, 21);
            this.feedWebSiteReceiverId.TabIndex = 2;
            // 
            // feedClosestAircaftReceiverId
            // 
            this.warningProvider.SetIconAlignment(this.feedClosestAircaftReceiverId, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.errorProvider.SetIconAlignment(this.feedClosestAircaftReceiverId, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.feedClosestAircaftReceiverId.Location = new System.Drawing.Point(245, 60);
            this.feedClosestAircaftReceiverId.Name = "feedClosestAircaftReceiverId";
            this.feedClosestAircaftReceiverId.Size = new System.Drawing.Size(200, 21);
            this.feedClosestAircaftReceiverId.TabIndex = 4;
            // 
            // feedFlightSimulatorXReceiverId
            // 
            this.warningProvider.SetIconAlignment(this.feedFlightSimulatorXReceiverId, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.errorProvider.SetIconAlignment(this.feedFlightSimulatorXReceiverId, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.feedFlightSimulatorXReceiverId.Location = new System.Drawing.Point(245, 87);
            this.feedFlightSimulatorXReceiverId.Name = "feedFlightSimulatorXReceiverId";
            this.feedFlightSimulatorXReceiverId.Size = new System.Drawing.Size(200, 21);
            this.feedFlightSimulatorXReceiverId.TabIndex = 6;
            // 
            // ParentPageReceivers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Name = "ParentPageReceivers";
            this.Size = new System.Drawing.Size(450, 190);
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
    }
}
