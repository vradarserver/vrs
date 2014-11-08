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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.PortableBinding;

namespace Test.VirtualRadar.Interface.PortableBinding
{
    /// <summary>
    /// Tests the light binding list. Note that it is assumed that the
    /// binding list is based on ObservableCollection, and that it is
    /// assumed that ObservableCollection is working. The tests here
    /// only look at the stuff built on top of that.
    /// </summary>
    [TestClass]
    public class NotifyListTests
    {
        #region Private class - ObservableClass
        /// <summary>
        /// A class that implements <see cref="INotifyPropertyChanged"/>.
        /// </summary>
        class ObservableClass : INotifyPropertyChanged
        {
            private int _Value;
            /// <summary>
            /// The value held by the class.
            /// </summary>
            public int Value
            {
                get { return _Value; }
                set { SetField(ref _Value, value, () => Value); }
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
                if(PropertyChanged != null) PropertyChanged(this, args);
            }

            /// <summary>
            /// Sets the field's value and raises <see cref="PropertyChanged"/>.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="field"></param>
            /// <param name="value"></param>
            /// <param name="selectorExpression"></param>
            /// <returns></returns>
            protected bool SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
            {
                if(EqualityComparer<T>.Default.Equals(field, value)) return false;
                field = value;

                if(selectorExpression == null) throw new ArgumentNullException("selectorExpression");
                MemberExpression body = selectorExpression.Body as MemberExpression;
                if(body == null) throw new ArgumentException("The body must be a member expression");
                OnPropertyChanged(new PropertyChangedEventArgs(body.Member.Name));

                return true;
            }
        }
        #endregion

        #region Fields
        public TestContext TestContext { get; set; }

        private NotifyList<int> _IntList;
        private NotifyList<ObservableClass> _ObservableList;

        private EventRecorder<ListChangedEventArgs> _ListChanged;
        private EventRecorder<NotifyCollectionChangedEventArgs> _CollectionChanged;

        private PropertyDescriptor _ObservableValueDescriptor;

        [TestInitialize]
        public void TestInitialise()
        {
            _IntList = new NotifyList<int>();
            _ObservableList = new NotifyList<ObservableClass>();

            _ListChanged = new EventRecorder<ListChangedEventArgs>();
            _CollectionChanged = new EventRecorder<NotifyCollectionChangedEventArgs>();

            _ObservableValueDescriptor = TypeDescriptor.GetProperties(typeof(ObservableClass))[PropertyHelper.ExtractName<ObservableClass>(r => r.Value)];
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }
        #endregion

        #region AttachEventRecorders
        private void AttachEventRecorders<T>(NotifyList<T> list)
        {
            list.ListChanged += _ListChanged.Handler;
            list.CollectionChanged += _CollectionChanged.Handler;
        }
        #endregion

        #region Ctors and properties
        [TestMethod]
        public void NotifyList_Initialises_To_Known_State_And_Properties_Work()
        {
            Assert.AreEqual(true, _IntList.AllowEdit);
            Assert.AreEqual(true, _IntList.AllowNew);
            Assert.AreEqual(true, _IntList.AllowRemove);
            Assert.AreEqual(0, _IntList.Count);
            Assert.AreEqual(false, _IntList.IsSorted);
            Assert.AreEqual(true, _IntList.RaiseListChangedEvents);
            Assert.AreEqual(true, _IntList.RaisesItemChangedEvents);
            Assert.AreEqual(ListSortDirection.Ascending, _IntList.SortDirection);
            Assert.IsNull(_IntList.SortProperty);
            Assert.AreEqual(true, _IntList.SupportsChangeNotification);
            Assert.AreEqual(false, _IntList.SupportsSearching);
            Assert.AreEqual(false, _IntList.SupportsSorting);
        }
        #endregion

        #region Add
        [TestMethod]
        public void NotifyList_Add_Struct_With_Events()
        {
            _IntList.Add(9);

            AttachEventRecorders(_IntList);
            _IntList.Add(10);

            Assert.AreEqual(1, _ListChanged.CallCount);
            Assert.AreEqual(ListChangedType.ItemAdded, _ListChanged.Args.ListChangedType);
            Assert.AreEqual(1, _ListChanged.Args.NewIndex);
            Assert.AreEqual(-1, _ListChanged.Args.OldIndex);
            Assert.AreEqual(null, _ListChanged.Args.PropertyDescriptor);

            Assert.AreEqual(1, _CollectionChanged.CallCount);
        }

        [TestMethod]
        public void NotifyList_Add_Does_Not_RaiseEvents_If_RaiseChangedEvents_Is_False()
        {
            AttachEventRecorders(_IntList);
            _IntList.RaiseListChangedEvents = false;

            _IntList.Add(10);

            Assert.AreEqual(0, _ListChanged.CallCount);
            Assert.AreEqual(0, _CollectionChanged.CallCount);
        }
        #endregion

        #region AddIndex
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void NotifyList_AddIndex_Is_Not_Implemented()
        {
            _ObservableList.AddIndex(_ObservableValueDescriptor);
        }
        #endregion

        #region AddNew
        [TestMethod]
        public void NotifyList_AddNew_Struct_With_Events()
        {
            _IntList.Add(10);

            AttachEventRecorders(_IntList);
            var added = _IntList.AddNew();

            Assert.AreEqual(0, added);

            Assert.AreEqual(1, _ListChanged.CallCount);
            Assert.AreEqual(ListChangedType.ItemAdded, _ListChanged.Args.ListChangedType);
            Assert.AreEqual(1, _ListChanged.Args.NewIndex);
            Assert.AreEqual(-1, _ListChanged.Args.OldIndex);
            Assert.AreEqual(null, _ListChanged.Args.PropertyDescriptor);

            Assert.AreEqual(1, _CollectionChanged.CallCount);
        }

        [TestMethod]
        public void NotifyList_AddNew_Class_With_Events()
        {
            _ObservableList.Add(null);

            AttachEventRecorders(_ObservableList);
            var added = (ObservableClass)_ObservableList.AddNew();

            Assert.AreEqual(0, added.Value);

            Assert.AreEqual(1, _ListChanged.CallCount);
            Assert.AreEqual(ListChangedType.ItemAdded, _ListChanged.Args.ListChangedType);
            Assert.AreEqual(1, _ListChanged.Args.NewIndex);
            Assert.AreEqual(-1, _ListChanged.Args.OldIndex);
            Assert.AreEqual(null, _ListChanged.Args.PropertyDescriptor);

            Assert.AreEqual(1, _CollectionChanged.CallCount);
        }

        [TestMethod]
        public void NotifyList_AddNew_Does_Not_RaiseEvents_If_RaiseChangedEvents_Is_False()
        {
            AttachEventRecorders(_IntList);
            _IntList.RaiseListChangedEvents = false;

            _IntList.AddNew();

            Assert.AreEqual(0, _ListChanged.CallCount);
            Assert.AreEqual(0, _CollectionChanged.CallCount);
        }
        #endregion

        #region ApplySort
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void NotifyList_ApplySort_Is_Not_Implemented()
        {
            _ObservableList.ApplySort(_ObservableValueDescriptor, ListSortDirection.Descending);
        }
        #endregion

        #region Clear
        [TestMethod]
        public void NotifyList_Clear_Struct_With_Events()
        {
            _IntList.Add(1);
            _IntList.Add(2);

            AttachEventRecorders(_IntList);
            _IntList.Clear();

            Assert.AreEqual(1, _ListChanged.CallCount);
            Assert.AreEqual(ListChangedType.Reset, _ListChanged.Args.ListChangedType);
            Assert.AreEqual(-1, _ListChanged.Args.NewIndex);
            Assert.AreEqual(-1, _ListChanged.Args.OldIndex);
            Assert.AreEqual(null, _ListChanged.Args.PropertyDescriptor);

            Assert.AreEqual(1, _CollectionChanged.CallCount);
        }

        [TestMethod]
        public void NotifyList_Clear_Does_Not_RaiseEvents_If_RaiseChangedEvents_Is_False()
        {
            _IntList.RaiseListChangedEvents = false;
            _IntList.Add(1);
            _IntList.Add(2);

            AttachEventRecorders(_IntList);
            _IntList.Clear();

            Assert.AreEqual(0, _ListChanged.CallCount);
            Assert.AreEqual(0, _CollectionChanged.CallCount);
        }
        #endregion

        #region Find
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void NotifyList_Find_Is_Not_Implemented()
        {
            _ObservableList.Find(_ObservableValueDescriptor, new ObservableClass() { Value = 10 });
        }
        #endregion

        #region Insert
        [TestMethod]
        public void NotifyList_Insert_Struct_With_Events()
        {
            _IntList.Add(1);
            _IntList.Add(3);

            AttachEventRecorders(_IntList);
            _IntList.Insert(1, 5);

            Assert.AreEqual(1, _ListChanged.CallCount);
            Assert.AreEqual(ListChangedType.ItemAdded, _ListChanged.Args.ListChangedType);
            Assert.AreEqual(1, _ListChanged.Args.NewIndex);
            Assert.AreEqual(-1, _ListChanged.Args.OldIndex);
            Assert.AreEqual(null, _ListChanged.Args.PropertyDescriptor);

            Assert.AreEqual(1, _CollectionChanged.CallCount);
        }

        [TestMethod]
        public void NotifyList_Insert_Does_Not_RaiseEvents_If_RaiseChangedEvents_Is_False()
        {
            _IntList.RaiseListChangedEvents = false;
            _IntList.Add(1);
            _IntList.Add(3);

            AttachEventRecorders(_IntList);
            _IntList.Insert(1, 5);

            Assert.AreEqual(0, _ListChanged.CallCount);
            Assert.AreEqual(0, _CollectionChanged.CallCount);
        }
        #endregion

        #region ListChanged
        [TestMethod]
        public void NotifyList_ListChanged_Raised_If_Observable_Object_Changes_After_Add()
        {
            var item = new ObservableClass();
            _ObservableList.Add(null);
            _ObservableList.Add(item);

            AttachEventRecorders(_ObservableList);
            item.Value++;

            Assert.AreEqual(1, _ListChanged.CallCount);
            Assert.AreEqual(ListChangedType.ItemChanged, _ListChanged.Args.ListChangedType);
            Assert.AreEqual(1, _ListChanged.Args.NewIndex);
            Assert.AreEqual(1, _ListChanged.Args.OldIndex);
            Assert.AreEqual(_ObservableValueDescriptor, _ListChanged.Args.PropertyDescriptor);

            Assert.AreEqual(0, _CollectionChanged.CallCount);
        }

        [TestMethod]
        public void NotifyList_ListChanged_Raised_If_Observable_Object_Changes_After_AddNew()
        {
            _ObservableList.Add(null);
            var item = (ObservableClass)_ObservableList.AddNew();

            AttachEventRecorders(_ObservableList);
            item.Value++;

            Assert.AreEqual(1, _ListChanged.CallCount);
            Assert.AreEqual(ListChangedType.ItemChanged, _ListChanged.Args.ListChangedType);
            Assert.AreEqual(1, _ListChanged.Args.NewIndex);
            Assert.AreEqual(1, _ListChanged.Args.OldIndex);
            Assert.AreEqual(_ObservableValueDescriptor, _ListChanged.Args.PropertyDescriptor);

            Assert.AreEqual(0, _CollectionChanged.CallCount);
        }

        [TestMethod]
        public void NotifyList_ListChanged_Not_Raised_If_Observable_Object_Changes_After_Clear()
        {
            var item = new ObservableClass();
            _ObservableList.Add(item);

            _ObservableList.Clear();
            AttachEventRecorders(_ObservableList);
            item.Value++;

            Assert.AreEqual(0, _ListChanged.CallCount);
        }

        [TestMethod]
        public void NotifyList_ListChanged_Raised_If_Observable_Object_Changes_After_Insert()
        {
            _ObservableList.Add(null);
            _ObservableList.Add(null);
            var item = new ObservableClass();
            _ObservableList.Insert(1, item);

            AttachEventRecorders(_ObservableList);
            item.Value++;

            Assert.AreEqual(1, _ListChanged.CallCount);
            Assert.AreEqual(ListChangedType.ItemChanged, _ListChanged.Args.ListChangedType);
            Assert.AreEqual(1, _ListChanged.Args.NewIndex);
            Assert.AreEqual(1, _ListChanged.Args.OldIndex);
            Assert.AreEqual(_ObservableValueDescriptor, _ListChanged.Args.PropertyDescriptor);

            Assert.AreEqual(0, _CollectionChanged.CallCount);
        }

        [TestMethod]
        public void ListBindingList_ListChanged_Not_Raised_If_Observable_Object_Changes_After_Remove()
        {
            var item = new ObservableClass();
            _ObservableList.Add(item);

            _ObservableList.Remove(item);
            AttachEventRecorders(_ObservableList);
            item.Value++;

            Assert.AreEqual(0, _ListChanged.CallCount);
        }

        [TestMethod]
        public void ListBindingList_ListChanged_Not_Raised_If_Observable_Object_Changes_After_RemoveAt()
        {
            var item = new ObservableClass();
            _ObservableList.Add(item);

            _ObservableList.RemoveAt(0);
            AttachEventRecorders(_ObservableList);
            item.Value++;

            Assert.AreEqual(0, _ListChanged.CallCount);
        }

        [TestMethod]
        public void ListBindingList_ListChanged_Not_Raised_If_Old_Observable_Object_Changes_After_Replace()
        {
            var item = new ObservableClass();
            _ObservableList.Add(item);

            _ObservableList[0] = new ObservableClass();
            AttachEventRecorders(_ObservableList);
            item.Value++;

            Assert.AreEqual(0, _ListChanged.CallCount);
        }

        [TestMethod]
        public void NotifyList_ListChanged_Raised_If_New_Observable_Object_Changes_After_Replace()
        {
            _ObservableList.Add(null);
            _ObservableList.Add(null);
            var item = new ObservableClass();
            _ObservableList[1] = item;

            AttachEventRecorders(_ObservableList);
            item.Value++;

            Assert.AreEqual(1, _ListChanged.CallCount);
            Assert.AreEqual(ListChangedType.ItemChanged, _ListChanged.Args.ListChangedType);
            Assert.AreEqual(1, _ListChanged.Args.NewIndex);
            Assert.AreEqual(1, _ListChanged.Args.OldIndex);
            Assert.AreEqual(_ObservableValueDescriptor, _ListChanged.Args.PropertyDescriptor);

            Assert.AreEqual(0, _CollectionChanged.CallCount);
        }

        [TestMethod]
        public void NotifyList_ListChanged_Not_Raised_If_Observable_Changes_When_RaiseChangedEvents_Is_False()
        {
            var item = new ObservableClass();
            _ObservableList.Add(item);

            AttachEventRecorders(_ObservableList);
            _ObservableList.RaiseListChangedEvents = false;
            item.Value++;

            Assert.AreEqual(0, _ListChanged.CallCount);
        }
        #endregion

        #region Move
        [TestMethod]
        public void NotifyList_Move_Struct_With_Events()
        {
            _IntList.Add(1);
            _IntList.Add(2);
            _IntList.Add(3);
            _IntList.Add(4);
            _IntList.Add(5);

            AttachEventRecorders(_IntList);
            _IntList.Move(1, 3);

            Assert.AreEqual(1, _ListChanged.CallCount);
            Assert.AreEqual(ListChangedType.ItemMoved, _ListChanged.Args.ListChangedType);
            Assert.AreEqual(3, _ListChanged.Args.NewIndex);
            Assert.AreEqual(1, _ListChanged.Args.OldIndex);
            Assert.AreEqual(null, _ListChanged.Args.PropertyDescriptor);

            Assert.AreEqual(1, _CollectionChanged.CallCount);
        }

        [TestMethod]
        public void NotifyList_Move_Does_Not_RaiseEvents_If_RaiseChangedEvents_Is_False()
        {
            _IntList.RaiseListChangedEvents = false;
            _IntList.Add(1);
            _IntList.Add(3);

            AttachEventRecorders(_IntList);
            _IntList.Move(0, 1);

            Assert.AreEqual(0, _ListChanged.CallCount);
            Assert.AreEqual(0, _CollectionChanged.CallCount);
        }
        #endregion

        #region Remove
        [TestMethod]
        public void NotifyList_Remove_Struct_With_Events()
        {
            _IntList.Add(9);
            _IntList.Add(10);
            _IntList.Add(11);

            AttachEventRecorders(_IntList);
            _IntList.Remove(10);

            Assert.AreEqual(1, _ListChanged.CallCount);
            Assert.AreEqual(ListChangedType.ItemDeleted, _ListChanged.Args.ListChangedType);
            Assert.AreEqual(1, _ListChanged.Args.NewIndex);
            Assert.AreEqual(-1, _ListChanged.Args.OldIndex);
            Assert.AreEqual(null, _ListChanged.Args.PropertyDescriptor);

            Assert.AreEqual(1, _CollectionChanged.CallCount);
        }

        [TestMethod]
        public void NotifyList_Remove_Does_Not_RaiseEvents_If_RaiseChangedEvents_Is_False()
        {
            _IntList.Add(10);
            _IntList.RaiseListChangedEvents = false;

            AttachEventRecorders(_IntList);
            _IntList.Remove(10);

            Assert.AreEqual(0, _ListChanged.CallCount);
            Assert.AreEqual(0, _CollectionChanged.CallCount);
        }
        #endregion

        #region RemoveAt
        [TestMethod]
        public void NotifyList_RemoveAt_Struct_With_Events()
        {
            _IntList.Add(9);
            _IntList.Add(10);
            _IntList.Add(11);

            AttachEventRecorders(_IntList);
            _IntList.RemoveAt(1);

            Assert.AreEqual(1, _ListChanged.CallCount);
            Assert.AreEqual(ListChangedType.ItemDeleted, _ListChanged.Args.ListChangedType);
            Assert.AreEqual(1, _ListChanged.Args.NewIndex);
            Assert.AreEqual(-1, _ListChanged.Args.OldIndex);
            Assert.AreEqual(null, _ListChanged.Args.PropertyDescriptor);

            Assert.AreEqual(1, _CollectionChanged.CallCount);
        }

        [TestMethod]
        public void NotifyList_RemoveAt_Does_Not_RaiseEvents_If_RaiseChangedEvents_Is_False()
        {
            _IntList.Add(10);
            _IntList.RaiseListChangedEvents = false;

            AttachEventRecorders(_IntList);
            _IntList.RemoveAt(0);

            Assert.AreEqual(0, _ListChanged.CallCount);
            Assert.AreEqual(0, _CollectionChanged.CallCount);
        }
        #endregion

        #region RemoveIndex
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void NotifyList_RemoveIndex_Is_Not_Implemented()
        {
            _ObservableList.RemoveIndex(_ObservableValueDescriptor);
        }
        #endregion

        #region RemoveSort
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void NotifyList_RemoveSort_Is_Not_Implemented()
        {
            _ObservableList.RemoveSort();
        }
        #endregion

        #region Replace
        [TestMethod]
        public void NotifyList_Replace_Struct_With_Events()
        {
            _IntList.Add(8);
            _IntList.Add(9);

            AttachEventRecorders(_IntList);
            _IntList[1] = 10;

            Assert.AreEqual(1, _ListChanged.CallCount);
            Assert.AreEqual(ListChangedType.ItemChanged, _ListChanged.Args.ListChangedType);
            Assert.AreEqual(1, _ListChanged.Args.NewIndex);
            Assert.AreEqual(-1, _ListChanged.Args.OldIndex);
            Assert.AreEqual(null, _ListChanged.Args.PropertyDescriptor);

            Assert.AreEqual(1, _CollectionChanged.CallCount);
        }

        [TestMethod]
        public void NotifyList_Replace_Does_Not_RaiseEvents_If_RaiseChangedEvents_Is_False()
        {
            _IntList.Add(10);
            _IntList.RaiseListChangedEvents = false;

            AttachEventRecorders(_IntList);
            _IntList[0] = 8;

            Assert.AreEqual(0, _ListChanged.CallCount);
            Assert.AreEqual(0, _CollectionChanged.CallCount);
        }
        #endregion
    }
}
