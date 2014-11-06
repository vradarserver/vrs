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
using System.Runtime.CompilerServices;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The namespace for all of the interfaces and objects that deal with reading and writing configuration information.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The application's configuration information is stored in <see cref="Configuration"/>, which is read and written by
    /// the singleton object <see cref="IConfigurationStorage"/>. This object raises an event when a new configuration is
    /// saved which the application can hook to automatically pick up changes to the configuration. To avoid repeatedly
    /// loading the configuration you can also use <see cref="ISharedConfiguration"/>, which caches the most recent
    /// configuration.
    /// </para><para>
    /// The program's installer writes a configuration file that contains settings that cannot be changed without stopping
    /// the program and re-running the installer. The settings are in <see cref="InstallerSettings"/> and are loaded by
    /// <see cref="IInstallerSettingsStorage"/>. Unlike <see cref="IConfigurationStorage"/> this interface is not a singleton,
    /// with it being read-only there is no need to notify the application when the values change and therefore no need to
    /// have a single object whose events can be hooked.
    /// </para><para>
    /// Finally we have <see cref="PluginSettings"/>, which is loaded and saved by <see cref="IPluginSettingsStorage"/>. This is
    /// likely to be of the most interest to plugin developers.
    /// </para><para>
    /// <img src="../ClassDiagrams/_VirtualRadarInterfaceSettings.jpg" alt="" title="VirtualRadar.Interface.Settings class diagram"/>
    /// </para></remarks>
    [CompilerGenerated]
    class NamespaceDoc
    {
    }
}
