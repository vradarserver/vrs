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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Localisation;
using System.IO;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A user control that can display a list of dictionary plugins.
    /// </summary>
    public partial class InvalidPluginsControl : BaseUserControl
    {
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public InvalidPluginsControl() : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Populates the control with the map of filenames to reasons.
        /// </summary>
        /// <param name="invalidPlugins"></param>
        public void Populate(IDictionary<string, string> invalidPlugins)
        {
            var startupPath = Application.StartupPath;
            if(startupPath.Length > 0) startupPath = Path.Combine(startupPath, String.Format("Plugins{0}", Path.DirectorySeparatorChar));

            listView.Items.Clear();
            foreach(var kvp in invalidPlugins) {
                var fileName = kvp.Key;
                var reason = kvp.Value;

                if(fileName.StartsWith(startupPath, StringComparison.OrdinalIgnoreCase) && fileName.Length > startupPath.Length) {
                    fileName = fileName.Substring(startupPath.Length);
                }

                listView.Items.Add(new ListViewItem(new string[] { fileName, reason }) { ToolTipText = fileName });
            }
        }

        /// <summary>
        /// Called when the control has been initialised but before it's shown on screen.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) FormsLocalise.Control(this);
        }
    }
}
