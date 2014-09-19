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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// The WinForms implementation of <see cref="ITimestampedExceptionView"/>.
    /// </summary>
    public partial class TimestampedExceptionView : BaseForm, ITimestampedExceptionView
    {
        /// <summary>
        /// The presenter.
        /// </summary>
        private ITimestampedExceptionPresenter _Presenter;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public List<TimestampedException> Exceptions { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public TimestampedException SelectedException
        {
            get { return GetSelectedListViewTag<TimestampedException>(listView); }
            set { SelectListViewItemByTag(listView, value); }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public TimestampedExceptionView()
        {
            InitializeComponent();
            Exceptions = new List<TimestampedException>();
        }

        /// <summary>
        /// Called after the view has loaded but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Form(this);
                _Presenter = Factory.Singleton.Resolve<ITimestampedExceptionPresenter>();
                _Presenter.Initialise(this);

                PopulateListView(listView, Exceptions, SelectedException, PopulateListViewItem, r => SelectedException = r);
                ShowExceptionDetail();
            }
        }

        /// <summary>
        /// Fills in a row in the list view.
        /// </summary>
        /// <param name="listViewItem"></param>
        private void PopulateListViewItem(ListViewItem listViewItem)
        {
            FillListViewItem<TimestampedException>(listViewItem, r => new string[] {
                _Presenter.FormatTime(r),
                r.Exception.Message ?? "",
            });
        }

        /// <summary>
        /// Displays the details of the selected exception.
        /// </summary>
        private void ShowExceptionDetail()
        {
            var ex = SelectedException;
            textBoxTime.Text = _Presenter.FormatTime(ex);
            textBoxMessage.Text = _Presenter.FullExceptionMessage(ex).Replace("\n", Environment.NewLine);
        }

        /// <summary>
        /// Called when the user selects a row in the list view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowExceptionDetail();
        }
    }
}
