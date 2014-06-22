namespace VirtualRadar.WinForms.OptionPage
{
    partial class PageReceiverLocations
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
            this.listReceiverLocations = new VirtualRadar.WinForms.Controls.BindingListView();
            this.SuspendLayout();
            // 
            // listReceiverLocations
            // 
            this.listReceiverLocations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listReceiverLocations.Location = new System.Drawing.Point(0, 0);
            this.listReceiverLocations.Name = "listReceiverLocations";
            this.listReceiverLocations.Size = new System.Drawing.Size(784, 421);
            this.listReceiverLocations.TabIndex = 0;
            // 
            // PageReceiverLocations
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.listReceiverLocations);
            this.Name = "PageReceiverLocations";
            this.Size = new System.Drawing.Size(784, 421);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.BindingListView listReceiverLocations;
    }
}
