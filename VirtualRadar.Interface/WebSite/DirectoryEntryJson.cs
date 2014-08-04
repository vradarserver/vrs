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

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The JSON object that carries directory information back to the VRS mothership.
    /// </summary>
    /// <remarks>
    /// These are not sent automatically - rather the VRS site requests one when it is
    /// checking to see if the site is still alive. The user has to register the site
    /// with the directory before the mothership will start requesting directory
    /// entries.
    /// </remarks>
    [DataContract]
    public class DirectoryEntryJson
    {
        /// <summary>
        /// Gets or sets the version number of VRS that the site is running.
        /// </summary>
        [DataMember]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the number of feeds that can be viewed from the site.
        /// </summary>
        [DataMember]
        public int NumberOfFeeds { get; set; }

        /// <summary>
        /// Gets or sets the highest number of aircraft across all feeds that can be viewed
        /// from the site.
        /// </summary>
        [DataMember]
        public int NumberOfAircraft { get; set; }
    }
}
