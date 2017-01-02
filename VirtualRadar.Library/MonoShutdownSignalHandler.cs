using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InterfaceFactory;
using Mono.Unix;
using Mono.Unix.Native;
using VirtualRadar.Interface;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The Mono version of <see cref="IShutdownSignalHandler"/>.
    /// </summary>
    /// <remarks>
    /// The .NET stub is <see cref="DotNetShutdownSignalHandler"/>.
    /// </remarks>
    class MonoShutdownSignalHandler : IShutdownSignalHandler
    {
        private bool _ConsoleCancelKeyPressHooked;
        private Thread _ShutdownSignalHandlerThread;

        private static MonoShutdownSignalHandler _Singleton;
        public IShutdownSignalHandler Singleton
        {
            get {
                if(_Singleton == null) {
                    _Singleton = new MonoShutdownSignalHandler();
                }
                return _Singleton;
            }
        }

        public void CloseMainViewOnShutdownSignal()
        {
            StartShutdownSignalHandlerThread();
            HookConsoleCancelKeyPress();
        }

        public void Cleanup()
        {
            StopShutdownSignalHandlerThread();
            UnhookConsoleCancelKeyPress();
        }

        private void StartShutdownSignalHandlerThread()
        {
            if(_ShutdownSignalHandlerThread == null) {
                _ShutdownSignalHandlerThread = new Thread(() => {
                    try {
                        var signals = new UnixSignal[] {
                            new UnixSignal(Signum.SIGINT),
                        };

                        while(true) {
                            if(UnixSignal.WaitAny(signals, -1) != -1) {
                                ProgramLifetime.MainView?.CloseView();
                            }
                        }
                    } catch(ThreadAbortException) {
                        ;   // these get rethrown automatically, I just don't want them logged
                    } catch(Exception ex) {
                        try {
                            var log = Factory.Singleton.Resolve<ILog>().Singleton;
                            log.WriteLine("Caught exception in shutdown signal handler: {0}", ex);
                        } catch {
                            // Don't let exceptions bubble out of a background thread
                        }
                    }
                });

                _ShutdownSignalHandlerThread.Start();
            }
        }

        private void StopShutdownSignalHandlerThread()
        {
            if(_ShutdownSignalHandlerThread != null) {
                try {
                    _ShutdownSignalHandlerThread.Abort();
                } catch {
                    ;
                }
                _ShutdownSignalHandlerThread = null;
            }
        }

        private void HookConsoleCancelKeyPress()
        {
            if(!_ConsoleCancelKeyPressHooked) {
                _ConsoleCancelKeyPressHooked = true;
                Console.CancelKeyPress += Console_CancelKeyPress;
            }
        }

        private void UnhookConsoleCancelKeyPress()
        {
            if(_ConsoleCancelKeyPressHooked) {
                _ConsoleCancelKeyPressHooked = false;
                Console.CancelKeyPress -= Console_CancelKeyPress;
            }
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            ProgramLifetime.MainView?.CloseView();
            e.Cancel = true;
        }
    }
}
