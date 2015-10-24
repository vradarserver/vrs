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
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A user control that shows details for many plugins simultaneously.
    /// </summary>
    public partial class PluginDetailsControl : BaseUserControl
    {
        /// <summary>
        /// A list of every panel created for a plugin.
        /// </summary>
        List<PluginDetailPanel> _PluginPanels = new List<PluginDetailPanel>();

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PluginDetailsControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Raised when the user elects to configure one of the plugins.
        /// </summary>
        public event EventHandler<PluginEventArgs> ConfigurePluginClicked;

        /// <summary>
        /// Raises <see cref="ConfigurePluginClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnConfigurePluginClicked(PluginEventArgs args)
        {
            EventHelper.Raise(ConfigurePluginClicked, this, args);
        }

        /// <summary>
        /// Displays information about the plugins passed across.
        /// </summary>
        /// <param name="plugins"></param>
        public void ShowPlugins(IEnumerable<IPlugin> plugins)
        {
            flowLayoutPanel.Controls.Clear();
            _PluginPanels.Clear();

            foreach(var plugin in plugins.OrderBy(p => p.Name)) {
                var detailPanel = new PluginDetailPanel() { Plugin = plugin };
                detailPanel.ConfigureClicked += Plugin_ConfigureClicked;
                flowLayoutPanel.Controls.Add(detailPanel);
                _PluginPanels.Add(detailPanel);
            }

            SetDetailPanelSizes();
        }

        /// <summary>
        /// Resizes all of the child panels so that they always fit properly.
        /// </summary>
        private void SetDetailPanelSizes()
        {
            foreach(var detailPanel in _PluginPanels) {
                detailPanel.Width = Width - (detailPanel.Margin.Horizontal + (SystemInformation.BorderSize.Width * 2) + SystemInformation.VerticalScrollBarWidth);
            }
        }

        /// <summary>
        /// Raised when the user clicks one of the plugin options buttons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Plugin_ConfigureClicked(object sender, PluginEventArgs args)
        {
            OnConfigurePluginClicked(args);
        }

        /// <summary>
        /// Raised when the control changes size.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void flowLayoutPanel_SizeChanged(object sender, EventArgs e)
        {
            SetDetailPanelSizes();
        }
    }
}
