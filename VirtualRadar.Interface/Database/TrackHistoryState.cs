// Copyright © 2018 onwards, Andrew Whewell
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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface.Database
{
    /// <summary>
    /// Describes the state of an aircraft's tracked values at a point in time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The first state record for a track has all of the available values filled in. Each
    /// subsequent record in date/time order only fills in values that have changed. If a
    /// field has not changed since the last State record then the field should be set to
    /// NULL.
    /// </para><para>
    /// The upside of writing NULL for unchanged values is that most database engines will
    /// represent NULL values very efficiently, in some cases they are represented by a
    /// single bit in the record. The downside is that you cannot read the state backwards,
    /// you need to have processed all of the records leading up to any particular state
    /// record (except for the first) in order to know the full state of the aircraft at a
    /// point in time.
    /// </para></remarks>
    public class TrackHistoryState
    {
        /// <summary>
        /// Gets or sets the state record's unique ID.
        /// </summary>
        public long TrackHistoryStateID { get; set; }

        /// <summary>
        /// Gets or sets the parent's ID.
        /// </summary>
        public long TrackHistoryID { get; set; }

        /// <summary>
        /// Gets or sets the point in time that the state represents.
        /// </summary>
        public DateTime TimestampUtc { get; set; }

        /// <summary>
        /// Gets or sets the relative order of this state record in all state records for <see cref="TrackHistoryID"/>
        /// </summary>
        /// <remarks>
        /// In principle sorting by <see cref="TimestampUtc"/> or this field should produce the same order. However, if
        /// the clock is corrected while a sequence is being recorded then <see cref="TimestampUtc"/> can be out of order
        /// whereas this field is always in order.
        /// </remarks>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the signal level at the time the state was saved. Note that this can be suppressed in options.
        /// </summary>
        public int? SignalLevel { get; set; }

        /// <summary>
        /// Gets or sets the ID of the receiver that was last tracking the aircraft.
        /// </summary>
        public int? ReceiverID { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's callsign at this point in time.
        /// </summary>
        public string Callsign { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the callsign was considered suspect at this point in time.
        /// </summary>
        public bool? IsCallsignSuspect { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's latitude at this point in time.
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's longitude at this point in time.
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that the aircraft position was coming from an MLAT receiver.
        /// </summary>
        public bool? IsMlat { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that the aircraft position was coming from TISB messages.
        /// </summary>
        public bool? IsTisb { get; set; }

        /// <summary>
        /// Gets or sets the altitude in feet.
        /// </summary>
        public int? AltitudeFeet { get; set; }

        /// <summary>
        /// Gets or sets the type of altitude that <see cref="AltitudeFeet"/> represents.
        /// </summary>
        public AltitudeType? AltitudeTypeID { get; set; }

        /// <summary>
        /// Gets or sets the target altitude on the autopilot.
        /// </summary>
        public int? TargetAltitudeFeet { get; set; }

        /// <summary>
        /// Gets or sets the air pressure in inches of mercury at this point in time.
        /// </summary>
        public float? AirPressureInHg { get; set; }

        /// <summary>
        /// Gets or sets the ground speed in knots.
        /// </summary>
        public float? GroundSpeedKnots { get; set; }

        /// <summary>
        /// Gets or sets the type of ground speed that <see cref="GroundSpeedKnots"/> represents.
        /// </summary>
        public SpeedType? SpeedTypeID { get; set; }

        /// <summary>
        /// Gets or sets the track in degrees.
        /// </summary>
        public float? TrackDegrees { get; set; }

        /// <summary>
        /// Gets or sets the target track on the autopilot.
        /// </summary>
        public float? TargetTrack { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="TrackDegrees"/> is the ground track or the aircraft's heading.
        /// </summary>
        public bool? TrackIsHeading { get; set; }

        /// <summary>
        /// Gets or sets the vertical speed in feet per minute.
        /// </summary>
        public int? VerticalRateFeetMin { get; set; }

        /// <summary>
        /// Gets or sets whether the VSI is reported by an instrument that uses barometric pressure or measures the distance above the elipsoid.
        /// </summary>
        public AltitudeType? VerticalRateTypeID { get; set; }

        /// <summary>
        /// Gets or sets the squawk as an octal code (e.g. squawk 1234 is recorded as integer 1234 not 668).
        /// </summary>
        public int? SquawkOctal { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that ident was active when the state was recorded.
        /// </summary>
        public bool? IdentActive { get; set; }

        /// <summary>
        /// Applies each state in order to a start state(an empty state if <paramref name="intoState"/> is null) and returns the end state.
        /// </summary>
        /// <param name="states"></param>
        /// <param name="intoState"></param>
        /// <returns></returns>
        public static TrackHistoryState MergeStates(IEnumerable<TrackHistoryState> states, TrackHistoryState intoState = null)
        {
            var result = intoState ?? new TrackHistoryState();

            foreach(var state in states) {
                if(state.TrackHistoryID != 0) {
                    result.TrackHistoryID = state.TrackHistoryID;
                }
                if(state.SequenceNumber != 0) {
                    result.SequenceNumber = state.SequenceNumber;
                }
                if(state.TimestampUtc != default(DateTime)) {
                    result.TimestampUtc = state.TimestampUtc;
                }

                if(state.AirPressureInHg != null)       result.AirPressureInHg =        state.AirPressureInHg;
                if(state.AltitudeFeet != null)          result.AltitudeFeet =           state.AltitudeFeet;
                if(state.AltitudeTypeID != null)          result.AltitudeTypeID =           state.AltitudeTypeID;
                if(state.Callsign != null)              result.Callsign =               state.Callsign;
                if(state.GroundSpeedKnots != null)      result.GroundSpeedKnots =       state.GroundSpeedKnots;
                if(state.IdentActive != null)           result.IdentActive =            state.IdentActive;
                if(state.IsCallsignSuspect != null)     result.IsCallsignSuspect =      state.IsCallsignSuspect;
                if(state.IsMlat != null)                result.IsMlat =                 state.IsMlat;
                if(state.IsTisb != null)                result.IsTisb =                 state.IsTisb;
                if(state.Latitude != null)              result.Latitude =               state.Latitude;
                if(state.Longitude != null)             result.Longitude =              state.Longitude;
                if(state.ReceiverID != null)            result.ReceiverID =             state.ReceiverID;
                if(state.SignalLevel != null)           result.SignalLevel =            state.SignalLevel;
                if(state.SpeedTypeID != null)             result.SpeedTypeID =              state.SpeedTypeID;
                if(state.SquawkOctal != null)           result.SquawkOctal =            state.SquawkOctal;
                if(state.TargetAltitudeFeet != null)    result.TargetAltitudeFeet =     state.TargetAltitudeFeet;
                if(state.TargetTrack != null)           result.TargetTrack =            state.TargetTrack;
                if(state.TrackDegrees != null)          result.TrackDegrees =           state.TrackDegrees;
                if(state.TrackIsHeading != null)        result.TrackIsHeading =         state.TrackIsHeading;
                if(state.VerticalRateFeetMin != null)   result.VerticalRateFeetMin =    state.VerticalRateFeetMin;
                if(state.VerticalRateTypeID != null)      result.VerticalRateTypeID =       state.VerticalRateTypeID;
            }

            return result;
        }
    }
}
