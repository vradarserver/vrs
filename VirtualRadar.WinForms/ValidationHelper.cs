// Copyright © 2010 onwards, Andrew Whewell
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
using VirtualRadar.Interface.View;
using System.Windows.Forms;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// An object that helps views display validation results correctly.
    /// </summary>
    public class ValidationHelper
    {
        /// <summary>
        /// A map of validation fields to the corresponding controls.
        /// </summary>
        private Dictionary<ValidationField, Control> _ValidationFieldMap = new Dictionary<ValidationField,Control>();

        /// <summary>
        /// The error provider that will be used to display validation results.
        /// </summary>
        private ErrorProvider _ErrorProvider;

        /// <summary>
        /// The error provider that will be used to display warnings to the user.
        /// </summary>
        private ErrorProvider _WarningProvider;

        /// <summary>
        /// Gets a value indicating whether the last set of validation results displayed represented a failed
        /// validation.
        /// </summary>
        public bool LastValidationFailed { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="errorProvider"></param>
        /// <param name="warningProvider"></param>
        public ValidationHelper(ErrorProvider errorProvider, ErrorProvider warningProvider = null)
        {
            _ErrorProvider = errorProvider;
            _WarningProvider = warningProvider;
        }

        /// <summary>
        /// Tells the helper which fields the view knows about and which control corresponds to which field.
        /// </summary>
        /// <param name="validationField"></param>
        /// <param name="control"></param>
        /// <remarks>
        /// It is not permissable to register the same field or control twice.
        /// </remarks>
        public void RegisterValidationField(ValidationField validationField, Control control)
        {
            if(control == null) throw new ArgumentNullException("control");
            if(_ValidationFieldMap.ContainsKey(validationField)) throw new InvalidOperationException($"An attempt was made to register the {validationField} validation field twice");
            if(_ValidationFieldMap.Where(kvp => kvp.Value == control).Any()) throw new InvalidOperationException($"An attempt was made to register the {control.Name} control twice");

            _ValidationFieldMap.Add(validationField, control);
        }

        /// <summary>
        /// Displays a set of validation results.
        /// </summary>
        /// <param name="validationResults"></param>
        public void ShowValidationResults(ValidationResults validationResults)
        {
            if(!validationResults.IsPartialValidation) {
                ClearAllMessages();
            } else {
                foreach(var fieldChecked in validationResults.PartialValidationFields) {
                    ClearAllMessages(fieldChecked.Field);
                }
            }
            LastValidationFailed = false;

            ShowValidationResultsAgainstControls(validationResults);

            if(validationResults.HasErrors) {
                LastValidationFailed = true;

                var donorControl = _ValidationFieldMap.Values.FirstOrDefault();
                var form = donorControl == null ? null : donorControl.FindForm();
                if(form != null) form.DialogResult = DialogResult.None;
            }
        }

        /// <summary>
        /// Displays validation results against controls.
        /// </summary>
        /// <param name="validationResults"></param>
        private void ShowValidationResultsAgainstControls(ValidationResults validationResults)
        {
            foreach(var validationResult in validationResults.Results) {
                Control control;
                if(_ValidationFieldMap.TryGetValue(validationResult.Field, out control)) {
                    var validateDelegate = control as IValidateDelegate;
                    if(validateDelegate != null) control = validateDelegate.GetValidationDisplayControl(validationResult.IsWarning ? _WarningProvider : _ErrorProvider);

                    var errorProvider = validationResult.IsWarning ? _WarningProvider : _ErrorProvider;
                    errorProvider.SetError(control, validationResult.Message);
                }
            }
        }

        /// <summary>
        /// Removes all error messages.
        /// </summary>
        /// <param name="justForField"></param>
        private void ClearAllMessages(ValidationField justForField = ValidationField.None)
        {
            foreach(var kvp in _ValidationFieldMap) {
                if(justForField != ValidationField.None && kvp.Key != justForField) {
                    continue;
                }

                var control = kvp.Value;
                var validateDelegate = control as IValidateDelegate;
                if(validateDelegate != null) {
                    if(_WarningProvider != null) control = validateDelegate.GetValidationDisplayControl(_WarningProvider);
                    if(_ErrorProvider != null)   control = validateDelegate.GetValidationDisplayControl(_ErrorProvider);
                }

                if(_ErrorProvider != null)      _ErrorProvider.SetError(control, null);
                if(_WarningProvider != null)    _WarningProvider.SetError(control, null);
            }
        }
    }
}
