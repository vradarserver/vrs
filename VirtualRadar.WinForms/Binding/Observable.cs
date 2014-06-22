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
