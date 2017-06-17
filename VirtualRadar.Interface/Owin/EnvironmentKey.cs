using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// A static collection of OWIN environment keys that <see cref="PipelineContext"/>, <see cref="PipelineRequest"/>
    /// and <see cref="PipelineResponse"/> create and/or read.
    /// </summary>
    public static class EnvironmentKey
    {
        /// <summary>
        /// The IP address of the machine that originally requested the page from the server.
        /// </summary>
        public static readonly string ClientIpAddress = "vrs.ClientIpAddress";

        /// <summary>
        /// The values used to determine the Client IP Address and Proxy IP Address.
        /// </summary>
        public static readonly string ClientIpAddressBasis = "vrs.ClientIpAddressBasis";

        /// <summary>
        /// The request IP address that has been parsed into a System.Net IPAddress.
        /// </summary>
        public static readonly string ClientIpAddressParsed = "vrs.RemoteIpAddressParsed";

        /// <summary>
        /// The request IP address and port joined together into an IPEndPoint.
        /// </summary>
        public static readonly string ClientIpEndPoint = "vrs.RemoteIpEndPoint";

        /// <summary>
        /// The is local or LAN bool inferred from the request address.
        /// </summary>
        public static readonly string IsLocalOrLan = "vrs.IsLocalOrLan";

        /// <summary>
        /// The user agent string on the request appears to be from a mobile device.
        /// </summary>
        public static readonly string IsMobileUserAgentString = "vrs.IsMobileUserAgentString";

        /// <summary>
        /// The user agent string on the request appears to be from a tablet device.
        /// </summary>
        public static readonly string IsTabletUserAgentString = "vrs.IsTabletUserAgentString";

        /// <summary>
        /// A <see cref="PipelineContext"/> that can be shared between all middleware.
        /// </summary>
        public static readonly string PipelineContext = "vrs.PipelineContext";

        /// <summary>
        /// The IP address of the proxy that the request came through, if any.
        /// </summary>
        public static readonly string ProxyIpAddress = "vrs.ProxyIpAddress";
    }
}
