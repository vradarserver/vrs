using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.Binding
{
    /// <summary>
    /// Binds a plain value to a ComboBox control.
    /// </summary>
    public class BindToComboBox : Binder<ComboBox>
    {
        public BindToComboBox(IObservable observable, ComboBox control) : base(observable, control)
        {
        }

        protected override object GetControlValue()
        {
            return Control.SelectedValue;
        }

        protected override void SetControlFromObservable()
        {
            var value = Observable.GetValue();
            if(value != null) Control.SelectedValue = value;        // Can't set ComboBox.SelectedValue to null
        }

        protected override void SetObservableFromControl()
        {
            Observable.SetValue(Control.SelectedValue);
        }

        protected override void HookControlChanged()
        {
            Control.SelectedValueChanged += Control_ValueChanged;
        }

        protected override void UnhookControlChanged()
        {
            Control.SelectedValueChanged -= Control_ValueChanged;
        }
    }
}
