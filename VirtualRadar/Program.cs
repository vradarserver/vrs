// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.CommandLine;
using VirtualRadar.Database.SQLite;
using VirtualRadar.Interface.Options;
using VirtualRadar.Library;
using VirtualRadar.Localisation;

namespace VirtualRadar
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var rootCommand = new RootCommand("Virtual Radar Server");
            var workingFolderOption = new Option<string>(
                "--working-folder",
                getDefaultValue: () => new EnvironmentOptions().WorkingFolder,
                description: "Sets the configuration and log folder"
            );
            rootCommand.Add(workingFolderOption);

            rootCommand.SetHandler((workingFolder) => {
                RunWebSite(workingFolder);
            },
            workingFolderOption);

            var exitCode = rootCommand.Invoke(args);
            Environment.Exit(exitCode);
        }

        private static void RunWebSite(string workingFolder)
        {
            Console.WriteLine($"{Strings.VirtualRadarServer}");
            Console.WriteLine($"{Strings.ConfigurationFolder}: {workingFolder}");

            if(!Directory.Exists(workingFolder)) {
                Console.WriteLine(String.Format(Strings.CreatingDirectory, workingFolder));
                Directory.CreateDirectory(workingFolder);
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(workingFolder)
                .AddJsonFile("configuration.json", optional: true, reloadOnChange: true);

            var builder = WebApplication.CreateBuilder(Array.Empty<string>());

            builder.Services.Configure<EnvironmentOptions>(options => {
                options.WorkingFolder = workingFolder;
            });

            builder.WebHost.ConfigureKestrel(options => {
                options.ListenAnyIP(80);
            });

            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            builder.Services
                .AddVirtualRadarHostGroup()
//                .AddVirtualRadarDatabaseSQLiteGroup()
                .AddVirtualRadarLibraryGroup();

            var app = builder.Build();

            if(!app.Environment.IsDevelopment()) {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            Console.WriteLine(Strings.SplashScreenStartingWebServer);

            app.Run();
        }
    }
}
