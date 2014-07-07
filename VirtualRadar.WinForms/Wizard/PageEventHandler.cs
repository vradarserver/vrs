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

namespace Gui.Wizard
{
	/// <summary>
	/// Delegate definition for handling NextPageEvents
	/// </summary>
	public delegate void PageEventHandler(object sender, PageEventArgs e);

	/// <summary>
	/// Arguments passed to an application when Page is closed in a wizard. The Next page to be displayed 
	/// can be changed, by the application, by setting the NextPage to a wizardPage which is part of the 
	/// wizard that generated this event.
	/// </summary>
	public class PageEventArgs : EventArgs
	{
		private int vPage;
		private PageCollection vPages;
		/// <summary>
		/// Constructs a new event
		/// </summary>
		/// <param name="index">The index of the next page in the collection</param>
		/// <param name="pages">Pages in the wizard that are acceptable to be </param>
		public PageEventArgs(int index, PageCollection pages) : base()
		{
			vPage = index;
			vPages = pages;
		}

		/// <summary>
		/// Gets/Sets the wizard page that will be displayed next. If you set this it must be to a wizardPage from the wizard.
		/// </summary>
		public WizardPage Page
		{
			get
			{
				//Is this a valid page
				if (vPage >=0 && vPage <vPages.Count)
					return vPages[vPage];
				return null;
			}
			set
			{
				if (vPages.Contains(value) == true)
				{
					//If this is a valid page then set it
					vPage = vPages.IndexOf(value);
				}
				else
				{
					throw new ArgumentOutOfRangeException("NextPage",value,"The page you tried to set was not found in the wizard.");
				}
			}
		}


		/// <summary>
		/// Gets the index of the page 
		/// </summary>
		public int PageIndex
		{
			get
			{
				return vPage;
			}
		}

	}
}
