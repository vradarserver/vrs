using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Wraps the console.
    /// </summary>
    public interface IConsole : ISingleton<IConsole>
    {
        /// <summary>
        /// Gets the current foreground colour.
        /// </summary>
        ConsoleColor ForegroundColor { get; set; }

        /// <summary>
        /// Returns true if there is a key available in the input buffer.
        /// </summary>
        bool KeyAvailable { get; }

        /// <summary>
        /// Reads the next key from the input buffer.
        /// </summary>
        /// <param name="intercept"></param>
        /// <returns></returns>
        ConsoleKeyInfo ReadKey(bool intercept = false);

        /// <summary>
        /// Sounds the buzzer.
        /// </summary>
        void Beep();

        /// <summary>
        /// Writes a message to the console.
        /// </summary>
        /// <param name="message"></param>
        void Write(string message);

        /// <summary>
        /// Writes a character to the console.
        /// </summary>
        /// <param name="value"></param>
        void Write(char value);

        /// <summary>
        /// Writes a blank line to the console.
        /// </summary>
        void WriteLine();

        /// <summary>
        /// Writes a message to the console and terminates it with a new line.
        /// </summary>
        /// <param name="message"></param>
        void WriteLine(string message);

        /// <summary>
        /// Writes a formatted message to the console and terminates it with a new line.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void WriteLine(string format, params object[] args);
    }
}
