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
using System.ComponentModel;
using System.Linq.Expressions;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The data-transfer object that carries the configuration of the raw message decoder.
    /// </summary>
    [Serializable]
    public class RawDecodingSettings : INotifyPropertyChanged
    {
        /// <summary>
        /// No longer used. Not marked as Obsolete because Microsoft decided it'd be a good idea to break loading and saving data via XmlSerialiser by ignoring Obsolete properties.
        /// </summary>
        // It'd be nice to use this wouldn't it? Microsoft rendered it unusable. // [Obsolete("Use Configuration.Receivers instead", false)]
        public int ReceiverLocationId { get; set; }

        private int _ReceiverRange;
        /// <summary>
        /// Gets or sets the range of the receiver in kilometres.
        /// </summary>
        public int ReceiverRange
        {
            get { return _ReceiverRange; }
            set { SetField(ref _ReceiverRange, value, nameof(ReceiverRange)); }
        }

        private bool _IgnoreMilitaryExtendedSquitter;
        /// <summary>
        /// Gets or sets a value indicating that DF19/AF0 is to be interpretted as an extended squitter message.
        /// </summary>
        public bool IgnoreMilitaryExtendedSquitter
        {
            get { return _IgnoreMilitaryExtendedSquitter; }
            set { SetField(ref _IgnoreMilitaryExtendedSquitter, value, nameof(IgnoreMilitaryExtendedSquitter)); }
        }

        private bool _SuppressReceiverRangeCheck;
        /// <summary>
        /// Gets or sets a value indicating that decoded locations that are further away than the receiver can see are still
        /// allowed through, disabling part of the ICAO reasonableness tests.
        /// </summary>
        public bool SuppressReceiverRangeCheck
        {
            get { return _SuppressReceiverRangeCheck; }
            set { SetField(ref _SuppressReceiverRangeCheck, value, nameof(SuppressReceiverRangeCheck)); }
        }

        private bool _UseLocalDecodeForInitialPosition;
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
        public bool UseLocalDecodeForInitialPosition
        {
            get { return _UseLocalDecodeForInitialPosition; }
            set { SetField(ref _UseLocalDecodeForInitialPosition, value, nameof(UseLocalDecodeForInitialPosition)); }
        }

        private int _AirborneGlobalPositionLimit;
        /// <summary>
        /// Gets or sets the maximum number of seconds that can elapse when performing global decoding on airborne position messages.
        /// </summary>
        public int AirborneGlobalPositionLimit
        {
            get { return _AirborneGlobalPositionLimit; }
            set { SetField(ref _AirborneGlobalPositionLimit, value, nameof(AirborneGlobalPositionLimit)); }
        }

        private int _FastSurfaceGlobalPositionLimit;
        /// <summary>
        /// Gets or sets the maximum number of seconds that can elapse when performing global decoding on surface position messages for vehicles travelling over 25 km/h.
        /// </summary>
        public int FastSurfaceGlobalPositionLimit
        {
            get { return _FastSurfaceGlobalPositionLimit; }
            set { SetField(ref _FastSurfaceGlobalPositionLimit, value, nameof(FastSurfaceGlobalPositionLimit)); }
        }

        private int _SlowSurfaceGlobalPositionLimit;
        /// <summary>
        /// Gets or sets the maximum number of seconds that can elapse when performing global decoding on surface position messages for vehicles travelling at or under 25 km/h.
        /// </summary>
        public int SlowSurfaceGlobalPositionLimit
        {
            get { return _SlowSurfaceGlobalPositionLimit; }
            set { SetField(ref _SlowSurfaceGlobalPositionLimit, value, nameof(SlowSurfaceGlobalPositionLimit)); }
        }

        private double _AcceptableAirborneSpeed;
        /// <summary>
        /// Gets or sets the maximum number of kilometres an aircraft can travel while airborne over 30 seconds before a local position decode is deemed invalid.
        /// </summary>
        public double AcceptableAirborneSpeed
        {
            get { return _AcceptableAirborneSpeed; }
            set { SetField(ref _AcceptableAirborneSpeed, value, nameof(AcceptableAirborneSpeed)); }
        }

        private double _AcceptableAirSurfaceTransitionSpeed;
        /// <summary>
        /// Gets or sets the maximum number of kilometres an aircraft can travel while landing or taking off over 30 seconds before a local position decode is deemed invalid.
        /// </summary>
        public double AcceptableAirSurfaceTransitionSpeed
        {
            get { return _AcceptableAirSurfaceTransitionSpeed; }
            set { SetField(ref _AcceptableAirSurfaceTransitionSpeed, value, nameof(AcceptableAirSurfaceTransitionSpeed)); }
        }

        private double _AcceptableSurfaceSpeed;
        /// <summary>
        /// Gets or sets the maximum number of kilometres an surface vehicle can travel over 30 seconds before a local position decode is deemed invalid.
        /// </summary>
        public double AcceptableSurfaceSpeed
        {
            get { return _AcceptableSurfaceSpeed; }
            set { SetField(ref _AcceptableSurfaceSpeed, value, nameof(AcceptableSurfaceSpeed)); }
        }

        private bool _IgnoreCallsignsInBds20;
        /// <summary>
        /// Gets or sets a value indicating that callsigns should not be extracted from BDS2,0 messages.
        /// </summary>
        public bool IgnoreCallsignsInBds20
        {
            get { return _IgnoreCallsignsInBds20; }
            set { SetField(ref _IgnoreCallsignsInBds20, value, nameof(IgnoreCallsignsInBds20)); }
        }

        private int _AcceptIcaoInPI0Count;
        /// <summary>
        /// Gets or sets the number of times the same ICAO is seen in PI0 message before it is accepted as valid.
        /// </summary>
        /// <remarks>The minimum value for this is 1 - i.e. accept it immediately.</remarks>
        public int AcceptIcaoInPI0Count
        {
            get { return _AcceptIcaoInPI0Count; }
            set { SetField(ref _AcceptIcaoInPI0Count, value, nameof(AcceptIcaoInPI0Count)); }
        }

        private int _AcceptIcaoInPI0Seconds;
        /// <summary>
        /// Gets or sets the number of seconds over which the same ICAO is seen in PI0 messages before it is accepted as valid.
        /// </summary>
        public int AcceptIcaoInPI0Seconds
        {
            get { return _AcceptIcaoInPI0Seconds; }
            set { SetField(ref _AcceptIcaoInPI0Seconds, value, nameof(AcceptIcaoInPI0Seconds)); }
        }

        private int _AcceptIcaoInNonPICount;
        /// <summary>
        /// Gets or sets the number of times the same ICAO is seen in messages that do not have PI before it is accepted as valid.
        /// </summary>
        /// <remarks>If this is zero then ICAOs are never accepted from messages that do not have PI.</remarks>
        public int AcceptIcaoInNonPICount
        {
            get { return _AcceptIcaoInNonPICount; }
            set { SetField(ref _AcceptIcaoInNonPICount, value, nameof(AcceptIcaoInNonPICount)); }
        }

        private int _AcceptIcaoInNonPISeconds;
        /// <summary>
        /// Gets or sets the number of seconds over which the same ICAO is seen in messages that do not have PI before it is accepted as valid.
        /// </summary>
        public int AcceptIcaoInNonPISeconds
        {
            get { return _AcceptIcaoInNonPISeconds; }
            set { SetField(ref _AcceptIcaoInNonPISeconds, value, nameof(AcceptIcaoInNonPISeconds)); }
        }

        private bool _SuppressIcao0;
        /// <summary>
        /// Gets or sets a value indicating that ICAOs of 000000 are to be suppressed.
        /// </summary>
        public bool SuppressIcao0
        {
            get { return _SuppressIcao0; }
            set { SetField(ref _SuppressIcao0, value, nameof(SuppressIcao0)); }
        }

        private bool _IgnoreInvalidCodeBlockInParityMessages;
        /// <summary>
        /// Gets or sets a value indicating that ICAOs that are not in a recognised code block
        /// are to be ignored for messages that have parity.
        /// </summary>
        public bool IgnoreInvalidCodeBlockInParityMessages
        {
            get { return _IgnoreInvalidCodeBlockInParityMessages; }
            set { SetField(ref _IgnoreInvalidCodeBlockInParityMessages, value, nameof(IgnoreInvalidCodeBlockInParityMessages)); }
        }

        private bool _IgnoreInvalidCodeBlockInOtherMessages;
        /// <summary>
        /// Gets or sets a value indicating that ICAOs that are not in a recognised code block
        /// are to be ignored for messages that do not have parity.
        /// </summary>
        public bool IgnoreInvalidCodeBlockInOtherMessages
        {
            get { return _IgnoreInvalidCodeBlockInOtherMessages; }
            set { SetField(ref _IgnoreInvalidCodeBlockInOtherMessages, value, nameof(IgnoreInvalidCodeBlockInOtherMessages)); }
        }

        private bool _SuppressTisbDecoding;
        /// <summary>
        /// Gets or sets a value indicating whether TIS-B messages should be used or ignored.
        /// </summary>
        public bool SuppressTisbDecoding
        {
            get { return _SuppressTisbDecoding; }
            set { SetField(ref _SuppressTisbDecoding, value, nameof(SuppressTisbDecoding)); }
        }

        private bool _AssumeDF18CF1IsIcao;
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
        public bool AssumeDF18CF1IsIcao
        {
            get { return _AssumeDF18CF1IsIcao; }
            set { SetField(ref _AssumeDF18CF1IsIcao, value, nameof(AssumeDF18CF1IsIcao)); }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            var handler = PropertyChanged;
            if(handler != null) {
                handler(this, args);
            }
        }

        /// <summary>
        /// Sets the field's value and raises <see cref="PropertyChanged"/>, but only when the value has changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="fieldName"></param>
        /// <returns>True if the value was set because it had changed, false if the value did not change and the event was not raised.</returns>
        protected bool SetField<T>(ref T field, T value, string fieldName)
        {
            var result = !EqualityComparer<T>.Default.Equals(field, value);
            if(result) {
                field = value;
                OnPropertyChanged(new PropertyChangedEventArgs(fieldName));
            }

            return result;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public RawDecodingSettings()
        {
            AirborneGlobalPositionLimit = 10;
            FastSurfaceGlobalPositionLimit = 25;
            SlowSurfaceGlobalPositionLimit = 50;
            AcceptableAirborneSpeed = 15.0;
            AcceptableAirSurfaceTransitionSpeed = 5.0;
            AcceptableSurfaceSpeed = 3.0;
            ReceiverRange = 650;
            SuppressReceiverRangeCheck = true;
            AcceptIcaoInNonPICount = 0;
            AcceptIcaoInNonPISeconds = 5;
            AcceptIcaoInPI0Count = 1;
            AcceptIcaoInPI0Seconds = 1;
            SuppressIcao0 = true;
            IgnoreInvalidCodeBlockInParityMessages = false;
            IgnoreInvalidCodeBlockInOtherMessages = true;
            AssumeDF18CF1IsIcao = true;
        }
    }
}
