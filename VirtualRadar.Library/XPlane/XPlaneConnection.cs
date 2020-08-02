// Copyright © 2020 onwards, Andrew Whewell
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
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.XPlane;

namespace VirtualRadar.Library.XPlane
{
    /// <summary>
    /// The default implementation of the object that manages a singleton connection to XPlane.
    /// </summary>
    class XPlaneConnection : IXPlaneConnection
    {
        /// <summary>
        /// The object that we use to talk to XPlane.
        /// </summary>
        private IXPlaneUdp _XplaneUdp;

        /// <summary>
        /// The shared configuration.
        /// </summary>
        private ISharedConfiguration _SharedConfig;

        /// <summary>
        /// The callsign used to distinguish the XPlane aircraft from other flight simulator aircraft.
        /// </summary>
        const string XPlaneCallsign = "XPLANE";

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ISimpleAircraftList FlightSimulatorAircraftList { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="settings"></param>
        public void Connect(XPlaneSettings settings)
        {
            if(_SharedConfig == null) {
                _SharedConfig = Factory.ResolveSingleton<ISharedConfiguration>();
            }

            if(_XplaneUdp != null) {
                Disconnect();
            }

            var xplaneUdp = Factory.Resolve<IXPlaneUdp>();
            xplaneUdp.Initialise(
                settings.Host,
                settings.XPlanePort,
                settings.ReplyPort
            );

            xplaneUdp.SendRPOS(1);
            xplaneUdp.StartListener();

            _XplaneUdp = xplaneUdp;
            _XplaneUdp.RposReplyReceived += XPlaneUdp_RposReplyReceived;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Disconnect()
        {
            if(_XplaneUdp != null) {
                _XplaneUdp.RposReplyReceived -= XPlaneUdp_RposReplyReceived;
                try {
                    _XplaneUdp.Dispose();
                } catch {
                }
                _XplaneUdp = null;
            }
        }

        /// <summary>
        /// Called when XPlane sends aircraft details.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XPlaneUdp_RposReplyReceived(object sender, EventArgs<XPlaneRposReply> e)
        {
            var xplane = e.Value;
            var config = _SharedConfig.Get();

            var aircraftList = FlightSimulatorAircraftList;
            if(aircraftList != null) {
                lock(aircraftList.ListSyncLock) {
                    var aircraft = aircraftList.Aircraft.FirstOrDefault(r => r.Callsign == XPlaneCallsign);
                    if(aircraft == null) {
                        aircraft = Factory.Resolve<IAircraft>();
                        aircraft.Icao24 = "000002";
                        aircraft.UniqueId = 2;
                        aircraft.Callsign = XPlaneCallsign;
                        aircraft.AltitudeType = AltitudeType.Barometric;
                        aircraftList.Aircraft.Add(aircraft);
                    }

                    lock(aircraft) {
                        var now = DateTime.UtcNow;
                        aircraft.DataVersion = Math.Max(now.Ticks, aircraft.DataVersion + 1);
                        aircraft.Latitude =     xplane.Latitude;
                        aircraft.Longitude =    xplane.Longitude;
                        aircraft.GroundSpeed =  xplane.GroundSpeedKnots;
                        aircraft.Track =        xplane.Heading;
                        aircraft.Altitude =     xplane.AltitudeFeet;
                        aircraft.VerticalRate = xplane.VerticalRateFeetPerSecond;

                        aircraft.UpdateCoordinates(now, config.GoogleMapSettings.ShortTrailLengthSeconds);
                    }
                }
            }
        }
    }
}
