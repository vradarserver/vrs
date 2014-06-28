using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VirtualRadar.WinForms.Binding
{
    /// <summary>
    /// Binds doubles to numeric up-down controls.
    /// </summary>
    public class BindDoubleToNumericUpDown : Binder<NumericUpDown>
    {
        public Observable<double> CastObservable { get { return (Observable<double>)Observable; } }

        public BindDoubleToNumericUpDown(Observable<double> observable, NumericUpDown control) : base(observable, control)
        {
        }

        protected override object GetControlValue()
        {
            return (double)Control.Value;
        }

        protected override void SetControlFromObservable()
        {
            Control.Value = (decimal)CastObservable.Value;
        }

        protected override void SetObservableFromControl()
        {
            CastObservable.Value = (double)GetControlValue();
        }

        protected override void HookControlChanged()
        {
            Control.ValueChanged += Control_ValueChanged;
        }

        protected override void UnhookControlChanged()
        {
            Control.ValueChanged -= Control_ValueChanged;
        }
    }
}
