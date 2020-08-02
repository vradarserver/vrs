using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.XPlane;

namespace VirtualRadar.Interface.View
{
    public interface IXPlaneView : IView
    {
        /// <summary>
        /// Gets or sets the host to connect to.
        /// </summary>
        string Host { get; set; }

        /// <summary>
        /// Gets or sets the port to connect to.
        /// </summary>
        int XPlanePort { get; set; }

        /// <summary>
        /// Gets or sets the port to listen on.
        /// </summary>
        int ReplyPort { get; set; }

        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        string ConnectionStatus { get; set; }
    }
}
