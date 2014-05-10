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

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// An enumeration of fields that are subject to validation.
    /// </summary>
    public enum ValidationField
    {
        /// <summary>
        /// No field defined.
        /// </summary>
        None,

        /// <summary>
        /// The IP address of an instance of BaseStation.
        /// </summary>
        BaseStationAddress,

        /// <summary>
        /// The port number of an instance of BaseStation.
        /// </summary>
        BaseStationPort,

        /// <summary>
        /// The full path to a database file that conforms with the de-facto standard for aircraft information as used by Kinetic.
        /// </summary>
        BaseStationDatabase,

        /// <summary>
        /// A value indicating whether something has been enabled or not.
        /// </summary>
        Enabled,

        /// <summary>
        /// End date.
        /// </summary>
        EndDate,

        /// <summary>
        /// The full path to the folder containing operator flag images.
        /// </summary>
        FlagsFolder,

        /// <summary>
        /// The initial number of seconds between refreshes of the Google Map aircraft list.
        /// </summary>
        InitialGoogleMapRefreshSeconds,

        /// <summary>
        /// The minimum number of seconds between refreshes of the Google Map aircraft list.
        /// </summary>
        MinimumGoogleMapRefreshSeconds,

        /// <summary>
        /// The full path to the folder containing pictures of aircraft.
        /// </summary>
        PicturesFolder,

        /// <summary>
        /// The full path to the folder containing silhouette images of aircraft.
        /// </summary>
        SilhouettesFolder,

        /// <summary>
        /// The name of the user that browser must send when authenticating.
        /// </summary>
        WebUserName,

        /// <summary>
        /// The location of a receiver.
        /// </summary>
        Location,

        /// <summary>
        /// The latitude portion of a location.
        /// </summary>
        Latitude,

        /// <summary>
        /// The longitude portion of a location.
        /// </summary>
        Longitude,

        /// <summary>
        /// The port number for a rebroadcast server.
        /// </summary>
        RebroadcastServerPort,

        /// <summary>
        /// The distance in kilometres over which a receiver can pick up messages.
        /// </summary>
        ReceiverRange,

        /// <summary>
        /// The time that can elapse between odd/even format CPR messages for global airborne position decoding.
        /// </summary>
        AirborneGlobalPositionLimit,

        /// <summary>
        /// The time that can elapse between odd/even format CPR messages for global fast surface vehicle position decoding.
        /// </summary>
        FastSurfaceGlobalPositionLimit,

        /// <summary>
        /// The time that can elapse between odd/even format CPR messages for global slow surface vehicle position decoding.
        /// </summary>
        SlowSurfaceGlobalPositionLimit,

        /// <summary>
        /// The highest speed in km/30 seconds allowable in the local decoding sanity check for airborne aircraft.
        /// </summary>
        AcceptableAirborneLocalPositionSpeed,

        /// <summary>
        /// The highest speed in km/30 seconds allowable in the local decoding sanity check for aircraft taking off or landing.
        /// </summary>
        AcceptableTransitionLocalPositionSpeed,

        /// <summary>
        /// The highest speed in km/30 seconds allowable in the local decoding sanity check for surface vehicles.
        /// </summary>
        AcceptableSurfaceLocalPositionSpeed,

        /// <summary>
        /// The port number that UPnP will assign to VRS.
        /// </summary>
        UPnpPortNumber,

        /// <summary>
        /// The number of idle minutes allowed to an Internet user before the site times out.
        /// </summary>
        InternetUserIdleTimeout,

        /// <summary>
        /// The initial zoom level for a Google map.
        /// </summary>
        GoogleMapZoomLevel,

        /// <summary>
        /// The number of days between version checks.
        /// </summary>
        CheckForNewVersions,

        /// <summary>
        /// The number of seconds after loss of signal before the aircraft is removed from the map.
        /// </summary>
        DisplayTimeout,

        /// <summary>
        /// The number of seconds after loss of signal before the aircraft is removed from the aircraft list.
        /// </summary>
        TrackingTimeout,

        /// <summary>
        /// The number of seconds a short trail lasts for.
        /// </summary>
        ShortTrailLength,

        /// <summary>
        /// The speed at which text converted to speech is read out.
        /// </summary>
        TextToSpeechSpeed,

        /// <summary>
        /// The COM port that a serial device is connected to.
        /// </summary>
        ComPort,

        /// <summary>
        /// The baud rate used on a COM port.
        /// </summary>
        BaudRate,

        /// <summary>
        /// The data bits used on a COM port.
        /// </summary>
        DataBits,

        /// <summary>
        /// A name or description.
        /// </summary>
        Name,

        /// <summary>
        /// A format.
        /// </summary>
        Format,

        /// <summary>
        /// A count of Mode-S messages that do not carry PI.
        /// </summary>
        AcceptIcaoInNonPICount,

        /// <summary>
        /// A number of seconds of Mode-S messages that do not carry PI.
        /// </summary>
        AcceptIcaoInNonPISeconds,

        /// <summary>
        /// A count of Mode-S messages that carry PI.
        /// </summary>
        AcceptIcaoInPI0Count,

        /// <summary>
        /// A number of seconds of Mode-S messages that carry PI.
        /// </summary>
        AcceptIcaoInPI0Seconds,

        /// <summary>
        /// The root folder of the site to serve.
        /// </summary>
        SiteRootFolder,

        /// <summary>
        /// A path and file to a page.
        /// </summary>
        PathAndFile,

        /// <summary>
        /// The receiver used by a rebroadcast server.
        /// </summary>
        RebroadcastReceiver,

        /// <summary>
        /// The receiver shown via the web site.
        /// </summary>
        WebSiteReceiver,

        /// <summary>
        /// The receiver that the Closest Aircraft desktop widget will use.
        /// </summary>
        ClosestAircraftReceiver,

        /// <summary>
        /// The receiver that drives some FSX features.
        /// </summary>
        FlightSimulatorXReceiver,

        /// <summary>
        /// The period of time that can elapse before a receiver is no longer considered to be the main source of messages for an ICAO.
        /// </summary>
        IcaoTimeout,

        /// <summary>
        /// A collection of receivers.
        /// </summary>
        ReceiverIds,

        /// <summary>
        /// The duration before a message is discarded from a rebroadcast server buffer.
        /// </summary>
        StaleSeconds,

        /// <summary>
        /// The login name for a user account.
        /// </summary>
        LoginName,

        /// <summary>
        /// The password for a user account.
        /// </summary>
        Password,
    }
}
