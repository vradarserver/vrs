// Copyright © 2022 onwards, Andrew Whewell
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
using System.Windows.Forms;
using VirtualRadar.Interface;
using VirtualRadar.WinForms;

namespace VirtualRadar.Plugin.Vatsim.WinForms
{
    public partial class OptionsView : Form
    {
        private ListViewHelper<GeofenceFeedOption, Guid> _GeofenceFeedListViewHelper;
        private ComboBoxHelper<GeofenceCentreOn, object> _CentreOnComboBoxHelper;
        private ComboBoxHelper<DistanceUnit, object>     _DistanceUnitComboBoxHelper;

        private Options _Options;
        internal Options Options
        {
            get => _Options;
            set {
                if(!Object.ReferenceEquals(_Options, value)) {
                    _Options = value;
                    CopyOptionsToControls();
                    EnableDisableControls();
                }
            }
        }

        private bool PluginEnabled
        {
            get => chkEnabled.Checked;
            set => chkEnabled.Checked = value;
        }

        private int RefreshIntervalSeconds
        {
            get => (int)nudRefreshIntervalSeconds.Value;
            set => nudRefreshIntervalSeconds.Value = value;
        }

        private bool AssumeSlowAircraftAreOnGround
        {
            get => chkAssumeSlowAircraftAreOnGround.Checked;
            set => chkAssumeSlowAircraftAreOnGround.Checked = value;
        }

        private int SlowAircraftThresholdSpeedKnots
        {
            get => (int)nudSlowAircraftThresholdSpeedKnots.Value;
            set => nudSlowAircraftThresholdSpeedKnots.Value = value;
        }

        private bool InferModelFromModelType
        {
            get => chkInferModelFromModelType.Checked;
            set => chkInferModelFromModelType.Checked = value;
        }

        private bool ShowInvalidRegistrations
        {
            get => chkShowInvalidRegistrations.Checked;
            set => chkShowInvalidRegistrations.Checked = value;
        }

        private string FeedName
        {
            get => txtFeedName.Text.Trim();
            set => txtFeedName.Text = value;
        }

        private GeofenceCentreOn CentreOn
        {
            get => _CentreOnComboBoxHelper.SelectedItem;
            set => _CentreOnComboBoxHelper.SelectedItem = value;
        }

        private double GeofenceWidth
        {
            get => (double)nudWidth.Value;
            set => nudWidth.Value = (decimal)value;
        }

        private double GeofenceHeight
        {
            get => (double)nudHeight.Value;
            set => nudHeight.Value = (decimal)value;
        }

        private DistanceUnit DistanceUnit
        {
            get => _DistanceUnitComboBoxHelper.SelectedItem;
            set => _DistanceUnitComboBoxHelper.SelectedItem = value;
        }

        private double Latitude
        {
            get => (double)nudLatitude.Value;
            set => nudLatitude.Value = (decimal)value;
        }

        private double Longitude
        {
            get => (double)nudLongitude.Value;
            set => nudLongitude.Value = (decimal)value;
        }

        private string AirportCode
        {
            get => txtAirportCode.Text.Trim().ToUpperInvariant();
            set => txtAirportCode.Text = value;
        }

        private int PilotCid
        {
            get => (int)nudPilotCid.Value;
            set => nudPilotCid.Value = value;
        }

        public OptionsView()
        {
            InitializeComponent();

            _GeofenceFeedListViewHelper = new ListViewHelper<GeofenceFeedOption, Guid>(
                lvwGeofencedFeeds,
                feed => new string[] {
                    feed.FeedName ?? "",
                    DescribeVatsim.GeofenceCentreOn(feed.CentreOn),
                    feed.Latitude?.ToString("N6") ?? "",
                    feed.Longitude?.ToString("N6") ?? "",
                    feed.AirportCode ?? "",
                    feed.PilotCid?.ToString() ?? "",
                    feed.Width.ToString("N2"),
                    feed.Height.ToString("N2"),
                    Describe.DistanceUnit(feed.DistanceUnit),
                },
                extractID: feed => feed.ID
            );

            _CentreOnComboBoxHelper = new ComboBoxHelper<GeofenceCentreOn, object>(
                cmbCentreOn,
                enumValue => DescribeVatsim.GeofenceCentreOn(enumValue)
            );
            _CentreOnComboBoxHelper.RebuildComboBox(Enum.GetValues(typeof(GeofenceCentreOn)).OfType<GeofenceCentreOn>());

            _DistanceUnitComboBoxHelper = new ComboBoxHelper<DistanceUnit, object>(
                cmbDistanceUnit,
                enumValue => Describe.DistanceUnit(enumValue)
            );
            _DistanceUnitComboBoxHelper.RebuildComboBox(Enum.GetValues(typeof(DistanceUnit)).OfType<DistanceUnit>());
        }

        private void CopyOptionsToControls()
        {
            AssumeSlowAircraftAreOnGround =     Options?.AssumeSlowAircraftAreOnGround ?? false;
            InferModelFromModelType =           Options?.InferModelFromModelType ?? false;
            PluginEnabled =                     Options?.Enabled ?? false;
            RefreshIntervalSeconds =            Options?.RefreshIntervalSeconds ?? 15;
            ShowInvalidRegistrations =          Options?.ShowInvalidRegistrations ?? false;
            SlowAircraftThresholdSpeedKnots =   Options?.SlowAircraftThresholdSpeedKnots ?? 40;

            _GeofenceFeedListViewHelper.RefreshList(Options?.GeofencedFeeds ?? new List<GeofenceFeedOption>());

            CopySelectedGeofenceFeedToControls();
        }

        private void CopyControlsToOptions()
        {
            Options.AssumeSlowAircraftAreOnGround =     AssumeSlowAircraftAreOnGround;
            Options.InferModelFromModelType =           InferModelFromModelType;
            Options.Enabled =                           PluginEnabled;
            Options.RefreshIntervalSeconds =            RefreshIntervalSeconds;
            Options.ShowInvalidRegistrations =          ShowInvalidRegistrations;
            Options.SlowAircraftThresholdSpeedKnots =   SlowAircraftThresholdSpeedKnots;

            Options.GeofencedFeeds.Clear();
            foreach(var feedOption in _GeofenceFeedListViewHelper.AllAttachedValues) {
                Options.GeofencedFeeds.Add(feedOption);
            }
        }

        private void CopySelectedGeofenceFeedToControls()
        {
            var feed = _GeofenceFeedListViewHelper.SelectedAttachedValues.FirstOrDefault();

            FeedName =          feed?.FeedName ?? "";
            CentreOn =          feed?.CentreOn ?? GeofenceCentreOn.Coordinate;
            GeofenceWidth =     feed?.Width ?? 0.0;
            GeofenceHeight =    feed?.Height ?? 0.0;
            DistanceUnit =      feed?.DistanceUnit ?? DistanceUnit.Kilometres;
            Latitude =          feed?.Latitude ?? 0.0;
            Longitude =         feed?.Longitude ?? 0.0;
            AirportCode =       feed?.AirportCode ?? "";
            PilotCid =          feed?.PilotCid ?? 0;
        }

        private void EnableDisableControls()
        {
            var selectedGeofence =  _GeofenceFeedListViewHelper.SelectedAttachedValues.FirstOrDefault();
            var geofenceSelected =  selectedGeofence != null;
            var useAirport =        selectedGeofence?.CentreOn == GeofenceCentreOn.Airport;
            var useCoordinate =     selectedGeofence?.CentreOn == GeofenceCentreOn.Coordinate;
            var usePilot =          selectedGeofence?.CentreOn == GeofenceCentreOn.PilotCid;

            txtFeedName.Enabled = geofenceSelected;
            cmbCentreOn.Enabled = geofenceSelected;
            nudWidth.Enabled = geofenceSelected;
            nudHeight.Enabled = geofenceSelected;
            cmbDistanceUnit.Enabled = geofenceSelected;
            nudLatitude.Enabled = useCoordinate;
            nudLongitude.Enabled = useCoordinate;
            txtAirportCode.Enabled = useAirport;
            nudPilotCid.Enabled = usePilot;

            btnDelete.Enabled = geofenceSelected;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                PluginLocalise.Form(this);
            }
        }

        private void lvwGeofencedFeeds_SelectedIndexChanged(object sender, EventArgs e)
        {
            CopySelectedGeofenceFeedToControls();
            EnableDisableControls();
        }

        private void UpdateSelectedGeofencedFeed(Action<GeofenceFeedOption> updateFeed)
        {
            var selectedFeed = _GeofenceFeedListViewHelper.SelectedAttachedValues.FirstOrDefault();
            if(selectedFeed != null) {
                updateFeed(selectedFeed);
                _GeofenceFeedListViewHelper.RefreshList(_Options.GeofencedFeeds);
            }
        }

        private void txtFeedName_TextChanged(object sender, EventArgs e) => UpdateSelectedGeofencedFeed(r => r.FeedName = FeedName);

        private void cmbCentreOn_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedGeofencedFeed(r => r.CentreOn = CentreOn);
            EnableDisableControls();
        }

        private void nudWidth_ValueChanged(object sender, EventArgs e) => UpdateSelectedGeofencedFeed(r => r.Width = GeofenceWidth);

        private void nudHeight_ValueChanged(object sender, EventArgs e) => UpdateSelectedGeofencedFeed(r => r.Height = GeofenceHeight);

        private void cmbDistanceUnit_SelectedIndexChanged(object sender, EventArgs e) => UpdateSelectedGeofencedFeed(r => r.DistanceUnit = DistanceUnit);

        private void nudLatitude_ValueChanged(object sender, EventArgs e) => UpdateSelectedGeofencedFeed(r => r.Latitude = Latitude);

        private void nudLongitude_ValueChanged(object sender, EventArgs e) => UpdateSelectedGeofencedFeed(r => r.Longitude = Longitude);

        private void txtAirportCode_TextChanged(object sender, EventArgs e) => UpdateSelectedGeofencedFeed(r => r.AirportCode = AirportCode);

        private void nudPilotCid_ValueChanged(object sender, EventArgs e) => UpdateSelectedGeofencedFeed(r => r.PilotCid = PilotCid);

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var newOptions = new GeofenceFeedOption();
            var newOptionsList = new GeofenceFeedOption[] { newOptions };

            Options.GeofencedFeeds.Add(newOptions);
            _GeofenceFeedListViewHelper.RefreshList(Options.GeofencedFeeds);
            _GeofenceFeedListViewHelper.ScrollToFirst(newOptionsList);
            _GeofenceFeedListViewHelper.SelectedAttachedValues = newOptionsList;
            txtFeedName.Focus();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var selectedOption = _GeofenceFeedListViewHelper.SelectedAttachedValues.FirstOrDefault();
            if(selectedOption != null) {
                _Options.GeofencedFeeds.Remove(selectedOption);
                _GeofenceFeedListViewHelper.RefreshList(_Options.GeofencedFeeds);
                CopySelectedGeofenceFeedToControls();
                EnableDisableControls();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            CopyControlsToOptions();
        }
    }
}
