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

        private void Value_CollectionChanged(object sender, EventArgs args)
        {
            if(!_SuppressEvents) {
                OnChanged(EventArgs.Empty);
            }
        }
    }
}
