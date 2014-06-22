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
