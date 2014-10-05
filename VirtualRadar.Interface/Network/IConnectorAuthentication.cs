using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface.Network
{
    /// <summary>
    /// The interface for all authentication classes.
    /// </summary>
    public interface IConnectorAuthentication
    {
        /// <summary>
        /// Gets the longest response that the class can handle.
        /// </summary>
        /// <remarks>
        /// If the connecting side returns a response that is longer than this then it
        /// is deemed invalid and the connection is closed.
        /// </remarks>
        int MaximumResponseLength { get; }

        /// <summary>
        /// Returns true if the response passed across is complete. The validity doesn't matter.
        /// </summary>
        /// <param name="response"></param>
        /// <returns>True if the byte array contains the entire response, false if it's too short.</returns>
        bool GetResponseIsComplete(byte[] response);

        /// <summary>
        /// Passed a complete response (as indicated by <see cref="GetResponseLength"/>, returns
        /// true if the response is valid and false if it does not.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        bool GetResponseIsValid(byte[] response);

        /// <summary>
        /// Returns the bytes that need to be sent to the listening side to authenticate.
        /// </summary>
        byte[] SendAuthentication();
    }
}
