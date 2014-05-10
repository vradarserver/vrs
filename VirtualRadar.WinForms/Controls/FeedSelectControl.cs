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
using VirtualRadar.Interface;
using InterfaceFactory;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A user control that presents a list of feeds and allows the user to choose from one of them.
    /// </summary>
    public partial class FeedSelectControl : BaseUserControl
    {
        private bool _Loaded;
        private IFeed _PreLoadSelectedFeed;
        private int _PreLoadSelectedFeedId;

        /// <summary>
        /// Gets or sets the selected feed.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IFeed SelectedFeed
        {
            get { return !_Loaded ? _PreLoadSelectedFeed : GetSelectedComboBoxValue<IFeed>(comboBox); }
            set { if(!_Loaded) _PreLoadSelectedFeed = value; else SelectComboBoxItemByValue(comboBox, value); }
        }

        /// <summary>
        /// Gets or sets the selected feed by its unique ID.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedFeedId
        {
            get { return !_Loaded ? _PreLoadSelectedFeedId : SelectedFeed == null ? 0 : SelectedFeed.UniqueId; }
            set { if(!_Loaded) _PreLoadSelectedFeedId = value; else SelectedFeed = GetComboBoxValues<IFeed>(comboBox).FirstOrDefault(r => r.UniqueId == value); }
        }

        /// <summary>
        /// Raised when the <see cref="SelectedFeed"/> changes.
        /// </summary>
        public event EventHandler SelectedFeedChanged;

        /// <summary>
        /// Raises <see cref="SelectedFeedChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnSelectedFeedChanged(EventArgs args)
        {
            if(SelectedFeedChanged != null) SelectedFeedChanged(this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FeedSelectControl() : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called after the control has been initialised but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                var receiverManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;
                FillDropDownWithValues(comboBox, receiverManager.Feeds, r => r.Name);

                _Loaded = true;
                if(_PreLoadSelectedFeed != null) SelectedFeed = _PreLoadSelectedFeed;
                if(_PreLoadSelectedFeedId != 0) SelectedFeedId = _PreLoadSelectedFeedId;
            }
        }

        /// <summary>
        /// Called when the combo box changes its selected value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedFeedChanged(e);
        }
    }
}
