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
using System.Windows.Forms;
using System.ComponentModel;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A wrapper around combo boxes handles some of the tedium involved.
    /// </summary>
    public class ComboBoxPlus : ComboBox
    {
        /// <summary>
        /// The item held by the combo box.
        /// </summary>
        class Item
        {
            public object Value { get; private set; }
            public string Description { get; private set; }
            public Func<object, object> IdentifierExtractor { get; set; }

            public Item() : this(null, "")
            {
            }

            public Item(object value, string description)
            {
                Value = value;
                Description = description;
            }

            public override string ToString()
            {
                return Description ?? "";
            }

            public override bool Equals(object obj)
            {
                var result = Object.ReferenceEquals(this, obj);
                if(!result) {
                    var other = obj as Item;
                    if(other != null) {
                        if(IdentifierExtractor == null) result = Object.Equals(other.Value, Value);
                        else {
                            var lhs = Value == null ? null : IdentifierExtractor(Value);
                            var rhs = other.Value == null ? null : IdentifierExtractor(other.Value);
                            result = Object.Equals(lhs, rhs);
                        }
                    }
                }

                return result;
            }

            public override int GetHashCode()
            {
                return Value == null ? 0 : Value.GetHashCode();
            }
        }

        /// <summary>
        /// The value assigned to SelectedSafeValue before the combo box was populated, if any.
        /// </summary>
        private object _SelectedSafeValue;

        /// <summary>
        /// Set to true once one of the Populate methods has been called.
        /// </summary>
        private bool _Populated;

        [RefreshProperties(RefreshProperties.Repaint)]
        [DefaultValue(ComboBoxStyle.DropDownList)]
        public new ComboBoxStyle DropDownStyle
        {
            get { return base.DropDownStyle; }
            set { base.DropDownStyle = value; }
        }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new object SelectedValue
        {
            get
            {
                if(!_Populated) return _SelectedSafeValue;
                else {
                    var item = base.SelectedItem as Item;
                    return item == null ? null : item.Value;
                }
            }
            set
            {
                if(!_Populated) _SelectedSafeValue = value;
                else {
                    var key = new Item(value, "");
                    var item = Items.OfType<Item>().FirstOrDefault(r => Object.Equals(r, key));
                    base.SelectedItem = item;
                }
            }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ComboBoxPlus()
        {
            base.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        /// <summary>
        /// Populates the items list with enum values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getDescription"></param>
        public void PopulateWithEnums<T>(Func<T, string> getDescription)
        {
            PopulateWithObjects<T>(Enum.GetValues(typeof(T)).OfType<T>(), getDescription);
        }

        /// <summary>
        /// Returns the selected value on the assumption that the combo box was populated with enums.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSelectedEnum<T>()
        {
            return GetSelectedObject<T>();
        }

        /// <summary>
        /// Populates the combo box with a collection of objects. The "value" of each object is the
        /// object itself.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="getDescription"></param>
        public void PopulateWithObjects<T>(IEnumerable<T> items, Func<T, string> getDescription)
        {
            var selectedValue = SelectedValue;

            Items.Clear();
            foreach(var value in items) {
                var description = getDescription(value);
                var item = new Item(value, description);
                Items.Add(item);
            }

            _Populated = true;
            SelectedValue = selectedValue;
        }

        /// <summary>
        /// Gets the selected object cast
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetSelectedObject<T>(T defaultValue = default(T))
        {
            var value = SelectedValue;
            return value == null ? defaultValue : (T)value;
        }

        /// <summary>
        /// Populates the combo box with objects that have an identifier. Assignment of different
        /// instances of the object with the same identifier will assign values from the item list.
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <typeparam name="TIdentifier"></typeparam>
        /// <param name="items"></param>
        /// <param name="getIdentifier"></param>
        /// <param name="getDescription"></param>
        public void PopulateWithIdObjects<TRecord, TIdentifier>(IEnumerable<TRecord> items, Func<TRecord, TIdentifier> getIdentifier, Func<TRecord, string> getDescription)
        {
            var selectedValue = SelectedValue;
            Func<object, object> identifierExtractor = (r) => (object)getIdentifier((TRecord)r);

            Items.Clear();
            foreach(var value in items) {
                var description = getDescription(value);
                var item = new Item(value, description) {
                    IdentifierExtractor = identifierExtractor,
                };
                Items.Add(item);
            }

            _Populated = true;
            SelectedValue = selectedValue;
        }

        /// <summary>
        /// Gets the selected ID object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetSelectedIdObject<T>(T defaultValue = default(T))
        {
            return GetSelectedObject<T>(defaultValue);
        }
    }
}
