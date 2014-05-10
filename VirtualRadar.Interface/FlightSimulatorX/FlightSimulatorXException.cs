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

namespace VirtualRadar.Interface.FlightSimulatorX
{
    /// <summary>
    /// Thrown when FlightSimulatorX reports an exception.
    /// </summary>
    [Serializable]
    public class FlightSimulatorXException : Exception
    {
        /// <summary>
        /// Gets the FSX exception identifier.
        /// </summary>
        public FlightSimulatorXExceptionCode ExceptionCode { get; private set; }

        /// <summary>
        /// Gets the FSX exception code that underlies <see cref="ExceptionCode"/>.
        /// </summary>
        public uint RawExceptionCode { get; private set; }

        /// <summary>
        /// Gets the index number (1-based) of the parameter that caused the exception. 0 indicates that the erroneous
        /// parameter number is not known / not applicable.
        /// </summary>
        public uint IndexNumber;

        /// <summary>
        /// Gets the identifier of the packet that caused the error.
        /// </summary>
        public uint SendID;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FlightSimulatorXException() { }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="message"></param>
        public FlightSimulatorXException(string message) : base(message) { }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public FlightSimulatorXException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected FlightSimulatorXException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="exceptionID"></param>
        /// <param name="indexNumber"></param>
        /// <param name="sendID"></param>
        public FlightSimulatorXException(uint exceptionID, uint indexNumber, uint sendID) : this(String.Format("FSX exception {0}({1}), parameter {2}, packet {3}", ConvertIdToExceptionCode(exceptionID), exceptionID, indexNumber, sendID))
        {
            RawExceptionCode = exceptionID;
            IndexNumber = indexNumber;
            SendID = sendID;

            ExceptionCode = ConvertIdToExceptionCode(exceptionID);
        }

        /// <summary>
        /// Returns the <see cref="FlightSimulatorXExceptionCode"/> corresponding to the FSX exception ID passed across.
        /// </summary>
        /// <param name="exceptionID"></param>
        /// <returns></returns>
        private static FlightSimulatorXExceptionCode ConvertIdToExceptionCode(uint exceptionID)
        {
            return Enum.IsDefined(typeof(FlightSimulatorXExceptionCode), exceptionID) ? (FlightSimulatorXExceptionCode)exceptionID : FlightSimulatorXExceptionCode.Unknown;
        }
    }
}
