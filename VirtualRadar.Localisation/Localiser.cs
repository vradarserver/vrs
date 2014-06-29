// Copyright © 2012 onwards, Andrew Whewell
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

namespace VirtualRadar.Localisation
{
    /// <summary>
    /// A class that can localise Windows Forms elements with strings taken from a strings resource file.
    /// </summary>
    /// <remarks><para>
    /// This class can be used by the plugins to translate strings for their UI.
    /// </para><para>
    /// All methods look for elements whose text contains tags delimited by double-colons. Those tags
    /// are replaced with strings whose name matches the text within the double-colon. So for example,
    /// a Label whose Text property is &quot;::UserName::&quot; would be subsituted with the content
    /// of the <em>UserName</em> string. Only chunks within double-colons are substituted.
    /// </para></remarks>
    public class Localiser
    {
        /// <summary>
        /// The object that can lookup localised strings for us.
        /// </summary>
        private LocalisedStringsMap _LocalisedStrings;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="stringsResourceType">The type of the Strings class created by the resource compiler.</param>
        public Localiser(Type stringsResourceType)
        {
            _LocalisedStrings = new LocalisedStringsMap(stringsResourceType);
        }

        /// <summary>
        /// Returns the localised version of the string identifier passed across.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string Lookup(string name)
        {
            return _LocalisedStrings.GetLocalisedString(name);
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
            var webBrowser = control as WebBrowser;
            if(webBrowser == null) {
                control.Text = GetLocalisedText(control.Text);

                ListView listView = control as ListView;
                if(listView != null) ListViewColumns(listView);
                else {
                    var toolStrip = control as ToolStrip;
                    if(toolStrip != null) ToolStrip(toolStrip);
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

            ToolStripMenuItem menuItem = item as ToolStripMenuItem;
            if(menuItem != null && menuItem.HasDropDownItems) {
                foreach(ToolStripItem childItem in menuItem.DropDownItems) {
                    ToolStripItem(childItem);
                }
            }
        }

        /// <summary>
        /// Returns the localised version of the text passed across.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks>If the text starts and ends with :: then the inner part is taken to be the name of a string in Strings
        /// and it's returned.</remarks>
        public string GetLocalisedText(String text)
        {
            string result = text;
            if(!String.IsNullOrEmpty(result) && result.Length > 4 && result.StartsWith("::")) {
                int endPosn = result.IndexOf("::", 2);
                if(endPosn != -1) result = String.Format("{0}{1}", _LocalisedStrings.GetLocalisedString(result.Substring(2, endPosn - 2)), result.Substring(endPosn + 2));
            }

            return result;
        }
    }
}
