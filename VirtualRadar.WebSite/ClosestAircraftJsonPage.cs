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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// A class that handles the requests from the proximity gadget / widgets
    /// </summary>
    class ClosestAircraftJsonPage : Page
    {
        /// <summary>
        /// The configuration setting that controls whether Internet clients can retrieve details of the closest aircraft.
        /// </summary>
        private bool _InternetClientCanRequestClosestAircraft;

        /// <summary>
        /// The singleton receiver manager - we retain a reference to it to save having to constantly resolve the singleton.
        /// </summary>
        private IFeedManager _ReceiverManager;

        /// <summary>
        /// The receiver to use when responding to requests for the closest aircraft.
        /// </summary>
        private int _ReceiverId;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="webSite"></param>
        public ClosestAircraftJsonPage(WebSite webSite) : base(webSite)
        {
        }

        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="configuration"></param>
        protected override void DoLoadConfiguration(Configuration configuration)
        {
            base.DoLoadConfiguration(configuration);
            _ReceiverManager = Factory.Resolve<IFeedManager>().Singleton;
            _InternetClientCanRequestClosestAircraft = configuration.InternetClientSettings.AllowInternetProximityGadgets;
            _ReceiverId = configuration.GoogleMapSettings.ClosestAircraftReceiverId;
        }

        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override bool DoHandleRequest(Interface.WebServer.IWebServer server, Interface.WebServer.RequestReceivedEventArgs args)
        {
            bool result = false;

            if((!args.IsInternetRequest || _InternetClientCanRequestClosestAircraft) && args.PathAndFile.Equals("/ClosestAircraft.json", StringComparison.OrdinalIgnoreCase)) {
                result = true;

                var latitude = QueryNDouble(args, "lat");
                var longitude = QueryNDouble(args, "lng");

                var json = new ProximityGadgetAircraftJson();

                if(latitude == null || longitude == null) json.WarningMessage = "Position not supplied";
                else {
                    var receiver = _ReceiverManager.GetByUniqueId(_ReceiverId, ignoreInvisibleFeeds: true);
                    if(receiver == null) json.WarningMessage = "Receiver is offline";
                    else {
                        var baseStationAircraftList = receiver.AircraftList;

                        long timeStamp, dataVersion;
                        var aircraftList = baseStationAircraftList.TakeSnapshot(out timeStamp, out dataVersion);

                        IAircraft closestAircraft = null;
                        double? closestDistance = null;

                        foreach(var aircraft in aircraftList) {
                            double? distance = null;
                            if(aircraft.Latitude != null && aircraft.Longitude != null) {
                                distance = GreatCircleMaths.Distance(latitude, longitude, aircraft.Latitude, aircraft.Longitude);
                                if(distance != null && closestAircraft == null || distance < closestDistance) {
                                    closestAircraft = aircraft;
                                    closestDistance = distance;
                                }
                            }

                            if(aircraft.Emergency == true) json.EmergencyAircraft.Add(CreateProximityGadgetClosestAircraftJson(aircraft, distance, latitude, longitude));
                        }

                        if(closestAircraft != null) {
                            var closestJsonAircraft = CreateProximityGadgetClosestAircraftJson(closestAircraft, closestDistance, latitude, longitude);
                            json.ClosestAircraft = closestJsonAircraft;
                        }
                    }
                }

                Responder.SendJson(args.Request, args.Response, json, null, MimeType.Text);       // The proximity gagdet is expecting the text MIME type and will ignore JSON...
                args.Classification = ContentClassification.Json;
            }

            return result;
        }

        /// <summary>
        /// Creates an object that represents an aircraft in <see cref="ProximityGadgetAircraftJson"/>.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="distance"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        private ProximityGadgetClosestAircraftJson CreateProximityGadgetClosestAircraftJson(IAircraft aircraft, double? distance, double? latitude, double? longitude)
        {
            var result = new ProximityGadgetClosestAircraftJson() {
                Altitude = FormatNullable(aircraft.Altitude),
                BearingFromHere = FormatNullable(GreatCircleMaths.Bearing(latitude, longitude, aircraft.Latitude, aircraft.Longitude, null, false, false), null, 1),
                Callsign = aircraft.Callsign,
                Destination = aircraft.Destination,
                DistanceFromHere = FormatNullable(distance, null, 2),
                Emergency = aircraft.Emergency.GetValueOrDefault(),
                GroundSpeed = FormatNullable(aircraft.GroundSpeed),
                HasPicture = !String.IsNullOrEmpty(aircraft.PictureFileName),
                Icao24 = aircraft.Icao24 ?? "",
                Icao24Invalid = aircraft.Icao24Invalid,
                Latitude = FormatNullable(aircraft.Latitude),
                Longitude = FormatNullable(aircraft.Longitude),
                Manufacturer = aircraft.Manufacturer,
                Model = aircraft.Model,
                Operator = aircraft.Operator,
                OperatorIcao = aircraft.OperatorIcao,
                Origin = aircraft.Origin,
                Registration = aircraft.Registration,
                Squawk = aircraft.Squawk.GetValueOrDefault() == 0 ? null : String.Format("{0:0000}", aircraft.Squawk),
                Track = FormatNullable(aircraft.Track, null, 1),
                Type = aircraft.Type,
                VerticalRate = FormatNullable(aircraft.VerticalRate),
            };
            result.Stopovers.AddRange(aircraft.Stopovers);

            return result;
        }
    }
}