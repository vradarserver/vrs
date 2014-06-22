using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VirtualRadar.WinForms.Binding
{
    /// <summary>
    /// Binds strings to vanilla text boxes.
    /// </summary>
    public class BindStringToTextBox : Binder<TextBox>
    {
        private Observable<string> CastObservable { get { return (Observable<string>)Observable; } }

        public BindStringToTextBox(IObservable observable, TextBox control) : base(observable, control)
        {
        }

        protected override void SetControlFromObservable()
        {
            Control.Text = CastObservable.Value;
        }

        protected override void SetObservableFromControl()
        {
            CastObservable.Value = Control.Text.Trim();
        }

        protected override void HookControlChanged()
        {
            Control.TextChanged += Control_ValueChanged;
        }

        protected override void UnhookControlChanged()
        {
            Control.TextChanged -= Control_ValueChanged;
        }

        protected override bool ControlValueEqualsObservableValue(bool fromControlToObservable)
        {
            bool result = Control.Text == CastObservable.Value;

            // We trim the text that we copy out of the control and into the observable, but we
            // don't want the trimmed text to trigger an update of the control - it can keep the
            // trailing text, we don't care.
            if(!result && (Control.Text ?? "").Trim() == (CastObservable.Value ?? "").Trim()) {
                result = true;
            }

            return result;
        }
    }
}
