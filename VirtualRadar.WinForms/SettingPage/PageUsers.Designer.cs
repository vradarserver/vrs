namespace VirtualRadar.WinForms.SettingPage
{
    partial class PageUsers
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
            this.labelUserManager = new System.Windows.Forms.Label();
            this.columnHeaderLoginName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label4 = new System.Windows.Forms.Label();
            this.columnHeaderUserName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listUsers = new VirtualRadar.WinForms.Controls.BindingListView();
            this.SuspendLayout();
            // 
            // labelUserManager
            // 
            this.labelUserManager.AutoSize = true;
            this.labelUserManager.Location = new System.Drawing.Point(155, 0);
            this.labelUserManager.Name = "labelUserManager";
            this.labelUserManager.Size = new System.Drawing.Size(10, 13);
            this.labelUserManager.TabIndex = 9;
            this.labelUserManager.Text = "-";
            // 
            // columnHeaderLoginName
            // 
            this.columnHeaderLoginName.Text = "::LoginName::";
            this.columnHeaderLoginName.Width = 150;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "::UserManager:::";
            // 
            // columnHeaderUserName
            // 
            this.columnHeaderUserName.Text = "::Name::";
            this.columnHeaderUserName.Width = 300;
            // 
            // listUsers
            // 
            this.listUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listUsers.CheckBoxes = true;
            this.listUsers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderLoginName,
            this.columnHeaderUserName});
            this.listUsers.DataSource = null;
            this.listUsers.Location = new System.Drawing.Point(0, 16);
            this.listUsers.Name = "listUsers";
            this.listUsers.Size = new System.Drawing.Size(636, 297);
            this.listUsers.TabIndex = 7;
            this.listUsers.FetchRecordContent += new System.EventHandler<VirtualRadar.WinForms.Controls.BindingListView.RecordContentEventArgs>(this.listUsers_FetchRecordContent);
            this.listUsers.AddClicked += new System.EventHandler(this.listUsers_AddClicked);
            this.listUsers.DeleteClicked += new System.EventHandler(this.listUsers_DeleteClicked);
            this.listUsers.EditClicked += new System.EventHandler(this.listUsers_EditClicked);
            this.listUsers.CheckedChanged += new System.EventHandler<VirtualRadar.WinForms.Controls.BindingListView.RecordCheckedEventArgs>(this.listUsers_CheckedChanged);
            // 
            // PageUsers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.labelUserManager);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.listUsers);
            this.Name = "PageUsers";
            this.Size = new System.Drawing.Size(636, 313);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelUserManager;
        private System.Windows.Forms.ColumnHeader columnHeaderLoginName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ColumnHeader columnHeaderUserName;
        private Controls.BindingListView listUsers;
    }
}
