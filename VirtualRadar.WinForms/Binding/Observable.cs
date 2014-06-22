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

namespace VirtualRadar.WinForms.Binding
{
    /// <summary>
    /// The simple generic implementation of IObservable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Observable<T> : IObservable
    {
        private T _Value;
        public T Value
        {
            get { return _Value; }
            set { SetValue(value, false); }
        }

        public event EventHandler Changed;

        protected virtual void OnChanged(EventArgs args)
        {
            if(Changed != null) Changed(this, args);
        }

        public object GetValue()
        {
            return (object)_Value;
        }

        public void SetValue(object value)
        {
            SetValue(value, false);
        }

        public void SetValue(object value, bool suppressEvents)
        {
            if(!Object.Equals(value, _Value)) {
                _Value = (T)value;
                if(!suppressEvents) OnChanged(EventArgs.Empty);
            }
        }

        public Type GetValueType()
        {
            return typeof(T);
        }
    }
}
