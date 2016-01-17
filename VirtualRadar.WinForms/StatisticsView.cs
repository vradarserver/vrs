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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface;
using InterfaceFactory;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// The WinForms implementation of <see cref="IStatisticsView"/>.
    /// </summary>
    public partial class StatisticsView : BaseForm, IStatisticsView
    {
        #region Fields
        /// <summary>
        /// The presenter that is controlling the view.
        /// </summary>
        private IStatisticsPresenter _Presenter;

        /// <summary>
        /// The object that's handling online help for us.
        /// </summary>
        private OnlineHelpHelper _OnlineHelp;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IStatistics Statistics { get; set; }

        private string _FeedName;
        /// <summary>
        /// Gets or sets the name of the feed.
        /// </summary>
        public string FeedName
        {
            get { return _FeedName; }
            set { _FeedName = value; Text = String.Format("{0} - {1}", Strings.StatisticsTitle, FeedName); }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long BytesReceived { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public TimeSpan ConnectedDuration { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public double ReceiverThroughput { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ReceiverBadChecksum { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long CurrentBufferSize { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ConnectorExceptionCount { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long BaseStationMessages { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long BadlyFormedBaseStationMessages { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public double BadlyFormedBaseStationMessagesRatio { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ModeSMessageCount { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ModeSNoAdsbPayload { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public double ModeSNoAdsbPayloadRatio { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ModeSShortFrame { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ModeSShortFrameUnusable { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public double ModeSShortFrameUnusableRatio { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ModeSLongFrame { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ModeSWithPI { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ModeSPIBadParity { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public double ModeSPIBadParityRatio { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long[] ModeSDFCount { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long AdsbMessages { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long AdsbRejected { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public double AdsbRejectedRatio { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long PositionSpeedCheckExceeded { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long PositionsReset { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long PositionsOutOfRange { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long[] AdsbMessageTypeCount { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long[] AdsbMessageFormatCount { get; private set; }
        #endregion

        #region Events exposed
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ResetCountersClicked;

        /// <summary>
        /// Raises <see cref="ResetCountersClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnResetCountersClicked(EventArgs args)
        {
            EventHelper.Raise(ResetCountersClicked, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler CloseClicked;

        /// <summary>
        /// Raises <see cref="CloseClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnCloseClicked(EventArgs args)
        {
            EventHelper.Raise(CloseClicked, this, args);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public StatisticsView() : base()
        {
            InitializeComponent();

            AdsbMessageTypeCount = new long[256];
            ModeSDFCount = new long[Enum.GetValues(typeof(DownlinkFormat)).OfType<DownlinkFormat>().Select(r => (int)r).Max() + 1];
            AdsbMessageFormatCount = new long[Enum.GetValues(typeof(MessageFormat)).OfType<MessageFormat>().Select(r => (int)r).Max() + 1];
        }
        #endregion

        #region UpdateCounters
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void UpdateCounters()
        {
            if(InvokeRequired) BeginInvoke(new MethodInvoker(() => { UpdateCounters(); }));
            else {
                UpdateLabel(labelDuration, String.Format("{0:00}:{1:00}:{2:00}", ConnectedDuration.Hours, ConnectedDuration.Minutes, ConnectedDuration.Seconds));
                UpdateCounterLabel(labelBytesReceived, BytesReceived);
                UpdateCounterLabel(labelBadChecksum, ReceiverBadChecksum);
                UpdateLabel(labelThroughput, String.Format("{0:N2} {1}", ReceiverThroughput, Strings.AcronymKilobytePerSecond));
                UpdateLabel(labelCurrentBufferSize, String.Format("{0:N0}", CurrentBufferSize));

                UpdateCounterLabel(labelBaseStationMessages, BaseStationMessages);
                UpdateRatioLabel(labelBaseStationBadlyFormatted, BadlyFormedBaseStationMessages, BadlyFormedBaseStationMessagesRatio);

                UpdateCounterLabel(labelModeSMessages, ModeSMessageCount);
                UpdateRatioLabel(labelNoAdsbPayload, ModeSNoAdsbPayload, ModeSNoAdsbPayloadRatio);
                UpdateCounterLabel(labelShortFrame, ModeSShortFrame);
                UpdateRatioLabel(labelShortFrameUnusable, ModeSShortFrameUnusable, ModeSShortFrameUnusableRatio);
                UpdateCounterLabel(labelLongFrame, ModeSLongFrame);
                UpdateCounterLabel(labelPIPresent, ModeSWithPI);
                UpdateRatioLabel(labelPIBadParity, ModeSPIBadParity, ModeSPIBadParityRatio);
                UpdateCounterListView(listViewModeSDFCounts, ModeSDFCount);

                UpdateCounterLabel(labelAdsbMessages, AdsbMessages);
                UpdateRatioLabel(labelAdsbRejected, AdsbRejected, AdsbRejectedRatio);
                UpdateCounterLabel(labelSpeedChecksExceeded, PositionSpeedCheckExceeded);
                UpdateCounterLabel(labelPositionResets, PositionsReset);
                UpdateCounterLabel(labelPositionsOutOfRange, PositionsOutOfRange);
                UpdateCounterListView(listViewAdsbTypeCounts, AdsbMessageTypeCount);
                UpdateCounterListView(listViewAdsbMessageFormatCounts, AdsbMessageFormatCount, i => ((MessageFormat)i).ToString());
            }
        }

        /// <summary>
        /// Refreshes the content of a label control.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="value"></param>
        private void UpdateLabel(Label label, string value)
        {
            if(label.Text != value) label.Text = value;
        }

        private void UpdateLinkLabel(LinkLabel linkLabel, string value)
        {
            if(linkLabel.Text != value) linkLabel.Text = value;
        }

        /// <summary>
        /// Refreshes the content of a label showing a single counter.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="counter"></param>
        private void UpdateCounterLabel(Label label, long counter)
        {
            UpdateLabel(label, counter.ToString("N0"));
        }

        /// <summary>
        /// Refreshes the content of a label showing a counter and a ratio.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="counter"></param>
        /// <param name="ratio"></param>
        private void UpdateRatioLabel(Label label, long counter, double ratio)
        {
            UpdateLabel(label, String.Format("{0:N0} ({1:N2}%)", counter, ratio * 100.0));
        }

        /// <summary>
        /// Refreshes the content of a listview of counters.
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="counters"></param>
        /// <param name="indexToLabelFunc"></param>
        private void UpdateCounterListView(ListView listView, long[] counters, Func<int, string> indexToLabelFunc = null)
        {
            var linesUsed = 0;
            for(var i = 0;i < counters.Length;++i) {
                var value = counters[i];
                if(value != 0L) {
                    var indexText = indexToLabelFunc == null ? i.ToString() : indexToLabelFunc(i);
                    var valueText = String.Format("{0:N0}", value);

                    ListViewItem listViewItem = linesUsed < listView.Items.Count ? listView.Items[linesUsed] : null;
                    if(listViewItem == null) listView.Items.Add(new ListViewItem(new string[] { indexText, valueText }));
                    else {
                        if(listViewItem.SubItems[0].Text != indexText) listViewItem.SubItems[0].Text = indexText;
                        if(listViewItem.SubItems[1].Text != valueText) listViewItem.SubItems[1].Text = valueText;
                    }

                    ++linesUsed;
                }
            }

            while(listView.Items.Count > linesUsed) {
                listView.Items.RemoveAt(linesUsed);
            }
        }
        #endregion

        #region Events consumed
        /// <summary>
        /// Called after the form has been initialised but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Localise.Form(this);

            _OnlineHelp = new OnlineHelpHelper(this, OnlineHelpAddress.WinFormsStatisticsDialog);

            _Presenter = Factory.Singleton.Resolve<IStatisticsPresenter>();
            _Presenter.Initialise(this);
        }

        /// <summary>
        /// Called when the form is closing but before it has closed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            OnCloseClicked(e);
        }

        /// <summary>
        /// Called when the user clicks the reset counters button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonResetCounters_Click(object sender, EventArgs e)
        {
            OnResetCountersClicked(e);
        }

        /// <summary>
        /// Called when the user clicks the Close button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion
    }
}
