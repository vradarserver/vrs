namespace VirtualRadar.WinForms
{
    partial class PluginsView
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
            this.pluginDetailsControl = new VirtualRadar.WinForms.Controls.PluginDetailsControl();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.invalidPluginCountControl = new VirtualRadar.WinForms.Controls.InvalidPluginCountControl();
            this.SuspendLayout();
            // 
            // pluginDetailsControl
            // 
            this.pluginDetailsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pluginDetailsControl.Location = new System.Drawing.Point(13, 13);
            this.pluginDetailsControl.Name = "pluginDetailsControl";
            this.pluginDetailsControl.Size = new System.Drawing.Size(571, 288);
            this.pluginDetailsControl.TabIndex = 0;
            this.pluginDetailsControl.ConfigurePluginClicked += new System.EventHandler<VirtualRadar.Interface.PluginEventArgs>(this.pluginDetailsControl_ConfigurePluginClicked);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(509, 318);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "::Close::";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // invalidPluginCountControl
            // 
            this.invalidPluginCountControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.invalidPluginCountControl.Location = new System.Drawing.Point(12, 328);
            this.invalidPluginCountControl.Name = "invalidPluginCountControl";
            this.invalidPluginCountControl.Size = new System.Drawing.Size(173, 13);
            this.invalidPluginCountControl.TabIndex = 1;
            // 
            // PluginsView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(596, 353);
            this.Controls.Add(this.invalidPluginCountControl);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.pluginDetailsControl);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PluginsView";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "::Plugins::";
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.PluginDetailsControl pluginDetailsControl;
        private System.Windows.Forms.Button buttonCancel;
        private Controls.InvalidPluginCountControl invalidPluginCountControl;
    }
}