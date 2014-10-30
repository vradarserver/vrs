using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;

namespace VirtualRadar.WinForms.PortableBinding
{
    /// <summary>
    /// A binder where the model has a list of values (usually identitifers) and the control holds
    /// a list of objects from which a subset can be chosen.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TControl"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TListModel"></typeparam>
    /// <remarks>
    /// Unlike <see cref="ValueFromListBinder"/>, which translates the list items into <see cref="ItemDescription"/>
    /// objects, this assumes that the control can hold items of <typeparamref name="TListModel"/> directly. The
    /// control cannot change the master list, it is a one-way binding from binder to control. The control is assumed
    /// to have some mechanism for marking which items are of interest to the user, it is these items that are bound
    /// to the model's list of values.
    /// </remarks>
    public abstract class ValueSubsetOfListBinder<TModel, TControl, TValue, TListModel> : ControlBinder
        where TControl: Control
    {
        #region Fields
        /// <summary>
        /// True if we are not to copy changes to the master list to the control.
        /// </summary>
        private bool _MasterListUpdatesLocked;

        /// <summary>
        /// True if the model list has had its list changed event hooked.
        /// </summary>
        private bool _ModelListHooked;

        /// <summary>
        /// True if the master list has had its list changed event hooked.
        /// </summary>
        private bool _MasterListHooked;
        #endregion

        #region Properties
        private IList<TListModel> _MasterList;
        /// <summary>
        /// Gets or sets the master list of all items the user can choose from. Cannot be set after initialisation.
        /// </summary>
        public IList<TListModel> MasterList
        {
            get { return _MasterList; }
            set { if(!Initialised) _MasterList = value; }
        }

        /// <summary>
        /// Gets the master list as an IBindingList.
        /// </summary>
        public IBindingList MasterListBindingList
        {
            get { return MasterList as IBindingList; }
        }

        /// <summary>
        /// Gets the model that owns the subset list.
        /// </summary>
        public TModel Model { get { return (TModel)ModelObject; } }

        private Expression<Func<TModel, IList<TValue>>> _GetModelValueExpression;
        private Func<TModel, IList<TValue>> _GetModelValue;
        /// <summary>
        /// Gets the model's value, the subset list.
        /// </summary>
        public IList<TValue> ModelValue
        {
            get { return _GetModelValue(Model); }
        }

        /// <summary>
        /// Gets the <see cref="ModelValue"/> list cast to an IBindingList.
        /// </summary>
        public IBindingList ModelValueBindingList
        {
            get { return ModelValue as IBindingList; }
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
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="control"></param>
        /// <param name="masterList"></param>
        /// <param name="getModelValue"></param>
        /// <param name="getRowValue"></param>
        public ValueSubsetOfListBinder(
            TModel model, TControl control, IList<TListModel> masterList,
            Expression<Func<TModel, IList<TValue>>> getModelValue)
            : base(model, control)
        {
            MasterList = masterList;
            _GetModelValueExpression = getModelValue;
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(disposing) {
                if(_MasterListHooked) {
                    MasterListBindingList.ListChanged -= MasterListBindingList_ListChanged;
                    _MasterListHooked = false;
                }
                if(_ModelListHooked) {
                    ModelValueBindingList.ListChanged -= ModelValueBindingList_ListChanged;
                    _ModelListHooked = false;
                }
            }
        }
        #endregion

        #region Initialise
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoInitialising()
        {
            _GetModelValue = _GetModelValueExpression.Compile();
            if(_GetListItemValue == null) {
                _GetListItemValue = (obj) => {
                    var boxed = (Object)obj;
                    var unboxed = (TValue)boxed;
                    return unboxed;
                };
            }

            if(ModelValueBindingList != null) {
                ModelValueBindingList.ListChanged += ModelValueBindingList_ListChanged;
                _ModelListHooked = true;
            }

            if(MasterListBindingList != null) {
                MasterListBindingList.ListChanged += MasterListBindingList_ListChanged;
                _MasterListHooked = true;
            }

            CopyMasterListToControl();

            base.DoInitialising();
        }
        #endregion

        #region ControlBinder Properties - ModelValueObject, ControlValueObject
        /// <summary>
        /// See base docs.
        /// </summary>
        public override object ModelValueObject
        {
            get             { return ModelValue; }
            protected set   { throw new InvalidOperationException("Attempt made to set the ModelValue for a list"); }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override object ControlValueObject
        {
            get             { return GetSubsetList(); }
            protected set   { throw new InvalidOperationException("Attempt made to set the ControlValue for a list"); }
        }
        #endregion

        #region CopyMasterListToControl
        /// <summary>
        /// Copies the master list to the control.
        /// </summary>
        protected void CopyMasterListToControl()
        {
            if(!_MasterListUpdatesLocked && !_UpdatesLocked) {
                _MasterListUpdatesLocked = true;
                _UpdatesLocked = true;
                try {
                    DoCopyMasterListToControl(MasterList);
                } finally {
                    _MasterListUpdatesLocked = false;
                    _UpdatesLocked = false;
                }

                CopyModelToControl();   // Copy in what we think should be set
                CopyControlToModel();   // Remove entries from model that no longer exist in control
            }
        }

        /// <summary>
        /// Copies the master list to the control, removing all sets.
        /// </summary>
        /// <param name="masterList"></param>
        protected abstract void DoCopyMasterListToControl(IList<TListModel> masterList);
        #endregion

        #region CopyModelToControl, CopyControlToModel
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoCopyModelToControl()
        {
            TValue[] onlyInControl, onlyInModel;
            GetDifferenceLists(out onlyInControl, out onlyInModel);

            foreach(var i in onlyInControl) {
                SetSubsetItem(i, set: false);
            }

            foreach(var i in onlyInModel) {
                SetSubsetItem(i, set: true);
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoCopyControlToModel()
        {
            TValue[] onlyInControl, onlyInModel;
            GetDifferenceLists(out onlyInControl, out onlyInModel);

            foreach(var i in onlyInModel) {
                ModelValue.Remove(i);
            }

            foreach(var i in onlyInControl) {
                ModelValue.Add(i);
            }
        }

        /// <summary>
        /// Builds lists of differences between what's in the model and what's in the control.
        /// </summary>
        /// <param name="onlyInControl"></param>
        /// <param name="onlyInModel"></param>
        private void GetDifferenceLists(out TValue[] onlyInControl, out TValue[] onlyInModel)
        {
            var modelItems = ModelValue;
            var controlItems = GetSubsetList();

            onlyInControl = controlItems.Where(r => !modelItems.Any(i => Object.Equals(r, i))).ToArray();
            onlyInModel = modelItems.Where(r => !controlItems.Any(i => Object.Equals(r, i))).ToArray();
        }

        /// <summary>
        /// Gets the list of chosen items from the control.
        /// </summary>
        /// <returns></returns>
        protected IList<TValue> GetSubsetList()
        {
            return DoGetSubsetList();
        }

        /// <summary>
        /// When overridden this gets the list of chosen items from the control.
        /// </summary>
        /// <returns></returns>
        protected abstract IList<TValue> DoGetSubsetList();

        /// <summary>
        /// Adds or removes an item to/from the subset held by the control.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="set"></param>
        protected void SetSubsetItem(TValue item, bool set)
        {
            DoSetSubsetItem(item, set);
        }

        /// <summary>
        /// Adds or removes an item to/from the subset.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="set"></param>
        protected abstract void DoSetSubsetItem(TValue item, bool set);
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called when the master list changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MasterListBindingList_ListChanged(object sender, ListChangedEventArgs e)
        {
            CopyMasterListToControl();
        }

        /// <summary>
        /// Called when the subset list changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ModelValueBindingList_ListChanged(object sender, ListChangedEventArgs e)
        {
            CopyModelToControl();
        }
        #endregion
    }
}
