namespace VirtualRadar.WinForms.SettingPage
{
    partial class PageMergedFeed
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
            this.label3 = new System.Windows.Forms.Label();
            this.checkBoxIgnoreAircraftWithNoPosition = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numericIcaoTimeout = new System.Windows.Forms.NumericUpDown();
            this.checkBoxEnabled = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.listReceiverIds = new VirtualRadar.WinForms.Controls.MasterListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colEnabled = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.numericIcaoTimeout)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "::Receivers:::";
            // 
            // checkBoxIgnoreAircraftWithNoPosition
            // 
            this.checkBoxIgnoreAircraftWithNoPosition.AutoSize = true;
            this.checkBoxIgnoreAircraftWithNoPosition.Location = new System.Drawing.Point(283, 50);
            this.checkBoxIgnoreAircraftWithNoPosition.Name = "checkBoxIgnoreAircraftWithNoPosition";
            this.checkBoxIgnoreAircraftWithNoPosition.Size = new System.Drawing.Size(174, 17);
            this.checkBoxIgnoreAircraftWithNoPosition.TabIndex = 16;
            this.checkBoxIgnoreAircraftWithNoPosition.Text = "::IgnoreAircraftWithNoPosition::";
            this.checkBoxIgnoreAircraftWithNoPosition.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "::IcaoTimeout:::";
            // 
            // numericIcaoTimeout
            // 
            this.numericIcaoTimeout.DecimalPlaces = 2;
            this.numericIcaoTimeout.Location = new System.Drawing.Point(200, 49);
            this.numericIcaoTimeout.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.numericIcaoTimeout.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericIcaoTimeout.Name = "numericIcaoTimeout";
            this.numericIcaoTimeout.Size = new System.Drawing.Size(77, 20);
            this.numericIcaoTimeout.TabIndex = 14;
            this.numericIcaoTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericIcaoTimeout.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // checkBoxEnabled
            // 
            this.checkBoxEnabled.AutoSize = true;
            this.checkBoxEnabled.Location = new System.Drawing.Point(200, 0);
            this.checkBoxEnabled.Name = "checkBoxEnabled";
            this.checkBoxEnabled.Size = new System.Drawing.Size(77, 17);
            this.checkBoxEnabled.TabIndex = 11;
            this.checkBoxEnabled.Text = "::Enabled::";
            this.checkBoxEnabled.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "::Name:::";
            // 
            // textBoxName
            // 
            this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxName.Location = new System.Drawing.Point(200, 23);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(436, 20);
            this.textBoxName.TabIndex = 13;
            // 
            // listReceiverIds
            // 
            this.listReceiverIds.AllowAdd = false;
            this.listReceiverIds.AllowDelete = false;
            this.listReceiverIds.AllowUpdate = false;
            this.listReceiverIds.CheckBoxes = true;
            this.listReceiverIds.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colEnabled});
            this.listReceiverIds.HideAllButList = true;
            this.listReceiverIds.Location = new System.Drawing.Point(200, 75);
            this.listReceiverIds.Name = "listReceiverIds";
            this.listReceiverIds.Size = new System.Drawing.Size(436, 300);
            this.listReceiverIds.TabIndex = 19;
            // 
            // colName
            // 
            this.colName.Text = "::Name::";
            this.colName.Width = 175;
            // 
            // colEnabled
            // 
            this.colEnabled.Text = "::Enabled::";
            this.colEnabled.Width = 90;
            // 
            // PageMergedFeed
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.listReceiverIds);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkBoxIgnoreAircraftWithNoPosition);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericIcaoTimeout);
            this.Controls.Add(this.checkBoxEnabled);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxName);
            this.Name = "PageMergedFeed";
            this.Size = new System.Drawing.Size(636, 375);
            ((System.ComponentModel.ISupportInitialize)(this.numericIcaoTimeout)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBoxIgnoreAircraftWithNoPosition;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericIcaoTimeout;
        private System.Windows.Forms.CheckBox checkBoxEnabled;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxName;
        private Controls.MasterListView listReceiverIds;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colEnabled;
    }
}
