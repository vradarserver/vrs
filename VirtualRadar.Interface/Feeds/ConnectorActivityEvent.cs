// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.Feeds
{
    /// <summary>
    /// Describes a single event that occurred for a connector.
    /// </summary>
    public class ConnectorActivityEvent
    {
        /// <summary>
        /// The next identifier to be assigned to a new event.
        /// </summary>
        private static long _NextId;

        /// <summary>
        /// Gets a unique identifier assigned to each event object created.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// Gets the name of the connector.
        /// </summary>
        public string ConnectorName { get; private set; }

        /// <summary>
        /// Gets the time (at UTC) that the activity took place.
        /// </summary>
        public DateTime Time { get; private set; }

        /// <summary>
        /// Gets the type of activity.
        /// </summary>
        public ConnectorActivityType Type { get; private set; }

        /// <summary>
        /// Gets a message describing the activity. The message is always in English, I need
        /// to be able to read them when supporting VRS.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the recorded exception.
        /// </summary>
        public TimestampedException Exception { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="connectorName"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public ConnectorActivityEvent(string connectorName, ConnectorActivityType type, string message) : this(connectorName, type, message, null)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="connectorName"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public ConnectorActivityEvent(string connectorName, string message, TimestampedException exception) : this(connectorName, ConnectorActivityType.Exception, message, exception)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="connectorName"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        protected ConnectorActivityEvent(string connectorName, ConnectorActivityType type, string message, TimestampedException exception)
        {
            Id = Interlocked.Increment(ref _NextId);
            ConnectorName = connectorName;
            Time = DateTime.UtcNow;
            Type = type;
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// Moves the activity's time forward one millisecond so that it sorts into the true order
        /// when two activities with the exact same time are recorded.
        /// </summary>
        public void ShiftTimeForwardOneMillisecond()
        {
            Time = Time.AddMilliseconds(1);
        }
    }
}
