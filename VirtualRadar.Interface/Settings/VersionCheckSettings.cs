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
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The configuration settings controlling the checking for new versions of the server.
    /// </summary>
    public class VersionCheckSettings : INotifyPropertyChanged
    {
        private bool _CheckAutomatically;
        /// <summary>
        /// Gets and sets a value indicating that the user wants the program to check for itself every so often.
        /// </summary>
        public bool CheckAutomatically
        {
            get { return _CheckAutomatically; }
            set { SetField(ref _CheckAutomatically, value, nameof(CheckAutomatically)); }
        }

        private int _CheckPeriodDays;
        /// <summary>
        /// Gets and sets the period between automatic checks.
        /// </summary>
        public int CheckPeriodDays
        {
            get { return _CheckPeriodDays; }
            set { SetField(ref _CheckPeriodDays, value, nameof(CheckPeriodDays)); }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            var handler = PropertyChanged;
            if(handler != null) {
                handler(this, args);
            }
        }

        /// <summary>
        /// Sets the field's value and raises <see cref="PropertyChanged"/>, but only when the value has changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="fieldName"></param>
        /// <returns>True if the value was set because it had changed, false if the value did not change and the event was not raised.</returns>
        protected bool SetField<T>(ref T field, T value, string fieldName)
        {
            var result = !EqualityComparer<T>.Default.Equals(field, value);
            if(result) {
                field = value;
                OnPropertyChanged(new PropertyChangedEventArgs(fieldName));
            }

            return result;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public VersionCheckSettings()
        {
            CheckAutomatically = true;
            CheckPeriodDays = 1;
        }
    }
}
