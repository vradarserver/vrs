using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface.PortableBinding
{
    /// <summary>
    /// A list collection that implements <see cref="INotifyCollectionChanged"/>, <see cref="INotifyPropertyChanged"/>,
    /// <see cref="IBindingList"/> and <see cref="IRaiseItemChangedEvents"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// This was not written because I had nothing better to do. I needed it because Mono's implementation
    /// of BindingList doesn't work. Only the bits of <see cref="IBindingList"/> that I need have been
    /// implemented. I implemented both ObservableCollection and IBindingList because I was using BindingList
    /// and I want this to work as a drop-in replacement (albeit one that doesn't work with .NET data binding).
    /// </remarks>
    [Serializable]
    public class NotifyList<T> : ObservableCollection<T>, IBindingList, IList, ICollection, IEnumerable, IEnumerable<T>, IRaiseItemChangedEvents
    {
        #region Fields
        /// <summary>
        /// True if the type parameter indicates that the items can be observed. This infers that we
        /// won't watch objects when the typeparam is object. However, this is true of the real
        /// BindingList too, so no worries there.
        /// </summary>
        private bool _ItemsAreObservable;

        /// <summary>
        /// The properties exposed by the type parameter type. Always null if <see cref="_ItemsAreObservable"/> is false.
        /// </summary>
        private PropertyDescriptorCollection _Properties;
        #endregion

        #region IBindingList properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool AllowEdit
        {
            get { return true; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool AllowNew
        {
            get { return true; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool AllowRemove
        {
            get { return true; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsSorted
        {
            get { return false; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ListSortDirection SortDirection
        {
            get { return ListSortDirection.Ascending; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public PropertyDescriptor SortProperty
        {
            get { return null; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool SupportsChangeNotification
        {
            get { return true; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool SupportsSearching
        {
            get { return false; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool SupportsSorting
        {
            get { return false; }
        }
        #endregion

        #region IRaiseItemChangedEvents properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool RaisesItemChangedEvents
        {
            get { return true; }
        }
        #endregion

        #region Other properties
        /// <summary>
        /// Gets or sets a value indicating whether Add, Insert and Remove raise <see cref="ListChanged"/> events.
        /// </summary>
        public bool RaiseListChangedEvents { get; set; }
        #endregion

        #region IBindingList events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event ListChangedEventHandler ListChanged;

        /// <summary>
        /// Raises <see cref="ListChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnListChanged(ListChangedEventArgs args)
        {
            if(ListChanged != null) ListChanged(this, args);
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public NotifyList() : base()
        {
            RaiseListChangedEvents = true;

            _ItemsAreObservable = typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(T));
            if(_ItemsAreObservable) _Properties = TypeDescriptor.GetProperties(typeof(T));
        }
        #endregion

        #region IBindingList methods
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public object AddNew()
        {
            var result = Activator.CreateInstance<T>();
            Add(result);

            return result;
        }
        #endregion

        #region IBindingList stubs
        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="property"></param>
        public void AddIndex(PropertyDescriptor property)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="direction"></param>
        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public int Find(PropertyDescriptor property, object key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="property"></param>
        public void RemoveIndex(PropertyDescriptor property)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public void RemoveSort()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ObservableCollection overrides
        /// <summary>
        /// Called when the underlying collection is changed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if(RaiseListChangedEvents) {
                base.OnCollectionChanged(e);

                var listChangedType = default(ListChangedType);
                var newIndex = -1;
                var oldIndex = -1;
                switch(e.Action) {
                    case NotifyCollectionChangedAction.Add:
                        listChangedType = ListChangedType.ItemAdded;
                        newIndex = e.NewStartingIndex;
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        listChangedType = ListChangedType.ItemDeleted;
                        newIndex = e.OldStartingIndex;
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        listChangedType = ListChangedType.Reset;
                        break;
                    case NotifyCollectionChangedAction.Move:
                        listChangedType = ListChangedType.ItemMoved;
                        oldIndex = e.OldStartingIndex;
                        newIndex = e.NewStartingIndex;
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        listChangedType = ListChangedType.ItemChanged;
                        newIndex = e.NewStartingIndex;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                var args = new ListChangedEventArgs(listChangedType, newIndex, oldIndex);
                OnListChanged(args);
            }
        }
        #endregion

        #region InsertItem
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void InsertItem(int index, T item)
        {
            if(_ItemsAreObservable) HookItem((INotifyPropertyChanged)item);
            base.InsertItem(index, item);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void ClearItems()
        {
            if(_ItemsAreObservable) {
                foreach(var item in Items) {
                    UnhookItem((INotifyPropertyChanged)item);
                }
            }

            base.ClearItems();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="index"></param>
        protected override void RemoveItem(int index)
        {
            if(_ItemsAreObservable) UnhookItem((INotifyPropertyChanged)Items[index]);
            base.RemoveItem(index);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void SetItem(int index, T item)
        {
            if(_ItemsAreObservable) {
                UnhookItem((INotifyPropertyChanged)Items[index]);
                HookItem((INotifyPropertyChanged)item);
            }

            base.SetItem(index, item);
        }
        #endregion

        #region HookItem, UnhookItem
        /// <summary>
        /// Hooks the <see cref="INotifyPropertyChanged"/> event on the item.
        /// </summary>
        /// <param name="item"></param>
        private void HookItem(INotifyPropertyChanged item)
        {
            if(item != null) {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        /// <summary>
        /// Unhooks the <see cref="INotifyPropertyChanged"/> event on the item.
        /// </summary>
        /// <param name="item"></param>
        private void UnhookItem(INotifyPropertyChanged item)
        {
            if(item != null) {
                item.PropertyChanged -= Item_PropertyChanged;
            }
        }
        #endregion

        #region EventHandlers
        /// <summary>
        /// Raised when a property changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(RaiseListChangedEvents) {
                var index = IndexOf((T)sender);
                var propertyDescriptor = _Properties[e.PropertyName];

                var args = new ListChangedEventArgs(ListChangedType.ItemChanged, index, propertyDescriptor);
                OnListChanged(args);
            }
        }
        #endregion
    }
}