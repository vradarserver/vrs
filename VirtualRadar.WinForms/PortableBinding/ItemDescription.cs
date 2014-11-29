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

namespace VirtualRadar.WinForms.PortableBinding
{
    /// <summary>
    /// Carries an item and its description.
    /// </summary>
    public class ItemDescription<T>
    {
        /// <summary>
        /// Gets the item that is being described.
        /// </summary>
        public T Item { get; private set; }

        /// <summary>
        /// Gets the item's description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="description"></param>
        public ItemDescription(T item, string description)
        {
            Item = item;
            Description = description;
        }

        /// <summary>
        /// See base class docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Description;
        }

        /// <summary>
        /// Returns true if both the item and the description are Object.Equal to the values
        /// held by this object.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public bool AreItemDescriptionEqual(T item, string description)
        {
            return Object.Equals(Item, item) && String.Equals(Description, description);
        }

        /// <summary>
        /// See base class docs. Objects are equal to this if they are of the correct class
        /// and both Item and Description are Equal to this object's <see cref="Item"/> and
        /// <see cref="Description"/>.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result) {
                var other = obj as ItemDescription<T>;
                result = other != null && Object.Equals(Item, other.Item) && String.Equals(Description, other.Description);
            }

            return result;
        }

        /// <summary>
        /// See base class docs.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var result = Object.Equals(Item, default(T)) ? 0 : Item.GetHashCode();
            return result;
        }
    }
}
