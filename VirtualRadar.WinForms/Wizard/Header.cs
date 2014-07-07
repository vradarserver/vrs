// Copyright © 2004 onwards, Al Gardner
// All rights reserved.
//
// http://www.codeproject.com/Articles/8197/Designer-centric-Wizard-control
//
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the software, you
// accept this license. If you do not accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
// same meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Gui.Wizard
{
	/// <summary>
	/// Summary description for WizardHeader.
	/// </summary>
	[Designer(typeof(HeaderDesigner))]
	public class Header : UserControl
	{
		private System.Windows.Forms.Panel pnlDockPadding;
		private System.Windows.Forms.Label lblDescription;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.PictureBox picIcon;
		private System.Windows.Forms.Panel pnl3dDark;
		private System.Windows.Forms.Panel pnl3dBright;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Constructor for Header
		/// </summary>
		public Header()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Header));
			this.pnlDockPadding = new System.Windows.Forms.Panel();
			this.lblDescription = new System.Windows.Forms.Label();
			this.lblTitle = new System.Windows.Forms.Label();
			this.picIcon = new System.Windows.Forms.PictureBox();
			this.pnl3dDark = new System.Windows.Forms.Panel();
			this.pnl3dBright = new System.Windows.Forms.Panel();
			this.pnlDockPadding.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlDockPadding
			// 
			this.pnlDockPadding.BackColor = System.Drawing.SystemColors.Window;
			this.pnlDockPadding.Controls.Add(this.lblDescription);
			this.pnlDockPadding.Controls.Add(this.lblTitle);
			this.pnlDockPadding.Controls.Add(this.picIcon);
			this.pnlDockPadding.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlDockPadding.DockPadding.Bottom = 4;
			this.pnlDockPadding.DockPadding.Left = 8;
			this.pnlDockPadding.DockPadding.Right = 4;
			this.pnlDockPadding.DockPadding.Top = 6;
			this.pnlDockPadding.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.pnlDockPadding.Location = new System.Drawing.Point(0, 0);
			this.pnlDockPadding.Name = "pnlDockPadding";
			this.pnlDockPadding.Size = new System.Drawing.Size(324, 64);
			this.pnlDockPadding.TabIndex = 6;
			// 
			// lblDescription
			// 
			this.lblDescription.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblDescription.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblDescription.Location = new System.Drawing.Point(8, 22);
			this.lblDescription.Name = "lblDescription";
			this.lblDescription.Size = new System.Drawing.Size(260, 38);
			this.lblDescription.TabIndex = 5;
			this.lblDescription.Text = "Description";
			// 
			// lblTitle
			// 
			this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblTitle.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblTitle.Location = new System.Drawing.Point(8, 6);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(260, 16);
			this.lblTitle.TabIndex = 4;
			this.lblTitle.Text = "Title";
			// 
			// picIcon
			// 
			this.picIcon.Dock = System.Windows.Forms.DockStyle.Right;
			this.picIcon.Image = ((System.Drawing.Image)(resources.GetObject("picIcon.Image")));
			this.picIcon.Location = new System.Drawing.Point(268, 6);
			this.picIcon.Name = "picIcon";
			this.picIcon.Size = new System.Drawing.Size(52, 54);
			this.picIcon.TabIndex = 3;
			this.picIcon.TabStop = false;
			// 
			// pnl3dDark
			// 
			this.pnl3dDark.BackColor = System.Drawing.SystemColors.ControlDark;
			this.pnl3dDark.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnl3dDark.Location = new System.Drawing.Point(0, 62);
			this.pnl3dDark.Name = "pnl3dDark";
			this.pnl3dDark.Size = new System.Drawing.Size(324, 1);
			this.pnl3dDark.TabIndex = 7;
			// 
			// pnl3dBright
			// 
			this.pnl3dBright.BackColor = System.Drawing.Color.White;
			this.pnl3dBright.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnl3dBright.Location = new System.Drawing.Point(0, 63);
			this.pnl3dBright.Name = "pnl3dBright";
			this.pnl3dBright.Size = new System.Drawing.Size(324, 1);
			this.pnl3dBright.TabIndex = 8;
			// 
			// Header
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CausesValidation = false;
			this.Controls.Add(this.pnl3dDark);
			this.Controls.Add(this.pnl3dBright);
			this.Controls.Add(this.pnlDockPadding);
			this.Name = "Header";
			this.Size = new System.Drawing.Size(324, 64);
			this.SizeChanged += new System.EventHandler(this.Header_SizeChanged);
			this.pnlDockPadding.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void ResizeImageAndText()
		{
			//Resize image 
			picIcon.Size= picIcon.Image.Size;
			//Relocate image according to its size
			picIcon.Top = (this.Height - picIcon.Height)/2;
			picIcon.Left = this.Width - picIcon.Width - 8;
			//Fit text around picture
			lblTitle.Width = picIcon.Left - lblTitle.Left;
			lblDescription.Width = picIcon.Left - lblDescription.Left;
		}

		private void Header_SizeChanged(object sender, System.EventArgs e)
		{
			ResizeImageAndText();
		}

		/// <summary>
		/// Get/Set the title for the wizard page
		/// </summary>
		[Category("Appearance")]
		public string Title
		{
			get
			{
				return lblTitle.Text;
			}
			set
			{
				lblTitle.Text = value;
			}
		}

		/// <summary>
		/// Gets/Sets the
		/// </summary>
		[Category("Appearance")]
		public string Description
		{
			get
			{
				return lblDescription.Text;
			}
			set
			{
				lblDescription.Text = value;
			}
		}

		/// <summary>
		/// Gets/Sets the Icon
		/// </summary>
		[Category("Appearance")]
		public Image Image
		{
			get
			{
				return picIcon.Image;
			}
			set
			{
				picIcon.Image = value;
				ResizeImageAndText();
			}
		}
	}
}
