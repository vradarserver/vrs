// Copyright © 2019 onwards, Andrew Whewell
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
using System.Threading.Tasks;
using System.Windows.Forms;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// WinForms-specific localisation methods moved out of <see cref="VirtualRadar.Localisation.Localiser"/>
    /// when converting to dotnet core 3.
    /// </summary>
    public class FormsLocaliser : Localiser
    {
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="stringsResourceType">The type of the Strings class created by the resource compiler.</param>
        public FormsLocaliser(Type stringsResourceType) : base(stringsResourceType)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="localisedStringsMap">The localised strings map to use when looking up strings.</param>
        public FormsLocaliser(LocalisedStringsMap localisedStringsMap) : base(localisedStringsMap)
        {
        }

        /// <summary>
        /// Localises a form's title and then recursively localises all of the controls on the form.
        /// </summary>
        /// <param name="form"></param>
        public void Form(Form form)
        {
            Control(form);
        }

        /// <summary>
        /// Localises an arbitrary control.
        /// </summary>
        /// <param name="control"></param>
        public void Control(Control control)
        {
            if(!(control is WebBrowser)) {
                control.Text = GetLocalisedText(control.Text);

                if(control is ListView listView) {
                    ListViewColumns(listView);
                } else if(control is ToolStrip toolStrip) {
                    ToolStrip(toolStrip);
                }

                if(control.HasChildren) {
                    foreach(Control child in control.Controls) {
                        Control(child);
                    }
                }
            }
        }

        /// <summary>
        /// Localises the columns in a list view.
        /// </summary>
        /// <param name="listView"></param>
        public void ListViewColumns(ListView listView)
        {
            foreach(ColumnHeader column in listView.Columns) {
                ColumnHeader(column);
            }
        }

        /// <summary>
        /// Localises a column header.
        /// </summary>
        /// <param name="column"></param>
        public void ColumnHeader(ColumnHeader column)
        {
            column.Text = GetLocalisedText(column.Text);
        }

        /// <summary>
        /// Localises a toolstrip.
        /// </summary>
        /// <param name="toolStrip"></param>
        public void ToolStrip(ToolStrip toolStrip)
        {
            foreach(ToolStripItem item in toolStrip.Items) {
                ToolStripItem(item);
            }
        }

        /// <summary>
        /// Localises a toolstrip item.
        /// </summary>
        /// <param name="item"></param>
        public void ToolStripItem(ToolStripItem item)
        {
            item.Text = GetLocalisedText(item.Text);

            if(item is ToolStripMenuItem menuItem && menuItem.HasDropDownItems) {
                foreach(ToolStripItem childItem in menuItem.DropDownItems) {
                    ToolStripItem(childItem);
                }
            }
        }
    }
}
