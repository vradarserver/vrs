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

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// A class that represents the content of a plugin manifest file.
    /// </summary>
    /// <remarks><para>
    /// The plugin manifest file tells Virtual Radar Server information about your
    /// plugin without having to load the plugin DLL. It can be used to make a decision
    /// about whether the plugin can be loaded or not.
    /// </para><para>
    /// The file has to be in the same folder as the plugin DLL and have the same filename
    /// as the dll but with an extension of <b>.xml</b>
    /// </para></remarks>
    /// <example>
    /// The manifest file is XML and an example is laid out below. Please refer to the various
    /// properties of this class for an explanantion of the different elements:
    /// <code>
    /// &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
    /// &lt;PluginManifest xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot; xmlns:xsd=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
    ///     &lt;MinimumVersion&gt;1.0.0&lt;/MinimumVersion&gt;
    ///     &lt;MaximumVersion&gt;1.0.1&lt;/MaximumVersion&gt;
    /// &lt;/PluginManifest&gt;
    /// </code>
    /// </example>
    [Serializable]
    public class PluginManifest
    {
        /// <summary>
        /// Gets or sets the earliest version of Virtual Radar Server that this plugin will work with.
        /// </summary>
        /// <remarks><para>
        /// The version should be of the form &quot;1.2.3&quot; - i.e. do not specify the Virtual Radar Server revision number
        /// here.
        /// </para><para>
        /// If the currently installed version of Virtual Radar Server is lower than the version specified here then the plugin
        /// will not be loaded. An entry will be shown in the plugin's list to explain why the plugin was not loaded.
        /// </para><para>
        /// If this value is null or an empty string then the check for the minimum version is disabled.
        /// </para></remarks>
        public string MinimumVersion { get; set; }

        /// <summary>
        /// Gets or sets the latest version of Virtual Radar Server that this plugin will work with.
        /// </summary>
        /// <remarks><para>
        /// The version should be of the form &quot;1.2.3&quot; - i.e. do not specify the Virtual Radar Server revision number
        /// here. 
        /// </para><para>
        /// If the currently installed version of Virtual Radar Server is higher than the version specified here then the plugin
        /// will not be loaded. An entry will be shown in the plugin's list to explain why the plugin was not loaded.
        /// </para><para>
        /// If this value is null or an empty string then the check for the minimum version is disabled.
        /// </para><para>
        /// If your plugin provides custom implementations of interfaces then it is recommended that you set this value to the
        /// version of Virtual Radar Server that you built the plugin against. In the long run the interfaces in Virtual Radar
        /// Server should settle down and not change between versions but in these early days they are likely to change quite
        /// often. If a later version of VRS changes the interfaces that you are overriding then your plugin will generate an
        /// exception message on startup - setting this property will prevent that. It will mean, however, that you will need
        /// to release new versions of your plugin when the main Virtual Radar Server application changes.
        /// </para></remarks>
        public string MaximumVersion { get; set; }
    }
}
