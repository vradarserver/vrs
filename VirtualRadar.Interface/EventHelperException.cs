using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Thrown when an event handler throws an exception in <see cref="EventHelper"/>.
    /// </summary>
    [Serializable]
    public class EventHelperException : Exception
    {
        /// <summary>
        /// Gets a collection of exceptions thrown by handlers.
        /// </summary>
        public Exception[] HandlerExceptions { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public EventHelperException()
        {
            Initialise();
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="message"></param>
        public EventHelperException(string message) : base(message)
        {
            Initialise();
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public EventHelperException(string message, Exception inner) : base(message, inner)
        {
            Initialise();
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected EventHelperException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Initialise();
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="handlerExceptions"></param>
        public EventHelperException(string message, IEnumerable<Exception> handlerExceptions) : base(message)
        {
            Initialise(handlerExceptions);
        }

        /// <summary>
        /// Initialises the exception's properties.
        /// </summary>
        /// <param name="exceptions"></param>
        private void Initialise(IEnumerable<Exception> exceptions = null)
        {
            HandlerExceptions = exceptions == null ? new Exception[0] : exceptions.ToArray();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = new StringBuilder(base.ToString());
            for(var i = 0;i < HandlerExceptions.Length;++i) {
                result.AppendLine(String.Format("--- Exception {0} ----------------------", i + 1));
                result.AppendLine(HandlerExceptions[i].ToString());
            }

            return result.ToString();
        }
    }
}
