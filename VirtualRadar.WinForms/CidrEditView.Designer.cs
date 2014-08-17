namespace VirtualRadar.WinForms
{
    partial class CidrEditView
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxCidr = new System.Windows.Forms.TextBox();
            this.labelFirstMatchingAddress = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labelLastMatchingAddress = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "::Cidr:::";
            // 
            // textBoxCidr
            // 
            this.textBoxCidr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCidr.Location = new System.Drawing.Point(200, 13);
            this.textBoxCidr.Name = "textBoxCidr";
            this.textBoxCidr.Size = new System.Drawing.Size(130, 20);
            this.textBoxCidr.TabIndex = 1;
            this.textBoxCidr.TextChanged += new System.EventHandler(this.textBoxCidr_TextChanged);
            this.textBoxCidr.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxCidr_KeyDown);
            // 
            // labelFirstMatchingAddress
            // 
            this.labelFirstMatchingAddress.AutoSize = true;
            this.labelFirstMatchingAddress.Location = new System.Drawing.Point(197, 36);
            this.labelFirstMatchingAddress.Name = "labelFirstMatchingAddress";
            this.labelFirstMatchingAddress.Size = new System.Drawing.Size(10, 13);
            this.labelFirstMatchingAddress.TabIndex = 3;
            this.labelFirstMatchingAddress.Text = "-";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 36);
            this.label3.Margin = new System.Windows.Forms.Padding(3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "::FromAddress:::";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 55);
            this.label2.Margin = new System.Windows.Forms.Padding(3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "::ToAddress:::";
            // 
            // labelLastMatchingAddress
            // 
            this.labelLastMatchingAddress.AutoSize = true;
            this.labelLastMatchingAddress.Location = new System.Drawing.Point(197, 55);
            this.labelLastMatchingAddress.Name = "labelLastMatchingAddress";
            this.labelLastMatchingAddress.Size = new System.Drawing.Size(10, 13);
            this.labelLastMatchingAddress.TabIndex = 5;
            this.labelLastMatchingAddress.Text = "-";
            // 
            // CidrEditView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(342, 84);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelLastMatchingAddress);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.labelFirstMatchingAddress);
            this.Controls.Add(this.textBoxCidr);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "CidrEditView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "::CidrEdit::";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxCidr;
        private System.Windows.Forms.Label labelFirstMatchingAddress;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelLastMatchingAddress;
    }
}