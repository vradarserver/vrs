namespace VirtualRadar.WinForms
{
    partial class ConnectorActivityLogView
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
            this.buttonCopySelectedItemsToClipboard = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.listView = new System.Windows.Forms.ListView();
            this.columnHeaderDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderConnector = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMessage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxConnectorFilter = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // buttonCopySelectedItemsToClipboard
            // 
            this.buttonCopySelectedItemsToClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCopySelectedItemsToClipboard.Location = new System.Drawing.Point(12, 513);
            this.buttonCopySelectedItemsToClipboard.Name = "buttonCopySelectedItemsToClipboard";
            this.buttonCopySelectedItemsToClipboard.Size = new System.Drawing.Size(75, 23);
            this.buttonCopySelectedItemsToClipboard.TabIndex = 3;
            this.buttonCopySelectedItemsToClipboard.Text = "::Copy::";
            this.buttonCopySelectedItemsToClipboard.UseVisualStyleBackColor = true;
            this.buttonCopySelectedItemsToClipboard.Click += new System.EventHandler(this.buttonCopySelectedItemsToClipboard_Click);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRefresh.Location = new System.Drawing.Point(93, 513);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(75, 23);
            this.buttonRefresh.TabIndex = 4;
            this.buttonRefresh.Text = "::Refresh::";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // listView
            // 
            this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderDate,
            this.columnHeaderConnector,
            this.columnHeaderType,
            this.columnHeaderMessage});
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.Location = new System.Drawing.Point(12, 40);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(750, 467);
            this.listView.TabIndex = 2;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.DoubleClick += new System.EventHandler(this.listView_DoubleClick);
            // 
            // columnHeaderDate
            // 
            this.columnHeaderDate.Text = "::Time::";
            this.columnHeaderDate.Width = 150;
            // 
            // columnHeaderConnector
            // 
            this.columnHeaderConnector.Text = "::Connector::";
            this.columnHeaderConnector.Width = 150;
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Text = "::Activity::";
            this.columnHeaderType.Width = 100;
            // 
            // columnHeaderMessage
            // 
            this.columnHeaderMessage.Text = "::Description::";
            this.columnHeaderMessage.Width = 324;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "::Connector:::";
            // 
            // comboBoxConnectorFilter
            // 
            this.comboBoxConnectorFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxConnectorFilter.FormattingEnabled = true;
            this.comboBoxConnectorFilter.Location = new System.Drawing.Point(166, 13);
            this.comboBoxConnectorFilter.Name = "comboBoxConnectorFilter";
            this.comboBoxConnectorFilter.Size = new System.Drawing.Size(168, 21);
            this.comboBoxConnectorFilter.TabIndex = 1;
            this.comboBoxConnectorFilter.SelectedIndexChanged += new System.EventHandler(this.comboBoxConnectorFilter_SelectedIndexChanged);
            // 
            // ConnectorActivityLogView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(775, 548);
            this.Controls.Add(this.comboBoxConnectorFilter);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCopySelectedItemsToClipboard);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.listView);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConnectorActivityLogView";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "::ConnectorActivityLog::";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCopySelectedItemsToClipboard;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader columnHeaderDate;
        private System.Windows.Forms.ColumnHeader columnHeaderConnector;
        private System.Windows.Forms.ColumnHeader columnHeaderType;
        private System.Windows.Forms.ColumnHeader columnHeaderMessage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxConnectorFilter;

    }
}