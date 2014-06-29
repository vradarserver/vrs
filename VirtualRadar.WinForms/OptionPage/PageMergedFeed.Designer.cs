namespace VirtualRadar.WinForms.OptionPage
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
            this.checkBoxEnabled = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.numericIcaoTimeout = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxIgnoreAircraftWithNoPosition = new System.Windows.Forms.CheckBox();
            this.listReceivers = new VirtualRadar.WinForms.Controls.ObservableListView();
            this.columnHeaderReceiverName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderReceiverEnabled = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericIcaoTimeout)).BeginInit();
            this.SuspendLayout();
            // 
            // checkBoxEnabled
            // 
            this.checkBoxEnabled.AutoSize = true;
            this.checkBoxEnabled.Location = new System.Drawing.Point(155, 0);
            this.checkBoxEnabled.Name = "checkBoxEnabled";
            this.checkBoxEnabled.Size = new System.Drawing.Size(77, 17);
            this.checkBoxEnabled.TabIndex = 3;
            this.checkBoxEnabled.Text = "::Enabled::";
            this.checkBoxEnabled.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "::Name:::";
            // 
            // textBoxName
            // 
            this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxName.Location = new System.Drawing.Point(155, 23);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(481, 20);
            this.textBoxName.TabIndex = 5;
            // 
            // numericIcaoTimeout
            // 
            this.numericIcaoTimeout.DecimalPlaces = 1;
            this.numericIcaoTimeout.Location = new System.Drawing.Point(155, 50);
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
            this.numericIcaoTimeout.Size = new System.Drawing.Size(120, 20);
            this.numericIcaoTimeout.TabIndex = 6;
            this.numericIcaoTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericIcaoTimeout.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "::IcaoTimeout:::";
            // 
            // checkBoxIgnoreAircraftWithNoPosition
            // 
            this.checkBoxIgnoreAircraftWithNoPosition.AutoSize = true;
            this.checkBoxIgnoreAircraftWithNoPosition.Location = new System.Drawing.Point(336, 51);
            this.checkBoxIgnoreAircraftWithNoPosition.Name = "checkBoxIgnoreAircraftWithNoPosition";
            this.checkBoxIgnoreAircraftWithNoPosition.Size = new System.Drawing.Size(174, 17);
            this.checkBoxIgnoreAircraftWithNoPosition.TabIndex = 8;
            this.checkBoxIgnoreAircraftWithNoPosition.Text = "::IgnoreAircraftWithNoPosition::";
            this.checkBoxIgnoreAircraftWithNoPosition.UseVisualStyleBackColor = true;
            // 
            // listReceivers
            // 
            this.listReceivers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listReceivers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderReceiverName,
            this.columnHeaderReceiverEnabled});
            this.listReceivers.Location = new System.Drawing.Point(155, 77);
            this.listReceivers.Name = "listReceivers";
            this.listReceivers.Size = new System.Drawing.Size(481, 298);
            this.listReceivers.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listReceivers.TabIndex = 9;
            this.listReceivers.FetchRecordContent += new System.EventHandler<VirtualRadar.WinForms.Controls.BindingListView.RecordContentEventArgs>(this.listReceivers_FetchRecordContent);
            // 
            // columnHeaderReceiverName
            // 
            this.columnHeaderReceiverName.Text = "::Name::";
            this.columnHeaderReceiverName.Width = 200;
            // 
            // columnHeaderReceiverEnabled
            // 
            this.columnHeaderReceiverEnabled.Text = "::Enabled::";
            this.columnHeaderReceiverEnabled.Width = 100;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "::Receivers:::";
            // 
            // PageMergedFeed
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.listReceivers);
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

        private System.Windows.Forms.CheckBox checkBoxEnabled;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.NumericUpDown numericIcaoTimeout;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxIgnoreAircraftWithNoPosition;
        private Controls.ObservableListView listReceivers;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ColumnHeader columnHeaderReceiverName;
        private System.Windows.Forms.ColumnHeader columnHeaderReceiverEnabled;
    }
}
