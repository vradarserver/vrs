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
using System.Windows.Forms;

namespace Gui.Wizard
{

	/// <summary>
	/// An inherited <see cref="InfoContainer"/> that contains a <see cref="Label"/> 
	/// with the description of the page.
	/// </summary>
	public class InfoPage : InfoContainer
	{
		private System.Windows.Forms.Label lblDescription;
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public InfoPage()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lblDescription = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblDescription
			// 
			this.lblDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblDescription.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblDescription.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblDescription.Location = new System.Drawing.Point(172, 56);
			this.lblDescription.Name = "lblDescription";
			this.lblDescription.Size = new System.Drawing.Size(304, 328);
			this.lblDescription.TabIndex = 8;
			this.lblDescription.Text = "This wizard enables you to...";
			// 
			// InfoPage
			// 
			this.Controls.Add(this.lblDescription);
			this.Name = "InfoPage";
			this.Controls.SetChildIndex(this.lblDescription, 0);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Gets/Sets the text on the info page
		/// </summary>
		[Category("Appearance")]
		public string PageText
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
	}
}

