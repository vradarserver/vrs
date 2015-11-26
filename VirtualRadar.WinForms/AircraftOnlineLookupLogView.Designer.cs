namespace VirtualRadar.WinForms
{
    partial class AircraftOnlineLookupLogView
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
            if(disposing) {
                if(components != null) components.Dispose();
                if(_Presenter != null) _Presenter.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listView = new VirtualRadar.WinForms.Controls.ListViewPlus();
            this.columnHeaderTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderIcao = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderRegistration = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderCountry = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderManufacturer = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderModel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderModelIcao = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderOperator = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderOperatorIcao = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSerial = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderYearBuilt = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listView
            // 
            this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderTime,
            this.columnHeaderIcao,
            this.columnHeaderRegistration,
            this.columnHeaderCountry,
            this.columnHeaderManufacturer,
            this.columnHeaderModel,
            this.columnHeaderModelIcao,
            this.columnHeaderOperator,
            this.columnHeaderOperatorIcao,
            this.columnHeaderSerial,
            this.columnHeaderYearBuilt});
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.Location = new System.Drawing.Point(13, 12);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(859, 457);
            this.listView.TabIndex = 0;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderTime
            // 
            this.columnHeaderTime.Text = "::Time::";
            // 
            // columnHeaderIcao
            // 
            this.columnHeaderIcao.Text = "::ICAO::";
            // 
            // columnHeaderRegistration
            // 
            this.columnHeaderRegistration.Text = "::Reg::";
            this.columnHeaderRegistration.Width = 80;
            // 
            // columnHeaderCountry
            // 
            this.columnHeaderCountry.Text = "::Country::";
            this.columnHeaderCountry.Width = 90;
            // 
            // columnHeaderManufacturer
            // 
            this.columnHeaderManufacturer.Text = "::Manufacturer::";
            this.columnHeaderManufacturer.Width = 92;
            // 
            // columnHeaderModel
            // 
            this.columnHeaderModel.Text = "::Model::";
            this.columnHeaderModel.Width = 123;
            // 
            // columnHeaderModelIcao
            // 
            this.columnHeaderModelIcao.Text = "::ICAO::";
            this.columnHeaderModelIcao.Width = 51;
            // 
            // columnHeaderOperator
            // 
            this.columnHeaderOperator.Text = "::Operator::";
            this.columnHeaderOperator.Width = 120;
            // 
            // columnHeaderOperatorIcao
            // 
            this.columnHeaderOperatorIcao.Text = "::ICAO::";
            this.columnHeaderOperatorIcao.Width = 50;
            // 
            // columnHeaderSerial
            // 
            this.columnHeaderSerial.Text = "::Serial::";
            // 
            // columnHeaderYearBuilt
            // 
            this.columnHeaderYearBuilt.Text = "::Year::";
            this.columnHeaderYearBuilt.Width = 47;
            // 
            // AircraftOnlineLookupLogView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 482);
            this.Controls.Add(this.listView);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AircraftOnlineLookupLogView";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "::AircraftDetailOnlineLookupLog::";
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.ListViewPlus listView;
        private System.Windows.Forms.ColumnHeader columnHeaderTime;
        private System.Windows.Forms.ColumnHeader columnHeaderIcao;
        private System.Windows.Forms.ColumnHeader columnHeaderRegistration;
        private System.Windows.Forms.ColumnHeader columnHeaderCountry;
        private System.Windows.Forms.ColumnHeader columnHeaderManufacturer;
        private System.Windows.Forms.ColumnHeader columnHeaderModel;
        private System.Windows.Forms.ColumnHeader columnHeaderModelIcao;
        private System.Windows.Forms.ColumnHeader columnHeaderOperator;
        private System.Windows.Forms.ColumnHeader columnHeaderOperatorIcao;
        private System.Windows.Forms.ColumnHeader columnHeaderSerial;
        private System.Windows.Forms.ColumnHeader columnHeaderYearBuilt;
    }
}