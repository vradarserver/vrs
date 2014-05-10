// Copyright © 2013 onwards, Andrew Whewell
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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.WinForms.Options;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// Exposes a property grid that can be used to display sheets.
    /// </summary>
    public partial class SheetHostControl : UserControl
    {
        /// <summary>
        /// Gets or sets the owning view.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OptionsPropertySheetView OptionsView { get; set; }

        /// <summary>
        /// Gets or sets the sheet to show to the user.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISheet Sheet
        {
            get { return propertyGrid.SelectedObject as ISheet; }
            set { propertyGrid.SelectedObject = value; }
        }

        /// <summary>
        /// Raised when a property value changes.
        /// </summary>
        public event EventHandler<SheetEventArgs> PropertyValueChanged;

        /// <summary>
        /// Raises <see cref="PropertyValueChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPropertyValueChanged(SheetEventArgs args)
        {
            if(PropertyValueChanged != null) PropertyValueChanged(this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public SheetHostControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Raised when the user changes a value on the sheet.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            ISheet sheet = null;
            for(var gridItem = e.ChangedItem.Parent;gridItem != null && sheet == null;gridItem = gridItem.Parent) {
                sheet = gridItem.Value as ISheet;
            }
            if(sheet != null) OnPropertyValueChanged(new SheetEventArgs(sheet));

            if(e.ChangedItem.PropertyDescriptor.Attributes.OfType<RaisesValuesChangedAttribute>().Any()) {
                OptionsView.RaiseValueChanged(this, EventArgs.Empty);
            }
        }
    }
}
