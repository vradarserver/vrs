using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VirtualRadar.Interface;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The .NET implementation of <see cref="IShutdownSignalHandler"/>.
    /// </summary>
    /// <remarks>
    /// The mono version is <see cref="MonoShutdownSignalHandler"/>.
    /// </remarks>
    class DotNetShutdownSignalHandler : IShutdownSignalHandler
    {
        private bool _ConsoleCancelKeyPressHooked;

        private static DotNetShutdownSignalHandler _Singleton;
        public IShutdownSignalHandler Singleton
        {
            get {
                if(_Singleton == null) {
                    _Singleton = new DotNetShutdownSignalHandler();
                }
                return _Singleton;
            }
        }

        public void CloseMainViewOnShutdownSignal()
        {
            if(!_ConsoleCancelKeyPressHooked) {
                _ConsoleCancelKeyPressHooked = true;
                Console.CancelKeyPress += Console_CancelKeyPress;
            }
        }

        public void Cleanup()
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
