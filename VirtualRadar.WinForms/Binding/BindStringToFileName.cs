using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.WinForms.Controls;
using System.Windows.Forms;

namespace VirtualRadar.WinForms.Binding
{
    public class BindStringToFileName : Binder<FileNameControl>
    {
        private Observable<string> CastObservable
        {
            get { return (Observable<string>)Observable; }
        }

        public BindStringToFileName(Observable<string> observable, FileNameControl control) : base(observable, control)
        {
        }

        protected override object GetControlValue()
        {
            return Control.FileName;
        }

        protected override void SetControlFromObservable()
        {
            Control.FileName = CastObservable.Value;
        }

        protected override void SetObservableFromControl()
        {
            CastObservable.Value = Control.FileName;
        }

        protected override void HookControlChanged()
        {
            Control.FileNameTextChanged += Control_ValueChanged;
        }

        protected override void UnhookControlChanged()
        {
            Control.FileNameTextChanged -= Control_ValueChanged;
        }
    }
}
