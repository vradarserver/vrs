// Copyright © 2013 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// The base class for all controls that act as parent pages for items in a collection in <see cref="Configuration"/>.
    /// </summary>
    public partial class ParentPage : BaseUserControl
    {
        #region Fields
        /// <summary>
        /// The next value to assign to the unique ID of a list record.
        /// </summary>
        private int _NextUniqueId;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the title of the page.
        /// </summary>
        public virtual string PageTitle { get { return "missing title"; } }

        /// <summary>
        /// Gets the icon for the page.
        /// </summary>
        public virtual Image Icon { get { return null; } }

        /// <summary>
        /// Gets or sets the tree node associated with this parent page.
        /// </summary>
        public TreeNode TreeNode { get; set; }

        /// <summary>
        /// Gets or sets the view that this page is handling child objects for.
        /// </summary>
        public OptionsPropertySheetView View { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ParentPage()
        {
            InitializeComponent();
        }
        #endregion

        #region Populate
        /// <summary>
        /// Creates sheets for the existing records in the appropriate collection held by the view.
        /// </summary>
        /// <param name="optionsView"></param>
        /// <returns></returns>
        public List<ISheet> Populate(OptionsPropertySheetView optionsView)
        {
            View = optionsView;

            var result = new List<ISheet>();
            _NextUniqueId = DoPopulate(optionsView, result);

            return result;
        }

        /// <summary>
        /// When overridden by a derivee this does the work for <see cref="Populate"/>.
        /// </summary>
        /// <param name="optionsView"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual int DoPopulate(OptionsPropertySheetView optionsView, List<ISheet> result)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region SetInitialValues
        /// <summary>
        /// Called once by the options view to allow the parent page to record any initial values in any property sheets
        /// it might be using to take input.
        /// </summary>
        public virtual void SetInitialValues()
        {
        }
        #endregion

        #region GetSettingButtonText, SettingButtonClicked
        /// <summary>
        /// Gets the setting button text to show for all child sheets.
        /// </summary>
        /// <returns></returns>
        public string GetSettingButtonText()
        {
            return DoGetSettingButtonText();
        }

        /// <summary>
        /// When overridden by a derivee this performs the work for <see cref="GetSettingButtonText"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual string DoGetSettingButtonText()
        {
            return null;
        }

        /// <summary>
        /// Called when the setting button has been clicked on a child sheet.
        /// </summary>
        public void SettingButtonClicked(ISheet sheet)
        {
            DoSettingButtonClicked(sheet);
        }

        /// <summary>
        /// When overridden by a derivee this performs the work for <see cref="SettingButtonClicked"/>.
        /// </summary>
        /// <param name="ambiguousSheet"></param>
        protected virtual void DoSettingButtonClicked(ISheet ambiguousSheet)
        {
        }
        #endregion

        #region SynchroniseValues, DoUpdateViewWithNewList
        /// <summary>
        /// Synchronises changes in the property sheet for a record with the record held by the view.
        /// </summary>
        /// <param name="sheet"></param>
        public void SynchroniseValues(ISheet sheet)
        {
            DoSynchroniseValues(sheet);
            DoUpdateViewWithNewList();
        }

        /// <summary>
        /// When overridden by the derivee this does the work for <see cref="SynchroniseValues"/>.
        /// </summary>
        /// <param name="ambiguousSheet"></param>
        protected virtual void DoSynchroniseValues(ISheet ambiguousSheet)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden by the derivee this does the work for <see cref="SynchroniseValues"/>.
        /// </summary>
        protected virtual void DoUpdateViewWithNewList()
        {
            ;
        }
        #endregion

        #region ShowNewRecord, RemoveRecordForSheet
        /// <summary>
        /// Tells the view about the introduction of a brand-new record.
        /// </summary>
        /// <param name="sheet"></param>
        protected virtual void ShowNewRecord(ISheet sheet)
        {
            if(View != null) View.ShowNewChildSheet(this, sheet);
            DoUpdateViewWithNewList();
        }

        /// <summary>
        /// Called by the view to remove a record from the collection associated with this page.
        /// </summary>
        /// <param name="sheet"></param>
        public void RemoveRecordForSheet(ISheet sheet)
        {
            DoRemoveRecordForSheet(sheet);
            DoUpdateViewWithNewList();
        }

        /// <summary>
        /// When overridden by a derivee this does the work for <see cref="RemoveRecordForSheet"/>.
        /// </summary>
        /// <param name="ambiguousSheet"></param>
        protected virtual void DoRemoveRecordForSheet(ISheet ambiguousSheet)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region GenerateUniqueId, GenerateUniqueName
        /// <summary>
        /// Creates a unique ID for a new record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records"></param>
        /// <param name="getUniqueId"></param>
        /// <returns></returns>
        protected virtual int GenerateUniqueId<T>(List<T> records, Func<T, int> getUniqueId)
        {
            var result = _NextUniqueId;
            while(records.Any(r => getUniqueId(r) == result)) {
                ++result;
            }
            _NextUniqueId = result + 1;

            return result;
        }

        /// <summary>
        /// Creates a unique case-insensitive name for a new record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records"></param>
        /// <param name="prefix"></param>
        /// <param name="alwaysAppendCounter"></param>
        /// <param name="getName"></param>
        /// <returns></returns>
        protected virtual string GenerateUniqueName<T>(List<T> records, string prefix, bool alwaysAppendCounter, Func<T, string> getName)
        {
            string result;
            int counter = 1;
            do {
                result = counter != 1 || alwaysAppendCounter ? String.Format("{0} {1}", prefix, counter) : prefix;
                ++counter;
            } while(records.Any(r => result.Equals(getName(r), StringComparison.OrdinalIgnoreCase)));

            return result;
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called after the page control has been created but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Control(this);
            }
        }
        #endregion
    }
}
