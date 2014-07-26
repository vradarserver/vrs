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
using System.Collections;
using VirtualRadar.Localisation;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A user control that can display the content of a list of aircraft and, if necessary, show updates
    /// to the list.
    /// </summary>
    public partial class AircraftListControl : BaseUserControl
    {
        #region Fields
        /// <summary>
        /// The object that can sort the list for us.
        /// </summary>
        private AutoListViewSorter _Sorter;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the selected aircraft in the list.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IAircraft SelectedAircraft
        {
            get { return listView.SelectedItems.Count == 0 ? null : (IAircraft)listView.SelectedItems[0].Tag; }
            set
            {
                foreach(ListViewItem item in listView.Items) {
                    item.Selected = value != null && ((IAircraft)item.Tag).UniqueId == value.UniqueId;
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AircraftListControl() : base()
        {
            InitializeComponent();
            
            _Sorter = new AutoListViewSorter(listView);
            listView.ListViewItemSorter = _Sorter;
        }
        #endregion

        #region InitialiseList, UpdateList
        /// <summary>
        /// Fills the list with the initial set of aircraft.
        /// </summary>
        /// <param name="aircraftList"></param>
        public void InitialiseList(IList<IAircraft> aircraftList)
        {
            listView.Items.Clear();
            foreach(var aircraft in aircraftList) {
                AddAircraftToList(aircraft);
            }
            Sort();
        }

        /// <summary>
        /// Updates the list of aircraft.
        /// </summary>
        /// <param name="snapshot"></param>
        public void UpdateList(IList<IAircraft> snapshot)
        {
            List<ListViewItem> deleteList = new List<ListViewItem>();
            foreach(ListViewItem item in listView.Items) {
                var aircraft = (IAircraft)item.Tag;
                if(snapshot.Where(a => a.UniqueId == aircraft.UniqueId).FirstOrDefault() == null) deleteList.Add(item);
            }

            foreach(var deleteItem in deleteList) {
                listView.Items.Remove(deleteItem);
            }

            foreach(var aircraft in snapshot) {
                ListViewItem item = FindItem(aircraft);
                if(item != null) UpdateAircraftInList(aircraft, item);
                else AddAircraftToList(aircraft);
            }

            Sort();
        }
        #endregion

        #region AddAircraftToList, UpdateAircraftInList, FindItem, Sort
        /// <summary>
        /// Adds the aircraft passed across to the list.
        /// </summary>
        /// <param name="aircraft"></param>
        private void AddAircraftToList(IAircraft aircraft)
        {
            if(aircraft.Latitude != null && aircraft.Longitude != null && (aircraft.Latitude != 0.0 || aircraft.Longitude != 0.0)) {
                ListViewItem item = new ListViewItem(new string[] { aircraft.Registration, aircraft.Icao24, aircraft.Type, aircraft.Operator });
                item.Tag = aircraft;

                listView.Items.Add(item);
            }
        }

        /// <summary>
        /// Updates an existing aircraft in the list.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="item"></param>
        private void UpdateAircraftInList(IAircraft aircraft, ListViewItem item)
        {
            if(item.SubItems[0].Text != aircraft.Registration)  item.SubItems[0].Text = aircraft.Registration;
            if(item.SubItems[1].Text != aircraft.Icao24)        item.SubItems[1].Text = aircraft.Icao24;
            if(item.SubItems[2].Text != aircraft.Type)          item.SubItems[2].Text = aircraft.Type;
            if(item.SubItems[3].Text != aircraft.Operator)      item.SubItems[3].Text = aircraft.Operator;
            item.Tag = aircraft;
        }

        /// <summary>
        /// Returns the list view item for the aircraft passed across or null if there is no row for the aircraft.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <returns></returns>
        private ListViewItem FindItem(IAircraft aircraft)
        {
            ListViewItem result = null;
            foreach(ListViewItem item in listView.Items) {
                var tag = (IAircraft)item.Tag;
                if(tag.UniqueId == aircraft.UniqueId) {
                    result = item;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Sorts the list view.
        /// </summary>
        private void Sort()
        {
            listView.Sort();
        }
        #endregion

        #region Events consumed
        /// <summary>
        /// Called when the control has been loaded and initialised but is not yet on display.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if(!DesignMode) Localise.Control(this);
        }
        #endregion
    }
}
