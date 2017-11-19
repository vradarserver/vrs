using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// A static collection of Virtual Radar Server custom OWIN environment keys.
    /// </summary>
    public static class EnvironmentKey
    {
        /// <summary>
        /// The IP address of the machine that originally requested the page from the server.
        /// </summary>
        public static readonly string ClientIpAddress = "vrs.req.ClientIpAddress";

        /// <summary>
        /// The values used to determine the Client IP Address and Proxy IP Address.
        /// </summary>
        public static readonly string ClientIpAddressBasis = "vrs.req.ClientIpAddressBasis";

        /// <summary>
        /// The request IP address that has been parsed into a System.Net IPAddress.
        /// </summary>
        public static readonly string ClientIpAddressParsed = "vrs.req.RemoteIpAddressParsed";

        /// <summary>
        /// The request IP address and port joined together into an IPEndPoint.
        /// </summary>
        public static readonly string ClientIpEndPoint = "vrs.req.RemoteIpEndPoint";

        /// <summary>
        /// The is local or LAN bool inferred from the request address.
        /// </summary>
        public static readonly string IsLocalOrLan = "vrs.req.IsLocalOrLan";

        /// <summary>
        /// The user agent string on the request appears to be from a mobile device.
        /// </summary>
        public static readonly string IsMobileUserAgentString = "vrs.req.IsMobileUserAgentString";

        /// <summary>
        /// The user agent string on the request appears to be from a tablet device.
        /// </summary>
        public static readonly string IsTabletUserAgentString = "vrs.req.IsTabletUserAgentString";

        /// <summary>
        /// A <see cref="PipelineContext"/> that can be shared between all middleware.
        /// </summary>
        public static readonly string PipelineContext = "vrs.PipelineContext";

        /// <summary>
        /// The IP address of the proxy that the request came through, if any.
        /// </summary>
        public static readonly string ProxyIpAddress = "vrs.req.ProxyIpAddress";

        /// <summary>
        /// An optional bool that, if present and true, stops JavaScript from being minified in the response.
        /// </summary>
        public static readonly string SuppressJavascriptMinification = "vrs.rsp.SuppressJavascriptMinification";

        /// <summary>
        /// An optional bool that, if present and true, prevents bundled Javascript from being returned.
        /// </summary>
        public static readonly string SuppressJavascriptBundles = "vrs.rsp.SuppressJavaScriptBundles";

        /// <summary>
        /// An optional bool that, if present and true, indicates that the request is coming through the <see cref="ILoopbackHost"/>.
        /// </summary>
        public static readonly string IsLoopbackRequest = "vrs.req.IsLoopback";
    }
}
