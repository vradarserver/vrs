// Copyright © 2014 onwards, Andrew Whewell
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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.Settings;
using VirtualRadar.WinForms.PortableBinding;
using VirtualRadar.Localisation;
using VirtualRadar.Interface;
using VirtualRadar.Interface.PortableBinding;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A control that can let the user enter a default access behaviour and a list of CIDRs.
    /// </summary>
    public partial class AccessControl : BaseUserControl
    {
        #region Fields
        /// <summary>
        /// Suppresses copies between the model and the controls.
        /// </summary>
        private bool _SuppressUpdates;

        /// <summary>
        /// The list of descriptions for the different values of <see cref="DefaultAccess"/>.
        /// </summary>
        private ItemDescriptionList<DefaultAccess> _DefaultAccessItems;

        /// <summary>
        /// Converts from the generic addresses list to a non-generic IList.
        /// </summary>
        private GenericListWrapper<string> _AddressesWrapper;

        /// <summary>
        /// The starting left position for the fields.
        /// </summary>
        private int _InitialFieldLeft;

        /// <summary>
        /// The gap between the list view and the right-hand side of the control.
        /// </summary>
        private int _ListViewRightGap;
        #endregion

        #region Properties
        private DefaultAccess _DefaultAccess;
        /// <summary>
        /// Gets or sets the default access chosen by the user.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DefaultAccess DefaultAccess
        {
            get { return _DefaultAccess; }
            set {
                if(_DefaultAccess != value) {
                    _DefaultAccess = value;
                    CopyDefaultAccessToControl();
                    OnDefaultAccessChanged(EventArgs.Empty);
                }
            }
        }

        private NotifyList<string> _Addresses = new NotifyList<string>();
        /// <summary>
        /// Gets the list of addresses chosen by the user.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList<string> Addresses
        {
            get { return _Addresses; }
        }

        private int _AlignmentFieldLeftPosition;
        /// <summary>
        /// Gets or sets the X position of the fields. Set to 0 to leave at default.
        /// </summary>
        [DefaultValue(0)]
        public int AlignmentFieldLeftPosition
        {
            get { return _AlignmentFieldLeftPosition; }
            set {
                if(_AlignmentFieldLeftPosition != value) {
                    _AlignmentFieldLeftPosition = value;
                    SetAlignmentFieldXPosition();
                }
            }
        }

        /// <summary>
        /// Sets the left position of the input fields.
        /// </summary>
        private void SetAlignmentFieldXPosition()
        {
            if(!DesignMode) {
                var left = AlignmentFieldLeftPosition == 0 ? _InitialFieldLeft : AlignmentFieldLeftPosition;
                comboBoxDefaultAccess.Left = left;
                listView.Left = left;
                listView.Width = (Width - _ListViewRightGap) - listView.Left;
            }
        }
        #endregion

        #region Events exposed
        /// <summary>
        /// Raised after <see cref="DefaultAccess"/> has changed.
        /// </summary>
        public event EventHandler DefaultAccessChanged;

        /// <summary>
        /// Raises <see cref="DefaultAccessChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnDefaultAccessChanged(EventArgs args)
        {
            EventHelper.Raise(DefaultAccessChanged, this, args);
        }

        /// <summary>
        /// Raised after <see cref="Addresses"/> has changed.
        /// </summary>
        public event EventHandler AddressesChanged;

        /// <summary>
        /// Raises <see cref="AddressesChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnAddressesChanged(EventArgs args)
        {
            EventHelper.Raise(AddressesChanged, this, args);
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AccessControl()
        {
            InitializeComponent();
            _InitialFieldLeft = comboBoxDefaultAccess.Left;
            _ListViewRightGap = Width - listView.Right;
        }
        #endregion

        #region OnLoad
        /// <summary>
        /// See base class docs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Control(this);

                var defaultAccesses = Enum.GetValues(typeof(DefaultAccess)).OfType<DefaultAccess>().ToArray();
                _DefaultAccessItems = new ItemDescriptionList<DefaultAccess>(defaultAccesses, r => Describe.DefaultAccess(r));
                foreach(var item in _DefaultAccessItems) {
                    comboBoxDefaultAccess.Items.Add(item);
                }
                CopyDefaultAccessToControl();

                _AddressesWrapper = new GenericListWrapper<string>(Addresses);
                _Addresses.ListChanged += Addresses_ListChanged;
                CopyAddressesToControl();
            }
        }
        #endregion

        #region RefreshAddresses, UpdateLabels, EnableDisableControls
        /// <summary>
        /// Refreshes the content of the Addresses list.
        /// </summary>
        /// <param name="addresses"></param>
        public void RefreshAddresses(IList<string> addresses)
        {
            var suppressUpdates = _SuppressUpdates;
            _SuppressUpdates = true;
            try {
                Addresses.Clear();
                Addresses.AddRange(addresses);
            } finally {
                _SuppressUpdates = suppressUpdates;
            }

            CopyAddressesToControl();
        }

        /// <summary>
        /// Updates the label text to reflect the default permissions.
        /// </summary>
        private void UpdateLabels()
        {
            var currentText = labelCidrList.Text;

            var text = "";
            switch(DefaultAccess) {
                case DefaultAccess.Allow:           text = Strings.DenyTheseAddresses; break;
                case DefaultAccess.Deny:            text = Strings.AllowTheseAddresses; break;
                case DefaultAccess.Unrestricted:    text = Strings.AllowTheseAddresses; break;
                default:                            throw new NotImplementedException();
            }

            if(text != currentText) labelCidrList.Text = String.Format("{0}:", text);
        }

        /// <summary>
        /// Enables / disables controls.
        /// </summary>
        private void EnableDisableControls()
        {
            listView.Enabled = DefaultAccess != DefaultAccess.Unrestricted;
        }
        #endregion

        #region Copy to/from controls
        /// <summary>
        /// Synchronises the default access combo box with our idea of what the selected value should be.
        /// </summary>
        private void CopyDefaultAccessToControl()
        {
            if(_DefaultAccessItems != null && !_SuppressUpdates) {
                _SuppressUpdates = true;
                try {
                    comboBoxDefaultAccess.SelectedItem = _DefaultAccessItems.FirstOrDefault(r => r.Item == _DefaultAccess);
                } finally {
                    _SuppressUpdates = false;
                }
            }
        }

        /// <summary>
        /// Copies the default access selected by the user to the property we expose.
        /// </summary>
        private void CopyControlToDefaultAccess()
        {
            if(_DefaultAccessItems != null && !_SuppressUpdates) {
                _SuppressUpdates = true;
                try {
                    var selectedItem = comboBoxDefaultAccess.SelectedItem as ItemDescription<DefaultAccess>;
                    if(selectedItem != null) DefaultAccess = selectedItem.Item;
                } finally {
                    _SuppressUpdates = false;
                }
            }

            UpdateLabels();
            EnableDisableControls();
        }

        /// <summary>
        /// Synchronises the content of the addresses list with the list of CIDR addresses.
        /// </summary>
        private void CopyAddressesToControl()
        {
            if(_AddressesWrapper != null && !_SuppressUpdates) {
                _SuppressUpdates = true;
                try {
                    if(listView.List == null) listView.List = _AddressesWrapper;
                    else                      listView.RefreshList();

                    OnAddressesChanged(EventArgs.Empty);
                } finally {
                    _SuppressUpdates = false;
                }
            }
        }
        #endregion

        #region AddOrInsertCidr
        /// <summary>
        /// Adds or inserts the CIDR address to the CIDR list.
        /// </summary>
        /// <param name="cidrText"></param>
        /// <param name="index"></param>
        private void AddOrInsertCidr(string cidrText, int index = -1)
        {
            Cidr cidr;
            if(Cidr.TryParse(cidrText, out cidr)) {
                var normalisedCidr = String.Format("{0}/{1}", cidr.MaskedAddress, cidr.BitmaskBits);
                if(!Addresses.Contains(normalisedCidr)) {
                    if(index == -1 || index >= Addresses.Count) Addresses.Add(normalisedCidr);
                    else                                        Addresses.Insert(index, normalisedCidr);
                }

                listView.SelectedRecord = normalisedCidr;
            }
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called when the default access combo box changes value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxDefaultAccess_SelectedIndexChanged(object sender, EventArgs e)
        {
            CopyControlToDefaultAccess();
        }

        /// <summary>
        /// Called when the list of addresses is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Addresses_ListChanged(object sender, ListChangedEventArgs e)
        {
            CopyAddressesToControl();
        }

        /// <summary>
        /// Called when the list view wants the columns for a CIDR address.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_FetchRecordContent(object sender, ListContentEventArgs e)
        {
            Cidr cidr = null;
            var cidrText = e.Record as string;
            if(!String.IsNullOrEmpty(cidrText)) Cidr.TryParse(cidrText, out cidr);

            e.ColumnTexts.Add(cidrText);
            e.ColumnTexts.Add(cidr == null ? "" : cidr.FirstMatchingAddress.ToString());
            e.ColumnTexts.Add(cidr == null ? "" : cidr.LastMatchingAddress.ToString());
        }

        /// <summary>
        /// Called when the list view wants to add a CIDR.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_AddClicked(object sender, EventArgs e)
        {
            using(var dialog = new CidrEditView()) {
                dialog.ShowDialog();
                if(dialog.CidrIsValid) {
                    AddOrInsertCidr(dialog.Cidr);
                }
            }
        }

        /// <summary>
        /// Called when the list view wants to edit a CIDR.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_EditClicked(object sender, EventArgs e)
        {
            var selectedCidrText = listView.SelectedRecord as string;
            var index = selectedCidrText == null ? -1 : Addresses.IndexOf(selectedCidrText);
            if(index != -1) {
                using(var dialog = new CidrEditView()) {
                    dialog.Cidr = selectedCidrText;
                    dialog.ShowDialog();
                    if(dialog.CidrIsValid) {
                        Addresses.RemoveAt(index);
                        AddOrInsertCidr(dialog.Cidr, index);
                    }
                }
            }
        }

        /// <summary>
        /// Called when the user wants to remove a CIDR.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_DeleteClicked(object sender, EventArgs e)
        {
            var selectedCidrs = listView.SelectedRecords.OfType<string>().ToArray();
            foreach(var selectedCidr in selectedCidrs) {
                var index = selectedCidr == null ? -1 : Addresses.IndexOf(selectedCidr);
                if(index != -1) Addresses.RemoveAt(index);
            }
        }
        #endregion
    }
}
