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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.WinForms.Binding;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A combo box that is hooked up to an observable list and exposes an
    /// identifier from the list.
    /// </summary>
    public class ObservableListComboBox : ComboBox
    {
        private bool _HookedObservableList;

        [RefreshProperties(RefreshProperties.Repaint)]
        [DefaultValue(ComboBoxStyle.DropDownList)]
        public new ComboBoxStyle DropDownStyle
        {
            get { return base.DropDownStyle; }
            set { base.DropDownStyle = value; }
        }

        private IObservableList _ObservableList;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IObservableList ObservableList
        {
            get { return _ObservableList; }
            set {
                UnhookObservableList();
                _ObservableList = value;
                Populate();
                HookObservableList();
            }
        }

        public ObservableListComboBox() : base()
        {
            base.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing) UnhookObservableList();
            base.Dispose(disposing);
        }

        private void UnhookObservableList()
        {
            if(_HookedObservableList) ObservableList.Changed -= ObservableList_Changed;
            _HookedObservableList = false;
        }

        private void HookObservableList()
        {
            if(!_HookedObservableList && ObservableList != null) {
                ObservableList.Changed += ObservableList_Changed;
                _HookedObservableList = true;
            }
        }

        protected virtual void Populate()
        {
            if(base.DataSource == null) base.DataSource = ObservableList.GetValue();
            base.DataManager.Refresh();
        }

        private void ObservableList_Changed(object sender, EventArgs args)
        {
            Populate();
        }
    }
}
