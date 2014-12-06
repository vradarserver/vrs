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
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A user control that presents information about a single plugin.
    /// </summary>
    public partial class PluginDetailPanel : BaseUserControl
    {
        private IPlugin _Plugin;
        /// <summary>
        /// Gets or sets the plugin whose details are to be shown to the user.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IPlugin Plugin
        {
            get { return _Plugin; }
            set
            {
                if(_Plugin != value) {
                    if(_Plugin != null) _Plugin.StatusChanged -= Plugin_StatusChanged;

                    _Plugin = value;
                    labelPluginName.Text = value.Name;
                    labelVersion.Text = value.Version;
                    labelStatus.Text = value.Status;
                    labelStatusDescription.Text = value.StatusDescription;
                    buttonConfigure.Enabled = value.HasOptions;

                    _Plugin.StatusChanged += Plugin_StatusChanged;
                }
            }
        }

        /// <summary>
        /// Raised when the user clicks the button to configure the plugin.
        /// </summary>
        public EventHandler<PluginEventArgs> ConfigureClicked;

        /// <summary>
        /// Raises <see cref="ConfigureClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnConfigureClicked(PluginEventArgs args)
        {
            if(ConfigureClicked != null) ConfigureClicked(this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PluginDetailPanel() : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Raised after the control has loaded but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) Localise.Control(this);
        }

        /// <summary>
        /// Raised when the user clicks the options button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonConfigure_Click(object sender, EventArgs e)
        {
            OnConfigureClicked(new PluginEventArgs(Plugin));
        }

        /// <summary>
        /// Called when the plugin changes its status. This may happen on a background thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Plugin_StatusChanged(object sender, EventArgs e)
        {
            if(InvokeRequired) {
                try {
                    BeginInvoke(new MethodInvoker(() => { Plugin_StatusChanged(sender, e); }));
                } catch(InvalidOperationException) {
                    ; // Required for mono
                }
            } else {
                labelStatus.Text = Plugin.Status;
                labelStatusDescription.Text = Plugin.StatusDescription;
            }
        }
    }
}
