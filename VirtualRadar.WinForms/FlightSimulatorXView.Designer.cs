namespace VirtualRadar.WinForms
{
    partial class FlightSimulatorXView
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FlightSimulatorXView));
            this.radioButtonFreezeMethod = new System.Windows.Forms.RadioButton();
            this.groupBoxAdsAircraft = new System.Windows.Forms.GroupBox();
            this.radioButtonSlewMethod = new System.Windows.Forms.RadioButton();
            this.labelRideStatus = new System.Windows.Forms.Label();
            this.buttonRideAircraft = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.aircraftListControl = new VirtualRadar.WinForms.Controls.AircraftListControl();
            this.linkLabelAddress = new System.Windows.Forms.LinkLabel();
            this.groupBoxAdsAircraft.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioButtonFreezeMethod
            // 
            this.radioButtonFreezeMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButtonFreezeMethod.AutoSize = true;
            this.radioButtonFreezeMethod.Location = new System.Drawing.Point(322, 219);
            this.radioButtonFreezeMethod.Name = "radioButtonFreezeMethod";
            this.radioButtonFreezeMethod.Size = new System.Drawing.Size(124, 17);
            this.radioButtonFreezeMethod.TabIndex = 4;
            this.radioButtonFreezeMethod.TabStop = true;
            this.radioButtonFreezeMethod.Text = "::UseFreezeMethod::";
            this.radioButtonFreezeMethod.UseVisualStyleBackColor = true;
            // 
            // groupBoxAdsAircraft
            // 
            this.groupBoxAdsAircraft.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxAdsAircraft.Controls.Add(this.aircraftListControl);
            this.groupBoxAdsAircraft.Controls.Add(this.radioButtonFreezeMethod);
            this.groupBoxAdsAircraft.Controls.Add(this.radioButtonSlewMethod);
            this.groupBoxAdsAircraft.Controls.Add(this.labelRideStatus);
            this.groupBoxAdsAircraft.Controls.Add(this.buttonRideAircraft);
            this.groupBoxAdsAircraft.Location = new System.Drawing.Point(8, 79);
            this.groupBoxAdsAircraft.Name = "groupBoxAdsAircraft";
            this.groupBoxAdsAircraft.Size = new System.Drawing.Size(527, 242);
            this.groupBoxAdsAircraft.TabIndex = 11;
            this.groupBoxAdsAircraft.TabStop = false;
            this.groupBoxAdsAircraft.Text = "::ADSBAircraft::";
            // 
            // radioButtonSlewMethod
            // 
            this.radioButtonSlewMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButtonSlewMethod.AutoSize = true;
            this.radioButtonSlewMethod.Location = new System.Drawing.Point(122, 219);
            this.radioButtonSlewMethod.Name = "radioButtonSlewMethod";
            this.radioButtonSlewMethod.Size = new System.Drawing.Size(115, 17);
            this.radioButtonSlewMethod.TabIndex = 3;
            this.radioButtonSlewMethod.TabStop = true;
            this.radioButtonSlewMethod.Text = "::UseSlewMethod::";
            this.radioButtonSlewMethod.UseVisualStyleBackColor = true;
            // 
            // labelRideStatus
            // 
            this.labelRideStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelRideStatus.Location = new System.Drawing.Point(122, 195);
            this.labelRideStatus.Name = "labelRideStatus";
            this.labelRideStatus.Size = new System.Drawing.Size(398, 13);
            this.labelRideStatus.TabIndex = 2;
            this.labelRideStatus.Text = "-";
            // 
            // buttonRideAircraft
            // 
            this.buttonRideAircraft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRideAircraft.Enabled = false;
            this.buttonRideAircraft.Location = new System.Drawing.Point(6, 190);
            this.buttonRideAircraft.Name = "buttonRideAircraft";
            this.buttonRideAircraft.Size = new System.Drawing.Size(110, 23);
            this.buttonRideAircraft.TabIndex = 1;
            this.buttonRideAircraft.Text = "::RideAircraft::";
            this.buttonRideAircraft.UseVisualStyleBackColor = true;
            this.buttonRideAircraft.Click += new System.EventHandler(this.buttonRideAircraft_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStatus.Location = new System.Drawing.Point(67, 5);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(468, 42);
            this.labelStatus.TabIndex = 8;
            this.labelStatus.Text = "::Disconnected::";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "::Status:::";
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(162, 50);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(180, 23);
            this.buttonConnect.TabIndex = 9;
            this.buttonConnect.Text = "::ConnectToFlightSimulatorX::";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(460, 338);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "::Close::";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Interval = 1000;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // aircraftListControl
            // 
            this.aircraftListControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.aircraftListControl.Location = new System.Drawing.Point(6, 19);
            this.aircraftListControl.Name = "aircraftListControl";
            this.aircraftListControl.Size = new System.Drawing.Size(515, 165);
            this.aircraftListControl.TabIndex = 5;
            // 
            // linkLabelAddress
            // 
            this.linkLabelAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelAddress.Location = new System.Drawing.Point(7, 343);
            this.linkLabelAddress.Name = "linkLabelAddress";
            this.linkLabelAddress.Size = new System.Drawing.Size(447, 13);
            this.linkLabelAddress.TabIndex = 10;
            this.linkLabelAddress.TabStop = true;
            this.linkLabelAddress.Text = "link to flight sim page";
            this.linkLabelAddress.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelAddress_LinkClicked);
            // 
            // FlightSimulatorXView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 367);
            this.Controls.Add(this.groupBoxAdsAircraft);
            this.Controls.Add(this.linkLabelAddress);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.buttonCancel);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FlightSimulatorXView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Flight Simulator X";
            this.groupBoxAdsAircraft.ResumeLayout(false);
            this.groupBoxAdsAircraft.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButtonFreezeMethod;
        private System.Windows.Forms.GroupBox groupBoxAdsAircraft;
        private System.Windows.Forms.RadioButton radioButtonSlewMethod;
        private System.Windows.Forms.Label labelRideStatus;
        private System.Windows.Forms.Button buttonRideAircraft;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Button buttonCancel;
        private Controls.AircraftListControl aircraftListControl;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.LinkLabel linkLabelAddress;
    }
}