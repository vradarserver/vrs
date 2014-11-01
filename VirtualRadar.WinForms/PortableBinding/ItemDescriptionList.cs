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
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace VirtualRadar.WinForms.PortableBinding
{
    /// <summary>
    /// A collection of ItemDescription objects.
    /// </summary>
    /// <typeparam name="T">The type of object in the list.</typeparam>
    public class ItemDescriptionList<T> : List<ItemDescription<T>>, IDisposable
    {
        /// <summary>
        /// True if the list's events have been hooked.
        /// </summary>
        private bool _ListHooked;

        /// <summary>
        /// Gets the underlying list.
        /// </summary>
        public IList<T> UnderlyingList { get; private set; }

        /// <summary>
        /// Gets the underlying list as an <see cref="IBindingList"/>.
        /// </summary>
        public IBindingList UnderlyingBindingList { get { return UnderlyingList as IBindingList; } }

        private Func<T, string> _GetDescription;
        /// <summary>
        /// Gets or sets a method that returns the description associated with an item.
        /// </summary>
        public Func<T, string> GetDescription
        {
            get { return _GetDescription; }
            set { _GetDescription = value; PopulateList(); }
        }

        /// <summary>
        /// Raised when one or more items in the list have changed description or value.
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// Raises <see cref="Changed"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnChanged(EventArgs args)
        {
            if(Changed != null) Changed(this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="getDescription"></param>
        public ItemDescriptionList(IList<T> list, Func<T, string> getDescription)
        {
            if(list == null) throw new ArgumentNullException("list");

            UnderlyingList = list;
            if(UnderlyingBindingList != null) {
                UnderlyingBindingList.ListChanged += UnderlyingBindingList_ListChanged;
                _ListHooked = true;
            }
            GetDescription = getDescription;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~ItemDescriptionList()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                if(_ListHooked) {
                    UnderlyingBindingList.ListChanged -= UnderlyingBindingList_ListChanged;
                    _ListHooked = false;
                }
            }
        }

        /// <summary>
        /// Fills itself with objects that record the current description for each item.
        /// </summary>
        private void PopulateList()
        {
            if(_GetDescription != null) {
                var hasChanged = false;

                for(var listElement = 0;listElement < UnderlyingList.Count;++listElement) {
                    var listItem = UnderlyingList[listElement];
                    var listDescription = GetDescription(listItem);

                    var existingItemDescription = this.Count > listElement ? this[listElement] : null;
                    if(existingItemDescription == null || !existingItemDescription.AreItemDescriptionEqual(listItem, listDescription)) {
                        hasChanged = true;
                        var listItemDescription = new ItemDescription<T>(listItem, listDescription);
                        if(existingItemDescription == null) this.Add(listItemDescription);
                        else                                this[listElement] = listItemDescription;
                    }
                }

                while(this.Count > UnderlyingList.Count) {
                    this.RemoveAt(UnderlyingList.Count);
                    hasChanged = true;
                }

                if(hasChanged) OnChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when the underlying list changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void UnderlyingBindingList_ListChanged(object sender, ListChangedEventArgs e)
        {
            PopulateList();
        }
    }
}
