namespace VirtualRadar.WinForms.SettingPage
{
    partial class PageWebServerUsers
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
            if (disposing && (components != null))
            {
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
            this.listUsers = new VirtualRadar.WinForms.Controls.MasterListView();
            this.columnHeaderLoginName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderEnabled = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxEnabled = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // listUsers
            // 
            this.listUsers.AllowAdd = false;
            this.listUsers.AllowDelete = false;
            this.listUsers.AllowUpdate = false;
            this.listUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listUsers.CheckBoxes = true;
            this.listUsers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderLoginName,
            this.columnHeaderEnabled,
            this.columnHeaderName});
            this.listUsers.HideAllButList = true;
            this.listUsers.Location = new System.Drawing.Point(200, 23);
            this.listUsers.Name = "listUsers";
            this.listUsers.Size = new System.Drawing.Size(436, 290);
            this.listUsers.TabIndex = 3;
            // 
            // columnHeaderLoginName
            // 
            this.columnHeaderLoginName.Text = "::LoginName::";
            this.columnHeaderLoginName.Width = 150;
            // 
            // columnHeaderEnabled
            // 
            this.columnHeaderEnabled.Text = "::Enabled::";
            this.columnHeaderEnabled.Width = 70;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "::Name::";
            this.columnHeaderName.Width = 190;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "::Users:::";
            // 
            // checkBoxEnabled
            // 
            this.checkBoxEnabled.AutoSize = true;
            this.checkBoxEnabled.Location = new System.Drawing.Point(200, 0);
            this.checkBoxEnabled.Name = "checkBoxEnabled";
            this.checkBoxEnabled.Size = new System.Drawing.Size(143, 17);
            this.checkBoxEnabled.TabIndex = 4;
            this.checkBoxEnabled.Text = "::UserMustAuthenticate::";
            this.checkBoxEnabled.UseVisualStyleBackColor = true;
            // 
            // PageWebServerAuthentication
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.listUsers);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBoxEnabled);
            this.Name = "PageWebServerAuthentication";
            this.Size = new System.Drawing.Size(636, 313);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.MasterListView listUsers;
        private System.Windows.Forms.ColumnHeader columnHeaderLoginName;
        private System.Windows.Forms.ColumnHeader columnHeaderEnabled;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxEnabled;
    }
}
