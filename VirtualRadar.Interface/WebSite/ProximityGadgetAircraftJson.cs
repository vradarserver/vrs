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
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The data transfer object for the JSON file that is sent to the Windows 7 promixity gadget.
    /// </summary>
    [DataContract]
    [KnownType(typeof(ProximityGadgetClosestAircraftJson))]
    public class ProximityGadgetAircraftJson
    {
        /// <summary>
        /// Gets or sets the closest aircraft, if any.
        /// </summary>
        [DataMember(Name="closest", EmitDefaultValue=false)]
        public ProximityGadgetClosestAircraftJson ClosestAircraft { get; set; }

        /// <summary>
        /// Gets a list of every aircraft that is transmitting an emergency squawk.
        /// </summary>
        [DataMember(Name="emergencyAircraft")]
        public List<ProximityGadgetClosestAircraftJson> EmergencyAircraft { get; private set; } = new List<ProximityGadgetClosestAircraftJson>();

        /// <summary>
        /// Gets or sets the content of any warnings that need to be transmitted back to the gadget.
        /// </summary>
        [DataMember(Name="warningText", EmitDefaultValue=false)]
        public string WarningMessage { get; set; }

        /// <summary>
        /// Returns a model given an aircraft list and a point to measure from.
        /// </summary>
        /// <param name="aircraftList"></param>
        /// <param name="originLatitude"></param>
        /// <param name="originLongitude"></param>
        /// <returns></returns>
        public static ProximityGadgetAircraftJson ToModel(IEnumerable<IAircraft> aircraftList, double? originLatitude, double? originLongitude)
        {
            ProximityGadgetAircraftJson result = null;

            if(aircraftList != null) {
                result = new ProximityGadgetAircraftJson();

                if(originLatitude == null || originLongitude == null) {
                    result.WarningMessage = "Position not supplied";
                } else {
                    IAircraft closestAircraft = null;
                    double? closestDistance = null;

                    foreach(var aircraft in aircraftList) {
                        double? distance = null;
                        if(aircraft.Latitude != null && aircraft.Longitude != null) {
                            distance = GreatCircleMaths.Distance(originLatitude, originLongitude, aircraft.Latitude, aircraft.Longitude);
                            if(distance != null && closestAircraft == null || distance < closestDistance) {
                                closestAircraft = aircraft;
                                closestDistance = distance;
                            }
                        }

                        if(aircraft.Emergency == true) {
                            result.EmergencyAircraft.Add(ProximityGadgetClosestAircraftJson.ToModel(aircraft, originLatitude, originLongitude));
                        }
                    }

                    if(closestAircraft != null) {
                        result.ClosestAircraft = ProximityGadgetClosestAircraftJson.ToModel(closestAircraft, originLatitude, originLongitude);
                    }
                }
            }

            return result;
        }
    }
}
