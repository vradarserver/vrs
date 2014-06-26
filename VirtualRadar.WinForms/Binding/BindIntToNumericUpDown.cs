using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VirtualRadar.WinForms.Binding
{
    /// <summary>
    /// Binds integers to numeric up-downs.
    /// </summary>
    public class BindIntToNumericUpDown : Binder<NumericUpDown>
    {
        public Observable<int> CastObservable { get { return (Observable<int>)Observable; } }

        public BindIntToNumericUpDown(Observable<int> observable, NumericUpDown control) : base(observable, control)
        {
        }

        protected override object GetControlValue()
        {
            return (int)Control.Value;
        }

        protected override void SetControlFromObservable()
        {
            Control.Value = CastObservable.Value;
        }

        protected override void SetObservableFromControl()
        {
            CastObservable.Value = (int)GetControlValue();
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
