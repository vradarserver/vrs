using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.WinForms.Binding;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A combo box that is hooked up to an observable list and exposes an
    /// identifier from the list.
    /// </summary>
    public class ObservableListComboBox : ComboBox
    {
        private bool _HookedObservableList;

        [RefreshProperties(RefreshProperties.Repaint)]
        [DefaultValue(ComboBoxStyle.DropDownList)]
        public new ComboBoxStyle DropDownStyle
        {
            get { return base.DropDownStyle; }
            set { base.DropDownStyle = value; }
        }

        private IObservableList _ObservableList;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IObservableList ObservableList
        {
            get { return _ObservableList; }
            set {
                UnhookObservableList();
                _ObservableList = value;
                Populate();
                HookObservableList();
            }
        }

        public ObservableListComboBox() : base()
        {
            base.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing) UnhookObservableList();
            base.Dispose(disposing);
        }

        private void UnhookObservableList()
        {
            if(_HookedObservableList) ObservableList.Changed -= ObservableList_Changed;
            _HookedObservableList = false;
        }

        private void HookObservableList()
        {
            if(!_HookedObservableList && ObservableList != null) {
                ObservableList.Changed += ObservableList_Changed;
                _HookedObservableList = true;
            }
        }

        protected virtual void Populate()
        {
            if(base.DataSource == null) base.DataSource = ObservableList.GetValue();
            base.DataManager.Refresh();
        }

        private void ObservableList_Changed(object sender, EventArgs args)
        {
            Populate();
        }
    }
}
