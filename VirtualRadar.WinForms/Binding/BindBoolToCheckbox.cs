using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VirtualRadar.WinForms.Binding
{
    public class BindBoolToCheckbox : Binder<CheckBox>
    {
        private Observable<bool> CastObservable
        {
            get { return (Observable<bool>)base.Observable; }
        }

        public BindBoolToCheckbox(Observable<bool> observable, CheckBox control) : base(observable, control)
        {
        }

        protected override object GetControlValue()
        {
            return Control.Checked;
        }

        protected override void SetControlFromObservable()
        {
            Control.Checked = CastObservable.Value;
        }

        protected override void SetObservableFromControl()
        {
            CastObservable.Value = Control.Checked;
        }

        protected override void HookControlChanged()
        {
            Control.CheckedChanged += Control_ValueChanged;
        }

        protected override void UnhookControlChanged()
        {
            Control.CheckedChanged -= Control_ValueChanged;
        }
    }
}
