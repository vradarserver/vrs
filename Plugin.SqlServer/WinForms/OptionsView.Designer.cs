namespace VirtualRadar.Plugin.SqlServer.WinForms
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxConnectionString = new System.Windows.Forms.TextBox();
            this.buttonTestConnection = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonUpdateSchema = new System.Windows.Forms.Button();
            this.linkLabelOpenUpdateSchemaSql = new System.Windows.Forms.LinkLabel();
            this.linkLabelImportFromBaseStation = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // checkBoxEnabled
            // 
            this.checkBoxEnabled.AutoSize = true;
            this.checkBoxEnabled.Location = new System.Drawing.Point(172, 12);
            this.checkBoxEnabled.Name = "checkBoxEnabled";
            this.checkBoxEnabled.Size = new System.Drawing.Size(77, 17);
            this.checkBoxEnabled.TabIndex = 0;
            this.checkBoxEnabled.Text = "::Enabled::";
            this.checkBoxEnabled.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(624, 110);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "::Cancel::";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(543, 110);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 7;
            this.buttonOK.Text = "::OK::";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "::ConnectionString:::";
            // 
            // textBoxConnectionString
            // 
            this.textBoxConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxConnectionString.Location = new System.Drawing.Point(172, 35);
            this.textBoxConnectionString.Name = "textBoxConnectionString";
            this.textBoxConnectionString.Size = new System.Drawing.Size(365, 20);
            this.textBoxConnectionString.TabIndex = 2;
            // 
            // buttonTestConnection
            // 
            this.buttonTestConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTestConnection.Location = new System.Drawing.Point(543, 33);
            this.buttonTestConnection.Name = "buttonTestConnection";
            this.buttonTestConnection.Size = new System.Drawing.Size(156, 23);
            this.buttonTestConnection.TabIndex = 3;
            this.buttonTestConnection.Text = "::TestConnection::";
            this.buttonTestConnection.UseVisualStyleBackColor = true;
            this.buttonTestConnection.Click += new System.EventHandler(this.ButtonTestConnection_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 65);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 6, 3, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(687, 30);
            this.label2.TabIndex = 5;
            this.label2.Text = "::OptionChangesNeedARestart::";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonUpdateSchema
            // 
            this.buttonUpdateSchema.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonUpdateSchema.Location = new System.Drawing.Point(15, 110);
            this.buttonUpdateSchema.Name = "buttonUpdateSchema";
            this.buttonUpdateSchema.Size = new System.Drawing.Size(156, 23);
            this.buttonUpdateSchema.TabIndex = 6;
            this.buttonUpdateSchema.Text = "::UpdateSchema::";
            this.buttonUpdateSchema.UseVisualStyleBackColor = true;
            this.buttonUpdateSchema.Click += new System.EventHandler(this.ButtonUpdateSchema_Click);
            // 
            // linkLabelOpenUpdateSchemaSql
            // 
            this.linkLabelOpenUpdateSchemaSql.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelOpenUpdateSchemaSql.AutoSize = true;
            this.linkLabelOpenUpdateSchemaSql.Location = new System.Drawing.Point(177, 115);
            this.linkLabelOpenUpdateSchemaSql.Name = "linkLabelOpenUpdateSchemaSql";
            this.linkLabelOpenUpdateSchemaSql.Size = new System.Drawing.Size(135, 13);
            this.linkLabelOpenUpdateSchemaSql.TabIndex = 9;
            this.linkLabelOpenUpdateSchemaSql.TabStop = true;
            this.linkLabelOpenUpdateSchemaSql.Text = "::OpenUpdateSchemaFile::";
            this.linkLabelOpenUpdateSchemaSql.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelOpenUpdateSchemaSql_LinkClicked);
            // 
            // linkLabelImportFromBaseStation
            // 
            this.linkLabelImportFromBaseStation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelImportFromBaseStation.AutoSize = true;
            this.linkLabelImportFromBaseStation.Location = new System.Drawing.Point(318, 115);
            this.linkLabelImportFromBaseStation.Name = "linkLabelImportFromBaseStation";
            this.linkLabelImportFromBaseStation.Size = new System.Drawing.Size(128, 13);
            this.linkLabelImportFromBaseStation.TabIndex = 10;
            this.linkLabelImportFromBaseStation.TabStop = true;
            this.linkLabelImportFromBaseStation.Text = "::ImportFromBaseStation::";
            this.linkLabelImportFromBaseStation.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelImportFromBaseStation_LinkClicked);
            // 
            // OptionsView
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(711, 145);
            this.Controls.Add(this.linkLabelImportFromBaseStation);
            this.Controls.Add(this.linkLabelOpenUpdateSchemaSql);
            this.Controls.Add(this.buttonUpdateSchema);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonTestConnection);
            this.Controls.Add(this.textBoxConnectionString);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.checkBoxEnabled);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsView";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "::PluginName::";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxEnabled;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxConnectionString;
        private System.Windows.Forms.Button buttonTestConnection;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonUpdateSchema;
        private System.Windows.Forms.LinkLabel linkLabelOpenUpdateSchemaSql;
        private System.Windows.Forms.LinkLabel linkLabelImportFromBaseStation;
    }
}
