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
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// A form that can be used to edit a collection of receivers.
    /// </summary>
    public partial class ReceiverListView : BaseForm
    {
        #region Properties
        /// <summary>
        /// Gets a collection of every receiver.
        /// </summary>
        public List<Receiver> Receivers { get; private set; }

        /// <summary>
        /// Gets a collection of unique IDs for every selected receiver.
        /// </summary>
        public List<int> SelectedReceiverIds { get; private set; }

        /// <summary>
        /// Gets or sets the currently selected receiver out of all of the selected receivers.
        /// </summary>
        public Receiver CurrentListReceiver
        {
            get { return GetSelectedListViewTag<Receiver>(listView); }
            set { SelectListViewItemByTag(listView, value); }
        }

        /// <summary>
        /// Gets or sets the currently selected receiver out of all of the eligible receivers.
        /// </summary>
        public Receiver CurrentSingleReceiver
        {
            get { return GetSelectedComboBoxValue<Receiver>(comboBox); }
            set { SelectComboBoxItemByValue(comboBox, value); }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ReceiverListView()
        {
            InitializeComponent();
            Receivers = new List<Receiver>();
            SelectedReceiverIds = new List<int>();
        }
        #endregion

        #region PopulateListView, FillListViewItem, PopulateDropDown, EnableDisableControls
        /// <summary>
        /// Populates the list view with selected receivers.
        /// </summary>
        private void PopulateListView()
        {
            var selectedReceivers = ReceiversForSelectedReceiverIds();
            PopulateListView(listView, selectedReceivers, CurrentListReceiver, FillListViewItem, r => CurrentListReceiver = r);
        }

        private void FillListViewItem(ListViewItem item)
        {
            FillListViewItem<Receiver>(item, r => new string[] { r.Name });
        }

        /// <summary>
        /// Populates the dropdown with all receivers.
        /// </summary>
        private void PopulateDropDown()
        {
            FillDropDownWithValues(comboBox, Receivers, r => r.Name);
        }

        /// <summary>
        /// Enables or disables controls.
        /// </summary>
        private void EnableDisableControls()
        {
            var currentSingleReceiver = CurrentSingleReceiver;
            var currentListReceiver = CurrentListReceiver;
            var singleReceiverIsSelected = CurrentSingleReceiver != null && SelectedReceiverIds.Contains(currentSingleReceiver.UniqueId);

            buttonAdd.Enabled = currentSingleReceiver != null && !singleReceiverIsSelected;
            buttonRemove.Enabled = currentSingleReceiver != null && singleReceiverIsSelected;
        }
        #endregion

        #region ReceiversForReceiverIds
        /// <summary>
        /// Returns a list of receivers corresponding to the selected receivers.
        /// </summary>
        /// <returns></returns>
        private List<Receiver> ReceiversForSelectedReceiverIds()
        {
            return Receivers.Where(r => SelectedReceiverIds.Contains(r.UniqueId)).ToList();
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Form(this);

                PopulateListView();
                PopulateDropDown();
                EnableDisableControls();
            }
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            var currentSingleReceiver = CurrentSingleReceiver;
            if(currentSingleReceiver != null && !SelectedReceiverIds.Contains(currentSingleReceiver.UniqueId)) {
                SelectedReceiverIds.Add(currentSingleReceiver.UniqueId);
                var item = new ListViewItem() { Tag = currentSingleReceiver };
                FillListViewItem(item);
                listView.Items.Add(item);
                CurrentListReceiver = currentSingleReceiver;
            }

            EnableDisableControls();
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            var currentSingleReceiver = CurrentSingleReceiver;
            var selectedIdIndex = currentSingleReceiver == null ? -1 : SelectedReceiverIds.IndexOf(currentSingleReceiver.UniqueId);

            if(selectedIdIndex != -1) {
                var item = FindListViewItemForRecord(listView, currentSingleReceiver);
                listView.Items.Remove(item);
                SelectedReceiverIds.RemoveAt(selectedIdIndex);
                PopulateListView();
            }

            EnableDisableControls();
        }
        #endregion
    }
}
