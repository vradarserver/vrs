// Copyright © 2017 onwards, Andrew Whewell
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
