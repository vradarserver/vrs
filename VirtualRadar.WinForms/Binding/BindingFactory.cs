// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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
                else if(IsKindOf<PasswordControl>(control)) result = new BindStringToPassword((Observable<string>)observable, (PasswordControl)control);
                else if(IsKindOf<TextBox>(control))         result = new BindStringToTextBox((Observable<string>)observable, (TextBox)control);
            } else if(valueType == typeof(bool)) {
                if(IsKindOf<CheckBox>(control))             result = new BindBoolToCheckbox((Observable<bool>)observable, (CheckBox)control);
            } else if(valueType == typeof(int)) {
                if(IsKindOf<NumericUpDown>(control))        result = new BindIntToNumericUpDown((Observable<int>)observable, (NumericUpDown)control);
            } else if(valueType == typeof(double)) {
                if(IsKindOf<NumericUpDown>(control))        result = new BindDoubleToNumericUpDown((Observable<double>)observable, (NumericUpDown)control);
            } else if(isList) {
                if(IsKindOf<ObservableListView>(control))   result = new BindCollectionToObservableListView((IObservableList)observable, (ObservableListView)control);
                else if(IsKindOf<BindingListView>(control)) result = new BindCollectionToListView((IObservableList)observable, (BindingListView)control);
            }

            if(result == null && IsKindOf<ComboBox>(control)) {
                result = new BindToComboBox(observable, (ComboBox)control);
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
            var result = typeof(T).IsAssignableFrom(control.GetType());

            return result;
        }
    }
}
