namespace VirtualRadar.Plugin.BaseStationDatabaseWriter.WinForms
{
    partial class OptionsView
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
            this.checkBoxEnabled = new System.Windows.Forms.CheckBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.linkLabelUseDefaultFileName = new System.Windows.Forms.LinkLabel();
            this.buttonCreateDatabase = new System.Windows.Forms.Button();
            this.checkBoxOnlyUpdateDatabasesCreatedByPlugin = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.fileNameDatabase = new VirtualRadar.WinForms.Controls.FileNameControl();
            this.comboBoxReceiverId = new System.Windows.Forms.ComboBox();
            this.checkBoxWriteOnlineLookupsToDatabase = new System.Windows.Forms.CheckBox();
            this.labelWriteOnlineLookupsNotice = new System.Windows.Forms.Label();
            this.checkBoxRefreshOutOfDateAircraft = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // checkBoxEnabled
            // 
            this.checkBoxEnabled.AutoSize = true;
            this.checkBoxEnabled.Location = new System.Drawing.Point(148, 13);
            this.checkBoxEnabled.Name = "checkBoxEnabled";
            this.checkBoxEnabled.Size = new System.Drawing.Size(77, 17);
            this.checkBoxEnabled.TabIndex = 0;
            this.checkBoxEnabled.Text = "::Enabled::";
            this.checkBoxEnabled.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(377, 236);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 11;
            this.buttonOK.Text = "::OK::";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(458, 236);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "::Cancel::";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "::DatabaseFileName:::";
            // 
            // linkLabelUseDefaultFileName
            // 
            this.linkLabelUseDefaultFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelUseDefaultFileName.Location = new System.Drawing.Point(148, 110);
            this.linkLabelUseDefaultFileName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.linkLabelUseDefaultFileName.Name = "linkLabelUseDefaultFileName";
            this.linkLabelUseDefaultFileName.Size = new System.Drawing.Size(351, 13);
            this.linkLabelUseDefaultFileName.TabIndex = 6;
            this.linkLabelUseDefaultFileName.TabStop = true;
            this.linkLabelUseDefaultFileName.Text = "::UseDefaultFileName::";
            this.linkLabelUseDefaultFileName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.linkLabelUseDefaultFileName.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelUseDefaultFileName_LinkClicked);
            // 
            // buttonCreateDatabase
            // 
            this.buttonCreateDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCreateDatabase.Location = new System.Drawing.Point(148, 236);
            this.buttonCreateDatabase.Name = "buttonCreateDatabase";
            this.buttonCreateDatabase.Size = new System.Drawing.Size(147, 23);
            this.buttonCreateDatabase.TabIndex = 10;
            this.buttonCreateDatabase.Text = "::CreateDatabase::";
            this.buttonCreateDatabase.UseVisualStyleBackColor = true;
            this.buttonCreateDatabase.Click += new System.EventHandler(this.buttonCreateDatabase_Click);
            // 
            // checkBoxOnlyUpdateDatabasesCreatedByPlugin
            // 
            this.checkBoxOnlyUpdateDatabasesCreatedByPlugin.AutoSize = true;
            this.checkBoxOnlyUpdateDatabasesCreatedByPlugin.Location = new System.Drawing.Point(148, 36);
            this.checkBoxOnlyUpdateDatabasesCreatedByPlugin.Name = "checkBoxOnlyUpdateDatabasesCreatedByPlugin";
            this.checkBoxOnlyUpdateDatabasesCreatedByPlugin.Size = new System.Drawing.Size(223, 17);
            this.checkBoxOnlyUpdateDatabasesCreatedByPlugin.TabIndex = 1;
            this.checkBoxOnlyUpdateDatabasesCreatedByPlugin.Text = "::OnlyUpdateDatabasesCreatedByPlugin::";
            this.checkBoxOnlyUpdateDatabasesCreatedByPlugin.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "::Receiver:::";
            // 
            // fileNameDatabase
            // 
            this.fileNameDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileNameDatabase.BrowserDefaultExt = ".sqb";
            this.fileNameDatabase.BrowserFilter = "SQLite database files (*.sqb)|*.sqb|All files (*.*)|*.*";
            this.fileNameDatabase.BrowserTitle = "";
            this.fileNameDatabase.Location = new System.Drawing.Point(148, 87);
            this.fileNameDatabase.Name = "fileNameDatabase";
            this.fileNameDatabase.Size = new System.Drawing.Size(385, 20);
            this.fileNameDatabase.TabIndex = 5;
            // 
            // comboBoxReceiverId
            // 
            this.comboBoxReceiverId.DisplayMember = "Name";
            this.comboBoxReceiverId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxReceiverId.Location = new System.Drawing.Point(148, 59);
            this.comboBoxReceiverId.Name = "comboBoxReceiverId";
            this.comboBoxReceiverId.Size = new System.Drawing.Size(150, 21);
            this.comboBoxReceiverId.TabIndex = 3;
            this.comboBoxReceiverId.ValueMember = "Value";
            // 
            // checkBoxWriteOnlineLookupsToDatabase
            // 
            this.checkBoxWriteOnlineLookupsToDatabase.AutoSize = true;
            this.checkBoxWriteOnlineLookupsToDatabase.Location = new System.Drawing.Point(148, 129);
            this.checkBoxWriteOnlineLookupsToDatabase.Name = "checkBoxWriteOnlineLookupsToDatabase";
            this.checkBoxWriteOnlineLookupsToDatabase.Size = new System.Drawing.Size(193, 17);
            this.checkBoxWriteOnlineLookupsToDatabase.TabIndex = 7;
            this.checkBoxWriteOnlineLookupsToDatabase.Text = "::WriteOnlineLookupsToDatabase::";
            this.checkBoxWriteOnlineLookupsToDatabase.UseVisualStyleBackColor = true;
            this.checkBoxWriteOnlineLookupsToDatabase.CheckedChanged += new System.EventHandler(this.checkBoxWriteOnlineLookupsToDatabase_CheckedChanged);
            // 
            // labelWriteOnlineLookupsNotice
            // 
            this.labelWriteOnlineLookupsNotice.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWriteOnlineLookupsNotice.Location = new System.Drawing.Point(148, 172);
            this.labelWriteOnlineLookupsNotice.Name = "labelWriteOnlineLookupsNotice";
            this.labelWriteOnlineLookupsNotice.Size = new System.Drawing.Size(385, 61);
            this.labelWriteOnlineLookupsNotice.TabIndex = 9;
            this.labelWriteOnlineLookupsNotice.Text = "::WriteOnlineLookupsNoticeAllAircraft::";
            // 
            // checkBoxRefreshOutOfDateAircraft
            // 
            this.checkBoxRefreshOutOfDateAircraft.AutoSize = true;
            this.checkBoxRefreshOutOfDateAircraft.Location = new System.Drawing.Point(148, 152);
            this.checkBoxRefreshOutOfDateAircraft.Name = "checkBoxRefreshOutOfDateAircraft";
            this.checkBoxRefreshOutOfDateAircraft.Size = new System.Drawing.Size(142, 17);
            this.checkBoxRefreshOutOfDateAircraft.TabIndex = 8;
            this.checkBoxRefreshOutOfDateAircraft.Text = "::UpdateExistingAircraft::";
            this.checkBoxRefreshOutOfDateAircraft.UseVisualStyleBackColor = true;
            // 
            // OptionsView
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(545, 271);
            this.Controls.Add(this.checkBoxRefreshOutOfDateAircraft);
            this.Controls.Add(this.labelWriteOnlineLookupsNotice);
            this.Controls.Add(this.checkBoxWriteOnlineLookupsToDatabase);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.fileNameDatabase);
            this.Controls.Add(this.checkBoxOnlyUpdateDatabasesCreatedByPlugin);
            this.Controls.Add(this.buttonCreateDatabase);
            this.Controls.Add(this.linkLabelUseDefaultFileName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.checkBoxEnabled);
            this.Controls.Add(this.comboBoxReceiverId);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsView";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "::OptionsViewTitle::";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxEnabled;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkLabelUseDefaultFileName;
        private System.Windows.Forms.Button buttonCreateDatabase;
        private System.Windows.Forms.CheckBox checkBoxOnlyUpdateDatabasesCreatedByPlugin;
        private VirtualRadar.WinForms.Controls.FileNameControl fileNameDatabase;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxReceiverId;
        private System.Windows.Forms.CheckBox checkBoxWriteOnlineLookupsToDatabase;
        private System.Windows.Forms.Label labelWriteOnlineLookupsNotice;
        private System.Windows.Forms.CheckBox checkBoxRefreshOutOfDateAircraft;
    }
}