// Copyright © 2018 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// Describes the settings for a map provider's tile server. Not all tile servers support all settings.
    /// </summary>
    [DataContract]
    public class TileServerSettings
    {
        /// <summary>
        /// A single expando option.
        /// </summary>
        [DataContract]
        public class ExpandoOption
        {
            /// <summary>
            /// Gets or sets the name of the option.
            /// </summary>
            [DataMember(IsRequired = true)]
            public string Option { get; set; }

            /// <summary>
            /// Gets or sets the option's value.
            /// </summary>
            [DataMember(IsRequired = true)]
            public string Value { get; set; }
        }

        /// <summary>
        /// Gets or sets the map provider that can use this tile server.
        /// </summary>
        [DataMember(IsRequired = true)]
        public MapProvider MapProvider { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the tile server is the default for the <see cref="MapProvider"/>.
        /// </summary>
        [DataMember]
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the order in which the entries should be displayed. Custom settings are always displayed after mothership ones.
        /// </summary>
        [DataMember]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the tile server settings are custom.
        /// </summary>
        [DataMember]
        public bool IsCustom { get; set; }

        /// <summary>
        /// Gets or sets the name of the tile server in the user interface. This must be unique for the <see cref="MapProvider"/>.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the tile server's URL.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the subdomains to substitutes into the URL, if any.
        /// </summary>
        [DataMember]
        public string Subdomains { get; set; }

        /// <summary>
        /// Gets or sets the API's version number.
        /// </summary>
        [DataMember]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the minimum allowable zoom.
        /// </summary>
        [DataMember]
        public int? MinZoom { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowable zoom.
        /// </summary>
        [DataMember]
        public int? MaxZoom { get; set; }

        /// <summary>
        /// Gets or sets a value to offset zoom numbers in tile URLs.
        /// </summary>
        [DataMember]
        public int? ZoomOffset { get; set; }

        /// <summary>
        /// Gets or sets the minimum zoom number the tile server supports.
        /// </summary>
        [DataMember]
        public int? MinNativeZoom { get; set; }

        /// <summary>
        /// Gets or sets the maximum zoom number the tile server supports.
        /// </summary>
        [DataMember]
        public int? MaxNativeZoom { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that Max and Min zoom should be reversed.
        /// </summary>
        [DataMember]
        public bool ZoomReverse { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that retina tiles should be requested.
        /// </summary>
        [DataMember]
        public bool DetectRetina { get; set; }

        /// <summary>
        /// Gets or sets the name of the CSS class to assign to the tiles.
        /// </summary>
        [DataMember]
        public string ClassName { get; set; }

        /// <summary>
        /// Gets or sets the attribution notice to show for the map provider.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Any HTML embedded in this will be escaped before use. You can use the following tags to indicate
        /// text that should be replaced with valid HTML when used:
        /// </para><para>
        /// [c] is replaced with &copy;
        /// </para><para>
        /// [a href=-href-] is replaced with &lt;a href=&quot;-href-&quot;&gt;. The end of the href is denoted by a space.
        /// </para><para>
        /// [/a] is replaced with &lt;a&gt;.
        /// </para><para>
        /// [attribution -name-] is replaced with the entire attribution string for the named tile server for
        /// the same map provider.
        /// </para></remarks>
        [DataMember(IsRequired = true)]
        public string Attribution { get; set; }

        /// <summary>
        /// Gets or sets extra options.
        /// </summary>
        [DataMember]
        public List<ExpandoOption> ExpandoOptions { get; set; } = new List<ExpandoOption>();
    }
}
