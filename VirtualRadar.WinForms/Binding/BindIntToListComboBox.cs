using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VirtualRadar.WinForms.Binding
{
    public class BindIntToListComboBox : Binder<ComboBox>
    {
        public Observable<int> CastObservable { get { return (Observable<int>)Observable; } }

        public BindIntToListComboBox(Observable<int> observable, ComboBox control) : base(observable, control)
        {
        }

        protected override object GetControlValue()
        {
            return Control.SelectedValue == null ? 0 : Control.SelectedValue;
        }

        protected override void SetControlFromObservable()
        {
            Control.SelectedValue = CastObservable.Value;
        }

        protected override void SetObservableFromControl()
        {
            CastObservable.Value = (int)GetControlValue();
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
