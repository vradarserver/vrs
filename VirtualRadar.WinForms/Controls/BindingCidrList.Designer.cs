namespace VirtualRadar.WinForms.Controls
{
    partial class BindingCidrList
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
            this.bindingListView = new VirtualRadar.WinForms.Controls.BindingListView();
            this.columnHeaderCidr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFromAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderToAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // bindingListView
            // 
            this.bindingListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderCidr,
            this.columnHeaderFromAddress,
            this.columnHeaderToAddress});
            this.bindingListView.DataSource = null;
            this.bindingListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bindingListView.Location = new System.Drawing.Point(0, 0);
            this.bindingListView.Name = "bindingListView";
            this.bindingListView.Size = new System.Drawing.Size(395, 251);
            this.bindingListView.TabIndex = 0;
            this.bindingListView.FetchRecordContent += new System.EventHandler<VirtualRadar.WinForms.Controls.BindingListView.RecordContentEventArgs>(this.bindingListView_FetchRecordContent);
            this.bindingListView.AddClicked += new System.EventHandler(this.bindingListView_AddClicked);
            this.bindingListView.DeleteClicked += new System.EventHandler(this.bindingListView_DeleteClicked);
            this.bindingListView.EditClicked += new System.EventHandler(this.bindingListView_EditClicked);
            // 
            // columnHeaderCidr
            // 
            this.columnHeaderCidr.Text = "::Cidr::";
            this.columnHeaderCidr.Width = 125;
            // 
            // columnHeaderFromAddress
            // 
            this.columnHeaderFromAddress.Text = "::FromAddress::";
            this.columnHeaderFromAddress.Width = 120;
            // 
            // columnHeaderToAddress
            // 
            this.columnHeaderToAddress.Text = "::ToAddress::";
            this.columnHeaderToAddress.Width = 120;
            // 
            // BindingCidrList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.bindingListView);
            this.Name = "BindingCidrList";
            this.Size = new System.Drawing.Size(395, 251);
            this.ResumeLayout(false);

        }

        #endregion

        private BindingListView bindingListView;
        private System.Windows.Forms.ColumnHeader columnHeaderCidr;
        private System.Windows.Forms.ColumnHeader columnHeaderFromAddress;
        private System.Windows.Forms.ColumnHeader columnHeaderToAddress;
    }
}
