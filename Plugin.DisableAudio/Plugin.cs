// Copyright © 2012 onwards, Andrew Whewell
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
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using InterfaceFactory;
using VirtualRadar.Interface;

namespace VirtualRadar.Plugin.DisableAudio
{
    /// <summary>
    /// The entry point for the plugin that replaces the audio interface implementations with stubs.
    /// </summary>
    public class Plugin : IPlugin
    {
        public string Id { get { return "VirtualRadarServer.Plugin.DisableAudio"; } }

        public string PluginFolder { get; set; }

        public string Name { get { return "Disable Audio Plugin"; } }

        public string Version { get { return "2.2.0"; } }

        public string Status { get; private set; }

        public string StatusDescription { get; private set; }

        public bool HasOptions { get { return false; } }

        public event EventHandler StatusChanged;

        protected virtual void OnStatusChanged(EventArgs args)
        {
            EventHelper.Raise(StatusChanged, this, args);
        }

        public void RegisterImplementations(InterfaceFactory.IClassFactory classFactory)
        {
            classFactory.Register<IAudio, Audio>();
            classFactory.Register<ISpeechSynthesizerWrapper, SpeechSynthesizerWrapper>();
        }

        public void Startup(PluginStartupParameters parameters)
        {
        }

        public void GuiThreadStartup()
        {
        }

        public void Shutdown()
        {
        }

        public void ShowWinFormsOptionsUI()
        {
        }
    }
}
