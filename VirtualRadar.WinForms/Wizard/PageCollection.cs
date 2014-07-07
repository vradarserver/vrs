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
using System.Windows.Forms;

namespace Gui.Wizard
{
	/// <summary>
	/// Summary description for PanelCollection.
	/// </summary>
	public class PageCollection : CollectionBase
	{
		private Wizard vParent;
		/// <summary>
		/// Constructor requires the  wizard that owns this collection
		/// </summary>
		/// <param name="parent">Wizard</param>
		public PageCollection(Wizard parent):base()
		{
			vParent = parent;
		}

		/// <summary>
		/// Returns the wizard that owns this collection
		/// </summary>
		public Wizard Parent
		{
			get 
			{
				return vParent;
			}
		}

		/// <summary>
		/// Finds the Page in the collection
		/// </summary>
		public WizardPage this[ int index ]  
		{
			get  
			{
				return( (WizardPage) List[index] );
			}
			set  
			{
				List[index] = value;
			}
		}


		/// <summary>
		/// Adds a WizardPage into the Collection
		/// </summary>
		/// <param name="value">The page to add</param>
		/// <returns></returns>
		public int Add(WizardPage value )  
		{		
			int result = List.Add( value );
			return result;
		}


		/// <summary>
		/// Adds an array of pages into the collection. Used by the Studio Designer generated coed
		/// </summary>
		/// <param name="pages">Array of pages to add</param>
		public void AddRange(WizardPage[] pages)
		{
			// Use external to validate and add each entry
			foreach(WizardPage page in pages)
			{
				this.Add(page);
			}
		}

		/// <summary>
		/// Finds the position of the page in the colleciton
		/// </summary>
		/// <param name="value">Page to find position of</param>
		/// <returns>Index of Page in collection</returns>
		public int IndexOf( WizardPage value )  
		{
			return( List.IndexOf( value ) );
		}

		/// <summary>
		/// Adds a new page at a particular position in the Collection
		/// </summary>
		/// <param name="index">Position</param>
		/// <param name="value">Page to be added</param>
		public void Insert( int index, WizardPage value )  
		{
			List.Insert(index, value );
		}


		/// <summary>
		/// Removes the given page from the collection
		/// </summary>
		/// <param name="value">Page to remove</param>
		public void Remove( WizardPage value )  
		{
			//Remove the item
			List.Remove( value );
		}

		/// <summary>
		/// Detects if a given Page is in the Collection
		/// </summary>
		/// <param name="value">Page to find</param>
		/// <returns></returns>
		public bool Contains( WizardPage value )  
		{
			// If value is not of type Int16, this will return false.
			return( List.Contains( value ) );
		}

		/// <summary>
		/// Propgate when a external designer modifies the pages
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		protected override void OnInsertComplete(int index, object value)
		{
			base.OnInsertComplete (index, value);
			//Showthe page added
			vParent.PageIndex = index;
		}
	
		/// <summary>
		/// Propogates when external designers remove items from page
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		protected override void OnRemoveComplete(int index, object value)
		{
			base.OnRemoveComplete (index, value);
			//If the page that was added was the one that was visible
			if (vParent.PageIndex == index)
			{
				//Can I show the one after
				if (index < InnerList.Count)
				{
					vParent.PageIndex = index;
				}
				else
				{
					//Can I show the end one (if not -1 makes everythign disappear
					vParent.PageIndex = InnerList.Count-1;
				}
			}
		}

	}
}
