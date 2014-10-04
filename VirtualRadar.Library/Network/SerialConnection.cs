// Copyright © 2014 onwards, Andrew Whewell
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
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Network;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// Describes a connection to a serial port.
    /// </summary>
    class SerialConnection : Connection
    {
        #region Fields
        /// <summary>
        /// Set to true when the code is trying to close the serial port.
        /// </summary>
        private bool _Closing;

        /// <summary>
        /// A wait handle that gets reset when read/write operations start and set when they finish.
        /// The Close and read/write methods use this to ensure that the operations finish using the
        /// serial port before the Close tries to close it.
        /// </summary>
        private EventWaitHandle _BusyReading = new EventWaitHandle(false, EventResetMode.ManualReset);
        #endregion

        #region Properties
        /// <summary>
        /// Gets the serial port that we're using.
        /// </summary>
        private SerialPort SerialPort { get; set; }

        /// <summary>
        /// Gets or sets the shutdown text.
        /// </summary>
        public string ShutdownText { get; private set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="initialStatus"></param>
        /// <param name="serialPort"></param>
        /// <param name="shutdownText"></param>
        public SerialConnection(Connector connector, ConnectionStatus initialStatus, SerialPort serialPort, string shutdownText) : base(connector, initialStatus)
        {
            SerialPort = serialPort;
            ShutdownText = shutdownText;
            Description = String.Format("Serial connection on {0} at {1:N0} baud", serialPort.PortName, serialPort.BaudRate);
        }
        #endregion

        #region GetSerialPort
        /// <summary>
        /// Gets the current value of <see cref="SerialPort"/>.
        /// </summary>
        /// <returns></returns>
        private SerialPort GetSerialPort()
        {
            lock(_SyncLock) {
                return SerialPort;
            }
        }
        #endregion

        #region AbandonConnection, Close
        /// <summary>
        /// See the base docs.
        /// </summary>
        protected override void AbandonConnection()
        {
            var closedPort = false;

            lock(_SyncLock) {
                if(SerialPort != null) {
                    closedPort = true;
                    var serialPort = SerialPort;
                    SerialPort = null;

                    try {
                        Close(serialPort);
                    } catch {
                    }
                }
            }

            if(closedPort) {
                ConnectionStatus = ConnectionStatus.Disconnected;
                if(_Connector != null) _Connector.ConnectionAbandoned(this);
            }
        }

        /// <summary>
        /// Closes the serial port.
        /// </summary>
        private void Close(SerialPort serialPort)
        {
            bool originalClosingValue = _Closing;
            try {
                _Closing = true;
                if(serialPort != null) {
                    if(serialPort.IsOpen) {
                        _BusyReading.WaitOne(2500);
                        SendText(serialPort, ShutdownText, 100);
                        serialPort.Close();
                    }

                    serialPort.Dispose();

                    // The documentation recommends pausing between closing & opening SerialPort, this
                    // just ensures that no open calls are made on this serial port for a bit.
                    Thread.Sleep(500);
                }
            } finally {
                _Closing = originalClosingValue;
            }
        }
        #endregion

        #region DoRead, DoWrite
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="op"></param>
        protected override void DoRead(ReadWriteOperation op)
        {
            var serialPort = GetSerialPort();
            if(serialPort != null) {
                _BusyReading.Reset();
                try {
                    var timedOut = false;
                    var bytesRead = 0;
                    do {
                        try {
                            timedOut = false;
                            while(!_Closing && serialPort.BytesToRead == 0) Thread.Sleep(1); // give up the rest of our time slice before trying again
                            if(!_Closing) bytesRead = serialPort.Read(op.Buffer, op.Offset, op.Length);
                        } catch(TimeoutException) {
                            bytesRead = -1;
                            timedOut = true;
                        }
                        serialPort = GetSerialPort();
                    } while(serialPort != null && !_Closing && (timedOut || bytesRead == 0));

                    op.BytesRead = bytesRead;
                    op.Abandon = serialPort == null || _Closing;
                } catch(Exception ex) {
                    if(_Connector != null) _Connector.RaiseConnectionException(this, ex);
                    op.Abandon = true;
                } finally {
                    _BusyReading.Set();
                }
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="op"></param>
        protected override void DoWrite(ReadWriteOperation op)
        {
            var serialPort = GetSerialPort();
            if(serialPort != null) {
                try {
                    serialPort.Write(op.Buffer, op.Offset, op.Length);
                } catch(Exception ex) {
                    if(_Connector != null) _Connector.RaiseConnectionException(this, ex);
                    op.Abandon = true;
                }
            }
        }
        #endregion

        #region SendText
        /// <summary>
        /// Sends text across the COM port, if open.
        /// </summary>
        /// <param name="serialPort"></param>
        /// <param name="text"></param>
        /// <param name="delayAfterSend"></param>
        public void SendText(string text, int delayAfterSend = 0)
        {
            SendText(SerialPort, text, delayAfterSend);
        }

        private void SendText(SerialPort serialPort, string text, int delayAfterSend)
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
