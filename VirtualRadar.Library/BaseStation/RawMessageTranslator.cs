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
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Library.BaseStation
{
    /// <summary>
    /// The default implementation of the <see cref="IRawMessageTranslator"/> interface.
    /// </summary>
    sealed class RawMessageTranslator : IRawMessageTranslator
    {
        #region Private class - PositionMessage, TrackedAircraft
        /// <summary>
        /// An internal class that associates the time a message was received with a position.
        /// </summary>
        class PositionMessage
        {
            public DateTime Time;
            public CompactPositionReportingCoordinate Cpr;

            public override string ToString()
            {
                return String.Format("{0:00}:{1:00}.{2:000}={3}", Time.Minute, Time.Second, Time.Millisecond, Cpr);
            }
        }

        /// <summary>
        /// An internal enumeration of the different stages that the determination of position goes through.
        /// </summary>
        enum TrackedAircraftState
        {
            /// <summary>
            /// The correct initial position has not yet been determined, a global decode is required to start the ball rolling.
            /// </summary>
            Acquiring,

            /// <summary>
            /// An initial position has been determined through a global decode but we are not certain that it is correct. Position
            /// updates will be made via local decodes but a second global decode will be made as soon as possible to confirm the
            /// validity of the first.
            /// </summary>
            Acquired,

            /// <summary>
            /// The correct initial position has been determined and subsequent position updates are being made through local decodes.
            /// </summary>
            Tracking,
        }

        /// <summary>
        /// An internal class that carries information about aircraft that are being tracked.
        /// </summary>
        class TrackedAircraft
        {
            public string Icao24;                           // The ICAO24 code of the aircraft.
            public DateTime LatestMessageUtc;               // Time at UTC of the latest message received from the vehicle.
            public PositionMessage LaterCpr;                // The latest CPR message from the vehicle.
            public PositionMessage EarlierCpr;              // The CPR message received directly before the latest.
            public TrackedAircraftState State;              // The current stage of position acquisition for the vehicle.
            public GlobalCoordinate LastPosition;           // The last position calculated for the vehicle.
            public DateTime LastPositionTime;               // The time of the message that gave rise to the last position calculated for the vehicle.
            public bool LastPositionIsSurface;              // True if LastPosition is a surface position, false if it is an airborne position
            public PositionMessage AcquisitionCpr;          // The value of LaterCpr recorded when the position was first acquired.
            public bool SeenAdsbCallsign;                   // True if an ADS-B message containing a callsign has been seen for the aircraft.
            public bool InitialLocalDecodingUnsafe;         // True if the user is allowing an initial fix by local decoding and it produced a wrong initial result for this aircraft.

            public void RecordMessage(DateTime time, CompactPositionReportingCoordinate cpr)
            {
                EarlierCpr = LaterCpr;
                LaterCpr = new PositionMessage() { Time = time, Cpr = cpr };
            }

            public override string ToString()
            {
                return Icao24 ?? "";
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object that provides clock services.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// The aircraft for which valid messages have already been received.
        /// </summary>
        private ExpiringDictionary<int, TrackedAircraft> _TrackedAircraft = new ExpiringDictionary<int, TrackedAircraft>(360000, 10000);

        /// <summary>
        /// A map of ICAO24 codes to the times that messages with parity for those codes were received.
        /// </summary>
        private ExpiringDictionary<int, List<DateTime>> _AcceptIcaoParity = new ExpiringDictionary<int, List<DateTime>>(360000, 10000);

        /// <summary>
        /// A map of ICAO24 codes to the times that messages for those codes were received.
        /// </summary>
        private ExpiringDictionary<int, List<DateTime>> _AcceptIcaoNonParity = new ExpiringDictionary<int, List<DateTime>>(360000, 10000);

        /// <summary>
        /// The object that can decode Compact Position Report coordinates for us.
        /// </summary>
        private ICompactPositionReporting _CompactPositionReporting;

        /// <summary>
        /// The resolution of surface encoded CPR coordinates in kilometres (1.25 metres).
        /// </summary>
        /// <remarks>
        /// Surface encoded CPR coordinates are accurate to 1.25 metres.
        /// </remarks>
        private const double SurfaceResolution = 0.00125;

        /// <summary>
        /// The resolution of airborne encoded CPR coordinates in kilometres (5 metres).
        /// </summary>
        /// <remarks>
        /// Airborne encoded CPR coordinates are accurate to 5 metres.
        /// </remarks>
        private const double AirborneResolution = 0.005;

        /// <summary>
        /// The speed at which a vehicle travelling on the surface is considered to be 'fast' in the ICAO CPR decoding specs.
        /// </summary>
        private const double SlowFastSurfaceTransitionSpeed = 25.0;

        /// <summary>
        /// The number of seconds over which one of the LocalDecodeMaxSpeed units is applied in the reasonableness tests for
        /// local decodes.
        /// </summary>
        /// <remarks><para>
        /// The ICAO spec recommends that the point resulting from a local decode is measured against the previous location to
        /// find the distance travelled over the time since the previous decode. If the speed exceeds so-many NMI per 30 seconds
        /// then the local decode is likely to be wrong, the vehicle will have had to travel excessively fast for it to be
        /// accurate. This is the time period, the denominator for that speed.
        /// </para><para>
        /// The spec makes it clear that if the duration between messages is less than this value then the distances are NOT
        /// prorated over the duration - e.g. if the maximum allowable distance for an airborne vehicle is 6nmi in 30 seconds then
        /// if it travels 5.5nmi in 10 seconds that's still allowed, whereas a prorated distance would be 2nmi for 10 seconds.
        /// </para><para>
        /// The spec is less clear about durations in excess of 30 seconds. The code takes the the duration divided by 30 seconds,
        /// rounding up to the next integer, and then the value is multiplied by the allowable distance(s). The distance travelled
        /// as implied by the local decode can't exceed that total. Bear in mind that errors in local decodes tend to be temporary
        /// and give rise to VERY large errors, normally a couple of degrees of longitude or latitude away from the true position.
        /// We can afford to be fairly generous when trying to determine whether the aircraft's speed would have been excessive to
        /// reach those positions.
        /// </para></remarks>
        private const int LocalDecodeMaxSpeedSeconds = 30;

        /// <summary>
        /// True if the object has been disposed of.
        /// </summary>
        private bool _Disposed;

        /// <summary>
        /// The standing data manager that the code will use.
        /// </summary>
        private IStandingDataManager _StandingDataManager;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IStatistics Statistics { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public GlobalCoordinate ReceiverLocation { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int ReceiverRangeKilometres { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IgnoreMilitaryExtendedSquitter { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool SuppressReceiverRangeCheck { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UseLocalDecodeForInitialPosition { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool SuppressCallsignsFromBds20 { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int GlobalDecodeAirborneThresholdMilliseconds { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int GlobalDecodeFastSurfaceThresholdMilliseconds { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int GlobalDecodeSlowSurfaceThresholdMilliseconds { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public double LocalDecodeMaxSpeedAirborne { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public double LocalDecodeMaxSpeedTransition { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public double LocalDecodeMaxSpeedSurface { get; set; }

        private int _TrackingTimeoutSeconds;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int TrackingTimeoutSeconds
        {
            get { return _TrackingTimeoutSeconds; }
            set {
                _TrackingTimeoutSeconds = value;
                _TrackedAircraft.ExpireMilliseconds = TrackingTimeoutSeconds * 1000;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int AcceptIcaoInNonPICount { get; set; }

        private int _AcceptIcaoInNonPiMilliseconds;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int AcceptIcaoInNonPIMilliseconds
        {
            get { return _AcceptIcaoInNonPiMilliseconds; }
            set {
                _AcceptIcaoInNonPiMilliseconds = value;
                _AcceptIcaoNonParity.ExpireMilliseconds = value + 1000;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int AcceptIcaoInPI0Count { get; set; }

        private int _AcceptIcaoInPI0Milliseconds;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int AcceptIcaoInPI0Milliseconds
        {
            get { return _AcceptIcaoInPI0Milliseconds; }
            set {
                _AcceptIcaoInPI0Milliseconds = value;
                _AcceptIcaoParity.ExpireMilliseconds = value + 1000;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IgnoreInvalidCodeBlockInParityMessages { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IgnoreInvalidCodeBlockInOtherMessages { get; set; }
        #endregion

        #region Events exposed
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<string>> PositionReset;

        /// <summary>
        /// Raises <see cref="PositionReset"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnPositionReset(EventArgs<string> args)
        {
            if(PositionReset != null) PositionReset(this, args);
        }
        #endregion

        #region Constructor and Finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public RawMessageTranslator()
        {
            _StandingDataManager = Factory.Singleton.Resolve<IStandingDataManager>().Singleton;
            _Clock = Factory.Singleton.Resolve<IClock>();

            GlobalDecodeAirborneThresholdMilliseconds = 10000;
            GlobalDecodeFastSurfaceThresholdMilliseconds = 25000;
            GlobalDecodeSlowSurfaceThresholdMilliseconds = 50000;
            LocalDecodeMaxSpeedAirborne = 15.0;
            LocalDecodeMaxSpeedTransition = 5.0;
            LocalDecodeMaxSpeedSurface = 3.0;
            ReceiverRangeKilometres = 650;
            TrackingTimeoutSeconds = 600;
            AcceptIcaoInPI0Count = 1;
            AcceptIcaoInPI0Milliseconds = 1000;
            IgnoreInvalidCodeBlockInOtherMessages = true;

            _CompactPositionReporting = Factory.Singleton.Resolve<ICompactPositionReporting>();
            _TrackedAircraft.CountChangedDelegate = TrackedAircraft_CountChanged;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~RawMessageTranslator()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if(disposing) {
                _Disposed = true;
                _TrackedAircraft.Dispose();
                _AcceptIcaoParity.Dispose();
                _AcceptIcaoNonParity.Dispose();
            }
        }
        #endregion

        #region Translate
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="messageReceivedUtc"></param>
        /// <param name="modeSMessage"></param>
        /// <param name="adsbMessage"></param>
        /// <returns></returns>
        public BaseStationMessage Translate(DateTime messageReceivedUtc, ModeSMessage modeSMessage, AdsbMessage adsbMessage)
        {
            if(Statistics == null) throw new InvalidOperationException("The Statistics must be provided before calling Translate");
            BaseStationMessage result = null;

            if(modeSMessage != null && !_Disposed) {
                var isValidMessage = DetermineWhetherValid(modeSMessage, messageReceivedUtc);

                if(isValidMessage) {
                    var trackedAircraft = _TrackedAircraft.GetForKeyAndRefresh(modeSMessage.Icao24);
                    if(trackedAircraft != null) {
                        trackedAircraft.LatestMessageUtc = messageReceivedUtc;
                    } else {
                        trackedAircraft = new TrackedAircraft() {
                            Icao24 = modeSMessage.FormattedIcao24,
                            LatestMessageUtc = messageReceivedUtc,
                         };
                        _TrackedAircraft.Add(modeSMessage.Icao24, trackedAircraft);
                    }

                    result = CreateBaseStationMessage(messageReceivedUtc, modeSMessage, adsbMessage, trackedAircraft);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if the message's ICAO code looks valid.
        /// </summary>
        /// <param name="modeSMessage"></param>
        /// <param name="messageReceivedUtc"></param>
        /// <returns></returns>
        private bool DetermineWhetherValid(ModeSMessage modeSMessage, DateTime messageReceivedUtc)
        {
            bool isValidMessage = false;

            switch(modeSMessage.DownlinkFormat) {
                case DownlinkFormat.AllCallReply:
                case DownlinkFormat.ExtendedSquitter:
                    isValidMessage = IsValidParityInterrogatorIdentifierMessage(modeSMessage, messageReceivedUtc);
                    break;
                case DownlinkFormat.ExtendedSquitterNonTransponder:
                    if(modeSMessage.ControlField != null) {
                        switch(modeSMessage.ControlField.Value) {
                            case ControlField.AdsbDeviceTransmittingIcao24:
                            case ControlField.AdsbRebroadcastOfExtendedSquitter:
                                isValidMessage = IsValidParityInterrogatorIdentifierMessage(modeSMessage, messageReceivedUtc);
                                break;
                            default:
                                isValidMessage = false;
                                break;
                        }
                    }
                    break;
                case DownlinkFormat.MilitaryExtendedSquitter:
                    if(modeSMessage.ApplicationField != null) {
                        switch(modeSMessage.ApplicationField.Value) {
                            case ApplicationField.ADSB:
                                if(!IgnoreMilitaryExtendedSquitter) isValidMessage = IsValidParityInterrogatorIdentifierMessage(modeSMessage, messageReceivedUtc);
                                break;
                            default:
                                isValidMessage = false;
                                break;
                        }
                    }
                    break;
                case DownlinkFormat.CommBAltitudeReply:
                case DownlinkFormat.CommBIdentityReply:
                case DownlinkFormat.CommD:
                case DownlinkFormat.LongAirToAirSurveillance:
                case DownlinkFormat.ShortAirToAirSurveillance:
                case DownlinkFormat.SurveillanceAltitudeReply:
                case DownlinkFormat.SurveillanceIdentityReply:
                    isValidMessage = IcaoSeenEnoughTimesToBeValid(modeSMessage, messageReceivedUtc);
                    if(!isValidMessage && Statistics != null) Statistics.Lock(r => ++r.ModeSShortFrameWithoutLongFrameMessagesReceived);
                    break;
            }

            return isValidMessage;
        }

        /// <summary>
        /// Returns true if the message - which must carry the ParityInterrogatorIdentifier field - appears to be valid.
        /// </summary>
        /// <param name="modeSMessage"></param>
        /// <param name="messageReceivedUtc"></param>
        /// <returns></returns>
        private bool IsValidParityInterrogatorIdentifierMessage(ModeSMessage modeSMessage, DateTime messageReceivedUtc)
        {
            var result = modeSMessage.ParityInterrogatorIdentifier == 0;
            if(!result && Statistics != null && modeSMessage.DownlinkFormat != DownlinkFormat.AllCallReply) {
                Statistics.Lock(r => ++r.AdsbRejected);
            }
            if(result) result = IcaoSeenEnoughTimesToBeValid(modeSMessage, messageReceivedUtc);

            return result;
        }

        /// <summary>
        /// Returns true if the user has enabled the confidence test and the ICAO has been seen enough times within
        /// a timespan to be considered valid.
        /// </summary>
        /// <param name="modeSMessage"></param>
        /// <param name="messageReceivedUtc"></param>
        /// <returns></returns>
        private bool IcaoSeenEnoughTimesToBeValid(ModeSMessage modeSMessage, DateTime messageReceivedUtc)
        {
            bool result = AircraftIsTracked(modeSMessage, messageReceivedUtc);
            if(!result) {
                if(modeSMessage.ParityInterrogatorIdentifier != null) {
                    result = AddIcaoToAcceptList(_AcceptIcaoParity, AcceptIcaoInPI0Count, AcceptIcaoInPI0Milliseconds, modeSMessage, messageReceivedUtc);
                } else {
                    if(AcceptIcaoInNonPICount > 0) result = AddIcaoToAcceptList(_AcceptIcaoNonParity, AcceptIcaoInNonPICount, AcceptIcaoInNonPIMilliseconds, modeSMessage, messageReceivedUtc);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if the ICAO appears in the list of ICAOs passed across enough times to be considered valid and updates the list.
        /// </summary>
        /// <param name="acceptList"></param>
        /// <param name="countThreshold"></param>
        /// <param name="millisecondsThreshold"></param>
        /// <param name="modeSMessage"></param>
        /// <param name="messageReceivedUtc"></param>
        /// <returns></returns>
        private bool AddIcaoToAcceptList(ExpiringDictionary<int, List<DateTime>> acceptList, int countThreshold, int millisecondsThreshold, ModeSMessage modeSMessage, DateTime messageReceivedUtc)
        {
            bool result = false;

            bool ignoreIcao = false;
            if(_StandingDataManager.CodeBlocksLoaded) {
                var testCodeBlockValidity = (modeSMessage.ParityInterrogatorIdentifier == null && IgnoreInvalidCodeBlockInOtherMessages) ||
                                            (modeSMessage.ParityInterrogatorIdentifier != null && IgnoreInvalidCodeBlockInParityMessages);
                if(testCodeBlockValidity) {
                    var codeBlock = _StandingDataManager.FindCodeBlock(modeSMessage.FormattedIcao24);
                    ignoreIcao = codeBlock == null || String.IsNullOrEmpty(codeBlock.Country);
                }
            }

            if(!ignoreIcao) {
                List<DateTime> messageTimes = acceptList.GetAndRefreshOrCreate(modeSMessage.Icao24, (unused) => new List<DateTime>());
                PruneAcceptMessageTimes(messageTimes, millisecondsThreshold);

                var count = messageTimes.Count;
                if(count == 0 || messageTimes[count - 1] != messageReceivedUtc) {
                    if(count + 1 < countThreshold) messageTimes.Add(messageReceivedUtc);
                    else {
                        result = true;
                        RemoveAcceptedIcaoFromAcceptList(modeSMessage.Icao24, _AcceptIcaoParity);
                        RemoveAcceptedIcaoFromAcceptList(modeSMessage.Icao24, _AcceptIcaoNonParity);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Removes the ICAO from the list of possibly acceptable ICAOs.
        /// </summary>
        /// <param name="icao24"></param>
        /// <param name="acceptList"></param>
        private void RemoveAcceptedIcaoFromAcceptList(int icao24, ExpiringDictionary<int, List<DateTime>> acceptList)
        {
            acceptList.RemoveIfExists(icao24);
        }

        /// <summary>
        /// Returns true if the Mode-S message is for an aircraft that is currently being tracked.
        /// </summary>
        /// <param name="modeSMessage"></param>
        /// <param name="messageReceivedUtc"></param>
        /// <returns></returns>
        private bool AircraftIsTracked(ModeSMessage modeSMessage, DateTime messageReceivedUtc)
        {
            var trackedAircraft = _TrackedAircraft.GetForKey(modeSMessage.Icao24);
            var result = trackedAircraft != null;
            if(result && (messageReceivedUtc - trackedAircraft.LatestMessageUtc).TotalSeconds >= TrackingTimeoutSeconds) result = false;

            return result;
        }

        /// <summary>
        /// Removes message times that fall outside the span of time that the messages must arrive within.
        /// </summary>
        /// <param name="messageTimes"></param>
        /// <param name="milliseconds"></param>
        private void PruneAcceptMessageTimes(List<DateTime> messageTimes, int milliseconds)
        {
            if(messageTimes.Count > 0) {
                var threshold = _Clock.UtcNow.AddMilliseconds(-milliseconds);
                var removeCount = -1;
                for(var i = messageTimes.Count - 1;i >= 0;--i) {
                    if(messageTimes[i] < threshold) {
                        removeCount = i + 1;
                        break;
                    }
                }
                if(removeCount >= 0) messageTimes.RemoveRange(0, removeCount);
            }
        }

        /// <summary>
        /// Creates a BaseStationMessage from the Mode-S and ADS-B message passed across.
        /// </summary>
        /// <param name="messageReceivedUtc"></param>
        /// <param name="modeSMessage"></param>
        /// <param name="adsbMessage"></param>
        /// <param name="trackedAircraft"></param>
        /// <returns></returns>
        private BaseStationMessage CreateBaseStationMessage(DateTime messageReceivedUtc, ModeSMessage modeSMessage, AdsbMessage adsbMessage, TrackedAircraft trackedAircraft)
        {
            var result = new BaseStationMessage() {
                SignalLevel = modeSMessage.SignalLevel,
                IsMlat = modeSMessage.IsMlat,
                MessageType = BaseStationMessageType.Transmission,
                TransmissionType = ConvertToTransmissionType(modeSMessage, adsbMessage),
                MessageGenerated = messageReceivedUtc,
                MessageLogged = messageReceivedUtc,
                Icao24 = modeSMessage.FormattedIcao24,
                Altitude = modeSMessage.Altitude,
                Squawk = modeSMessage.Identity,
            };

            ApplyModeSValues(modeSMessage, trackedAircraft, result);
            if(adsbMessage != null) ApplyAdsbValues(messageReceivedUtc, adsbMessage, trackedAircraft, result);

            result.GroundSpeed = Round.GroundSpeed(result.GroundSpeed);
            result.Track = Round.Track(result.Track);
            result.Latitude = Round.Coordinate(result.Latitude);
            result.Longitude = Round.Coordinate(result.Longitude);

            return result;
        }

        /// <summary>
        /// Creates a <see cref="BaseStationSupplementaryMessage"/> and returns it, or returns it if it already exists.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private BaseStationSupplementaryMessage CreateSupplementary(BaseStationMessage message)
        {
            if(message.Supplementary == null) message.Supplementary = new BaseStationSupplementaryMessage();

            return message.Supplementary;
        }

        /// <summary>
        /// Applies values from the Mode-S message to the BaseStation message being formed.
        /// </summary>
        /// <param name="modeSMessage"></param>
        /// <param name="trackedAircraft"></param>
        /// <param name="baseStationMessage"></param>
        private void ApplyModeSValues(ModeSMessage modeSMessage, TrackedAircraft trackedAircraft, BaseStationMessage baseStationMessage)
        {
            if(modeSMessage.FlightStatus != null) {
                var flightStatus = modeSMessage.FlightStatus.Value;
                baseStationMessage.SquawkHasChanged = flightStatus == FlightStatus.AirborneAlert || flightStatus == FlightStatus.OnGroundAlert || flightStatus == FlightStatus.SpiWithAlert;
                baseStationMessage.IdentActive = flightStatus == FlightStatus.SpiWithAlert || flightStatus == FlightStatus.SpiWithNoAlert;

                switch(flightStatus) {
                    case FlightStatus.Airborne:
                    case FlightStatus.AirborneAlert: baseStationMessage.OnGround = false; break;
                    case FlightStatus.OnGround:
                    case FlightStatus.OnGroundAlert: baseStationMessage.OnGround = true; break;
                }
            }

            if(modeSMessage.Identity != null) baseStationMessage.Emergency = modeSMessage.Identity == 7500 || modeSMessage.Identity == 7600 || modeSMessage.Identity == 7700;

            if(modeSMessage.Capability != null) {
                if(modeSMessage.Capability == Capability.HasCommACommBAndAirborne) baseStationMessage.OnGround = false;
                else if(modeSMessage.Capability == Capability.HasCommACommBAndOnGround) baseStationMessage.OnGround = true;
            }

            if(modeSMessage.VerticalStatus != null) baseStationMessage.OnGround = modeSMessage.VerticalStatus == VerticalStatus.OnGround;

            if(!String.IsNullOrEmpty(modeSMessage.PossibleCallsign) && !SuppressCallsignsFromBds20 && !trackedAircraft.SeenAdsbCallsign) {
                baseStationMessage.Callsign = modeSMessage.PossibleCallsign.Trim();
                CreateSupplementary(baseStationMessage).CallsignIsSuspect = true;
            }
        }

        /// <summary>
        /// Applies values from the ADS-B message to the BaseStation message being formed.
        /// </summary>
        /// <param name="messageReceivedUtc"></param>
        /// <param name="adsbMessage"></param>
        /// <param name="trackedAircraft"></param>
        /// <param name="baseStationMessage"></param>
        private void ApplyAdsbValues(DateTime messageReceivedUtc, AdsbMessage adsbMessage, TrackedAircraft trackedAircraft, BaseStationMessage baseStationMessage)
        {
            if(adsbMessage.AirbornePosition != null)            ApplyAdsbAirbornePosition(messageReceivedUtc, adsbMessage, trackedAircraft, baseStationMessage);
            if(adsbMessage.AirborneVelocity != null)            ApplyAdsbAirborneVelocity(adsbMessage, baseStationMessage);
            if(adsbMessage.AircraftOperationalStatus != null)   ApplyAdsbAircraftOperationalStatus(adsbMessage, baseStationMessage);
            if(adsbMessage.AircraftStatus != null)              ApplyAdsbAircraftStatus(adsbMessage, baseStationMessage);
            if(adsbMessage.IdentifierAndCategory != null)       ApplyAdsbIdentifierAndCategory(adsbMessage, trackedAircraft, baseStationMessage);
            if(adsbMessage.SurfacePosition != null)             ApplyAdsbSurfacePosition(messageReceivedUtc, adsbMessage, trackedAircraft, baseStationMessage);
            if(adsbMessage.TargetStateAndStatus != null)        ApplyAdsbTargetStateAndStatus(adsbMessage, baseStationMessage);
        }

        private void ApplyAdsbAirbornePosition(DateTime messageReceivedUtc, AdsbMessage adsbMessage, TrackedAircraft trackedAircraft, BaseStationMessage baseStationMessage)
        {
            if(adsbMessage.AirbornePosition.BarometricAltitude != null) baseStationMessage.Altitude = adsbMessage.AirbornePosition.BarometricAltitude;
            else if(adsbMessage.AirbornePosition.GeometricAltitude != null) {
                baseStationMessage.Altitude = adsbMessage.AirbornePosition.GeometricAltitude;
                CreateSupplementary(baseStationMessage).AltitudeIsGeometric = true;
            }

            if(adsbMessage.AirbornePosition.CompactPosition != null) {
                trackedAircraft.RecordMessage(messageReceivedUtc, adsbMessage.AirbornePosition.CompactPosition);
                DecodePosition(baseStationMessage, trackedAircraft, null);
            }

            baseStationMessage.SquawkHasChanged = adsbMessage.AirbornePosition.SurveillanceStatus == SurveillanceStatus.TemporaryAlert;
            baseStationMessage.IdentActive = adsbMessage.AirbornePosition.SurveillanceStatus == SurveillanceStatus.SpecialPositionIdentification;
        }

        private void ApplyAdsbAirborneVelocity(AdsbMessage adsbMessage, BaseStationMessage baseStationMessage)
        {
            if(adsbMessage.AirborneVelocity.VectorVelocity != null) {
                baseStationMessage.GroundSpeed = (float?)adsbMessage.AirborneVelocity.VectorVelocity.Speed;
                baseStationMessage.Track = (float?)adsbMessage.AirborneVelocity.VectorVelocity.Bearing;
            } else if(adsbMessage.AirborneVelocity.VelocityType == VelocityType.AirspeedSubsonic || adsbMessage.AirborneVelocity.VelocityType == VelocityType.AirspeedSupersonic) {
                baseStationMessage.GroundSpeed = (float?)adsbMessage.AirborneVelocity.Airspeed;
                baseStationMessage.Track = (float?)adsbMessage.AirborneVelocity.Heading;

                CreateSupplementary(baseStationMessage);
                baseStationMessage.Supplementary.SpeedType = adsbMessage.AirborneVelocity.AirspeedIsTrueAirspeed ? SpeedType.TrueAirSpeed : SpeedType.IndicatedAirSpeed;
                baseStationMessage.Supplementary.TrackIsHeading = true;
            }

            baseStationMessage.VerticalRate = adsbMessage.AirborneVelocity.VerticalRate;
            if(adsbMessage.AirborneVelocity.VerticalRateIsBarometric) {
                CreateSupplementary(baseStationMessage).VerticalRateIsGeometric = true;
            }
        }

        private void ApplyAdsbAircraftOperationalStatus(AdsbMessage adsbMessage, BaseStationMessage baseStationMessage)
        {
            var supplementary = CreateSupplementary(baseStationMessage);
            switch(adsbMessage.AircraftOperationalStatus.AdsbVersion) {
                case 0: supplementary.TransponderType = TransponderType.Adsb0; break;
                case 1: supplementary.TransponderType = TransponderType.Adsb1; break;
                case 2: supplementary.TransponderType = TransponderType.Adsb2; break;
            }
        }

        private void ApplyAdsbAircraftStatus(AdsbMessage adsbMessage, BaseStationMessage baseStationMessage)
        {
            if(adsbMessage.AircraftStatus.EmergencyStatus != null) {
                baseStationMessage.Squawk = adsbMessage.AircraftStatus.EmergencyStatus.Squawk;
                baseStationMessage.Emergency = adsbMessage.AircraftStatus.EmergencyStatus.EmergencyState != EmergencyState.None;
            }
        }

        private void ApplyAdsbIdentifierAndCategory(AdsbMessage adsbMessage, TrackedAircraft trackedAircraft, BaseStationMessage baseStationMessage)
        {
            if(adsbMessage.IdentifierAndCategory.Identification != null) {
                trackedAircraft.SeenAdsbCallsign = true;
                baseStationMessage.Callsign = adsbMessage.IdentifierAndCategory.Identification.Trim();
                CreateSupplementary(baseStationMessage).CallsignIsSuspect = false;
            }
        }

        private void ApplyAdsbSurfacePosition(DateTime messageReceivedUtc, AdsbMessage adsbMessage, TrackedAircraft trackedAircraft, BaseStationMessage baseStationMessage)
        {
            baseStationMessage.OnGround = true;
            baseStationMessage.GroundSpeed = (float?)adsbMessage.SurfacePosition.GroundSpeed;
            baseStationMessage.Track = (float?)adsbMessage.SurfacePosition.GroundTrack;
            if(adsbMessage.SurfacePosition.IsReversing) {
                CreateSupplementary(baseStationMessage).SpeedType = SpeedType.GroundSpeedReversing;
            }

            if(adsbMessage.SurfacePosition.CompactPosition != null) {
                trackedAircraft.RecordMessage(messageReceivedUtc, adsbMessage.SurfacePosition.CompactPosition);
                DecodePosition(baseStationMessage, trackedAircraft, adsbMessage.SurfacePosition.GroundSpeed);
            }
        }

        private void ApplyAdsbTargetStateAndStatus(AdsbMessage adsbMessage, BaseStationMessage baseStationMessage)
        {
            if(adsbMessage.TargetStateAndStatus.Version1 != null) {
                var tss1 = adsbMessage.TargetStateAndStatus.Version1;

                baseStationMessage.Emergency = tss1.EmergencyState != EmergencyState.None;

                var supplementary = CreateSupplementary(baseStationMessage);
                supplementary.TransponderType = TransponderType.Adsb1;
                if(tss1.TargetAltitude != null) supplementary.TargetAltitude = tss1.TargetAltitude;
                if(tss1.TargetHeading != null) supplementary.TargetHeading = tss1.TargetHeading;
            }

            if(adsbMessage.TargetStateAndStatus.Version2 != null) {
                var tss2 = adsbMessage.TargetStateAndStatus.Version2;

                var supplementary = CreateSupplementary(baseStationMessage);
                supplementary.TransponderType = TransponderType.Adsb2;
                if(tss2.SelectedAltitude != null) supplementary.TargetAltitude = tss2.SelectedAltitude;
                if(tss2.SelectedHeading != null) supplementary.TargetHeading = (float)tss2.SelectedHeading;
            }
        }

        /// <summary>
        /// Decodes the vehicle's position.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="trackedAircraft"></param>
        /// <param name="groundSpeed"></param>
        private void DecodePosition(BaseStationMessage result, TrackedAircraft trackedAircraft, double? groundSpeed)
        {
            GlobalCoordinate position = null;
            TrackedAircraftState newState = trackedAircraft.State;

            if(UseLocalDecodeForInitialPosition && trackedAircraft.EarlierCpr == null && trackedAircraft.LaterCpr != null && ReceiverLocation != null && !trackedAircraft.InitialLocalDecodingUnsafe) {
                position = _CompactPositionReporting.LocalDecode(trackedAircraft.LaterCpr.Cpr, ReceiverLocation);
                newState = TrackedAircraftState.Acquired;
            } else if(trackedAircraft.LaterCpr != null && trackedAircraft.EarlierCpr != null) {
                switch(trackedAircraft.State) {
                    case TrackedAircraftState.Acquiring:
                        position = GlobalCprDecode(trackedAircraft, groundSpeed);
                        newState = TrackedAircraftState.Acquired;
                        break;
                    case TrackedAircraftState.Acquired:
                        position = LocalCprDecode(trackedAircraft);
                        if(position != null && trackedAircraft.AcquisitionCpr != trackedAircraft.EarlierCpr) {
                            var globalPosition = GlobalCprDecode(trackedAircraft, groundSpeed);
                            if(globalPosition != null) {
                                var distance = GreatCircleMaths.Distance(position.Latitude, position.Longitude, globalPosition.Latitude, globalPosition.Longitude);
                                var isValid = true;
                                switch(trackedAircraft.LaterCpr.Cpr.NumberOfBits) {
                                    case 19:    isValid = distance <= SurfaceResolution; break;
                                    default:    isValid = distance <= AirborneResolution; break;
                                }
                                if(!isValid) position = ResetPositionTracking(trackedAircraft);
                                else         newState = TrackedAircraftState.Tracking;
                            }
                        }
                        break;
                    case TrackedAircraftState.Tracking:
                        position = LocalCprDecode(trackedAircraft);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            if(!SuppressReceiverRangeCheck && position != null && ReceiverLocation != null) {
                var distance = GreatCircleMaths.Distance(ReceiverLocation.Latitude, ReceiverLocation.Longitude, position.Latitude, position.Longitude);
                if(distance > ReceiverRangeKilometres) {
                    position = null;
                    if(Statistics != null) Statistics.Lock(r => ++r.AdsbPositionsOutsideRange);
                }
            }

            if(position != null) {
                result.Latitude = position.Latitude;
                result.Longitude = position.Longitude;
                trackedAircraft.LastPosition = position;
                trackedAircraft.LastPositionTime = trackedAircraft.LaterCpr.Time;
                trackedAircraft.LastPositionIsSurface = trackedAircraft.LaterCpr.Cpr.NumberOfBits == 19;
                if(newState == TrackedAircraftState.Acquired && trackedAircraft.State != newState) trackedAircraft.AcquisitionCpr = trackedAircraft.LaterCpr;
                trackedAircraft.State = newState;
            }
        }

        /// <summary>
        /// Resets the position tracking state of the aircraft.
        /// </summary>
        /// <param name="trackedAircraft"></param>
        /// <returns></returns>
        private GlobalCoordinate ResetPositionTracking(TrackedAircraft trackedAircraft)
        {
            trackedAircraft.LastPosition = null;
            trackedAircraft.EarlierCpr = trackedAircraft.LaterCpr = trackedAircraft.AcquisitionCpr = null;
            trackedAircraft.InitialLocalDecodingUnsafe = true;
            trackedAircraft.State = TrackedAircraftState.Acquiring;

            if(Statistics != null) Statistics.Lock(r => ++r.AdsbPositionsReset);

            OnPositionReset(new EventArgs<string>(trackedAircraft.Icao24));

            return null;
        }

        /// <summary>
        /// Performs a global decode of the tracked aircraft's position when the last two messages are odd and even.
        /// </summary>
        /// <param name="trackedAircraft"></param>
        /// <param name="groundSpeed"></param>
        /// <returns></returns>
        private GlobalCoordinate GlobalCprDecode(TrackedAircraft trackedAircraft, double? groundSpeed)
        {
            GlobalCoordinate result = null;

            var threshold = trackedAircraft.LaterCpr.Cpr.NumberOfBits == 19 ? groundSpeed == null || groundSpeed > 25.0 ? GlobalDecodeFastSurfaceThresholdMilliseconds
                                                                                                                        : GlobalDecodeSlowSurfaceThresholdMilliseconds
                                                                            : GlobalDecodeAirborneThresholdMilliseconds;
            if((trackedAircraft.LaterCpr.Time - trackedAircraft.EarlierCpr.Time).TotalMilliseconds <= threshold) {
                result = _CompactPositionReporting.GlobalDecode(trackedAircraft.EarlierCpr.Cpr, trackedAircraft.LaterCpr.Cpr, ReceiverLocation);
            }

            return result;
        }

        /// <summary>
        /// Performs a local decode of the tracked aircraft's position against the previous position of the aircraft.
        /// </summary>
        /// <param name="trackedAircraft"></param>
        /// <returns></returns>
        private GlobalCoordinate LocalCprDecode(TrackedAircraft trackedAircraft)
        {
            var cpr = trackedAircraft.LaterCpr.Cpr;

            var result = _CompactPositionReporting.LocalDecode(cpr, trackedAircraft.LastPosition);
            if(result != null) {
                var distance = GreatCircleMaths.Distance(result.Latitude, result.Longitude, trackedAircraft.LastPosition.Latitude, trackedAircraft.LastPosition.Longitude);
                var countPeriods = (int)Math.Max(1, Math.Ceiling((trackedAircraft.LaterCpr.Time - trackedAircraft.LastPositionTime).TotalSeconds / LocalDecodeMaxSpeedSeconds));
                bool earlierIsSurface = trackedAircraft.LastPositionIsSurface;
                bool laterIsSurface = cpr.NumberOfBits == 19;
                double allowableDistance = 0.0;
                double distancePerPeriod = laterIsSurface ? LocalDecodeMaxSpeedSurface : LocalDecodeMaxSpeedAirborne;
                if(earlierIsSurface != laterIsSurface) {
                    allowableDistance = LocalDecodeMaxSpeedTransition;
                    distancePerPeriod = LocalDecodeMaxSpeedAirborne;
                    --countPeriods;
                }
                allowableDistance += countPeriods * distancePerPeriod;

                if(distance >= allowableDistance) {
                    result = null;
                    if(Statistics != null) Statistics.Lock(r => ++r.AdsbPositionsExceededSpeedCheck);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the transmission type corresponding to raw messages passed across.
        /// </summary>
        /// <param name="modeSMessage"></param>
        /// <param name="adsbMessage"></param>
        /// <returns></returns>
        private BaseStationTransmissionType ConvertToTransmissionType(ModeSMessage modeSMessage, AdsbMessage adsbMessage)
        {
            BaseStationTransmissionType result = BaseStationTransmissionType.None;

            if(adsbMessage == null) {
                switch(modeSMessage.DownlinkFormat) {
                    case DownlinkFormat.CommBAltitudeReply:
                    case DownlinkFormat.SurveillanceAltitudeReply:          result = BaseStationTransmissionType.SurveillanceAlt; break;
                    case DownlinkFormat.CommBIdentityReply:
                    case DownlinkFormat.SurveillanceIdentityReply:          result = BaseStationTransmissionType.SurveillanceId; break;
                    case DownlinkFormat.CommD:
                    case DownlinkFormat.ExtendedSquitter:
                    case DownlinkFormat.ExtendedSquitterNonTransponder:
                    case DownlinkFormat.MilitaryExtendedSquitter:
                    case DownlinkFormat.AllCallReply:                       result = BaseStationTransmissionType.AllCallReply; break;
                    case DownlinkFormat.LongAirToAirSurveillance:
                    case DownlinkFormat.ShortAirToAirSurveillance:          result = BaseStationTransmissionType.AirToAir; break;
                    default:                                                throw new NotImplementedException();
                }
            } else {
                switch(adsbMessage.MessageFormat) {
                    case MessageFormat.AirbornePosition:                    result = BaseStationTransmissionType.AirbornePosition; break;
                    case MessageFormat.AirborneVelocity:                    result = BaseStationTransmissionType.AirborneVelocity; break;
                    case MessageFormat.IdentificationAndCategory:           result = BaseStationTransmissionType.IdentificationAndCategory; break;
                    case MessageFormat.SurfacePosition:                     result = BaseStationTransmissionType.SurfacePosition; break;
                    case MessageFormat.AircraftOperationalStatus:
                    case MessageFormat.AircraftStatus:
                    case MessageFormat.None:
                    case MessageFormat.NoPositionInformation:
                    case MessageFormat.TargetStateAndStatus:                result = BaseStationTransmissionType.AllCallReply; break;
                }
            }

            return result;
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called whenever the <see cref="_TrackedAircraft"/> count changes.
        /// </summary>
        /// <param name="newCount"></param>
        private void TrackedAircraft_CountChanged(int newCount)
        {
            if(Statistics != null) Statistics.Lock(r => r.AdsbAircraftTracked = newCount);
        }
        #endregion
    }
}
