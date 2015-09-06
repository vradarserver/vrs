// Copyright © 2015 onwards, Andrew Whewell
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A panel that maps a single value to a set of radio buttons. A bit like <see cref="RadioPanelApp.RadioPanel"/>
    /// but without the binding.
    /// </summary>
    public class RadioButtonPanel : Panel
    {
        /// <summary>
        /// A private class that holds the mapping between radio buttons and values.
        /// </summary>
        protected class RadioButtonValue
        {
            /// <summary>
            /// Gets or sets the radio button that has a value mapped to it.
            /// </summary>
            public RadioButton RadioButton { get; set; }

            /// <summary>
            /// Gets or sets the value mapped to the radio button.
            /// </summary>
            public object Value { get; set; }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="radioButton"></param>
            /// <param name="value"></param>
            public RadioButtonValue(RadioButton radioButton, object value)
            {
                RadioButton = radioButton;
                Value = value;
            }
        }

        /// <summary>
        /// True if the <see cref="CheckedChanged"/> event is to be suppressed.
        /// </summary>
        protected bool _SuppressChangedEvent;

        /// <summary>
        /// The values associated with each radio button.
        /// </summary>
        protected List<RadioButtonValue> _RadioButtonValues = new List<RadioButtonValue>();

        /// <summary>
        /// Gets the collection of radio buttons within the panel.
        /// </summary>
        public RadioButton[] RadioButtons
        {
            get { return Controls.OfType<RadioButton>().ToArray(); }
        }

        /// <summary>
        /// Gets the checked radio button.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RadioButton CheckedRadioButton
        {
            get { return RadioButtons.FirstOrDefault(r => r.Checked); }
            set {
                var suppressChangedEvent = _SuppressChangedEvent;
                _SuppressChangedEvent = true;
                var raiseChanged = false;
                try {
                    foreach(var radioButton in RadioButtons) {
                        var setChecked = radioButton == value;
                        if(setChecked != radioButton.Checked) {
                            radioButton.Checked = setChecked;
                            raiseChanged = true;
                        }
                    }
                    if(raiseChanged) {
                        OnCheckedChanged(EventArgs.Empty);
                    }
                } finally {
                    _SuppressChangedEvent = suppressChangedEvent;
                }
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the checked radio button or null if no button is checked.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Value
        {
            get {
                var checkedRadioButton = CheckedRadioButton;
                var radioButtonValue = checkedRadioButton == null ? null : _RadioButtonValues.First(r => r.RadioButton == checkedRadioButton);
                return radioButtonValue == null ? null : radioButtonValue.Value;
            }
            set {
                var radioButtonValue = _RadioButtonValues.FirstOrDefault(r => Object.Equals(r.Value, value));
                if(radioButtonValue != null) {
                    CheckedRadioButton = radioButtonValue.RadioButton;
                }
            }
        }

        /// <summary>
        /// Raised when one of the radio button's checked state changes.
        /// </summary>
        public event EventHandler CheckedChanged;

        /// <summary>
        /// Raises <see cref="CheckedChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnCheckedChanged(EventArgs args)
        {
            if(!_SuppressChangedEvent && CheckedChanged != null) CheckedChanged(this, args);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if(disposing) {
                RemoveAllButtonMappings();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Maps a value to a radio button.
        /// </summary>
        /// <param name="radioButton"></param>
        /// <param name="value"></param>
        public void MapButtonValue(RadioButton radioButton, object value)
        {
            if(RadioButtons.Contains(radioButton) && !_RadioButtonValues.Any(r => radioButton == r.RadioButton)) {
                var radioButtonValue = new RadioButtonValue(radioButton, value);
                HookRadioButton(radioButton);
                _RadioButtonValues.Add(radioButtonValue);
            }
        }

        /// <summary>
        /// Maps values to every radio button in the control.
        /// </summary>
        /// <param name="getMappingValue"></param>
        public void MapButtonValues(Func<RadioButton, object> getMappingValue)
        {
            foreach(var radioButton in RadioButtons) {
                var value = getMappingValue(radioButton);
                if(value == null) {
                    throw new InvalidOperationException(String.Format("Need to supply a value for radio button {0}", radioButton.Name));
                }
                MapButtonValue(radioButton, value);
            }
        }

        /// <summary>
        /// Removes the mapping of a value to a radio button.
        /// </summary>
        /// <param name="radioButton"></param>
        public void RemoveButtonMapping(RadioButton radioButton)
        {
            var radioButtonValue = _RadioButtonValues.FirstOrDefault(r => r.RadioButton == radioButton);
            if(radioButtonValue != null) {
                UnhookRadioButton(radioButton);
                _RadioButtonValues.Remove(radioButtonValue);
            }
        }

        /// <summary>
        /// Removes the mapping of all radio buttons.
        /// </summary>
        public void RemoveAllButtonMappings()
        {
            foreach(var radioButtonValue in _RadioButtonValues) {
                UnhookRadioButton(radioButtonValue.RadioButton);
            }
            _RadioButtonValues.Clear();
        }

        /// <summary>
        /// Hooks the interesting events on the radio button.
        /// </summary>
        /// <param name="radioButton"></param>
        private void HookRadioButton(RadioButton radioButton)
        {
            radioButton.CheckedChanged += RadioButton_CheckedChanged;
        }

        /// <summary>
        /// Unhooks the interesting events on the radio button.
        /// </summary>
        /// <param name="radioButton"></param>
        private void UnhookRadioButton(RadioButton radioButton)
        {
            radioButton.CheckedChanged -= RadioButton_CheckedChanged;
        }

        /// <summary>
        /// Called when one of the mapped radio buttons changes value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void RadioButton_CheckedChanged(object sender, EventArgs args)
        {
            OnCheckedChanged(args);
        }
    }
}
