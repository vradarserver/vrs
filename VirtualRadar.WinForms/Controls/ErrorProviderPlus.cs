using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// An error provider that can be made to clear all of the errors that it has set.
    /// </summary>
    public class ErrorProviderPlus : ErrorProvider
    {
        /// <summary>
        /// A collection of every error set with <see cref="SetClearableError"/>.
        /// </summary>
        private Dictionary<Control, string> _ClearableErrors = new Dictionary<Control,string>();

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ErrorProviderPlus() : base()
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="parentControl"></param>
        public ErrorProviderPlus(ContainerControl parentControl) : base(parentControl)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="container"></param>
        public ErrorProviderPlus(IContainer container) : base(container)
        {
        }

        /// <summary>
        /// A wrapper around SetError that records the control being assigned an error.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="value"></param>
        public void SetClearableError(Control control, string value)
        {
            SetError(control, value);
            if(_ClearableErrors.ContainsKey(control)) _ClearableErrors.Remove(control);
            if(!String.IsNullOrEmpty(value)) _ClearableErrors.Add(control, value);
        }

        /// <summary>
        /// Clears all errors set with <see cref="SetClearableError"/>.
        /// </summary>
        public void ClearErrors()
        {
            foreach(var kvp in _ClearableErrors) {
                SetError(kvp.Key, null);
            }
            _ClearableErrors.Clear();
        }
    }
}
