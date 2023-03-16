﻿// Copyright © 2014 onwards, Andrew Whewell
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
    /// The interface for a singleton object that records activity across every connector
    /// created by the application.
    /// </summary>
    public interface IConnectorActivityLog
    {
        /// <summary>
        /// Raised when any connector records some activity.
        /// </summary>
        event EventHandler<EventArgs<ConnectorActivityEvent>> ActivityRecorded;

        /// <summary>
        /// Called by every connector to register itself with the logger.
        /// </summary>
        /// <param name="connector"></param>
        void RecordConnectorCreated(IConnector connector);

        /// <summary>
        /// Called by every connector to deregister itself from the logger.
        /// </summary>
        /// <param name="connector"></param>
        void RecordConnectorDestroyed(IConnector connector);

        /// <summary>
        /// Returns the last so-many activities recorded by the logger. The exact number is
        /// undefined, but it always contains the latest set of activities.
        /// </summary>
        /// <returns></returns>
        ConnectorActivityEvent[] GetActivityHistory();

        /// <summary>
        /// Returns an array of every connector currently being tracked by the logger.
        /// </summary>
        /// <returns></returns>
        IConnector[] GetActiveConnectors();
    }
}
