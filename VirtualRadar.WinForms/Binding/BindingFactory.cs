using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.WinForms.Controls;
using System.Collections;

namespace VirtualRadar.WinForms.Binding
{
    /// <summary>
    /// Manages the creation of binders.
    /// </summary>
    public static class BindingFactory
    {
        public static IBinder CreateBinder(IObservable observable, Control control)
        {
            IBinder result = null;

            var valueType = observable.GetValueType();
            var isList = typeof(IEnumerable).IsAssignableFrom(valueType);

            if(valueType == typeof(string)) {
                if(IsKindOf<FileNameControl>(control))      result = new BindStringToFileName((Observable<string>)observable, (FileNameControl)control);
                else if(IsKindOf<FolderControl>(control))   result = new BindStringToFolder((Observable<string>)observable, (FolderControl)control);
                else if(IsKindOf<TextBox>(control))         result = new BindStringToTextBox((Observable<string>)observable, (TextBox)control);
            } else if(valueType == typeof(bool)) {
                if(IsKindOf<CheckBox>(control))             result = new BindBoolToCheckbox((Observable<bool>)observable, (CheckBox)control);
            } else if(valueType == typeof(int)) {
                var comboBox = control as ComboBox;
                if(comboBox.DropDownStyle == ComboBoxStyle.DropDownList) {
                    result = new BindIntToListComboBox((Observable<int>)observable, comboBox);
                }
            } else if(isList) {
                if(IsKindOf<BindingListView>(control))      result = new BindCollectionToListView(observable, (BindingListView)control);
            }

            if(result == null) {
                throw new NotImplementedException(String.Format("Need to implement a binder between {0} properties and {1} controls", valueType.Name, control.GetType().Name));
            }

            return result;
        }

        private static bool IsKindOf<T>(Control control)
            where T: Control
        {
            var controlType = control.GetType();
            var result = controlType.IsAssignableFrom(typeof(T));

            return result;
        }
    }
}
