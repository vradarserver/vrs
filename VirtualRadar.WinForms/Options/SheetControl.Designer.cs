using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.WinForms.Options
{
    partial class SheetControl
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
                if(components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SheetControl
            // 
            this.Name = "SheetControl";
            this.Size = new System.Drawing.Size(359, 311);
            this.ResumeLayout(false);

        }
    }
}
