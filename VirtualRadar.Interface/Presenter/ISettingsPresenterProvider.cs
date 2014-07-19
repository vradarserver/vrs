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
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace VirtualRadar.Interface.Presenter
{
    /// <summary>
    /// The interface for objects that abstract away the environment for <see cref="ISettingsPresenter"/> tests.
    /// </summary>
    public interface ISettingsPresenterProvider : IDisposable
    {
        /// <summary>
        /// Returns the exception (if any) that was thrown while attempting to establish a connection to a data feed
        /// using the network settings passed across.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        Exception TestNetworkConnection(string address, int port);

        /// <summary>
        /// Returns the exception (if any) that was thrown while attempting to establish a connection to a data feed
        /// using the serial settings passed across.
        /// </summary>
        /// <param name="comPort"></param>
        /// <param name="baudRate"></param>
        /// <param name="dataBits"></param>
        /// <param name="stopBits"></param>
        /// <param name="parity"></param>
        /// <param name="handShake"></param>
        /// <returns></returns>
        Exception TestSerialConnection(string comPort, int baudRate, int dataBits, StopBits stopBits, Parity parity, Handshake handShake);

        /// <summary>
        /// Returns true if there is a file with the full path as passed.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        bool FileExists(string fileName);

        /// <summary>
        /// Returns true if there is a folder with the path as passed.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        bool FolderExists(string folder);

        /// <summary>
        /// Returns a collection of voice names for the installed text-to-speech service. Guaranteed not to throw an exception
        /// and to be current (i.e. no caching).
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetVoiceNames();

        /// <summary>
        /// Returns a list of serial port names. Guaranteed not to throw an exception and to be current (i.e. no caching).
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetSerialPortNames();

        /// <summary>
        /// Conducts a test of the text-to-speech settings passed across.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="rate"></param>
        void TestTextToSpeech(string name, int rate);
    }
}
