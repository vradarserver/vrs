using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.Binding
{
    public class BindStringToFolder : Binder<FolderControl>
    {
        private Observable<string> CastObservable
        {
            get { return (Observable<string>)Observable; }
        }

        public BindStringToFolder(Observable<string> observable, FolderControl control) : base(observable, control)
        {
        }

        protected override object GetControlValue()
        {
            return Control.Folder;
        }

        protected override void SetControlFromObservable()
        {
            Control.Folder = CastObservable.Value;
        }

        protected override void SetObservableFromControl()
        {
            CastObservable.Value = Control.Folder;
        }

        protected override void HookControlChanged()
        {
            Control.FolderTextChanged += Control_ValueChanged;
        }

        protected override void UnhookControlChanged()
        {
            Control.FolderTextChanged -= Control_ValueChanged;
        }
    }
}
