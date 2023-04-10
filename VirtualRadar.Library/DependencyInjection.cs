// Copyright © 2022 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.Extensions.DependencyInjection;

namespace VirtualRadar.Library
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddVirtualRadarLibraryGroup(this IServiceCollection services)
        {
#pragma warning disable CS0618 // Type or member is obsolete... we mark services that have been made public for unit tests as obsolete

            services.AddSingleton<VirtualRadar.Interface.IApplicationLifetime,                          ApplicationLifetime>();
            services.AddSingleton<VirtualRadar.Interface.IClock,                                        Clock>();
            services.AddSingleton<VirtualRadar.Interface.IFileSystem,                                   FileSystem>();
            services.AddSingleton<VirtualRadar.Interface.IHeartbeatService,                             HeartbeatService>();
            services.AddSingleton<VirtualRadar.Interface.IHttpClientService,                            HttpClientService>();
            services.AddSingleton<VirtualRadar.Interface.ILog,                                          Log>();
            services.AddSingleton<VirtualRadar.Interface.IThreadingEnvironmentProvider,                 ThreadingEnvironmentProvider>();
            services.AddSingleton<VirtualRadar.Interface.IWebAddressManager,                            WebAddressManager>();
            services.AddSingleton<VirtualRadar.Interface.Adsb.ICompactPositionReportingEncoderDecoder,  Adsb.CompactPositionReportingEncoderDecoder>();
            services.AddSingleton<VirtualRadar.Interface.Options.ICoreSettingsStorage,                  Options.CoreSettingsStorage>();

            services.AddScoped<VirtualRadar.Interface.Settings.IConfigurationStorage,                   Settings.ConfigurationStorage>();

            // The feed manager creates a separate scope for each receiver that is instantiated
            services.AddScoped<VirtualRadar.Interface.Adsb.IAdsbTranslator,                             Adsb.AdsbTranslator>();
            services.AddScoped<VirtualRadar.Interface.ModeS.IModeSParity,                               ModeS.ModeSParity>();
            services.AddScoped<VirtualRadar.Interface.ModeS.IModeSTranslator,                           ModeS.ModeSTranslator>();

            services.AddTransient<VirtualRadar.Interface.IXmlSerialiser,                                XmlSerialiser>();

            return services;

#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
