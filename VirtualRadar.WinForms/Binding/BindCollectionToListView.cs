using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using VirtualRadar.Interface;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.Binding
{
    /// <summary>
    /// A binder that manages the binding between a list and a binding list view.
    /// </summary>
    public class BindCollectionToListView : Binder<BindingListView>
    {
        public BindCollectionToListView(IObservable observable, BindingListView control) : base(observable, control)
        {
        }

        protected override void SetControlFromObservable()
        {
            Control.Records = (IList)Observable.GetValue();
        }

        protected override void SetObservableFromControl()
        {
            Observable.SetValue(Control.Records);
        }

        protected override void HookControlChanged()
        {
            ;
        }

        protected override void UnhookControlChanged()
        {
            ;
        }

        protected override bool ControlValueEqualsObservableValue(bool fromControlToObservable)
        {
            var observableList = (Observable.GetValue() as IList) ?? new object[]{};
            var controlList = Control.Records ?? new object[]{};

            return observableList.HasSameContentAs(controlList);
        }
    }
}
