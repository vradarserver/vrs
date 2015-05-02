// Copyright © 2014 onwards, Andrew Whewell
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
using VirtualRadar.Interface.Adsb;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for objects that test the sanity of position reports and altitudes
    /// received for aircraft.
    /// </summary>
    public interface IAircraftSanityChecker : IDisposable
    {
        /// <summary>
        /// Returns true if the string represents a valid ICAO code.
        /// </summary>
        /// <param name="icao"></param>
        /// <returns></returns>
        bool IsGoodAircraftIcao(string icao);

        /// <summary>
        /// Checks the altitude for an aircraft.
        /// </summary>
        /// <param name="aircraftId">The unique identifier of the aircraft.</param>
        /// <param name="messageReceived">The UTC date and time that the message was received.</param>
        /// <param name="altitude">The altitude in feet.</param>
        /// <returns>A value indicating whether the altitude could be correct.</returns>
        /// <remarks><para>
        /// The code needs to build up some history before it can determine whether the aircraft could
        /// attain the altitude specified without breaking the laws of physics. The first call for an
        /// aircraft will always return Uncertain. If the second altitude is compatible with the first
        /// then it will return ProbablyRight, otherwise if at least one of them is wrong then more
        /// messages may need to be received until the code can start returning Certainty values of
        /// ProbablyRight. Whenever this returns CertainlyWrong you should discard the altitude.
        /// </para><para>
        /// Once this method returns CertainlyWrong it will reset and will not return ProbablyRight
        /// until a pair of altitudes have been received that seem to be sane.
        /// </para></remarks>
        Certainty CheckAltitude(int aircraftId, DateTime messageReceived, int altitude);

        /// <summary>
        /// Returns the first altitude that appears to be in line with the current altitude recorded
        /// for the aircraft.
        /// </summary>
        /// <param name="aircraftId"></param>
        /// <returns>The first good altitude seen for the aircraft or null if no good altitude currently exists.</returns>
        /// <remarks>
        /// The sanity checker maintains a history of altitudes that have been received, up to the point
        /// where a call to <see cref="CheckAltitude"/> returns a ProbablyRight result. Once an altitude
        /// is probably right you can call this method to get the earliest altitude seen that is probably
        /// correct when compared to the latest altitude. If the latest altitude is not ProbablyRight
        /// then this method returns null. Once two ProbablyRight altitudes are seen in succession the
        /// history will be wiped and the function will return null until the aircraft is removed from
        /// the history (which happens after roughly 10 minutes regardless of configuration settings).
        /// </remarks>
        int? FirstGoodAltitude(int aircraftId);

        /// <summary>
        /// Checks the position for an ICAO.
        /// </summary>
        /// <param name="aircraftId">The unique identifier of the aircraft.</param>
        /// <param name="messageReceived">The UTC date and time that the message was received.</param>
        /// <param name="latitude">The latitude that the aircraft has reported.</param>
        /// <param name="longitude">The longitude that the aircraft has reported.</param>
        /// <returns>A value indicating whether the position could be correct.</returns>
        /// <remarks><para>
        /// The code needs to build up some history before it can determine whether the aircraft could
        /// reach the position specified without travelling faster than it can. The first call for an
        /// aircraft will always return Uncertain. If the second position is compatible with the first
        /// then it will return ProbablyRight, otherwise if at least one of them is wrong then more
        /// messages may need to be received until the code can start returning Certainty values of
        /// ProbablyRight. Whenever this returns CertainlyWrong you should discard the position.
        /// </para><para>
        /// Once this method returns CertainlyWrong it will reset and will not return ProbablyRight
        /// until a pair of positions have been received that seem to be sane.
        /// </para></remarks>
        Certainty CheckPosition(int aircraftId, DateTime messageReceived, double latitude, double longitude);

        /// <summary>
        /// Returns the first position that appears to be in line with the current position recorded
        /// for the aircraft.
        /// </summary>
        /// <param name="aircraftId"></param>
        /// <returns>The first good position seen for the aircraft or null if no good position currently exists.</returns>
        /// <remarks>
        /// The sanity checker maintains a history of positions that have been received, up to the point
        /// where a call to <see cref="CheckPosition"/> returns a ProbablyRight result. Once a position
        /// is probably right you can call this method to get the earliest position seen that is probably
        /// correct when compared to the latest position. If the latest position is not ProbablyRight
        /// then this method returns null. Once two ProbablyRight positions are seen in succession the
        /// history will be wiped and the function will return null until the aircraft is removed from
        /// the history (which happens after roughly 10 minutes regardless of configuration settings).
        /// </remarks>
        GlobalCoordinate FirstGoodPosition(int aircraftId);

        /// <summary>
        /// Resets the sanity checks for the aircraft passed across.
        /// </summary>
        /// <param name="aircraftId"></param>
        /// <remarks>
        /// This resets the state for the aircraft passed across such that the next sanity check will behave
        /// as though the checker had never seen the aircraft before.
        /// </remarks>
        void ResetAircraft(int aircraftId);
    }
}
