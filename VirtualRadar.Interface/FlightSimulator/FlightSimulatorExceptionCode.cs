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

namespace VirtualRadar.Interface.FlightSimulator
{
    /// <summary>
    /// An enumeration of the reasons given by Flight Simulator X for throwing an exception.
    /// </summary>
    #pragma warning disable 1591 // no XML comments - these are straight from SimConnect, people should refer to the documentation for that.
    public enum FlightSimulatorExceptionCode : uint
    {
        // These are all straight copies of the error codes in the SimConnect documentation.
        None = 0,
        Error = 1,
        SizeMismatch = 2,
        UnrecognizedId = 3,
        Unopened = 4,
        VersionMismatch = 5,
        TooManyGroups = 6,
        NameUnrecognized = 7,
        TooManyEventNames = 8,
        EventIdDuplicate = 9,
        TooManyMaps = 10,
        TooManyObjects = 11,
        TooManyRequests = 12,
        WeatherInvalidPort = 13,
        WeatherInvalidMetar = 14,
        WeatherUnableToGetObservation = 15,
        WeatherUnableToCreateStation = 16,
        WeatherUnableToRemoveStation = 17,
        Invalid_dataType = 18,
        Invalid_dataSize = 19,
        DataError = 20,
        InvalidArray = 21,
        CreateObjectFailed = 22,
        LoadFlightplanFailed = 23,
        OperationInvalidForOjbectType = 24,
        IllegalOperation = 25,
        AlreadySubscribed = 26,
        InvalidEnum = 27,
        DefinitionError = 28,
        DuplicateId = 29,
        DatumId = 30,
        OutOfBounds = 31,
        AlreadyCreated = 32,
        ObjectOutsideReality_bubble = 33,
        ObjectContainer = 34,
        ObjectAi = 35,
        ObjectAtc = 36,
        ObjectSchedule = 37,
        Unknown = 0xffffffff,
      }
      #pragma warning restore 1591
}
