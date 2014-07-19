// Copyright (c) 2006, Andrew Davey
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by the <organization>.
// 4. Neither the name of the <organization> nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY <COPYRIGHT HOLDER> ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Equin.ApplicationFramework
{
    /// <summary>
    /// A searchable, sortable, filterable, data bindable view of a list of objects.
    /// </summary>
    /// <typeparam name="T">The type of object in the list.</typeparam>
    public class BindingListView<T> : AggregateBindingListView<T>
    {
        /// <summary>
        /// Creates a new <see cref="BindingListView&lt;T&gt;"/> of a given IBindingList.
        /// All items in the list must be of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="list">The list of objects to base the view on.</param>
        public BindingListView(IList list)
            : base()
        {
            DataSource = list;
        }

        public BindingListView(IContainer container)
            : base(container)
        {
            DataSource = null;
        }

        [DefaultValue(null)]
        [AttributeProvider(typeof(IListSource))]
        public IList DataSource
        {
            get
            {
                IEnumerator<IList> e = GetSourceLists().GetEnumerator();
                e.MoveNext();
                return e.Current;
            }
            set
            {
                if (value == null)
                {
                    // Clear all current data
                    SourceLists = new BindingList<IList<T>>();
                    NewItemsList = null;
                    FilterAndSort();
                    OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
                    return;
                }

                if (!(value is ICollection<T>))
                {
                    // list is not a strongy-type collection.
                    // Check that items in list are all of type T
                    foreach (object item in value)
                    {
                        if (!(item is T))
                        {
                            throw new ArgumentException(string.Format("Item in list is not of type {0}", typeof(T).FullName), "DataSource");
                        }
                    }
                }

                SourceLists = new object[] { value };
                NewItemsList = value;
            }
        }

        private bool ShouldSerializeDataSource()
        {
            return (SourceLists.Count > 0);
        }

        protected override void SourceListsChanged(object sender, ListChangedEventArgs e)
        {
            if ((SourceLists.Count > 1 && e.ListChangedType == ListChangedType.ItemAdded) || e.ListChangedType == ListChangedType.ItemDeleted)
            {
                throw new Exception("BindingListView allows strictly one source list.");
            }
            else
            {
                base.SourceListsChanged(sender, e);
            }
        }
    }
}
