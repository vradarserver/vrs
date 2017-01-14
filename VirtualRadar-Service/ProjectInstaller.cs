// Copyright © 2017 onwards, Andrew Whewell
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
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar
{
    /// <summary>
    /// The installer class that ManagedInstallerClass will call.
    /// </summary>
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        /// <summary>
        /// The name of the Virtual Radar Server service.
        /// </summary>
        internal static readonly string ServiceName = "VirtualRadarServerService";

        /// <summary>
        /// Gets or sets the options that control how the service is installed.
        /// </summary>
        internal static Options Options { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ProjectInstaller()
        {
            var serviceProcessInstaller = new ServiceProcessInstaller() {
                Username = Options.UserName,
                Password = Options.Password,
            };

            var serviceInstaller = new ServiceInstaller() {
                Description =       "A web server that shows the positions of aircraft on a map",
                DisplayName =       "Virtual Radar Server",
                ServiceName =       ServiceName,
                StartType =         Options.StartupType == StartupType.Manual ? ServiceStartMode.Manual : ServiceStartMode.Automatic,
                DelayedAutoStart =  Options.StartupType == StartupType.DelayedAutomatic,
            };

            Installers.AddRange(new Installer[] {
                serviceProcessInstaller,
                serviceInstaller,
            });
        }
    }
}
