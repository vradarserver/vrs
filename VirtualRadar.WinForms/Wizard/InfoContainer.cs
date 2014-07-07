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
using System.Windows.Forms.Design;

namespace Gui.Wizard
{
	/// <summary>
	/// Summary description for UserControl1.
	/// </summary>
	[Designer(typeof(InfoContainerDesigner))]
	public class InfoContainer: System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.PictureBox picImage;
		private System.Windows.Forms.Label lblTitle;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// 
		/// </summary>
		public InfoContainer()
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(InfoContainer));
			this.picImage = new System.Windows.Forms.PictureBox();
			this.lblTitle = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// picImage
			// 
			this.picImage.Dock = System.Windows.Forms.DockStyle.Left;
			this.picImage.Image = ((System.Drawing.Image)(resources.GetObject("picImage.Image")));
			this.picImage.Location = new System.Drawing.Point(0, 0);
			this.picImage.Name = "picImage";
			this.picImage.Size = new System.Drawing.Size(164, 388);
			this.picImage.TabIndex = 0;
			this.picImage.TabStop = false;
			// 
			// lblTitle
			// 
			this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblTitle.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblTitle.Location = new System.Drawing.Point(172, 4);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(304, 48);
			this.lblTitle.TabIndex = 7;
			this.lblTitle.Text = "Welcome to the / Completing the <Title> Wizard";
			// 
			// InfoContainer
			// 
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.lblTitle);
			this.Controls.Add(this.picImage);
			this.Name = "InfoContainer";
			this.Size = new System.Drawing.Size(480, 388);
			this.Load += new System.EventHandler(this.InfoContainer_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void InfoContainer_Load(object sender, System.EventArgs e)
		{
			//Handle really irating resize that doesn't take account of Anchor
			lblTitle.Left = picImage.Width+8;
			lblTitle.Width = (this.Width-4)-lblTitle.Left;
		}

		/// <summary>
		/// Get/Set the title for the info page
		/// </summary>
		[Category("Appearance")]
		public string PageTitle
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
		/// Gets/Sets the Icon
		/// </summary>
		[Category("Appearance")]
		public Image Image
		{
			get
			{
				return picImage.Image;
			}
			set
			{
				picImage.Image = value;
			}
		}


	}
}
