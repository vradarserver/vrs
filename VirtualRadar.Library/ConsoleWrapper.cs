using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualRadar.Interface;

namespace VirtualRadar.Library
{
    class ConsoleWrapper : IConsole
    {
        private static ConsoleWrapper _Singleton;
        public IConsole Singleton
        {
            get {
                if(_Singleton == null) {
                    _Singleton = new ConsoleWrapper();
                }
                return _Singleton;
            }
        }

        public ConsoleColor ForegroundColor
        {
            get { return Console.ForegroundColor; }
            set { Console.ForegroundColor = value; }
        }

        public bool KeyAvailable
        {
            get { return Console.KeyAvailable; }
        }

        public void Beep()
        {
            Console.Beep();
        }

        public ConsoleKeyInfo ReadKey(bool intercept = false)
        {
            return Console.ReadKey(intercept);
        }

        public void Write(string message)
        {
            Console.Write(message);
        }

        public void Write(char value)
        {
            Console.Write(value);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}
