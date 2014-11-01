namespace VirtualRadar.WinForms.Controls
{
    partial class AccessControl
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
                if(_DefaultAccessItems != null) {
                    _DefaultAccessItems.Dispose();
                    _DefaultAccessItems = null;
                }
                if(_AddressesWrapper != null) {
                    _Addresses.ListChanged -= Addresses_ListChanged;
                }
                if(components != null) components.Dispose();
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
            this.labelCidrList = new System.Windows.Forms.Label();
            this.columnHeaderCidr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFromAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderToAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.comboBoxDefaultAccess = new System.Windows.Forms.ComboBox();
            this.label17 = new System.Windows.Forms.Label();
            this.listView = new VirtualRadar.WinForms.Controls.MasterListView();
            this.SuspendLayout();
            // 
            // labelCidrList
            // 
            this.labelCidrList.AutoSize = true;
            this.labelCidrList.Location = new System.Drawing.Point(0, 62);
            this.labelCidrList.Name = "labelCidrList";
            this.labelCidrList.Size = new System.Drawing.Size(126, 13);
            this.labelCidrList.TabIndex = 6;
            this.labelCidrList.Text = "::AllowTheseAddresses:::";
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
            // comboBoxDefaultAccess
            // 
            this.comboBoxDefaultAccess.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDefaultAccess.FormattingEnabled = true;
            this.comboBoxDefaultAccess.Location = new System.Drawing.Point(200, 0);
            this.comboBoxDefaultAccess.Name = "comboBoxDefaultAccess";
            this.comboBoxDefaultAccess.Size = new System.Drawing.Size(150, 21);
            this.comboBoxDefaultAccess.TabIndex = 5;
            this.comboBoxDefaultAccess.SelectedIndexChanged += new System.EventHandler(this.comboBoxDefaultAccess_SelectedIndexChanged);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(0, 3);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(91, 13);
            this.label17.TabIndex = 4;
            this.label17.Text = "::DefaultAccess:::";
            // 
            // listView
            // 
            this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderCidr,
            this.columnHeaderFromAddress,
            this.columnHeaderToAddress});
            this.listView.Location = new System.Drawing.Point(200, 27);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(423, 134);
            this.listView.TabIndex = 7;
            this.listView.FetchRecordContent += new System.EventHandler<VirtualRadar.WinForms.Controls.ListContentEventArgs>(this.listView_FetchRecordContent);
            this.listView.AddClicked += new System.EventHandler(this.listView_AddClicked);
            this.listView.DeleteClicked += new System.EventHandler(this.listView_DeleteClicked);
            this.listView.EditClicked += new System.EventHandler(this.listView_EditClicked);
            // 
            // AccessControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listView);
            this.Controls.Add(this.labelCidrList);
            this.Controls.Add(this.comboBoxDefaultAccess);
            this.Controls.Add(this.label17);
            this.Name = "AccessControl";
            this.Size = new System.Drawing.Size(623, 161);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelCidrList;
        private System.Windows.Forms.ComboBox comboBoxDefaultAccess;
        private System.Windows.Forms.Label label17;
        private MasterListView listView;
        private System.Windows.Forms.ColumnHeader columnHeaderCidr;
        private System.Windows.Forms.ColumnHeader columnHeaderFromAddress;
        private System.Windows.Forms.ColumnHeader columnHeaderToAddress;
    }
}
