namespace VirtualRadar.WinForms.OptionPage
{
    partial class PageReceiverLocation
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
            this.numericLongitude = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.numericLatitude = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericLongitude)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLatitude)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "::Longitude:::";
            // 
            // numericLongitude
            // 
            this.numericLongitude.DecimalPlaces = 6;
            this.numericLongitude.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericLongitude.Location = new System.Drawing.Point(155, 53);
            this.numericLongitude.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.numericLongitude.Minimum = new decimal(new int[] {
            180,
            0,
            0,
            -2147483648});
            this.numericLongitude.Name = "numericLongitude";
            this.numericLongitude.Size = new System.Drawing.Size(120, 20);
            this.numericLongitude.TabIndex = 7;
            this.numericLongitude.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "::Latitude:::";
            // 
            // numericLatitude
            // 
            this.numericLatitude.DecimalPlaces = 6;
            this.numericLatitude.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericLatitude.Location = new System.Drawing.Point(155, 27);
            this.numericLatitude.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.numericLatitude.Minimum = new decimal(new int[] {
            90,
            0,
            0,
            -2147483648});
            this.numericLatitude.Name = "numericLatitude";
            this.numericLatitude.Size = new System.Drawing.Size(120, 20);
            this.numericLatitude.TabIndex = 5;
            this.numericLatitude.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "::Name:::";
            // 
            // textBoxName
            // 
            this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxName.Location = new System.Drawing.Point(155, 0);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(481, 20);
            this.textBoxName.TabIndex = 4;
            // 
            // PageReceiverLocation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numericLongitude);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericLatitude);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxName);
            this.Name = "PageReceiverLocation";
            this.Size = new System.Drawing.Size(636, 375);
            ((System.ComponentModel.ISupportInitialize)(this.numericLongitude)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLatitude)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.NumericUpDown numericLatitude;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericLongitude;
    }
}
