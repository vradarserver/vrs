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
using System.Windows.Forms;
using System.ComponentModel;
using System.ComponentModel.Design;

using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Gui.Wizard
{
	/// <summary>
	/// 
	/// </summary>
	[Designer(typeof(Gui.Wizard.WizardPageDesigner))]
	public class WizardPage : Panel
	{
        /// <summary>
        /// No documentation supplied.
        /// </summary>
        [DefaultValue(typeof(Color), "Window")]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

		/// <summary>
		/// Event called before this page is closed when the back button is pressed. If you don't want to show pageIndex -1 then
		/// set page to be the new page that you wish to show
		/// </summary>
		public event PageEventHandler CloseFromBack;
		/// <summary>
		/// Event called before this page is closed when the next button is pressed. If you don't want to show pageIndex -1 then
		/// set page to be the new page that you wish to show 
		/// </summary>
		public event PageEventHandler CloseFromNext;
		/// <summary>
		/// Event called after this page is shown when the back button is pressed.
		/// </summary>
		public event EventHandler ShowFromBack;
		/// <summary>
		/// Event called after this page is shown when the next button is pressed. 
		/// </summary>
		public event EventHandler ShowFromNext;

		/// <summary>
		/// Fires the CloseFromBack Event
		/// </summary>
		/// <param name="wiz">Wizard to pass into the sender argument</param>
		/// <returns>Index of Page that the event handlers would like to see next</returns>
		public int OnCloseFromBack(Wizard wiz)
		{
			//Event args thinks that the next pgae will be the one before this one
			PageEventArgs e = new PageEventArgs(wiz.PageIndex -1, wiz.Pages);
			//Tell anybody who listens
			if (CloseFromBack != null)
				CloseFromBack(wiz, e);
			//And then tell whomever called me what the prefered page is
			return e.PageIndex;
		}

		/// <summary>
		/// Fires the CloseFromNextEvent
		/// </summary>
		/// <param name="wiz">Sender</param>
		public int OnCloseFromNext(Wizard wiz)
		{	
			//Event args thinks that the next pgae will be the one before this one
			PageEventArgs e = new PageEventArgs(wiz.PageIndex +1, wiz.Pages);
			//Tell anybody who listens
			if (CloseFromNext != null)
				CloseFromNext(wiz, e);
			//And then tell whomever called me what the prefered page is
			return e.PageIndex;
		}
		
		/// <summary>
		/// Fires the showFromBack event
		/// </summary>
		/// <param name="wiz">sender</param>
		public void OnShowFromBack(Wizard wiz)
		{
			if (ShowFromBack != null)
				ShowFromBack(wiz, EventArgs.Empty);
		}

		/// <summary>
		/// Fires the showFromNext event
		/// </summary>
		/// <param name="wiz">Sender</param>
		public void OnShowFromNext(Wizard wiz)
		{
			if (ShowFromNext != null)
				ShowFromNext(wiz, EventArgs.Empty);
		}

        /// <summary>
        /// No documentation supplied.
        /// </summary>
		[Category("Wizard")]
		public bool IsFinishPage
		{
			get
			{
				return _IsFinishPage;
			}
			set
			{
				_IsFinishPage=value;
			}
		}
		private bool _IsFinishPage = false;
	
        /// <summary>
        /// No documentation supplied.
        /// </summary>
        public WizardPage() : base()
        {
            BackColor = SystemColors.Window;
        }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing"></param>

		protected override void Dispose( bool disposing ) 
		{
			if( disposing ) 
			{
//				//Unregister callbacks
//				ClearChangeNotifications();      
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Set the focus to the contained control with a lowest tabIndex
		/// </summary>
		public void FocusFirstTabIndex()
		{
			//Activate the first control in the Panel
			Control found = null;
			//find the control with the lowest 
			foreach (Control control in this.Controls)
			{
				if (control.CanFocus && (found == null || control.TabIndex < found.TabIndex))
				{
					found = control;
				}
			}
			//Have we actually found anything
			if (found != null)
			{
				//Focus the found control
				found.Focus();
			}
			else
			{
				//Just focus the wizard Page
				this.Focus();
			}
		}

	}
}