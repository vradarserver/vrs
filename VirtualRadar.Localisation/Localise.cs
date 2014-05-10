// Copyright © 2010 onwards, Andrew Whewell
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
using System.Reflection;

namespace VirtualRadar.Localisation
{
    /// <summary>
    /// A static wrapper around an instance of <see cref="Localiser"/> for the Virtual Radar Server's application
    /// strings class.
    /// </summary>
    public static class Localise
    {
        /// <summary>
        /// The object that's doing all of the work for us.
        /// </summary>
        static Localiser _Localiser = new Localiser(typeof(Strings));

        /// <summary>
        /// See <see cref="Localiser.Lookup"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string Lookup(string name)
        {
            return _Localiser.Lookup(name);
        }

        /// <summary>
        /// See <see cref="Localiser.Form"/>.
        /// </summary>
        /// <param name="form"></param>
        public static void Form(Form form)
        {
            _Localiser.Form(form);
        }

        /// <summary>
        /// See <see cref="Localiser.Control"/>.
        /// </summary>
        /// <param name="control"></param>
        public static void Control(Control control)
        {
            _Localiser.Control(control);
        }

        /// <summary>
        /// See <see cref="Localiser.ListViewColumns"/>.
        /// </summary>
        /// <param name="listView"></param>
        public static void ListViewColumns(ListView listView)
        {
            _Localiser.ListViewColumns(listView);
        }

        /// <summary>
        /// See <see cref="Localiser.ColumnHeader"/>.
        /// </summary>
        /// <param name="column"></param>
        public static void ColumnHeader(ColumnHeader column)
        {
            _Localiser.ColumnHeader(column);
        }

        /// <summary>
        /// See <see cref="Localiser.ToolStrip"/>.
        /// </summary>
        /// <param name="toolStrip"></param>
        public static void ToolStrip(ToolStrip toolStrip)
        {
            _Localiser.ToolStrip(toolStrip);
        }

        /// <summary>
        /// See <see cref="Localiser.ToolStripItem"/>.
        /// </summary>
        /// <param name="item"></param>
        public static void ToolStripItem(ToolStripItem item)
        {
            _Localiser.ToolStripItem(item);
        }

        /// <summary>
        /// See <see cref="Localiser.GetLocalisedText"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetLocalisedText(String text)
        {
            return _Localiser.GetLocalisedText(text);
        }
    }
}
