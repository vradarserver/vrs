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

            public Item() : this(null, "")
            {
            }

            public Item(object value, string description)
            {
                Value = value;
                Description = description;
            }
        }

        [RefreshProperties(RefreshProperties.Repaint)]
        [DefaultValue(ComboBoxStyle.DropDownList)]
        public new ComboBoxStyle DropDownStyle
        {
            get { return base.DropDownStyle; }
            set { base.DropDownStyle = value; }
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
            PopulateWithCollection<T>(Enum.GetValues(typeof(T)).OfType<T>(), getDescription);
        }

        /// <summary>
        /// Populates the combo box with a collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="getDescription"></param>
        public void PopulateWithCollection<T>(IEnumerable<T> items, Func<T, string> getDescription)
        {
            var selectedValue = SelectedValue;

            var dataSource = items.Select(r => new Item(r, getDescription(r))).ToArray();
            DataSource = dataSource;
            DisplayMember = "Description";
            ValueMember = "Value";
            DataManager.Refresh();

            if(selectedValue != null) SelectedValue = selectedValue;
        }
    }
}
