using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InterfaceFactory;
using VirtualRadar;
using VirtualRadar.Interface;

namespace VirtualRadar
{
    /// <summary>
    /// The Virtual Radar Server service class.
    /// </summary>
    public partial class Service : ServiceBase
    {
        /// <summary>
        /// How long to wait for the program to shut down cleanly before we give up.
        /// </summary>
        private const int ShutdownTimeoutMilliseconds = 2 * 60000;

        /// <summary>
        /// The service's main thread.
        /// </summary>
        private Thread _ServiceThread;

        /// <summary>
        /// True if the service should launch the debugger when starting up.
        /// </summary>
        private bool _TriggerDebugger;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Service()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when Windows wants to start the service.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            args = PreprocessArgs(args);

            if(_TriggerDebugger) {
                System.Diagnostics.Debugger.Launch();
            }

            _ServiceThread = new Thread(ServiceThread);
            _ServiceThread.Name = "ServiceMainThread";
            _ServiceThread.Start(args);
        }

        /// <summary>
        /// Called when Windows wants to stop the service.
        /// </summary>
        protected override void OnStop()
        {
            if(_ServiceThread != null) {
                IConsole console = null;
                try {
                    console = Factory.Singleton.Resolve<IConsole>().Singleton;
                } catch {
                    console = null;
                }

                console?.WriteLine("Stopping service at {0:yyyy-MM-dd HH:mm:ss.fff} (UTC)", DateTime.UtcNow);
                ProgramLifetime.MainView?.CloseView();
                ProgramLifetime.WaitForShutdown(ShutdownTimeoutMilliseconds);
                console?.WriteLine("Service stopped at {0:yyyy-MM-dd HH:mm:ss.fff} (UTC)", DateTime.UtcNow);
            }
        }

        /// <summary>
        /// Filters out VRS arguments that can't be used when running as a service and service-only arguments.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private string[] PreprocessArgs(IEnumerable<string> args)
        {
            var result = new List<string>();

            foreach(var arg in args ?? new string[] {}) {
                switch(arg.ToLower()) {
                    case "-nogui":
                        // Don't let these get through to the program
                        break;
                    case "-debugonstart":
                        _TriggerDebugger = true;
                        break;
                    default:
                        result.Add(arg);
                        break;
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// The Virtual Radar Server main thread when running as a service.
        /// </summary>
        /// <param name="state"></param>
        private void ServiceThread(object state)
        {
            try {
                var args = (string[])state;

                ProgramLifetime.InitialiseUnhandledExceptionHandling();
                ProgramLifetime.PrepassCommandLineArgs(args);
                ProgramLifetime.Headless = true;

                ApplicationInformation.SetHeadless(ProgramLifetime.Headless);

                InitialiseClassFactory();

                ProgramLifetime.InitialiseManagers();
                ProgramLifetime.LoadPlugins();
                ProgramLifetime.SingleInstanceStart(args);
            } catch(ThreadAbortException) {
                ;
            } catch(Exception ex) {
                try {
                    var log = Factory.Singleton.Resolve<ILog>().Singleton;
                    log.WriteLine("Caught exception in ServiceThread: {0}", ex);
                } catch {
                    ;
                }

                Stop();
            }
        }

        /// <summary>
        /// Sets up the class factory for the service.
        /// </summary>
        private static void InitialiseClassFactory()
        {
            Factory.Singleton.Register<IApplicationInformation, ApplicationInformation>();

            SQLiteWrapper.Implementations.Register(Factory.Singleton);
            VirtualRadar.Library.Implementations.Register(Factory.Singleton);
            VirtualRadar.Database.Implementations.Register(Factory.Singleton);
            VirtualRadar.WebServer.Implementations.Register(Factory.Singleton);
            VirtualRadar.WebSite.Implementations.Register(Factory.Singleton);
            VirtualRadar.Headless.Implementations.Register(Factory.Singleton);

            Factory.Singleton.Register<IConsole, ConsoleWrapper>();
        }
    }
}
