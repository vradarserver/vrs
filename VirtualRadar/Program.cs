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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar
{
    /// <summary>
    /// Main class for the application.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ProgramLifetime.InitialiseUnhandledExceptionHandling();
            ProgramLifetime.PrepassCommandLineArgs(args);

            ApplicationInformation.SetHeadless(ProgramLifetime.Headless);
            if(!ProgramLifetime.Headless) {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
            }

            InitialiseClassFactory();

            if(ProgramLifetime.Headless) {
                VirtualRadar.Interop.Console.ShowConsole();
            }
            if(args.Contains("-showConfigFolder")) {
                ShowConfigurationFolder();
            }

            ProgramLifetime.InitialiseManagers();
            ProgramLifetime.LoadPlugins();
            ProgramLifetime.SingleInstanceStart(args);

            // Calling Environment.Exit rather than falling off the end of Main will ensure that all threads get shut down
            Environment.Exit(0);
        }

        private static void InitialiseClassFactory()
        {
            Factory.Singleton.Register<IApplicationInformation, ApplicationInformation>();

            SQLiteWrapper.Implementations.Register(Factory.Singleton);
            VirtualRadar.Library.Implementations.Register(Factory.Singleton);
            VirtualRadar.Database.Implementations.Register(Factory.Singleton);
            VirtualRadar.WebServer.Implementations.Register(Factory.Singleton);
            VirtualRadar.WebSite.Implementations.Register(Factory.Singleton);
            if(!ProgramLifetime.Headless) {
                VirtualRadar.WinForms.Implementations.Register(Factory.Singleton);
            } else {
                VirtualRadar.Headless.Implementations.Register(Factory.Singleton);
            }
        }

        private static void ShowConfigurationFolder()
        {
            var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
            var folderMessage = String.Format("Configuration folder: {0}", configurationStorage.Folder);
            Console.WriteLine(folderMessage);
            Factory.Singleton.Resolve<IMessageBox>().Show("Configuration Folder");
        }
   }
}