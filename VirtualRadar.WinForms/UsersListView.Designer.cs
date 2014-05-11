namespace VirtualRadar.WinForms
{
    partial class UsersListView
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
            this.textBoxLoginName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panelLoginName = new System.Windows.Forms.Panel();
            this.buttonAddLoginName = new System.Windows.Forms.Button();
            this.listView = new System.Windows.Forms.ListView();
            this.columnHeaderLoginName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderEnabled = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonClose = new System.Windows.Forms.Button();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.panelLoginName.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxLoginName
            // 
            this.textBoxLoginName.Location = new System.Drawing.Point(134, 2);
            this.textBoxLoginName.Name = "textBoxLoginName";
            this.textBoxLoginName.Size = new System.Drawing.Size(147, 20);
            this.textBoxLoginName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "::LoginName:::";
            // 
            // panelLoginName
            // 
            this.panelLoginName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelLoginName.Controls.Add(this.buttonAddLoginName);
            this.panelLoginName.Controls.Add(this.textBoxLoginName);
            this.panelLoginName.Controls.Add(this.label1);
            this.panelLoginName.Location = new System.Drawing.Point(13, 13);
            this.panelLoginName.Name = "panelLoginName";
            this.panelLoginName.Size = new System.Drawing.Size(554, 23);
            this.panelLoginName.TabIndex = 0;
            // 
            // buttonAddLoginName
            // 
            this.buttonAddLoginName.Location = new System.Drawing.Point(287, 0);
            this.buttonAddLoginName.Name = "buttonAddLoginName";
            this.buttonAddLoginName.Size = new System.Drawing.Size(75, 23);
            this.buttonAddLoginName.TabIndex = 2;
            this.buttonAddLoginName.Text = "::Add::";
            this.buttonAddLoginName.UseVisualStyleBackColor = true;
            this.buttonAddLoginName.Click += new System.EventHandler(this.buttonAddLoginName_Click);
            // 
            // listView
            // 
            this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView.CheckBoxes = true;
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderLoginName,
            this.columnHeaderName,
            this.columnHeaderEnabled});
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.Location = new System.Drawing.Point(12, 42);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(554, 309);
            this.listView.TabIndex = 1;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderLoginName
            // 
            this.columnHeaderLoginName.Text = "::LoginNameTitle::";
            this.columnHeaderLoginName.Width = 160;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "::Name::";
            this.columnHeaderName.Width = 287;
            // 
            // columnHeaderEnabled
            // 
            this.columnHeaderEnabled.Text = "::Enabled::";
            this.columnHeaderEnabled.Width = 77;
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonClose.Location = new System.Drawing.Point(492, 365);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 2;
            this.buttonClose.Text = "::Close::";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // errorProvider
            // 
            this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider.ContainerControl = this;
            // 
            // UsersListView
            // 
            this.AcceptButton = this.buttonAddLoginName;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(579, 400);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.listView);
            this.Controls.Add(this.panelLoginName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "UsersListView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "::Users::";
            this.panelLoginName.ResumeLayout(false);
            this.panelLoginName.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxLoginName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelLoginName;
        private System.Windows.Forms.Button buttonAddLoginName;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader columnHeaderLoginName;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderEnabled;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.ErrorProvider errorProvider;
    }
}