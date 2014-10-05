using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface.Network
{
    /// <summary>
    /// Describes a single event that occurred for a connector.
    /// </summary>
    public class ConnectorActivityEvent
    {
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
