// Copyright © 2010 onwards, Andrew Whewell
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
using System.Text;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Interface.Listener;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The class that passes parameters to <see cref="IPlugin.Startup"/>.
    /// </summary>
    public class PluginStartupParameters
    {
        /// <summary>
        /// Gets the object that keeps track of the simulated aircraft flying in Flight Simulator X.
        /// </summary>
        public ISimpleAircraftList FlightSimulatorAircraftList { get; private set; }

        /// <summary>
        /// Gets the object that is controlling the exposure of the web server via UPnP.
        /// </summary>
        public IUniversalPlugAndPlayManager UPnpManager { get; private set; }

        /// <summary>
        /// Gets the object that serves up the website pages via <see cref="WebServer"/>.
        /// </summary>
        public IWebSite WebSite { get; private set; }

        /// <summary>
        /// Gets the folder that the plugin was installed into.
        /// </summary>
        /// <remarks>
        /// This is the folder under [PROGRAM FILES]\VirtualRadar\Plugins that the plugin has been installed to. So for
        /// example, if VRS was installed to c:\Program Files\VirtualRadar and the plugin was installed to
        /// c:\ProgramFiles\VirtualRadar\Plugins\MyPlugin then the value of this property would be
        /// c:\ProgramFiles\VirtualRadar\Plugins\MyPlugin.
        /// </remarks>
        public string PluginFolder { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="flightSimulatorAircraftList"></param>
        /// <param name="uPnpManager"></param>
        /// <param name="webSite"></param>
        /// <param name="pluginFolder"></param>
        public PluginStartupParameters(ISimpleAircraftList flightSimulatorAircraftList, IUniversalPlugAndPlayManager uPnpManager, IWebSite webSite, string pluginFolder)
        {
            FlightSimulatorAircraftList = flightSimulatorAircraftList;
            UPnpManager = uPnpManager;
            WebSite = webSite;
            PluginFolder = pluginFolder;
        }
    }
}
