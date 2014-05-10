namespace VirtualRadar.Plugin.CustomContent.WinForms
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
            this.components = new System.ComponentModel.Container();
            this.checkBoxPluginViewEnabled = new System.Windows.Forms.CheckBox();
            this.folderControlSiteRootFolder = new VirtualRadar.WinForms.Controls.FolderControl();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.comboBoxInjectAt = new System.Windows.Forms.ComboBox();
            this.fileNameControlInjectFile = new VirtualRadar.WinForms.Controls.FileNameControl();
            this.textBoxInjectPathAndFile = new System.Windows.Forms.TextBox();
            this.comboBoxInjectOf = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxInjectEnabled = new System.Windows.Forms.CheckBox();
            this.buttonReset = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.listViewInjectSettings = new System.Windows.Forms.ListView();
            this.columnHeaderEnabled = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderAt = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderOf = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPathAndFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonNew = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBoxPluginViewEnabled
            // 
            this.checkBoxPluginViewEnabled.AutoSize = true;
            this.checkBoxPluginViewEnabled.Location = new System.Drawing.Point(12, 12);
            this.checkBoxPluginViewEnabled.Name = "checkBoxPluginViewEnabled";
            this.checkBoxPluginViewEnabled.Size = new System.Drawing.Size(77, 17);
            this.checkBoxPluginViewEnabled.TabIndex = 0;
            this.checkBoxPluginViewEnabled.Text = "::Enabled::";
            this.checkBoxPluginViewEnabled.UseVisualStyleBackColor = true;
            // 
            // folderControlSiteRootFolder
            // 
            this.folderControlSiteRootFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.errorProvider.SetIconAlignment(this.folderControlSiteRootFolder, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.folderControlSiteRootFolder.Location = new System.Drawing.Point(122, 19);
            this.folderControlSiteRootFolder.Name = "folderControlSiteRootFolder";
            this.folderControlSiteRootFolder.Size = new System.Drawing.Size(524, 20);
            this.folderControlSiteRootFolder.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "::SiteRootLabel:::";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(508, 417);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "::OK::";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(589, 417);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "::Cancel::";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // errorProvider
            // 
            this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider.ContainerControl = this;
            // 
            // comboBoxInjectAt
            // 
            this.comboBoxInjectAt.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxInjectAt.FormattingEnabled = true;
            this.errorProvider.SetIconAlignment(this.comboBoxInjectAt, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.comboBoxInjectAt.Location = new System.Drawing.Point(122, 45);
            this.comboBoxInjectAt.Name = "comboBoxInjectAt";
            this.comboBoxInjectAt.Size = new System.Drawing.Size(121, 21);
            this.comboBoxInjectAt.TabIndex = 3;
            // 
            // fileNameControlInjectFile
            // 
            this.fileNameControlInjectFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileNameControlInjectFile.BrowserAddExtension = false;
            this.fileNameControlInjectFile.BrowserCheckFileExists = true;
            this.fileNameControlInjectFile.BrowserFilter = "All files (*.*)|*.*|JavaScript files (*.js)|*.js|HTML files (*.html)|*.html";
            this.errorProvider.SetIconAlignment(this.fileNameControlInjectFile, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.fileNameControlInjectFile.Location = new System.Drawing.Point(122, 19);
            this.fileNameControlInjectFile.Name = "fileNameControlInjectFile";
            this.fileNameControlInjectFile.Size = new System.Drawing.Size(524, 20);
            this.fileNameControlInjectFile.TabIndex = 1;
            // 
            // textBoxInjectPathAndFile
            // 
            this.textBoxInjectPathAndFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.errorProvider.SetIconAlignment(this.textBoxInjectPathAndFile, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.textBoxInjectPathAndFile.Location = new System.Drawing.Point(122, 72);
            this.textBoxInjectPathAndFile.Name = "textBoxInjectPathAndFile";
            this.textBoxInjectPathAndFile.Size = new System.Drawing.Size(524, 20);
            this.textBoxInjectPathAndFile.TabIndex = 7;
            // 
            // comboBoxInjectOf
            // 
            this.comboBoxInjectOf.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxInjectOf.FormattingEnabled = true;
            this.errorProvider.SetIconAlignment(this.comboBoxInjectOf, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.comboBoxInjectOf.Location = new System.Drawing.Point(350, 45);
            this.comboBoxInjectOf.Name = "comboBoxInjectOf";
            this.comboBoxInjectOf.Size = new System.Drawing.Size(121, 21);
            this.comboBoxInjectOf.TabIndex = 5;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBoxInjectEnabled);
            this.groupBox1.Controls.Add(this.buttonReset);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.comboBoxInjectOf);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.comboBoxInjectAt);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.fileNameControlInjectFile);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBoxInjectPathAndFile);
            this.groupBox1.Location = new System.Drawing.Point(12, 214);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(652, 129);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "::InjectHtml::";
            // 
            // checkBoxInjectEnabled
            // 
            this.checkBoxInjectEnabled.AutoSize = true;
            this.checkBoxInjectEnabled.Location = new System.Drawing.Point(122, 102);
            this.checkBoxInjectEnabled.Name = "checkBoxInjectEnabled";
            this.checkBoxInjectEnabled.Size = new System.Drawing.Size(77, 17);
            this.checkBoxInjectEnabled.TabIndex = 9;
            this.checkBoxInjectEnabled.Text = "::Enabled::";
            this.checkBoxInjectEnabled.UseVisualStyleBackColor = true;
            // 
            // buttonReset
            // 
            this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReset.Location = new System.Drawing.Point(571, 98);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(75, 23);
            this.buttonReset.TabIndex = 8;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(269, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(33, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "::Of:::";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "::At:::";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "::InjectFile:::";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "::Address:::";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.folderControlSiteRootFolder);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(12, 349);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(652, 53);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "::ReplaceAndAddSiteContent::";
            // 
            // listViewInjectSettings
            // 
            this.listViewInjectSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewInjectSettings.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderEnabled,
            this.columnHeaderFileName,
            this.columnHeaderAt,
            this.columnHeaderOf,
            this.columnHeaderPathAndFile});
            this.listViewInjectSettings.FullRowSelect = true;
            this.listViewInjectSettings.GridLines = true;
            this.listViewInjectSettings.HideSelection = false;
            this.listViewInjectSettings.Location = new System.Drawing.Point(12, 35);
            this.listViewInjectSettings.Name = "listViewInjectSettings";
            this.listViewInjectSettings.Size = new System.Drawing.Size(652, 144);
            this.listViewInjectSettings.TabIndex = 1;
            this.listViewInjectSettings.UseCompatibleStateImageBehavior = false;
            this.listViewInjectSettings.View = System.Windows.Forms.View.Details;
            this.listViewInjectSettings.SelectedIndexChanged += new System.EventHandler(this.listViewInjectSettings_SelectedIndexChanged);
            // 
            // columnHeaderEnabled
            // 
            this.columnHeaderEnabled.Text = "::Enabled::";
            this.columnHeaderEnabled.Width = 63;
            // 
            // columnHeaderFileName
            // 
            this.columnHeaderFileName.Text = "::Inject::";
            this.columnHeaderFileName.Width = 212;
            // 
            // columnHeaderAt
            // 
            this.columnHeaderAt.Text = "::At::";
            this.columnHeaderAt.Width = 50;
            // 
            // columnHeaderOf
            // 
            this.columnHeaderOf.Text = "::Of::";
            this.columnHeaderOf.Width = 48;
            // 
            // columnHeaderPathAndFile
            // 
            this.columnHeaderPathAndFile.Text = "::Address::";
            this.columnHeaderPathAndFile.Width = 225;
            // 
            // buttonDelete
            // 
            this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDelete.Location = new System.Drawing.Point(93, 185);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(75, 23);
            this.buttonDelete.TabIndex = 3;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonNew
            // 
            this.buttonNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonNew.Location = new System.Drawing.Point(12, 185);
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Size = new System.Drawing.Size(75, 23);
            this.buttonNew.TabIndex = 2;
            this.buttonNew.Text = "New";
            this.buttonNew.UseVisualStyleBackColor = true;
            this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
            // 
            // OptionsView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(676, 452);
            this.Controls.Add(this.listViewInjectSettings);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonNew);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.checkBoxPluginViewEnabled);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsView";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "::OptionsTitle::";
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxPluginViewEnabled;
        private VirtualRadar.WinForms.Controls.FolderControl folderControlSiteRootFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListView listViewInjectSettings;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonNew;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxInjectPathAndFile;
        private VirtualRadar.WinForms.Controls.FileNameControl fileNameControlInjectFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxInjectAt;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBoxInjectOf;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.CheckBox checkBoxInjectEnabled;
        private System.Windows.Forms.ColumnHeader columnHeaderEnabled;
        private System.Windows.Forms.ColumnHeader columnHeaderFileName;
        private System.Windows.Forms.ColumnHeader columnHeaderAt;
        private System.Windows.Forms.ColumnHeader columnHeaderOf;
        private System.Windows.Forms.ColumnHeader columnHeaderPathAndFile;
    }
}