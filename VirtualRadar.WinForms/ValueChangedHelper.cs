// Copyright © 2013 onwards, Andrew Whewell
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

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// A utility class that hooks the appropriate value-changed event on controls and performs some action when the value changes.
    /// </summary>
    public class ValueChangedHelper
    {
        /// <summary>
        /// The action to take when a value changes on any hooked control.
        /// </summary>
        private Action<object, EventArgs> _ValueChangedAction;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="valueChangedAction"></param>
        public ValueChangedHelper(Action<object, EventArgs> valueChangedAction)
        {
            _ValueChangedAction = valueChangedAction;
        }

        /// <summary>
        /// Hooks all of the controls / control wrappers passed across.
        /// </summary>
        /// <param name="controls"></param>
        public void HookValueChanged(IEnumerable<object> controls)
        {
            foreach(var control in controls) {
                var         hooked = HookControl<TextBox>(control,          r => r.TextChanged += ValueChanged);
                if(!hooked) hooked = HookControl<ComboBox>(control,         r => r.SelectedIndexChanged += ValueChanged);
                if(!hooked) hooked = HookControl<NumericUpDown>(control,    r => r.ValueChanged += ValueChanged);
                if(!hooked) hooked = HookControl<DateTimePicker>(control,   r => r.ValueChanged += ValueChanged);
                if(!hooked) hooked = HookControl<CheckBox>(control,         r => r.CheckedChanged += ValueChanged);
                if(!hooked) hooked = HookControl<FileNameControl>(control,  r => r.FileNameTextChanged += ValueChanged);
                if(!hooked) hooked = HookControl<FolderControl>(control,    r => r.FolderTextChanged += ValueChanged);

                if(!hooked) throw new NotImplementedException($"Need to add code to hook {control.GetType().Name} controls");
            }
        }

        private bool HookControl<T>(object control, Action<T> hookControl)
            where T: class
        {
            var castControl = control as T;
            var result = castControl != null;
            if(result) hookControl(castControl);

            return result;
        }

        /// <summary>
        /// The event raised when a control changes value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ValueChanged(object sender, EventArgs args)
        {
            if(_ValueChangedAction != null) _ValueChangedAction(sender, args);
        }
    }
}
