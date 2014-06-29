using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.WinForms.Controls;
using System.Collections;

namespace VirtualRadar.WinForms.Binding
{
    /// <summary>
    /// Binds a list to a control that shows a master list and binds the collection
    /// to a editable subset of that list.
    /// </summary>
    public class BindCollectionToObservableListView : Binder<ObservableListView>
    {
        public IObservableList CastObservable { get { return (IObservableList)Observable; } }

        public BindCollectionToObservableListView(IObservableList observable, ObservableListView control) : base(observable, control)
        {
        }

        protected override void SetControlFromObservable()
        {
            var disconnectedList = CastObservable.GetListValue().OfType<object>().ToArray();
            Control.CheckedItemsList = disconnectedList;
        }

        protected override void SetObservableFromControl()
        {
            var checkedItems = Control.CheckedItemsList.OfType<object>();
            CastObservable.ReplaceContent(checkedItems);
        }

        protected override void HookControlChanged()
        {
            Control.CheckedItemsListChanged += Control_ValueChanged;
        }

        protected override void UnhookControlChanged()
        {
            Control.CheckedItemsListChanged -= Control_ValueChanged;
        }

        protected override bool ControlValueEqualsObservableValue(bool fromControlToObservable)
        {
            var observableList = CastObservable.GetListValue() ?? new object[]{};
            var controlList = Control.CheckedItemsList.OfType<object>().ToArray() ?? new object[]{};

            return observableList.HasSameContentAs(controlList, orderMustMatch: false);
        }
    }
}
