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
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningProvider)).BeginInit();
            this.SuspendLayout();
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
            this.feedWebSiteReceiverId.Location = new System.Drawing.Point(245, 33);
            this.feedWebSiteReceiverId.Name = "feedWebSiteReceiverId";
            this.feedWebSiteReceiverId.Size = new System.Drawing.Size(200, 21);
            this.feedWebSiteReceiverId.TabIndex = 2;
            // 
            // feedClosestAircaftReceiverId
            // 
            this.feedClosestAircaftReceiverId.Location = new System.Drawing.Point(245, 60);
            this.feedClosestAircaftReceiverId.Name = "feedClosestAircaftReceiverId";
            this.feedClosestAircaftReceiverId.Size = new System.Drawing.Size(200, 21);
            this.feedClosestAircaftReceiverId.TabIndex = 4;
            // 
            // feedFlightSimulatorXReceiverId
            // 
            this.feedFlightSimulatorXReceiverId.Location = new System.Drawing.Point(245, 87);
            this.feedFlightSimulatorXReceiverId.Name = "feedFlightSimulatorXReceiverId";
            this.feedFlightSimulatorXReceiverId.Size = new System.Drawing.Size(200, 21);
            this.feedFlightSimulatorXReceiverId.TabIndex = 6;
            // 
            // ParentPageReceivers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.feedFlightSimulatorXReceiverId);
            this.Controls.Add(this.feedClosestAircaftReceiverId);
            this.Controls.Add(this.feedWebSiteReceiverId);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonNew);
            this.Name = "ParentPageReceivers";
            this.Size = new System.Drawing.Size(458, 120);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
