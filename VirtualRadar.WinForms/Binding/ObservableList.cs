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
using System.Collections.ObjectModel;

namespace VirtualRadar.WinForms.Binding
{
    public class ObservableList<T> : IObservableList
    {
        private bool _SuppressEvents;

        private ObservableCollection<T> _Value = new ObservableCollection<T>();
        public ObservableCollection<T> Value
        {
            get { return _Value; }
        }

        public event EventHandler Changed;

        protected virtual void OnChanged(EventArgs args)
        {
            if(Changed != null) Changed(this, args);
        }

        public ObservableList()
        {
            _Value.CollectionChanged += Value_CollectionChanged;
        }

        public object GetValue()
        {
            return _Value;
        }

        public void SetValue(IEnumerable<T> value)
        {
            SetValue((object)value, false);
        }

        public void SetValue(IEnumerable<T> value, bool suppressEvents)
        {
            SetValue((object)value, suppressEvents);
        }

        public void SetValue(object value)
        {
            SetValue(value, false);
        }

        public void SetValue(object value, bool suppressEvents)
        {
            var list = value as IEnumerable<T>;
            if(!Object.ReferenceEquals(_Value, list) && !_Value.SequenceEqual(list)) {
                var innerSuppress = _SuppressEvents;
                try {
                    _SuppressEvents = true;
                    _Value.Clear();
                    if(list != null) {
                        foreach(var item in list) {
                            _Value.Add(item);
                        }
                    }
                } finally {
                    _SuppressEvents = innerSuppress;
                }

                if(!suppressEvents) {
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        public Type GetValueType()
        {
            return typeof(ObservableCollection<T>);
        }

        public void ReplaceContent(IEnumerable<T> newList)
        {
            ((IObservableList)this).ReplaceContent(newList.Cast<object>());
        }

        void IObservableList.ReplaceContent(IEnumerable<object> newList)
        {
            var changed = false;

            var suppressEvents = _SuppressEvents;
            _SuppressEvents = true;
            try {
                var list = newList.OfType<T>().ToArray();
                changed = !list.SequenceEqual(Value);

                if(changed) {
                    Value.Clear();
                    foreach(var item in list) {
                        Value.Add(item);
                    }
                }
            } finally {
                _SuppressEvents = suppressEvents;
            }

            if(changed) OnChanged(EventArgs.Empty);
        }

        private void Value_CollectionChanged(object sender, EventArgs args)
        {
            if(!_SuppressEvents) {
                OnChanged(EventArgs.Empty);
            }
        }
    }
}
