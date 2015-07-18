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
using System.Runtime.Serialization;
using System.Text;
using VirtualRadar.Interface.Network;

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// A DTO that carries information about a feed.
    /// </summary>
    [DataContract(Name="Feed")]
    public class FeedStatus : ICloneable
    {
        /// <summary>
        /// Gets or sets the unique ID of the feed.
        /// </summary>
        [DataMember(Name="Id")]
        public int FeedId { get; set; }

        /// <summary>
        /// Gets or sets a value that is incremented any time there is a change of value for the feed.
        /// </summary>
        public long DataVersion { get; set; }

        /// <summary>
        /// Gets or sets the name of the feed.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that this is a merged feed as opposed to a reciever.
        /// </summary>
        [DataMember(Name="Merged")]
        public bool IsMergedFeed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the feed has a polar plot attached to it.
        /// </summary>
        [DataMember(Name="Polar")]
        public bool HasPolarPlot { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that an aircraft list is tracking the aircraft on the feed.
        /// </summary>
        [DataMember]
        public bool HasAircraftList { get; set; }

        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        [DataMember(Name="Connection")]
        public ConnectionStatus ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets a description of the connection status.
        /// </summary>
        [DataMember(Name="ConnDesc")]
        public string ConnectionStatusDescription { get; set; }

        /// <summary>
        /// Gets or sets the total number of messages received.
        /// </summary>
        [DataMember(Name="Msgs")]
        public long TotalMessages { get; set; }

        /// <summary>
        /// Gets or sets the total number of bad messages received.
        /// </summary>
        [DataMember(Name="BadMsgs")]
        public long TotalBadMessages { get; set; }

        /// <summary>
        /// Gets or sets the total number of bad messages.
        /// </summary>
        [DataMember(Name="Tracked")]
        public int TotalAircraft { get; set; }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}: {1}", FeedId, Name);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var result = Activator.CreateInstance(GetType()) as FeedStatus;
            result.ConnectionStatus =               ConnectionStatus;
            result.ConnectionStatusDescription =    ConnectionStatusDescription;
            result.DataVersion =                    DataVersion;
            result.FeedId =                         FeedId;
            result.HasAircraftList =                HasAircraftList;
            result.HasPolarPlot =                   HasPolarPlot;
            result.IsMergedFeed =                   IsMergedFeed;
            result.Name =                           Name;
            result.TotalAircraft =                  TotalAircraft;
            result.TotalBadMessages =               TotalBadMessages;
            result.TotalMessages =                  TotalMessages;

            return result;
        }
    }
}
