namespace VirtualRadar.WinForms
{
    partial class ReceiverConfigurationWizard
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelBorder = new System.Windows.Forms.Panel();
            this.panelHeading = new System.Windows.Forms.Panel();
            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.textBoxNetworkAddress = new System.Windows.Forms.TextBox();
            this.wizard = new Gui.Wizard.Wizard();
            this.wizardPageDedicatedReceiver = new Gui.Wizard.WizardPage();
            this.radioPanelDedicatedReceiver = new RadioPanelApp.RadioPanel();
            this.radioButtonDedicatedPlaneFinderRadar = new System.Windows.Forms.RadioButton();
            this.radioButtonDedicatedRadarBox = new System.Windows.Forms.RadioButton();
            this.radioButtonDedicatedOther = new System.Windows.Forms.RadioButton();
            this.radioButtonDedicatedKinetics = new System.Windows.Forms.RadioButton();
            this.radioButtonDedicatedMicroAdsb = new System.Windows.Forms.RadioButton();
            this.radioButtonDedicatedBeast = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.wizardPageSdrDecoder = new Gui.Wizard.WizardPage();
            this.radioPanelSdrDecoder = new RadioPanelApp.RadioPanel();
            this.radioButtonSdrFR24Feeder = new System.Windows.Forms.RadioButton();
            this.radioButtonSdrAdsbSharp = new System.Windows.Forms.RadioButton();
            this.radioButtonSdrOther = new System.Windows.Forms.RadioButton();
            this.radioButtonSdrRtl1090 = new System.Windows.Forms.RadioButton();
            this.radioButtonSdrGrAirModes = new System.Windows.Forms.RadioButton();
            this.radioButtonSdrDump1090 = new System.Windows.Forms.RadioButton();
            this.radioButtonSdrModesdeco = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.wizardPageSdrOrDedicated = new Gui.Wizard.WizardPage();
            this.radioPanelReceiverClass = new RadioPanelApp.RadioPanel();
            this.radioButtonReceiverSdr = new System.Windows.Forms.RadioButton();
            this.radioButtonReceiverDedicated = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.wizardPageFinish = new Gui.Wizard.WizardPage();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.wizardPageNetworkAddress = new Gui.Wizard.WizardPage();
            this.label8 = new System.Windows.Forms.Label();
            this.labelNetworkAddressTitle = new System.Windows.Forms.Label();
            this.wizardPageLoopback = new Gui.Wizard.WizardPage();
            this.radioPanelLoopback = new RadioPanelApp.RadioPanel();
            this.radioButtonLoopbackNo = new System.Windows.Forms.RadioButton();
            this.radioButtonLoopbackYes = new System.Windows.Forms.RadioButton();
            this.labelLoopbackTitle = new System.Windows.Forms.Label();
            this.wizardPageKineticConnection = new Gui.Wizard.WizardPage();
            this.label7 = new System.Windows.Forms.Label();
            this.radioPanelKineticConnection = new RadioPanelApp.RadioPanel();
            this.radioButtonKineticConnectHardware = new System.Windows.Forms.RadioButton();
            this.radioButtonKineticConnectBaseStation = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.wizardPageConnectionType = new Gui.Wizard.WizardPage();
            this.radioPanelConnectionType = new RadioPanelApp.RadioPanel();
            this.radioButtonConnectionTypeNetwork = new System.Windows.Forms.RadioButton();
            this.radioButtonConnectionTypeUsb = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.radioButtonDedicatedAirnavXRange = new System.Windows.Forms.RadioButton();
            this.wizardPageWebAddress = new Gui.Wizard.WizardPage();
            this.textBoxWebAddress = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.labelWebAddressTitle = new System.Windows.Forms.Label();
            this.panelBorder.SuspendLayout();
            this.panelHeading.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.wizard.SuspendLayout();
            this.wizardPageDedicatedReceiver.SuspendLayout();
            this.radioPanelDedicatedReceiver.SuspendLayout();
            this.wizardPageSdrDecoder.SuspendLayout();
            this.radioPanelSdrDecoder.SuspendLayout();
            this.wizardPageSdrOrDedicated.SuspendLayout();
            this.radioPanelReceiverClass.SuspendLayout();
            this.wizardPageFinish.SuspendLayout();
            this.wizardPageNetworkAddress.SuspendLayout();
            this.wizardPageLoopback.SuspendLayout();
            this.radioPanelLoopback.SuspendLayout();
            this.wizardPageKineticConnection.SuspendLayout();
            this.radioPanelKineticConnection.SuspendLayout();
            this.wizardPageConnectionType.SuspendLayout();
            this.radioPanelConnectionType.SuspendLayout();
            this.wizardPageWebAddress.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBorder
            // 
            this.panelBorder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBorder.BackColor = System.Drawing.SystemColors.WindowText;
            this.panelBorder.Controls.Add(this.panelHeading);
            this.panelBorder.Location = new System.Drawing.Point(0, 0);
            this.panelBorder.Margin = new System.Windows.Forms.Padding(0);
            this.panelBorder.Name = "panelBorder";
            this.panelBorder.Size = new System.Drawing.Size(569, 56);
            this.panelBorder.TabIndex = 1;
            // 
            // panelHeading
            // 
            this.panelHeading.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelHeading.BackColor = System.Drawing.SystemColors.Window;
            this.panelHeading.Controls.Add(this.pictureBoxLogo);
            this.panelHeading.Controls.Add(this.label1);
            this.panelHeading.Location = new System.Drawing.Point(0, 0);
            this.panelHeading.Margin = new System.Windows.Forms.Padding(0);
            this.panelHeading.Name = "panelHeading";
            this.panelHeading.Size = new System.Drawing.Size(569, 55);
            this.panelHeading.TabIndex = 0;
            // 
            // pictureBoxLogo
            // 
            this.pictureBoxLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxLogo.Location = new System.Drawing.Point(509, 4);
            this.pictureBoxLogo.Name = "pictureBoxLogo";
            this.pictureBoxLogo.Size = new System.Drawing.Size(48, 48);
            this.pictureBoxLogo.TabIndex = 1;
            this.pictureBoxLogo.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(4, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(319, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "::ReceiverConfigurationWizard::";
            // 
            // errorProvider
            // 
            this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider.ContainerControl = this;
            // 
            // textBoxNetworkAddress
            // 
            this.errorProvider.SetIconAlignment(this.textBoxNetworkAddress, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.textBoxNetworkAddress.Location = new System.Drawing.Point(141, 46);
            this.textBoxNetworkAddress.MaxLength = 256;
            this.textBoxNetworkAddress.Name = "textBoxNetworkAddress";
            this.textBoxNetworkAddress.Size = new System.Drawing.Size(131, 20);
            this.textBoxNetworkAddress.TabIndex = 10;
            // 
            // wizard
            // 
            this.wizard.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.wizard.BackColor = System.Drawing.SystemColors.Control;
            this.wizard.Controls.Add(this.wizardPageWebAddress);
            this.wizard.Controls.Add(this.wizardPageNetworkAddress);
            this.wizard.Controls.Add(this.wizardPageLoopback);
            this.wizard.Controls.Add(this.wizardPageKineticConnection);
            this.wizard.Controls.Add(this.wizardPageConnectionType);
            this.wizard.Controls.Add(this.wizardPageFinish);
            this.wizard.Controls.Add(this.wizardPageDedicatedReceiver);
            this.wizard.Controls.Add(this.wizardPageSdrDecoder);
            this.wizard.Controls.Add(this.wizardPageSdrOrDedicated);
            this.wizard.Location = new System.Drawing.Point(0, 57);
            this.wizard.Margin = new System.Windows.Forms.Padding(0);
            this.wizard.Name = "wizard";
            this.wizard.Pages.AddRange(new Gui.Wizard.WizardPage[] {
            this.wizardPageSdrOrDedicated,
            this.wizardPageSdrDecoder,
            this.wizardPageDedicatedReceiver,
            this.wizardPageConnectionType,
            this.wizardPageKineticConnection,
            this.wizardPageLoopback,
            this.wizardPageNetworkAddress,
            this.wizardPageWebAddress,
            this.wizardPageFinish});
            this.wizard.Size = new System.Drawing.Size(569, 350);
            this.wizard.TabIndex = 0;
            // 
            // wizardPageDedicatedReceiver
            // 
            this.wizardPageDedicatedReceiver.Controls.Add(this.radioPanelDedicatedReceiver);
            this.wizardPageDedicatedReceiver.Controls.Add(this.label4);
            this.wizardPageDedicatedReceiver.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPageDedicatedReceiver.IsFinishPage = false;
            this.wizardPageDedicatedReceiver.Location = new System.Drawing.Point(0, 0);
            this.wizardPageDedicatedReceiver.Name = "wizardPageDedicatedReceiver";
            this.wizardPageDedicatedReceiver.Size = new System.Drawing.Size(569, 302);
            this.wizardPageDedicatedReceiver.TabIndex = 5;
            // 
            // radioPanelDedicatedReceiver
            // 
            this.radioPanelDedicatedReceiver.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.radioPanelDedicatedReceiver.Controls.Add(this.radioButtonDedicatedAirnavXRange);
            this.radioPanelDedicatedReceiver.Controls.Add(this.radioButtonDedicatedPlaneFinderRadar);
            this.radioPanelDedicatedReceiver.Controls.Add(this.radioButtonDedicatedRadarBox);
            this.radioPanelDedicatedReceiver.Controls.Add(this.radioButtonDedicatedOther);
            this.radioPanelDedicatedReceiver.Controls.Add(this.radioButtonDedicatedKinetics);
            this.radioPanelDedicatedReceiver.Controls.Add(this.radioButtonDedicatedMicroAdsb);
            this.radioPanelDedicatedReceiver.Controls.Add(this.radioButtonDedicatedBeast);
            this.radioPanelDedicatedReceiver.Location = new System.Drawing.Point(12, 44);
            this.radioPanelDedicatedReceiver.Name = "radioPanelDedicatedReceiver";
            this.radioPanelDedicatedReceiver.Size = new System.Drawing.Size(545, 172);
            this.radioPanelDedicatedReceiver.TabIndex = 9;
            this.radioPanelDedicatedReceiver.ValueMember = null;
            // 
            // radioButtonDedicatedPlaneFinderRadar
            // 
            this.radioButtonDedicatedPlaneFinderRadar.AutoSize = true;
            this.radioButtonDedicatedPlaneFinderRadar.Location = new System.Drawing.Point(3, 72);
            this.radioButtonDedicatedPlaneFinderRadar.Name = "radioButtonDedicatedPlaneFinderRadar";
            this.radioButtonDedicatedPlaneFinderRadar.Size = new System.Drawing.Size(93, 17);
            this.radioButtonDedicatedPlaneFinderRadar.TabIndex = 3;
            this.radioButtonDedicatedPlaneFinderRadar.Text = "::PlaneFinder::";
            this.radioButtonDedicatedPlaneFinderRadar.UseVisualStyleBackColor = true;
            // 
            // radioButtonDedicatedRadarBox
            // 
            this.radioButtonDedicatedRadarBox.AutoSize = true;
            this.radioButtonDedicatedRadarBox.Location = new System.Drawing.Point(3, 3);
            this.radioButtonDedicatedRadarBox.Name = "radioButtonDedicatedRadarBox";
            this.radioButtonDedicatedRadarBox.Size = new System.Drawing.Size(222, 17);
            this.radioButtonDedicatedRadarBox.TabIndex = 0;
            this.radioButtonDedicatedRadarBox.Text = "::RecConWizDedicatedAirNavRadarBox::";
            this.radioButtonDedicatedRadarBox.UseVisualStyleBackColor = true;
            // 
            // radioButtonDedicatedOther
            // 
            this.radioButtonDedicatedOther.AutoSize = true;
            this.radioButtonDedicatedOther.Location = new System.Drawing.Point(3, 148);
            this.radioButtonDedicatedOther.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.radioButtonDedicatedOther.Name = "radioButtonDedicatedOther";
            this.radioButtonDedicatedOther.Size = new System.Drawing.Size(63, 17);
            this.radioButtonDedicatedOther.TabIndex = 5;
            this.radioButtonDedicatedOther.Text = "::Other::";
            this.radioButtonDedicatedOther.UseVisualStyleBackColor = true;
            // 
            // radioButtonDedicatedKinetics
            // 
            this.radioButtonDedicatedKinetics.AutoSize = true;
            this.radioButtonDedicatedKinetics.Location = new System.Drawing.Point(3, 49);
            this.radioButtonDedicatedKinetics.Name = "radioButtonDedicatedKinetics";
            this.radioButtonDedicatedKinetics.Size = new System.Drawing.Size(180, 17);
            this.radioButtonDedicatedKinetics.TabIndex = 2;
            this.radioButtonDedicatedKinetics.Text = "::RecConWizDedicatedKinetics::";
            this.radioButtonDedicatedKinetics.UseVisualStyleBackColor = true;
            // 
            // radioButtonDedicatedMicroAdsb
            // 
            this.radioButtonDedicatedMicroAdsb.AutoSize = true;
            this.radioButtonDedicatedMicroAdsb.Location = new System.Drawing.Point(3, 95);
            this.radioButtonDedicatedMicroAdsb.Name = "radioButtonDedicatedMicroAdsb";
            this.radioButtonDedicatedMicroAdsb.Size = new System.Drawing.Size(193, 17);
            this.radioButtonDedicatedMicroAdsb.TabIndex = 4;
            this.radioButtonDedicatedMicroAdsb.Text = "::RecConWizDedicatedMicroAdsb::";
            this.radioButtonDedicatedMicroAdsb.UseVisualStyleBackColor = true;
            // 
            // radioButtonDedicatedBeast
            // 
            this.radioButtonDedicatedBeast.AutoSize = true;
            this.radioButtonDedicatedBeast.Location = new System.Drawing.Point(3, 26);
            this.radioButtonDedicatedBeast.Name = "radioButtonDedicatedBeast";
            this.radioButtonDedicatedBeast.Size = new System.Drawing.Size(170, 17);
            this.radioButtonDedicatedBeast.TabIndex = 1;
            this.radioButtonDedicatedBeast.Text = "::RecConWizDedicatedBeast::";
            this.radioButtonDedicatedBeast.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(12, 18);
            this.label4.Margin = new System.Windows.Forms.Padding(3, 20, 3, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(172, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "::RecConWizDedicatedTitle::";
            // 
            // wizardPageSdrDecoder
            // 
            this.wizardPageSdrDecoder.Controls.Add(this.radioPanelSdrDecoder);
            this.wizardPageSdrDecoder.Controls.Add(this.label3);
            this.wizardPageSdrDecoder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPageSdrDecoder.IsFinishPage = false;
            this.wizardPageSdrDecoder.Location = new System.Drawing.Point(0, 0);
            this.wizardPageSdrDecoder.Name = "wizardPageSdrDecoder";
            this.wizardPageSdrDecoder.Size = new System.Drawing.Size(569, 302);
            this.wizardPageSdrDecoder.TabIndex = 0;
            // 
            // radioPanelSdrDecoder
            // 
            this.radioPanelSdrDecoder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.radioPanelSdrDecoder.Controls.Add(this.radioButtonSdrFR24Feeder);
            this.radioPanelSdrDecoder.Controls.Add(this.radioButtonSdrAdsbSharp);
            this.radioPanelSdrDecoder.Controls.Add(this.radioButtonSdrOther);
            this.radioPanelSdrDecoder.Controls.Add(this.radioButtonSdrRtl1090);
            this.radioPanelSdrDecoder.Controls.Add(this.radioButtonSdrGrAirModes);
            this.radioPanelSdrDecoder.Controls.Add(this.radioButtonSdrDump1090);
            this.radioPanelSdrDecoder.Controls.Add(this.radioButtonSdrModesdeco);
            this.radioPanelSdrDecoder.Location = new System.Drawing.Point(15, 46);
            this.radioPanelSdrDecoder.Name = "radioPanelSdrDecoder";
            this.radioPanelSdrDecoder.Size = new System.Drawing.Size(542, 174);
            this.radioPanelSdrDecoder.TabIndex = 8;
            this.radioPanelSdrDecoder.ValueMember = null;
            // 
            // radioButtonSdrFR24Feeder
            // 
            this.radioButtonSdrFR24Feeder.AutoSize = true;
            this.radioButtonSdrFR24Feeder.Location = new System.Drawing.Point(3, 49);
            this.radioButtonSdrFR24Feeder.Name = "radioButtonSdrFR24Feeder";
            this.radioButtonSdrFR24Feeder.Size = new System.Drawing.Size(169, 17);
            this.radioButtonSdrFR24Feeder.TabIndex = 2;
            this.radioButtonSdrFR24Feeder.TabStop = true;
            this.radioButtonSdrFR24Feeder.Text = "::RecConWizSdrFR24Feeder::";
            this.radioButtonSdrFR24Feeder.UseVisualStyleBackColor = true;
            // 
            // radioButtonSdrAdsbSharp
            // 
            this.radioButtonSdrAdsbSharp.AutoSize = true;
            this.radioButtonSdrAdsbSharp.Location = new System.Drawing.Point(3, 3);
            this.radioButtonSdrAdsbSharp.Name = "radioButtonSdrAdsbSharp";
            this.radioButtonSdrAdsbSharp.Size = new System.Drawing.Size(162, 17);
            this.radioButtonSdrAdsbSharp.TabIndex = 0;
            this.radioButtonSdrAdsbSharp.Text = "::RecConWizSdrAdsbSharp::";
            this.radioButtonSdrAdsbSharp.UseVisualStyleBackColor = true;
            // 
            // radioButtonSdrOther
            // 
            this.radioButtonSdrOther.AutoSize = true;
            this.radioButtonSdrOther.Location = new System.Drawing.Point(3, 148);
            this.radioButtonSdrOther.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.radioButtonSdrOther.Name = "radioButtonSdrOther";
            this.radioButtonSdrOther.Size = new System.Drawing.Size(63, 17);
            this.radioButtonSdrOther.TabIndex = 6;
            this.radioButtonSdrOther.Text = "::Other::";
            this.radioButtonSdrOther.UseVisualStyleBackColor = true;
            // 
            // radioButtonSdrRtl1090
            // 
            this.radioButtonSdrRtl1090.AutoSize = true;
            this.radioButtonSdrRtl1090.Location = new System.Drawing.Point(3, 118);
            this.radioButtonSdrRtl1090.Name = "radioButtonSdrRtl1090";
            this.radioButtonSdrRtl1090.Size = new System.Drawing.Size(147, 17);
            this.radioButtonSdrRtl1090.TabIndex = 5;
            this.radioButtonSdrRtl1090.Text = "::RecConWizSdrRtl1090::";
            this.radioButtonSdrRtl1090.UseVisualStyleBackColor = true;
            // 
            // radioButtonSdrGrAirModes
            // 
            this.radioButtonSdrGrAirModes.AutoSize = true;
            this.radioButtonSdrGrAirModes.Location = new System.Drawing.Point(3, 72);
            this.radioButtonSdrGrAirModes.Name = "radioButtonSdrGrAirModes";
            this.radioButtonSdrGrAirModes.Size = new System.Drawing.Size(165, 17);
            this.radioButtonSdrGrAirModes.TabIndex = 3;
            this.radioButtonSdrGrAirModes.Text = "::RecConWizSdrGrAirModes::";
            this.radioButtonSdrGrAirModes.UseVisualStyleBackColor = true;
            // 
            // radioButtonSdrDump1090
            // 
            this.radioButtonSdrDump1090.AutoSize = true;
            this.radioButtonSdrDump1090.Location = new System.Drawing.Point(3, 26);
            this.radioButtonSdrDump1090.Name = "radioButtonSdrDump1090";
            this.radioButtonSdrDump1090.Size = new System.Drawing.Size(162, 17);
            this.radioButtonSdrDump1090.TabIndex = 1;
            this.radioButtonSdrDump1090.Text = "::RecConWizSdrDump1090::";
            this.radioButtonSdrDump1090.UseVisualStyleBackColor = true;
            // 
            // radioButtonSdrModesdeco
            // 
            this.radioButtonSdrModesdeco.AutoSize = true;
            this.radioButtonSdrModesdeco.Location = new System.Drawing.Point(3, 95);
            this.radioButtonSdrModesdeco.Name = "radioButtonSdrModesdeco";
            this.radioButtonSdrModesdeco.Size = new System.Drawing.Size(166, 17);
            this.radioButtonSdrModesdeco.TabIndex = 4;
            this.radioButtonSdrModesdeco.Text = "::RecConWizSdrModesdeco::";
            this.radioButtonSdrModesdeco.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(12, 20);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 20, 3, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(133, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "::RecConWizSdrTitle::";
            // 
            // wizardPageSdrOrDedicated
            // 
            this.wizardPageSdrOrDedicated.Controls.Add(this.radioPanelReceiverClass);
            this.wizardPageSdrOrDedicated.Controls.Add(this.label2);
            this.wizardPageSdrOrDedicated.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPageSdrOrDedicated.IsFinishPage = false;
            this.wizardPageSdrOrDedicated.Location = new System.Drawing.Point(0, 0);
            this.wizardPageSdrOrDedicated.Margin = new System.Windows.Forms.Padding(0);
            this.wizardPageSdrOrDedicated.Name = "wizardPageSdrOrDedicated";
            this.wizardPageSdrOrDedicated.Size = new System.Drawing.Size(569, 302);
            this.wizardPageSdrOrDedicated.TabIndex = 3;
            // 
            // radioPanelReceiverClass
            // 
            this.radioPanelReceiverClass.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.radioPanelReceiverClass.Controls.Add(this.radioButtonReceiverSdr);
            this.radioPanelReceiverClass.Controls.Add(this.radioButtonReceiverDedicated);
            this.radioPanelReceiverClass.Location = new System.Drawing.Point(15, 47);
            this.radioPanelReceiverClass.Name = "radioPanelReceiverClass";
            this.radioPanelReceiverClass.Size = new System.Drawing.Size(542, 51);
            this.radioPanelReceiverClass.TabIndex = 3;
            this.radioPanelReceiverClass.ValueMember = null;
            // 
            // radioButtonReceiverSdr
            // 
            this.radioButtonReceiverSdr.AutoSize = true;
            this.radioButtonReceiverSdr.Location = new System.Drawing.Point(3, 3);
            this.radioButtonReceiverSdr.Name = "radioButtonReceiverSdr";
            this.radioButtonReceiverSdr.Size = new System.Drawing.Size(178, 17);
            this.radioButtonReceiverSdr.TabIndex = 1;
            this.radioButtonReceiverSdr.Text = "::RecConWizReceiverClassSdr::";
            this.radioButtonReceiverSdr.UseVisualStyleBackColor = true;
            // 
            // radioButtonReceiverDedicated
            // 
            this.radioButtonReceiverDedicated.AutoSize = true;
            this.radioButtonReceiverDedicated.Location = new System.Drawing.Point(3, 26);
            this.radioButtonReceiverDedicated.Name = "radioButtonReceiverDedicated";
            this.radioButtonReceiverDedicated.Size = new System.Drawing.Size(211, 17);
            this.radioButtonReceiverDedicated.TabIndex = 2;
            this.radioButtonReceiverDedicated.Text = "::RecConWizReceiverClassDedicated::";
            this.radioButtonReceiverDedicated.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 20);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 20, 3, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(195, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "::RecConWizReceiverClassTitle::";
            // 
            // wizardPageFinish
            // 
            this.wizardPageFinish.Controls.Add(this.label10);
            this.wizardPageFinish.Controls.Add(this.label9);
            this.wizardPageFinish.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPageFinish.IsFinishPage = true;
            this.wizardPageFinish.Location = new System.Drawing.Point(0, 0);
            this.wizardPageFinish.Name = "wizardPageFinish";
            this.wizardPageFinish.Size = new System.Drawing.Size(569, 302);
            this.wizardPageFinish.TabIndex = 10;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 41);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(133, 13);
            this.label10.TabIndex = 10;
            this.label10.Text = "::RecConWizFinishAction::";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(12, 18);
            this.label9.Margin = new System.Windows.Forms.Padding(3, 20, 3, 10);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(147, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "::RecConWizFinishTitle::";
            // 
            // wizardPageNetworkAddress
            // 
            this.wizardPageNetworkAddress.Controls.Add(this.textBoxNetworkAddress);
            this.wizardPageNetworkAddress.Controls.Add(this.label8);
            this.wizardPageNetworkAddress.Controls.Add(this.labelNetworkAddressTitle);
            this.wizardPageNetworkAddress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPageNetworkAddress.IsFinishPage = false;
            this.wizardPageNetworkAddress.Location = new System.Drawing.Point(0, 0);
            this.wizardPageNetworkAddress.Name = "wizardPageNetworkAddress";
            this.wizardPageNetworkAddress.Size = new System.Drawing.Size(569, 302);
            this.wizardPageNetworkAddress.TabIndex = 9;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 49);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(45, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "::UNC:::";
            // 
            // labelNetworkAddressTitle
            // 
            this.labelNetworkAddressTitle.AutoSize = true;
            this.labelNetworkAddressTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNetworkAddressTitle.Location = new System.Drawing.Point(12, 20);
            this.labelNetworkAddressTitle.Margin = new System.Windows.Forms.Padding(3, 20, 3, 10);
            this.labelNetworkAddressTitle.Name = "labelNetworkAddressTitle";
            this.labelNetworkAddressTitle.Size = new System.Drawing.Size(316, 13);
            this.labelNetworkAddressTitle.TabIndex = 8;
            this.labelNetworkAddressTitle.Text = "Enter the network address for <INSERT NAME HERE>";
            // 
            // wizardPageLoopback
            // 
            this.wizardPageLoopback.Controls.Add(this.radioPanelLoopback);
            this.wizardPageLoopback.Controls.Add(this.labelLoopbackTitle);
            this.wizardPageLoopback.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPageLoopback.IsFinishPage = false;
            this.wizardPageLoopback.Location = new System.Drawing.Point(0, 0);
            this.wizardPageLoopback.Name = "wizardPageLoopback";
            this.wizardPageLoopback.Size = new System.Drawing.Size(569, 302);
            this.wizardPageLoopback.TabIndex = 8;
            // 
            // radioPanelLoopback
            // 
            this.radioPanelLoopback.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.radioPanelLoopback.Controls.Add(this.radioButtonLoopbackNo);
            this.radioPanelLoopback.Controls.Add(this.radioButtonLoopbackYes);
            this.radioPanelLoopback.Location = new System.Drawing.Point(13, 47);
            this.radioPanelLoopback.Name = "radioPanelLoopback";
            this.radioPanelLoopback.Size = new System.Drawing.Size(544, 50);
            this.radioPanelLoopback.TabIndex = 8;
            this.radioPanelLoopback.ValueMember = null;
            // 
            // radioButtonLoopbackNo
            // 
            this.radioButtonLoopbackNo.AutoSize = true;
            this.radioButtonLoopbackNo.Location = new System.Drawing.Point(4, 27);
            this.radioButtonLoopbackNo.Name = "radioButtonLoopbackNo";
            this.radioButtonLoopbackNo.Size = new System.Drawing.Size(51, 17);
            this.radioButtonLoopbackNo.TabIndex = 1;
            this.radioButtonLoopbackNo.TabStop = true;
            this.radioButtonLoopbackNo.Text = "::No::";
            this.radioButtonLoopbackNo.UseVisualStyleBackColor = true;
            // 
            // radioButtonLoopbackYes
            // 
            this.radioButtonLoopbackYes.AutoSize = true;
            this.radioButtonLoopbackYes.Location = new System.Drawing.Point(4, 4);
            this.radioButtonLoopbackYes.Name = "radioButtonLoopbackYes";
            this.radioButtonLoopbackYes.Size = new System.Drawing.Size(55, 17);
            this.radioButtonLoopbackYes.TabIndex = 0;
            this.radioButtonLoopbackYes.TabStop = true;
            this.radioButtonLoopbackYes.Text = "::Yes::";
            this.radioButtonLoopbackYes.UseVisualStyleBackColor = true;
            // 
            // labelLoopbackTitle
            // 
            this.labelLoopbackTitle.AutoSize = true;
            this.labelLoopbackTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLoopbackTitle.Location = new System.Drawing.Point(12, 20);
            this.labelLoopbackTitle.Margin = new System.Windows.Forms.Padding(3, 20, 3, 10);
            this.labelLoopbackTitle.Name = "labelLoopbackTitle";
            this.labelLoopbackTitle.Size = new System.Drawing.Size(309, 13);
            this.labelLoopbackTitle.TabIndex = 7;
            this.labelLoopbackTitle.Text = "Is <INSERT NAME HERE> running on this computer?";
            // 
            // wizardPageKineticConnection
            // 
            this.wizardPageKineticConnection.Controls.Add(this.label7);
            this.wizardPageKineticConnection.Controls.Add(this.radioPanelKineticConnection);
            this.wizardPageKineticConnection.Controls.Add(this.label6);
            this.wizardPageKineticConnection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPageKineticConnection.IsFinishPage = false;
            this.wizardPageKineticConnection.Location = new System.Drawing.Point(0, 0);
            this.wizardPageKineticConnection.Name = "wizardPageKineticConnection";
            this.wizardPageKineticConnection.Size = new System.Drawing.Size(569, 302);
            this.wizardPageKineticConnection.TabIndex = 7;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label7.Location = new System.Drawing.Point(12, 105);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(545, 181);
            this.label7.TabIndex = 2;
            this.label7.Text = "::RecConWizKineticConNote::";
            // 
            // radioPanelKineticConnection
            // 
            this.radioPanelKineticConnection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.radioPanelKineticConnection.Controls.Add(this.radioButtonKineticConnectHardware);
            this.radioPanelKineticConnection.Controls.Add(this.radioButtonKineticConnectBaseStation);
            this.radioPanelKineticConnection.Location = new System.Drawing.Point(15, 47);
            this.radioPanelKineticConnection.Name = "radioPanelKineticConnection";
            this.radioPanelKineticConnection.Size = new System.Drawing.Size(542, 55);
            this.radioPanelKineticConnection.TabIndex = 7;
            this.radioPanelKineticConnection.ValueMember = null;
            // 
            // radioButtonKineticConnectHardware
            // 
            this.radioButtonKineticConnectHardware.AutoSize = true;
            this.radioButtonKineticConnectHardware.Location = new System.Drawing.Point(4, 27);
            this.radioButtonKineticConnectHardware.Name = "radioButtonKineticConnectHardware";
            this.radioButtonKineticConnectHardware.Size = new System.Drawing.Size(173, 17);
            this.radioButtonKineticConnectHardware.TabIndex = 1;
            this.radioButtonKineticConnectHardware.TabStop = true;
            this.radioButtonKineticConnectHardware.Text = "::RecConWizKineticConDirect::";
            this.radioButtonKineticConnectHardware.UseVisualStyleBackColor = true;
            // 
            // radioButtonKineticConnectBaseStation
            // 
            this.radioButtonKineticConnectBaseStation.AutoSize = true;
            this.radioButtonKineticConnectBaseStation.Location = new System.Drawing.Point(4, 4);
            this.radioButtonKineticConnectBaseStation.Name = "radioButtonKineticConnectBaseStation";
            this.radioButtonKineticConnectBaseStation.Size = new System.Drawing.Size(55, 17);
            this.radioButtonKineticConnectBaseStation.TabIndex = 0;
            this.radioButtonKineticConnectBaseStation.TabStop = true;
            this.radioButtonKineticConnectBaseStation.Text = "::Yes::";
            this.radioButtonKineticConnectBaseStation.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(12, 20);
            this.label6.Margin = new System.Windows.Forms.Padding(3, 20, 3, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(175, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "::RecConWizKineticConTitle::";
            // 
            // wizardPageConnectionType
            // 
            this.wizardPageConnectionType.Controls.Add(this.radioPanelConnectionType);
            this.wizardPageConnectionType.Controls.Add(this.label5);
            this.wizardPageConnectionType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPageConnectionType.IsFinishPage = false;
            this.wizardPageConnectionType.Location = new System.Drawing.Point(0, 0);
            this.wizardPageConnectionType.Name = "wizardPageConnectionType";
            this.wizardPageConnectionType.Size = new System.Drawing.Size(569, 302);
            this.wizardPageConnectionType.TabIndex = 6;
            // 
            // radioPanelConnectionType
            // 
            this.radioPanelConnectionType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.radioPanelConnectionType.Controls.Add(this.radioButtonConnectionTypeNetwork);
            this.radioPanelConnectionType.Controls.Add(this.radioButtonConnectionTypeUsb);
            this.radioPanelConnectionType.Location = new System.Drawing.Point(15, 46);
            this.radioPanelConnectionType.Name = "radioPanelConnectionType";
            this.radioPanelConnectionType.Size = new System.Drawing.Size(542, 58);
            this.radioPanelConnectionType.TabIndex = 8;
            this.radioPanelConnectionType.ValueMember = null;
            // 
            // radioButtonConnectionTypeNetwork
            // 
            this.radioButtonConnectionTypeNetwork.AutoSize = true;
            this.radioButtonConnectionTypeNetwork.Location = new System.Drawing.Point(3, 3);
            this.radioButtonConnectionTypeNetwork.Name = "radioButtonConnectionTypeNetwork";
            this.radioButtonConnectionTypeNetwork.Size = new System.Drawing.Size(77, 17);
            this.radioButtonConnectionTypeNetwork.TabIndex = 6;
            this.radioButtonConnectionTypeNetwork.Text = "::Network::";
            this.radioButtonConnectionTypeNetwork.UseVisualStyleBackColor = true;
            // 
            // radioButtonConnectionTypeUsb
            // 
            this.radioButtonConnectionTypeUsb.AutoSize = true;
            this.radioButtonConnectionTypeUsb.Location = new System.Drawing.Point(3, 26);
            this.radioButtonConnectionTypeUsb.Name = "radioButtonConnectionTypeUsb";
            this.radioButtonConnectionTypeUsb.Size = new System.Drawing.Size(59, 17);
            this.radioButtonConnectionTypeUsb.TabIndex = 7;
            this.radioButtonConnectionTypeUsb.Text = "::USB::";
            this.radioButtonConnectionTypeUsb.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(12, 20);
            this.label5.Margin = new System.Windows.Forms.Padding(3, 20, 3, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(206, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "::RecConWizConnectionTypeTitle::";
            // 
            // radioButtonDedicatedAirnavXRange
            // 
            this.radioButtonDedicatedAirnavXRange.AutoSize = true;
            this.radioButtonDedicatedAirnavXRange.Location = new System.Drawing.Point(3, 118);
            this.radioButtonDedicatedAirnavXRange.Name = "radioButtonDedicatedAirnavXRange";
            this.radioButtonDedicatedAirnavXRange.Size = new System.Drawing.Size(106, 17);
            this.radioButtonDedicatedAirnavXRange.TabIndex = 6;
            this.radioButtonDedicatedAirnavXRange.Text = "::AirnavXRange::";
            this.radioButtonDedicatedAirnavXRange.UseVisualStyleBackColor = true;
            // 
            // wizardPageWebAddress
            // 
            this.wizardPageWebAddress.Controls.Add(this.textBoxWebAddress);
            this.wizardPageWebAddress.Controls.Add(this.label11);
            this.wizardPageWebAddress.Controls.Add(this.labelWebAddressTitle);
            this.wizardPageWebAddress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPageWebAddress.IsFinishPage = false;
            this.wizardPageWebAddress.Location = new System.Drawing.Point(0, 0);
            this.wizardPageWebAddress.Name = "wizardPageWebAddress";
            this.wizardPageWebAddress.Size = new System.Drawing.Size(569, 302);
            this.wizardPageWebAddress.TabIndex = 11;
            // 
            // textBoxWebAddress
            // 
            this.errorProvider.SetIconAlignment(this.textBoxWebAddress, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.textBoxWebAddress.Location = new System.Drawing.Point(141, 46);
            this.textBoxWebAddress.MaxLength = 256;
            this.textBoxWebAddress.Name = "textBoxWebAddress";
            this.textBoxWebAddress.Size = new System.Drawing.Size(416, 20);
            this.textBoxWebAddress.TabIndex = 13;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(12, 49);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(83, 13);
            this.label11.TabIndex = 12;
            this.label11.Text = "::WebAddress:::";
            // 
            // labelWebAddressTitle
            // 
            this.labelWebAddressTitle.AutoSize = true;
            this.labelWebAddressTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWebAddressTitle.Location = new System.Drawing.Point(12, 20);
            this.labelWebAddressTitle.Margin = new System.Windows.Forms.Padding(3, 20, 3, 10);
            this.labelWebAddressTitle.Name = "labelWebAddressTitle";
            this.labelWebAddressTitle.Size = new System.Drawing.Size(294, 13);
            this.labelWebAddressTitle.TabIndex = 11;
            this.labelWebAddressTitle.Text = "Enter the web address for <INSERT NAME HERE>";
            // 
            // ReceiverConfigurationWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.ClientSize = new System.Drawing.Size(569, 406);
            this.Controls.Add(this.panelBorder);
            this.Controls.Add(this.wizard);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ReceiverConfigurationWizard";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "::ReceiverConfigurationWizard::";
            this.panelBorder.ResumeLayout(false);
            this.panelHeading.ResumeLayout(false);
            this.panelHeading.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.wizard.ResumeLayout(false);
            this.wizardPageDedicatedReceiver.ResumeLayout(false);
            this.wizardPageDedicatedReceiver.PerformLayout();
            this.radioPanelDedicatedReceiver.ResumeLayout(false);
            this.radioPanelDedicatedReceiver.PerformLayout();
            this.wizardPageSdrDecoder.ResumeLayout(false);
            this.wizardPageSdrDecoder.PerformLayout();
            this.radioPanelSdrDecoder.ResumeLayout(false);
            this.radioPanelSdrDecoder.PerformLayout();
            this.wizardPageSdrOrDedicated.ResumeLayout(false);
            this.wizardPageSdrOrDedicated.PerformLayout();
            this.radioPanelReceiverClass.ResumeLayout(false);
            this.radioPanelReceiverClass.PerformLayout();
            this.wizardPageFinish.ResumeLayout(false);
            this.wizardPageFinish.PerformLayout();
            this.wizardPageNetworkAddress.ResumeLayout(false);
            this.wizardPageNetworkAddress.PerformLayout();
            this.wizardPageLoopback.ResumeLayout(false);
            this.wizardPageLoopback.PerformLayout();
            this.radioPanelLoopback.ResumeLayout(false);
            this.radioPanelLoopback.PerformLayout();
            this.wizardPageKineticConnection.ResumeLayout(false);
            this.wizardPageKineticConnection.PerformLayout();
            this.radioPanelKineticConnection.ResumeLayout(false);
            this.radioPanelKineticConnection.PerformLayout();
            this.wizardPageConnectionType.ResumeLayout(false);
            this.wizardPageConnectionType.PerformLayout();
            this.radioPanelConnectionType.ResumeLayout(false);
            this.radioPanelConnectionType.PerformLayout();
            this.wizardPageWebAddress.ResumeLayout(false);
            this.wizardPageWebAddress.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Gui.Wizard.Wizard wizard;
        private Gui.Wizard.WizardPage wizardPageSdrOrDedicated;
        private System.Windows.Forms.RadioButton radioButtonReceiverDedicated;
        private System.Windows.Forms.RadioButton radioButtonReceiverSdr;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panelBorder;
        private System.Windows.Forms.Panel panelHeading;
        private System.Windows.Forms.Label label1;
        private Gui.Wizard.WizardPage wizardPageSdrDecoder;
        private System.Windows.Forms.Label label3;
        private Gui.Wizard.WizardPage wizardPageDedicatedReceiver;
        private System.Windows.Forms.RadioButton radioButtonDedicatedOther;
        private System.Windows.Forms.RadioButton radioButtonDedicatedMicroAdsb;
        private System.Windows.Forms.RadioButton radioButtonDedicatedBeast;
        private System.Windows.Forms.RadioButton radioButtonDedicatedRadarBox;
        private System.Windows.Forms.RadioButton radioButtonDedicatedKinetics;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton radioButtonSdrOther;
        private System.Windows.Forms.RadioButton radioButtonSdrGrAirModes;
        private System.Windows.Forms.RadioButton radioButtonSdrModesdeco;
        private System.Windows.Forms.RadioButton radioButtonSdrDump1090;
        private System.Windows.Forms.RadioButton radioButtonSdrAdsbSharp;
        private System.Windows.Forms.RadioButton radioButtonSdrRtl1090;
        private Gui.Wizard.WizardPage wizardPageConnectionType;
        private System.Windows.Forms.RadioButton radioButtonConnectionTypeUsb;
        private System.Windows.Forms.RadioButton radioButtonConnectionTypeNetwork;
        private System.Windows.Forms.Label label5;
        private RadioPanelApp.RadioPanel radioPanelReceiverClass;
        private RadioPanelApp.RadioPanel radioPanelSdrDecoder;
        private RadioPanelApp.RadioPanel radioPanelConnectionType;
        private RadioPanelApp.RadioPanel radioPanelDedicatedReceiver;
        private Gui.Wizard.WizardPage wizardPageKineticConnection;
        private System.Windows.Forms.Label label7;
        private RadioPanelApp.RadioPanel radioPanelKineticConnection;
        private System.Windows.Forms.RadioButton radioButtonKineticConnectHardware;
        private System.Windows.Forms.RadioButton radioButtonKineticConnectBaseStation;
        private System.Windows.Forms.Label label6;
        private Gui.Wizard.WizardPage wizardPageLoopback;
        private RadioPanelApp.RadioPanel radioPanelLoopback;
        private System.Windows.Forms.RadioButton radioButtonLoopbackNo;
        private System.Windows.Forms.RadioButton radioButtonLoopbackYes;
        private System.Windows.Forms.Label labelLoopbackTitle;
        private Gui.Wizard.WizardPage wizardPageNetworkAddress;
        private System.Windows.Forms.Label labelNetworkAddressTitle;
        private System.Windows.Forms.TextBox textBoxNetworkAddress;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private Gui.Wizard.WizardPage wizardPageFinish;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.PictureBox pictureBoxLogo;
        private System.Windows.Forms.RadioButton radioButtonSdrFR24Feeder;
        private System.Windows.Forms.RadioButton radioButtonDedicatedPlaneFinderRadar;
        private System.Windows.Forms.RadioButton radioButtonDedicatedAirnavXRange;
        private Gui.Wizard.WizardPage wizardPageWebAddress;
        private System.Windows.Forms.TextBox textBoxWebAddress;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label labelWebAddressTitle;
    }
}