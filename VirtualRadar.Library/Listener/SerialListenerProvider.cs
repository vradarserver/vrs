// Copyright © 2012 onwards, Andrew Whewell
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
using VirtualRadar.Interface.Listener;
using System.IO.Ports;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics;
using System.Runtime.Remoting;

namespace VirtualRadar.Library.Listener
{
    /// <summary>
    /// The default implementation of <see cref="ISerialListenerProvider"/>.
    /// </summary>
    /// <remarks>
    /// It is a requirement of these providers that they run on a background thread. I had originally implemented
    /// this using the SerialPort.BaseStream's BeginRead / EndRead pattern, which worked just fine on Windows.
    /// However it would block on Mono :( So I've had to resort to using ReadBytes, which blocks, and doing that
    /// on a background thread.
    /// </remarks>
    class SerialListenerProvider : ISerialListenerProvider
    {
        #region Fields
        /// <summary>
        /// The serial port in use.
        /// </summary>
        private SerialPort _SerialPort;

        /// <summary>
        /// Set to true when the code is trying to close the serial port.
        /// </summary>
        private bool _Closing;

        /// <summary>
        /// The delegate used to call <see cref="OpenSerialPortInBackground"/> on a background thread.
        /// </summary>
        private OpenSerialPortDelegate _OpenSerialPortCaller;

        /// <summary>
        /// The delegate used to call <see cref="ReadBytesInBackground"/> on a background thread.
        /// </summary>
        private ReadBytesDelegate _ReadBytesDelegate;

        /// <summary>
        /// A wait handle that gets reset when the background read starts and set when it finishes. The
        /// Close and read methods use this to ensure that the reads finish using the serial port before
        /// the Close tries to close it.
        /// </summary>
        private EventWaitHandle _BusyReading = new EventWaitHandle(false, EventResetMode.ManualReset);
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ComPort { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int BaudRate { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int DataBits { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public StopBits StopBits { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Parity Parity { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Handshake Handshake { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string StartupText { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ShutdownText { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public byte[] ReadBuffer { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public SerialListenerProvider()
        {
            ReadBuffer = new byte[8192];
            _OpenSerialPortCaller = new OpenSerialPortDelegate(OpenSerialPortInBackground);
            _ReadBytesDelegate = new ReadBytesDelegate(ReadBytesInBackground);
        }
        #endregion

        #region BeginConnect, EndConnect, Close, BeginRead, EndRead
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IAsyncResult BeginConnect(AsyncCallback callback)
        {
            Close();

            _BusyReading.Set();
            _SerialPort = new SerialPort(ComPort, BaudRate, Parity, DataBits, StopBits) {
                ReadTimeout = 1000,
                WriteTimeout = 1000,
                Handshake = Handshake,
                ReadBufferSize = 8192,
            };

            return _OpenSerialPortCaller.BeginInvoke(_SerialPort, callback, null);
        }

        private delegate SerialPort OpenSerialPortDelegate(SerialPort serialPort);

        private SerialPort OpenSerialPortInBackground(SerialPort serialPort)
        {
            Debug.WriteLine("Opening serial port");
            serialPort.Open();
            Debug.WriteLine("Opened serial port");

            SendText(serialPort, StartupText);

            return serialPort;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public bool EndConnect(IAsyncResult asyncResult)
        {
            object serialPort = null;
            try {
                serialPort = _OpenSerialPortCaller.EndInvoke(asyncResult);
            } catch(NullReferenceException) {
                // Exception spam, this gets thrown if the serial port is disposed while the serial port has hung on the open
            }

            return serialPort == _SerialPort;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Close()
        {
            bool originalClosingValue = _Closing;
            try {
                _Closing = true;
                if(_SerialPort != null) {
                    if(_SerialPort.IsOpen) {
                        Debug.WriteLine("Waiting on reads to finish before closing serial port");
                        if(!_BusyReading.WaitOne(2500)) {
                            Debug.WriteLine("Wait for reads timed out");
                        }

                        Debug.WriteLine("Sending close command to serial port");
                        SendText(_SerialPort, ShutdownText, 100);

                        Debug.WriteLine("Closing serial port");
                        _SerialPort.Close();
                    }

                    _SerialPort.Dispose();
                    _SerialPort = null;

                    Thread.Sleep(500); // documentation recommends pausing between closing & opening SerialPort
                }
            } finally {
                _Closing = originalClosingValue;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IAsyncResult BeginRead(AsyncCallback callback)
        {
            return _ReadBytesDelegate.BeginInvoke(_SerialPort, ReadBuffer, callback, null);
        }

        private delegate int ReadBytesDelegate(SerialPort serialPort, byte[] buffer);

        private int ReadBytesInBackground(SerialPort serialPort, byte[] buffer)
        {
            bool timedOut = false;
            int result = 0;

            try {
                _BusyReading.Reset();

                do {
                    try {
                        timedOut = false;
                        while(!_Closing && serialPort.BytesToRead == 0) Thread.Sleep(1); // give up the rest of our time slice before trying again
                        if(!_Closing) result = serialPort.Read(buffer, 0, buffer.Length);
                    } catch(TimeoutException) {
                        result = -1;
                        timedOut = true;
                    }
                } while(!_Closing && (timedOut || result == 0));
            } finally {
                _BusyReading.Set();
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public int EndRead(IAsyncResult asyncResult)
        {
            int result = 0;
            try {
                result = _ReadBytesDelegate.EndInvoke(asyncResult);
            } catch(RemotingException) {
                // This happens when the provider is changed mid-read. The current design of the listener is flawed, it is
                // too hard to match up the begin reads and end reads when half the call is in the listener and the other
                // half is in the provider. However I'm too close to releasing this to redesign the way the listener and
                // providers interact and I while this isn't pretty and it can leak handles it should only happen very rarely,
                // so for now I'm just going to swallow the exception. In the next release of VRS I'll make the providers
                // more self-contained and make life a lot simpler, but probably at the expense of not being able to unit
                // test the reconnect logic... the reason we have this schizo behaviour is so that I can test that it'll
                // reconnect when a connection breaks, and it worked fine when all I had was the network provider, but now
                // that the COM port provider is on the scene it's just getting too damn fiddly. The providers need to be
                // more self-contained.
                //
                // The new provider model won't get done until after December 2012, I've a ton of paying work on up until
                // the end of the year... which is why I want to get this released sooner rather than later, if I don't get
                // it done now I won't have another chance until 2013.
            }
            if(result == 0 && (_Closing || _SerialPort == null || !_SerialPort.IsOpen)) throw new ObjectDisposedException("fake"); // Fake behaviour of a socket being closed - stops reconnect in listener;

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="milliseconds"></param>
        public void Sleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }
        #endregion

        #region SendText
        /// <summary>
        /// Sends text across the COM port, if open.
        /// </summary>
        /// <param name="serialPort"></param>
        /// <param name="text"></param>
        /// <param name="delayAfterSend"></param>
        private void SendText(SerialPort serialPort, string text, int delayAfterSend = 0)
        {
            if(!String.IsNullOrEmpty(text)) {
                if(serialPort != null && serialPort.IsOpen) {
                    serialPort.Write(text.Replace("\\r", "\r").Replace("\\n", "\n"));
                    if(delayAfterSend > 0) Thread.Sleep(delayAfterSend);
                }
            }
        }
        #endregion
    }
}
