namespace VirtualRadar.WinForms.SettingPage
{
    partial class PageWebServerAdministrators
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
            this.listAdministrators = new VirtualRadar.WinForms.Controls.MasterListView();
            this.columnHeaderLoginName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderEnabled = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // listAdministrators
            // 
            this.listAdministrators.AllowAdd = false;
            this.listAdministrators.AllowDelete = false;
            this.listAdministrators.AllowUpdate = false;
            this.listAdministrators.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listAdministrators.CheckBoxes = true;
            this.listAdministrators.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderLoginName,
            this.columnHeaderEnabled,
            this.columnHeaderName});
            this.listAdministrators.HideAllButList = true;
            this.listAdministrators.Location = new System.Drawing.Point(200, 0);
            this.listAdministrators.Name = "listAdministrators";
            this.listAdministrators.Size = new System.Drawing.Size(436, 313);
            this.listAdministrators.TabIndex = 6;
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
            this.label1.Location = new System.Drawing.Point(0, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "::Administrators:::";
            // 
            // PageWebServerAdministrators
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.listAdministrators);
            this.Controls.Add(this.label1);
            this.Name = "PageWebServerAdministrators";
            this.Size = new System.Drawing.Size(636, 313);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.MasterListView listAdministrators;
        private System.Windows.Forms.ColumnHeader columnHeaderLoginName;
        private System.Windows.Forms.ColumnHeader columnHeaderEnabled;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.Label label1;
    }
}
