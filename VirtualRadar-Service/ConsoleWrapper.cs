using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar
{
    class ConsoleWrapper : IConsole
    {
        private const string LogFileName = "ServiceMessages.txt";
        private static string LogFullPath;

        private static ConsoleWrapper _Singleton;
        public IConsole Singleton
        {
            get {
                if(_Singleton == null) {
                    _Singleton = new ConsoleWrapper();

                    var folder = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.Folder;
                    LogFullPath = Path.Combine(folder, LogFileName);

                    if(!Directory.Exists(folder)) {
                        Directory.CreateDirectory(folder);
                    }

                    File.WriteAllLines(LogFullPath, new string[] {
                        String.Format("Service started at {0:yyyy-MM-dd HH:mm:ss.fff} (UTC)", DateTime.UtcNow),
                    });
                }
                return _Singleton;
            }
        }

        public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Gray;

        public bool KeyAvailable
        {
            get { return false; }
        }

        public void Beep()
        {
            ;
        }

        public ConsoleKeyInfo ReadKey(bool intercept = false)
        {
            return new ConsoleKeyInfo();
        }

        public void Write(string message)
        {
            File.AppendAllText(LogFullPath, message);
        }

        public void Write(char value)
        {
            Write(new string(value, 1));
        }

        public void WriteLine()
        {
            WriteLine("");
        }

        public void WriteLine(string message)
        {
            File.AppendAllLines(LogFullPath, new string[] { message });
        }

        public void WriteLine(string format, params object[] args)
        {
            WriteLine(String.Format(format, args));
        }
    }
}
