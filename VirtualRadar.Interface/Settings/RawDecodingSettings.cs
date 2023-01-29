// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The data-transfer object that carries the configuration of the raw message decoder.
    /// </summary>
    [Serializable]
    public class RawDecodingSettings
    {
        /// <summary>
        /// No longer used. Not marked as Obsolete because Microsoft decided it'd be a good idea to break loading and saving data via XmlSerialiser by ignoring Obsolete properties.
        /// </summary>
        // It'd be nice to use this wouldn't it? Microsoft rendered it unusable. // [Obsolete("Use Configuration.Receivers instead", false)]
        public int ReceiverLocationId { get; set; }

        /// <summary>
        /// Gets or sets the range of the receiver in kilometres.
        /// </summary>
        public int ReceiverRange { get; set; } = 650;

        /// <summary>
        /// Gets or sets a value indicating that DF19/AF0 is to be interpretted as an extended squitter message.
        /// </summary>
        public bool IgnoreMilitaryExtendedSquitter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that decoded locations that are further away than the receiver can see are still
        /// allowed through, disabling part of the ICAO reasonableness tests.
        /// </summary>
        public bool SuppressReceiverRangeCheck { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating that local decoding should be used to determine the initial position of an aircraft
        /// instead of global decoding of an odd / even frame.
        /// </summary>
        /// <remarks>
        /// If the aircraft is so far away that the local decode produces the wrong position then it will eventually be picked up
        /// by the ICAO reasonableness tests (perform global decode using CPR values not used in the initial decode and confirm
        /// that position corresponds). If it fails that test then the position is reset and the next initial fix is forced to
        /// be made using a global decode.
        /// </remarks>
        public bool UseLocalDecodeForInitialPosition { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of seconds that can elapse when performing global decoding on airborne position messages.
        /// </summary>
        public int AirborneGlobalPositionLimit { get; set; } = 10;

        /// <summary>
        /// Gets or sets the maximum number of seconds that can elapse when performing global decoding on surface position messages for vehicles travelling over 25 km/h.
        /// </summary>
        public int FastSurfaceGlobalPositionLimit { get; set; } = 25;

        /// <summary>
        /// Gets or sets the maximum number of seconds that can elapse when performing global decoding on surface position messages for vehicles travelling at or under 25 km/h.
        /// </summary>
        public int SlowSurfaceGlobalPositionLimit { get; set; } = 50;

        /// <summary>
        /// Gets or sets the maximum number of kilometres an aircraft can travel while airborne over 30 seconds before a local position decode is deemed invalid.
        /// </summary>
        public double AcceptableAirborneSpeed { get; set; } = 15.0;

        /// <summary>
        /// Gets or sets the maximum number of kilometres an aircraft can travel while landing or taking off over 30 seconds before a local position decode is deemed invalid.
        /// </summary>
        public double AcceptableAirSurfaceTransitionSpeed { get; set; } = 5.0;

        /// <summary>
        /// Gets or sets the maximum number of kilometres an surface vehicle can travel over 30 seconds before a local position decode is deemed invalid.
        /// </summary>
        public double AcceptableSurfaceSpeed { get; set; } = 3.0;

        /// <summary>
        /// Gets or sets a value indicating that callsigns should not be extracted from BDS2,0 messages.
        /// </summary>
        public bool IgnoreCallsignsInBds20 { get; set; }

        /// <summary>
        /// Gets or sets the number of times the same ICAO is seen in PI0 message before it is accepted as valid.
        /// </summary>
        /// <remarks>The minimum value for this is 1 - i.e. accept it immediately.</remarks>
        public int AcceptIcaoInPI0Count { get; set; } = 1;

        /// <summary>
        /// Gets or sets the number of seconds over which the same ICAO is seen in PI0 messages before it is accepted as valid.
        /// </summary>
        public int AcceptIcaoInPI0Seconds { get; set; } = 1;

        /// <summary>
        /// Gets or sets the number of times the same ICAO is seen in messages that do not have PI before it is accepted as valid.
        /// </summary>
        /// <remarks>If this is zero then ICAOs are never accepted from messages that do not have PI.</remarks>
        public int AcceptIcaoInNonPICount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of seconds over which the same ICAO is seen in messages that do not have PI before it is accepted as valid.
        /// </summary>
        public int AcceptIcaoInNonPISeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets a value indicating that ICAOs of 000000 are to be suppressed.
        /// </summary>
        public bool SuppressIcao0 { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating that ICAOs that are not in a recognised code block
        /// are to be ignored for messages that have parity.
        /// </summary>
        public bool IgnoreInvalidCodeBlockInParityMessages { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating that ICAOs that are not in a recognised code block
        /// are to be ignored for messages that do not have parity.
        /// </summary>
        public bool IgnoreInvalidCodeBlockInOtherMessages { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether TIS-B messages should be used or ignored.
        /// </summary>
        public bool SuppressTisbDecoding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether DF18 CF1 messages should be used.
        /// </summary>
        /// <remarks>
        /// DF18 is the non-transponder extended squitter (i.e. messages from things that are not aircraft).
        /// The control field has two common values. CF0 indicates an ADSB message where the 3 byte AA ICAO ID
        /// is a valid ICAO ID, CF1 indicates same but the ID is not valid ICAO, it is meaningful only to the
        /// operator of the vehicle. However, some airports have been seen operating a fleet of ground vehicles
        /// where all of them are allocated ICAOs from a small range of valid IDs but some transmit CF0 and
        /// some CF1. This flag tells VRS to assume that CF1 IDs are valid ICAO and use them.
        /// </remarks>
        public bool AssumeDF18CF1IsIcao { get; set; } = true;
    }
}
