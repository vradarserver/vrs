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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Localisation;
using VirtualRadar.WinForms;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar
{
    /// <summary>
    /// Main class for the application.
    /// </summary>
    static class Program
    {
        #region Fields
        /// <summary>
        /// The name of the mutex that we use to ensure only one instance will run.
        /// </summary>
        private const string _SingleInstanceMutexName = "VirtualRadarServer-SJKADBK42348J";

        /// <summary>
        /// The main window.
        /// </summary>
        private static IMainView _MainView;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the culture info that was forced into use by the -culture command-line switch.
        /// </summary>
        public static CultureInfo ForcedCultureInfo { get; set; }
        #endregion

        #region Main
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            InitialiseUnhandledExceptionHandling();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            foreach(string arg in args) {
                if(arg.ToUpper().StartsWith("-CULTURE:")) {
                    ForcedCultureInfo = new CultureInfo(arg.Substring(9));
                    Thread.CurrentThread.CurrentUICulture = ForcedCultureInfo;
                    Thread.CurrentThread.CurrentCulture = ForcedCultureInfo;
                }
                if(arg.ToUpper() == "-DEFAULTFONTS") {
                    VirtualRadar.WinForms.FontFactory.DisableFontReplacement = true;
                }
            }

            Factory.Singleton.Register<IApplicationInformation, ApplicationInformation>();
            SQLiteWrapper.Implementations.Register(Factory.Singleton);
            VirtualRadar.Library.Implementations.Register(Factory.Singleton);
            VirtualRadar.Database.Implementations.Register(Factory.Singleton);
            VirtualRadar.WebServer.Implementations.Register(Factory.Singleton);
            VirtualRadar.WebSite.Implementations.Register(Factory.Singleton);

            if(args.Contains("-showConfigFolder")) {
                var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
                var folderMessage = String.Format("Configuration folder: {0}", configurationStorage.Folder);
                Console.WriteLine(folderMessage);
                MessageBox.Show(folderMessage, "Configuration Folder");
            }

            // Mono doesn't support the heartbeat timer on network connections so we force use of a dirtier equivalent
            if(Factory.Singleton.Resolve<IRuntimeEnvironment>().IsMono) {
                Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.CoarseListenerTimeout = 10;
            }

            var pluginManager = Factory.Singleton.Resolve<IPluginManager>().Singleton;
            pluginManager.LoadPlugins();

            bool mutexAcquired;
            bool allowMultipleInstances = args.Any(a => a.ToUpper().StartsWith("-WORKINGFOLDER:"));
            using(var singleInstanceMutex = CheckForOtherRunningInstances(out mutexAcquired, allowMultipleInstances)) {
                try {
                    CheckForHttpListenerSupport();
                    CheckForDotNetThreePointFive();

                    StartApplication(args);
                } finally {
                    if(mutexAcquired) singleInstanceMutex.ReleaseMutex();
                }
            }

            // Calling Environment.Exit rather than falling off the end of Main will ensure that background threads get shut down
            Environment.Exit(0);
        }

        /// <summary>
        /// Registers event handlers with .NET that will be called when unhandled exceptions are thrown.
        /// </summary>
        private static void InitialiseUnhandledExceptionHandling()
        {
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        /// <summary>
        /// Creates the mutex that will be held for the duration of the application. The mutex is used to prevent
        /// multiple instances from running. Quits the program if another instance is seen.
        /// </summary>
        /// <returns></returns>
        private static Mutex CheckForOtherRunningInstances(out bool mutexAcquired, bool allowMultipleInstances)
        {
            mutexAcquired = false;
            var result = new Mutex(false, _SingleInstanceMutexName);

            var runtimeEnvironment = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton;
            if(!runtimeEnvironment.IsMono) {
                var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
                var securitySettings = new MutexSecurity();
                securitySettings.AddAccessRule(allowEveryoneRule);
                result.SetAccessControl(securitySettings);

                if(!allowMultipleInstances) {
                    try {
                        mutexAcquired = result.WaitOne(1000, false);
                        if(!mutexAcquired) {
                            MessageBox.Show(Strings.AnotherInstanceRunningFull, Strings.AnotherInstanceRunningTitle);
                            Environment.Exit(1);
                        }
                    } catch(AbandonedMutexException) { }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks that the operating system supports the use of the .NET HttpListener object. Quits if it doesn't.
        /// </summary>
        private static void CheckForHttpListenerSupport()
        {
            if(!HttpListener.IsSupported) {
                MessageBox.Show(Strings.WindowsVersionTooLowFull, Strings.WindowsVersionTooLowTitle);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Checks that we're running under .NET 3.5 or better. If it isn't then the application quits.
        /// </summary>
        private static void CheckForDotNetThreePointFive()
        {
            var runtimeEnvironment = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton;
            if(!runtimeEnvironment.IsMono) {
                try {
                    TestCanLoadThreePointFiveObject();
                } catch(FileNotFoundException) {
                    if(MessageBox.Show(Strings.DotNetVersionTooLowFull, Strings.DotNetVersionTooLowTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes) {
                        Process.Start("http://www.microsoft.com/downloads/details.aspx?FamilyID=ab99342f-5d1a-413d-8319-81da479ab0d7&displaylang=en");
                    }
                    Environment.Exit(1);
                }
            }
        }

        /// <summary>
        /// Performs a series of calls that will not prevent the application from loading but will cause the framework
        /// to throw a FileNotFound exception if an attempt is made to call the method under .NET 2.
        /// </summary>
        private static void TestCanLoadThreePointFiveObject()
        {
            // This will only work in 3.0 and above, and then only the full version of 3.5 (not the client profile)
            var speech = Factory.Singleton.Resolve<ISpeechSynthesizerWrapper>();
            speech.Dispose();

            // This will only work in 3.5 and above
            List<int> x = new List<int>();
            x.FirstOrDefault();
        }

        /// <summary>
        /// Displays the splash screen (whose presenter builds up most of the objects used by the program) and then the
        /// main view once the splash screen has finished. When the main view quits the program shuts down.
        /// </summary>
        /// <param name="args"></param>
        private static void StartApplication(string[] args)
        {
            IUniversalPlugAndPlayManager uPnpManager = null;
            IBaseStationAircraftList baseStationAircraftList = null;
            ISimpleAircraftList flightSimulatorXAircraftList = null;
            bool loadSucceded = false;

            using(var splashScreen = new SplashView()) {
                splashScreen.Initialise(args, BackgroundThread_ExceptionCaught);
                splashScreen.ShowDialog();

                loadSucceded = splashScreen.LoadSucceeded;
                uPnpManager = splashScreen.UPnpManager;
                baseStationAircraftList = splashScreen.BaseStationAircraftList;
                flightSimulatorXAircraftList = splashScreen.FlightSimulatorXAircraftList;
            }

            try {
                if(loadSucceded) {
                    using(var mainWindow = new MainView()) {
                        _MainView = mainWindow;
                        mainWindow.Initialise(uPnpManager, flightSimulatorXAircraftList);
                        Application.Run(mainWindow);
                    }
                }
            } finally {
                using(var shutdownWindow = new ShutdownView()) {
                    shutdownWindow.Initialise(uPnpManager, baseStationAircraftList);
                    shutdownWindow.ShowDialog();
                    Thread.Sleep(1000);
                }
            }
        }
        #endregion

        #region CurrentDomain_UnhandledException, Application_ThreadException, BackgroundThread_ExceptionCaught, ShowException
        /// <summary>
        /// Called when an unhandled exception was caught for any thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Don't translate, I don't want to hide errors if the translation throws exceptions
            Exception ex = e.ExceptionObject as Exception;
            if(ex != null) ShowException(ex);
            else MessageBox.Show(String.Format("An exception that was not of type Exception was caught.\r\n{0}", e.ExceptionObject), "Unknown Exception Caught");
        }

        /// <summary>
        /// Called when an unhandled exception was caught for the GUI thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            ShowException(e.Exception);
        }

        /// <summary>
        /// Called when objects that utilise background threads catch an exception on that background thread. Note
        /// that this is almost certainly called from a non-GUI thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void BackgroundThread_ExceptionCaught(object sender, EventArgs<Exception> args)
        {
            if(_MainView != null) _MainView.BubbleExceptionToGui(args.Value);
            else {
                var log = Factory.Singleton.Resolve<ILog>().Singleton;
                log.WriteLine("Unhandled exception caught in BaseStationAircraftList before GUI available to show to user: {0}", args.Value.ToString());
            }
        }

        /// <summary>
        /// Shows the details of an exception to the user and logs it.
        /// </summary>
        /// <param name="ex"></param>
        static void ShowException(Exception ex)
        {
            // Don't translate, I don't want to confuse things if the translation throws exceptions

            var buffer = new StringBuilder();
            buffer.AppendFormat("An unhandled exception was caught: {0}\r\nFull message:{1}\r\n", ex.Message, ex.ToString());
            for(Exception innerEx = ex.InnerException;innerEx != null;innerEx = innerEx.InnerException) {
                buffer.AppendFormat("\r\nINNER EXCEPTION: {0}\r\n{1}\r\n", innerEx.Message, innerEx.ToString());
            }
            var message = buffer.ToString();

            try {
                Clipboard.SetText(message);
            } catch { }

            ILog log = null;
            try {
                log = Factory.Singleton.Resolve<ILog>().Singleton;
                if(log != null) log.WriteLine(message);
            } catch { }

            try {
                MessageBox.Show(message, "Unhandled Exception Caught");
            } catch(Exception doubleEx) {
                Debug.WriteLine(String.Format("Program.ShowException caught double-exception: {0} when trying to display / log {1}", doubleEx.ToString(), ex.ToString()));
                try {
                    if(log != null) log.WriteLine("Caught exception while trying to show a previous exception: {0}", doubleEx.ToString());
                } catch { }
            }
        }
        #endregion
   }
}