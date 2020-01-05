using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// The interface for a web API message handler that uses basic authorization to
    /// add a principal to the web API request.
    /// </summary>
    /// <remarks>
    /// This must be cast to a System.Net.Http.DelegatingHandler before use.
    /// </remarks>
    [Obsolete("Not required, principal should be getting set up by OWIN pipeline not a message handler")]
    public interface IBasicAuthenticationWebApiMessageHandler
    {
    }
}
