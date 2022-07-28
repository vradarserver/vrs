// Copyright © 2015 onwards, Andrew Whewell
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
    /// Default WinForms implementation of <see cref="IAircraftOnlineLookupLogView"/>.
    /// </summary>
    public partial class AircraftOnlineLookupLogView : BaseForm, IAircraftOnlineLookupLogView
    {
        /// <summary>
        /// A row that gets displayed in the list.
        /// </summary>
        class DisplayRow
        {
            public DateTime Time { get; set; }

            public string Icao { get; set; }

            public string Registration { get; set; }

            public string Country { get; set; }

            public string Manufacturer { get; set; }

            public string Model { get; set; }

            public string ModelIcao { get; set; }

            public string Operator { get; set; }

            public string OperatorIcao { get; set; }

            public string Serial { get; set; }

            public int? Year { get; set; }
        }

        /// <summary>
        /// Map of aircraft ICAOs to the list view item that represents them.
        /// </summary>
        private Dictionary<string, ListViewItem> _IcaoListViewItemMap = new Dictionary<string,ListViewItem>();

        /// <summary>
        /// The presenter that controls the form.
        /// </summary>
        private IAircraftOnlineLookupLogPresenter _Presenter;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AircraftOnlineLookupLogView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="logEntries"></param>
        public void Populate(IEnumerable<AircraftOnlineLookupLogEntry> logEntries)
        {
            if(InvokeRequired) {
                BeginInvoke(new MethodInvoker(() => Populate(logEntries)));
            } else {
                var icaoMap = new Dictionary<string, AircraftOnlineLookupLogEntry>();
                foreach(var logEntry in logEntries) {
                    var key = NormaliseIcao(logEntry.Icao);
                    if(icaoMap.ContainsKey(key)) icaoMap[key] = logEntry;
                    else                         icaoMap.Add(key, logEntry);
                }

                RemoveOldEntries(icaoMap);
                UpdateExistingEntries(icaoMap);
                InsertNewEntries(icaoMap);
            }
        }

        private string NormaliseIcao(string icao)
        {
            return (icao ?? "").ToUpperInvariant();
        }

        private void RemoveOldEntries(Dictionary<string, AircraftOnlineLookupLogEntry> icaoMap)
        {
            var oldEntries = _IcaoListViewItemMap.Keys.Except(icaoMap.Keys).ToArray();
            foreach(var oldEntry in oldEntries) {
                listView.Items.Remove(_IcaoListViewItemMap[oldEntry]);
                _IcaoListViewItemMap.Remove(oldEntry);
            }
        }

        private void UpdateExistingEntries(Dictionary<string, AircraftOnlineLookupLogEntry> icaoMap)
        {
            foreach(AircraftOnlineLookupLogEntry freshValues in icaoMap.Values) {
                var key = NormaliseIcao(freshValues.Icao);
                ListViewItem listViewItem;
                if(_IcaoListViewItemMap.TryGetValue(key, out listViewItem)) {
                    var displayRow = (DisplayRow)listViewItem.Tag;
                    if(displayRow.Country !=        (freshValues.Detail == null ? null : freshValues.Detail.Country) ||
                       displayRow.Manufacturer !=   (freshValues.Detail == null ? null : freshValues.Detail.Manufacturer) ||
                       displayRow.Model !=          (freshValues.Detail == null ? null : freshValues.Detail.Model) ||
                       displayRow.ModelIcao !=      (freshValues.Detail == null ? null : freshValues.Detail.ModelIcao) ||
                       displayRow.Operator !=       (freshValues.Detail == null ? null : freshValues.Detail.Operator) ||
                       displayRow.OperatorIcao !=   (freshValues.Detail == null ? null : freshValues.Detail.OperatorIcao) ||
                       displayRow.Registration !=   (freshValues.Detail == null ? null : freshValues.Detail.Registration) ||
                       displayRow.Serial !=         (freshValues.Detail == null ? null : freshValues.Detail.Serial) ||
                       displayRow.Time !=                                                freshValues.ResponseUtc ||
                       displayRow.Year !=           (freshValues.Detail == null ? null : freshValues.Detail.YearBuilt)) {
                        UpdateListViewItem(listViewItem, freshValues);
                    }
                }
            }
        }

        private void InsertNewEntries(Dictionary<string, AircraftOnlineLookupLogEntry> icaoMap)
        {
            var newEntries = icaoMap.Keys.Except(_IcaoListViewItemMap.Keys).ToArray();
            foreach(var icao in newEntries) {
                var freshValues = icaoMap[icao];
                var listViewItem = new ListViewItem() {
                    Tag = new DisplayRow(),
                };
                UpdateListViewItem(listViewItem, freshValues);

                _IcaoListViewItemMap.Add(icao, listViewItem);
                listView.Items.Add(listViewItem);
            }
        }

        private void UpdateListViewItem(ListViewItem listViewItem, AircraftOnlineLookupLogEntry freshValues)
        {
            while(listViewItem.SubItems.Count < 11) {
                listViewItem.SubItems.Add("");
            }

            var displayRow = (DisplayRow)listViewItem.Tag;
            displayRow.Country =        freshValues.Detail == null ? null : freshValues.Detail.Country;
            displayRow.Icao    =        freshValues.Icao;
            displayRow.Manufacturer =   freshValues.Detail == null ? null : freshValues.Detail.Manufacturer;
            displayRow.Model =          freshValues.Detail == null ? null : freshValues.Detail.Model;
            displayRow.ModelIcao =      freshValues.Detail == null ? null : freshValues.Detail.ModelIcao;
            displayRow.Operator =       freshValues.Detail == null ? null : freshValues.Detail.Operator;
            displayRow.OperatorIcao =   freshValues.Detail == null ? null : freshValues.Detail.OperatorIcao;
            displayRow.Registration =   freshValues.Detail == null ? null : freshValues.Detail.Registration;
            displayRow.Serial =         freshValues.Detail == null ? null : freshValues.Detail.Serial;
            displayRow.Time =           freshValues.ResponseUtc;
            displayRow.Year =           freshValues.Detail == null ? null : freshValues.Detail.YearBuilt;

            for(var i = 0;i < listViewItem.SubItems.Count;++i) {
                var subItem = listViewItem.SubItems[i];
                switch(i) {
                    case 0:  subItem.Text = displayRow.Time.ToLocalTime().ToString("HH:mm:ss"); break;
                    case 1:  subItem.Text = displayRow.Icao; break;
                    case 2:  subItem.Text = displayRow.Registration ?? ""; break;
                    case 3:  subItem.Text = displayRow.Country ?? ""; break;
                    case 4:  subItem.Text = displayRow.Manufacturer ?? ""; break;
                    case 5:  subItem.Text = displayRow.Model ?? ""; break;
                    case 6:  subItem.Text = displayRow.ModelIcao ?? ""; break;
                    case 7:  subItem.Text = displayRow.Operator ?? ""; break;
                    case 8:  subItem.Text = displayRow.OperatorIcao ?? ""; break;
                    case 9:  subItem.Text = displayRow.Serial ?? ""; break;
                    case 10: subItem.Text = displayRow.Year == null ? "" : displayRow.Year.ToString(); break;
                }
            }
        }

        /// <summary>
        /// Called after the form has loaded but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Form(this);

                _Presenter = Factory.Resolve<IAircraftOnlineLookupLogPresenter>();
                _Presenter.Initialise(this);
            }
        }
    }
}
