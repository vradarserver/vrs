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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.WebServer.HttpListener
{
    /// <summary>
    /// The default implementation of <see cref="IWebServerProvider"/> for <see cref="OwinWebServer"/>.
    /// </summary>
    sealed class OwinWebServerProvider : IWebServerProvider
    {
        public DateTime UtcNow
        {
            get { return DateTime.UtcNow; }
        }

        public string ListenerPrefix { get; set; }

        public AuthenticationSchemes AuthenticationSchemes { get; set; }

        public AuthenticationSchemes AdministratorAuthenticationScheme { get; set; }

        public string ListenerRealm { get; private set; }

        public bool IsListening { get; private set; }

        public bool EnableCompression { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if(disposing) {
                ;
            }
        }

        public void StartListener()
        {
        }

        public void StopListener()
        {
        }

        public IAsyncResult BeginGetContext(AsyncCallback callback)
        {
            return null;
        }

        public IContext EndGetContext(IAsyncResult asyncResult)
        {
            return null;
        }

        private IPAddress[] _LocalIpAddresses;
        public IPAddress[] GetHostAddresses()
        {
            if(_LocalIpAddresses == null) {
                if(!Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton.IsMono) {
                    _LocalIpAddresses = Dns.GetHostAddresses("");
                } else {
                    // Ugh...
                    try {
                        _LocalIpAddresses = Dns.GetHostAddresses("");
                    } catch {
                        try {
                            // Copied from here:
                            // http://forums.xamarin.com/discussion/348/acquire-device-ip-addresses-monotouch-since-ios6-0
                            // Except this doesn't work on Mono under OSX because of a bug in GetAllNetworkinterfaces...
                            var addresses = new List<IPAddress>();
                            foreach (var netInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()) {
                                if (netInterface.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211 ||
                                    netInterface.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet) {
                                    foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses) {
                                        if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork) {
                                            var ipAddress = addrInfo.Address;
                                            addresses.Add(ipAddress);
                                        }
                                    }
                                }  
                            }
                            _LocalIpAddresses = addresses.ToArray();
                        } catch {
                            _LocalIpAddresses = new IPAddress[0];
                        }
                    }
                }
            }
            return _LocalIpAddresses;
        }
    }
}
