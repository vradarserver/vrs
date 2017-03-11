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
        /// A <see cref="PipelineContext"/> that can be shared between all middleware.
        /// </summary>
        public static readonly string PipelineContext = "vrs.PipelineContext";

        /// <summary>
        /// The request IP address parsed into a System.Net IPAddress.
        /// </summary>
        public static readonly string RequestRemoteIpAddressParsed = "vrs.RequestRemoteIpAddressParsed";
    }
}
