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
using System.Linq;
using System.Text;
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Interface.Adsb;

namespace VirtualRadar.Interface.BaseStation
{
    /// <summary>
    /// The interface for classes that can translate Mode-S and ADS-B messages into BaseStation messages.
    /// </summary>
    /// <remarks>
    /// Implementations are not guaranteed to be thread-safe.
    /// </remarks>
    public interface IRawMessageTranslator : IDisposable
    {
        /// <summary>
        /// Gets or sets the statistics to update when translating messages.
        /// </summary>
        IStatistics Statistics { get; set; }

        /// <summary>
        /// Gets or sets the location of the receiver.
        /// </summary>
        /// <remarks>
        /// If this is null then the <see cref="Translate"/> method cannot determine the location of ground vehicles using
        /// local CPR decoding.
        /// </remarks>
        GlobalCoordinate ReceiverLocation { get; set; }

        /// <summary>
        /// Gets or sets the maximum range of the receiver in kilometres.
        /// </summary>
        /// <remarks>
        /// This is used in the reasonableness test applied to decoded locations. It defaults to 650km, which is ~350
        /// nautical miles. This gives a range ~100 nmi past the horizon for receivers at sea level.
        /// </remarks>
        int ReceiverRangeKilometres { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that military extended squitter messages are to be ignored.
        /// </summary>
        /// <remarks>
        /// There is some confusion over whether Mode-S DF19 AF0 messages are to be interpreted in the same manner as
        /// DF17 messages. By default the code will process them in the same manner, if this flag is set then they will
        /// be ignored completely.
        /// </remarks>
        bool IgnoreMilitaryExtendedSquitter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that decoded locations that are further away than the receiver can see are still
        /// allowed through, disabling part of the ICAO reasonableness tests.
        /// </summary>
        bool SuppressReceiverRangeCheck { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that local decoding should be used to determine the initial position of an aircraft
        /// instead of global decoding of an odd / even frame.
        /// </summary>
        /// <remarks>
        /// If the aircraft is so far away that the local decode produces the wrong position then it will eventually be picked up
        /// by the ICAO reasonableness tests (perform another global decode using CPR values not used in the initial decode and
        /// confirm that position corresponds). If it fails that test then the position is reset and the next initial fix is forced
        /// to be made using a global decode.
        /// </remarks>
        bool UseLocalDecodeForInitialPosition { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that callsigns are not to be used if they came from what appeared to be a Mode-S BDS2,0
        /// message.
        /// </summary>
        /// <remarks>
        /// BDS2,0 messages cannot be accurately identified so callsigns that are extracted from them are potentially inaccurate. Most
        /// of the time they're correct so they're allowed by default.
        /// </remarks>
        bool SuppressCallsignsFromBds20 { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds that an odd/even pair of CPR coordinates must be received within to be
        /// usable for a global decode of airborne position.
        /// </summary>
        /// <remarks>
        /// This defaults to 10 seconds, as per the ICAO specs.
        /// </remarks>
        int GlobalDecodeAirborneThresholdMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds that an odd/even pair of CPR coordinates must be received within to be
        /// usable for a global decode of surface position of a vehicle travelling faster than 25 knots.
        /// </summary>
        /// <remarks>
        /// This defaults to 25 seconds, as per the ICAO specs.
        /// </remarks>
        int GlobalDecodeFastSurfaceThresholdMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds that an odd/even pair of CPR coordinates must be received within to be
        /// usable for a global decode of surface position of a vehicle travelling at or below 25 knots.
        /// </summary>
        /// <remarks>
        /// This defaults to 50 seconds, as per the ICAO specs.
        /// </remarks>
        int GlobalDecodeSlowSurfaceThresholdMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the number of kilometres over 30 seconds that an airborne vehicle can travel before a local decode is deemed to be inaccurate.
        /// </summary>
        double LocalDecodeMaxSpeedAirborne { get; set; }

        /// <summary>
        /// Gets or sets the number of kilometres / 30 seconds that a vehicle tranisitioning between airborne and surface positions can travel before a local decode is deemed to be inaccurate.
        /// </summary>
        double LocalDecodeMaxSpeedTransition { get; set; }

        /// <summary>
        /// Gets or sets the number of kilometres / 30 seconds that a vehicle on the ground can travel before a local decode is deemed to be inaccurate.
        /// </summary>
        double LocalDecodeMaxSpeedSurface { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds before aircraft that are no longer being received are forgotten about.
        /// </summary>
        int TrackingTimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the number of times an ICAO has to be seen in a Mode-S message (regardless of whether it carries PI) before it will be accepted. 0 disables the feature.
        /// </summary>
        int AcceptIcaoInNonPICount { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds over which <see cref="AcceptIcaoInNonPICount"/> will be measured.
        /// </summary>
        int AcceptIcaoInNonPIMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the number of times an ICAO has to be seen in a Mode-S message that carries PI, and where PI = 0, before it will be accepted.
        /// </summary>
        int AcceptIcaoInPI0Count { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds over which <see cref="AcceptIcaoInPI0Count"/> will be measured.
        /// </summary>
        int AcceptIcaoInPI0Milliseconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that ICAOs that are not in a recognised code block
        /// are to be ignored for messages that have parity.
        /// </summary>
        bool IgnoreInvalidCodeBlockInParityMessages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that ICAOs that are not in a recognised code block
        /// are to be ignored for messages that do not have parity.
        /// </summary>
        bool IgnoreInvalidCodeBlockInOtherMessages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether TIS-B messages should be ignored.
        /// </summary>
        bool SuppressTisbDecoding { get; set; }

        /// <summary>
        /// Raised during the processing of <see cref="Translate"/> when it becomes apparent that a previous position
        /// established for an aircraft was wrong and it has been reset.
        /// </summary>
        event EventHandler<EventArgs<string>> PositionReset;

        /// <summary>
        /// Translates the Mode-S message, and the optional ADS-B message carried by it, into a BaseStation message.
        /// </summary>
        /// <param name="messageReceivedUtc">The time that the message was originally received at UTC.</param>
        /// <param name="modeSMessage">A Mode-S message.</param>
        /// <param name="adsbMessage">Either the ADS-B message carried by the Mode-S message or null.</param>
        /// <returns>A BaseStation message translated from the Mode-S and ADS-B messages, or null if no translation is possible.</returns>
        BaseStationMessage Translate(DateTime messageReceivedUtc, ModeSMessage modeSMessage, AdsbMessage adsbMessage);
    }
}
