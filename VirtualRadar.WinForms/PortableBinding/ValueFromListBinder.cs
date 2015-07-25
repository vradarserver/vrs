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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms.PortableBinding
{
    /// <summary>
    /// The base class for binders that let the user choose a single value
    /// from a list of values. If the list implements <see cref="IBindingList"/> then changes to the list are copied into the control.
    /// </summary>
    /// <typeparam name="TModel">The type of object that holds the value bound to a property on one object in the list of TListModel objects.</typeparam>
    /// <typeparam name="TControl">The type of control that renders the list and lets the user choose a single item.</typeparam>
    /// <typeparam name="TValue">The type of value to copy from the selected item to the control.</typeparam>
    /// <typeparam name="TListModel">The type of value held in the binding list.</typeparam>
    /// <remarks>
    /// Changes in the bound property on the model change which item is shown as selected in the list.
    /// Changes to the list of models are reflected on the display.
    /// The value copied from the row to the model is assumed to be a value that uniquely identifies the
    /// row in the list.
    /// </remarks>
    public abstract class ValueFromListBinder<TModel, TControl, TValue, TListModel> : ValueBinder<TModel, TControl, TValue>
        where TModel: class, INotifyPropertyChanged
        where TControl: Control
    {
        #region Fields
        /// <summary>
        /// The list of descriptions that we're going to use for the list.
        /// </summary>
        protected ItemDescriptionList<TListModel> ItemDescriptions { get; private set; }

        /// <summary>
        /// True if copies between lists are locked.
        /// </summary>
        protected bool _ListUpdatesLocked;
        #endregion

        #region Properties
        private IList<TListModel> _List;
        /// <summary>
        /// Gets the list.
        /// </summary>
        public IList<TListModel> List
        {
            get { return _List; }
            set { if(!Initialised) _List = value; }
        }

        private Func<TListModel, string> _GetListItemDescription;
        /// <summary>
        /// Gets or sets a method that returns a list item's description. Cannot be set after initialisation.
        /// </summary>
        public Func<TListModel, string> GetListItemDescription
        {
            get { return _GetListItemDescription; }
            set { if(!Initialised) _GetListItemDescription = value; }
        }

        private Func<TListModel, TValue> _GetListItemValue;
        /// <summary>
        /// Gets or sets a method that returns the list item's value. Cannot be set after initialisation.
        /// </summary>
        public Func<TListModel, TValue> GetListItemValue
        {
            get { return _GetListItemValue; }
            set { if(!Initialised) _GetListItemValue = value; }
        }

        private bool _SortList;
        /// <summary>
        /// Gets or sets a value indicating that the list should be sorted.
        /// </summary>
        public bool SortList
        {
            get { return _SortList; }
            set { if(!Initialised) _SortList = value; }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="control"></param>
        /// <param name="list"></param>
        /// <param name="getModelValue"></param>
        /// <param name="setModelValue"></param>
        public ValueFromListBinder(
            TModel model, TControl control, IList<TListModel> list,
            Expression<Func<TModel, TValue>> getModelValue,
            Action<TModel, TValue> setModelValue)
            : this(model, control, list, getModelValue, setModelValue, null, null)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="control"></param>
        /// <param name="list"></param>
        /// <param name="getModelValue"></param>
        /// <param name="setModelValue"></param>
        /// <param name="getControlValue"></param>
        /// <param name="setControlValue"></param>
        public ValueFromListBinder(
            TModel model, TControl control, IList<TListModel> list,
            Expression<Func<TModel, TValue>> getModelValue,
            Action<TModel, TValue> setModelValue,
            Func<TControl, TValue> getControlValue,
            Action<TControl, TValue> setControlValue)
            : base(model, control, getModelValue, setModelValue, getControlValue, setControlValue)
        {
            List = list;
            _GetControlValue = (r) => GetSelectedListValue();
            _SetControlValue = (r,v) => SetSelectedListValue(v);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if(disposing) {
                if(ItemDescriptions != null) {
                    ItemDescriptions.Dispose();
                    ItemDescriptions.Clear();
                    ItemDescriptions = null;
                }
            }

            base.Dispose(disposing);
        }
        #endregion

        #region DoInitialising
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoInitialising()
        {
            if(_GetListItemDescription == null) {
                _GetListItemDescription = (obj) => obj.ToString();
            }
            if(_GetListItemValue == null) {
                _GetListItemValue = (obj) => {
                    var boxed = (Object)obj;
                    var unboxed = (TValue)boxed;
                    return unboxed;
                };
            }
            ItemDescriptions = new ItemDescriptionList<TListModel>(List, _GetListItemDescription);
            ItemDescriptions.Changed += ItemDescriptions_Changed;

            CopyListToControl();

            base.DoInitialising();
        }
        #endregion

        #region CopyListToControl, DoCopyListToControl, GetSelectedListValue, SetSelectedListValue
        /// <summary>
        /// Copies the list to the control.
        /// </summary>
        protected void CopyListToControl()
        {
            if(!_ListUpdatesLocked) {
                var selectedValue = GetSelectedListValue();

                _ListUpdatesLocked = true;
                try {
                    var list = SortList ? DoSortList(ItemDescriptions) : ItemDescriptions;
                    DoCopyListToControl(list);
                } finally {
                    _ListUpdatesLocked = false;
                }

                SetSelectedListValue(selectedValue);
            }
        }

        private IEnumerable<ItemDescription<TListModel>> DoSortList(ItemDescriptionList<TListModel> list)
        {
            var result = list.OrderBy(r => r.Description).ToArray();
            return result;
        }

        /// <summary>
        /// Copies the item descriptions to the control.
        /// </summary>
        /// <param name="itemDescriptions"></param>
        protected abstract void DoCopyListToControl(IEnumerable<ItemDescription<TListModel>> itemDescriptions);

        /// <summary>
        /// Gets the value for the selected item from the list.
        /// </summary>
        /// <returns></returns>
        protected TValue GetSelectedListValue()
        {
            return DoGetSelectedListValue();
        }

        /// <summary>
        /// Does the work of getting the selected item from the list.
        /// </summary>
        /// <returns></returns>
        protected abstract TValue DoGetSelectedListValue();

        /// <summary>
        /// Sets the value for the selected item in the list.
        /// </summary>
        /// <param name="value"></param>
        protected void SetSelectedListValue(TValue value)
        {
            DoSetSelectedListValue(value);
        }

        /// <summary>
        /// Does the work of setting the value for the selected item in the list.
        /// </summary>
        /// <param name="value"></param>
        protected abstract void DoSetSelectedListValue(TValue value);
        #endregion

        #region DoCopyModelToControl, DoCopyControlToModel
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoCopyModelToControl()
        {
            if(!_ListUpdatesLocked) {
                var value = ModelValue;
                SetSelectedListValue(value);

                var notInList = !Object.Equals(GetSelectedListValue(), value);
                if(notInList) DoCopyControlToModel();
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoCopyControlToModel()
        {
            if(!_ListUpdatesLocked) {
                var value = GetSelectedListValue();
                ModelValue = value;
            }
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called when the underlying list, or one of the values within the list, changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ItemDescriptions_Changed(object sender, EventArgs args)
        {
            CopyListToControl();
        }
        #endregion
    }
}
