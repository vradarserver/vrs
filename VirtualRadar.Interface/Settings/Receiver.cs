// Copyright © 2013 onwards, Andrew Whewell
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
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// A settings object that carries information about a receiever.
    /// </summary>
    public class Receiver : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets a value indicating that the receiver is to be used.
        /// </summary>
        private bool _Enabled;
        public bool Enabled
        {
            get { return _Enabled; }
            set { SetField(ref _Enabled, value, () => Enabled); }
        }

        /// <summary>
        /// Gets or sets a unique identifier for the receiever. This is unique across receivers and merged feeds. It cannot be zero.
        /// </summary>
        private int _UniqueId;
        public int UniqueId
        {
            get { return _UniqueId; }
            set { SetField(ref _UniqueId, value, () => UniqueId); }
        }

        /// <summary>
        /// Gets or sets the unique name of the receiver.
        /// </summary>
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { SetField(ref _Name, value, () => Name); }
        }

        /// <summary>
        /// Gets or sets the source of data for the receiever.
        /// </summary>
        private DataSource _DataSource;
        public DataSource DataSource
        {
            get { return _DataSource; }
            set { SetField(ref _DataSource, value, () => DataSource); }
        }

        /// <summary>
        /// Gets or sets the mechanism to use to connect to the data source.
        /// </summary>
        private ConnectionType _ConnectionType;
        public ConnectionType ConnectionType
        {
            get { return _ConnectionType; }
            set { SetField(ref _ConnectionType, value, () => ConnectionType); }
        }

        /// <summary>
        /// Gets or sets a value indicating that the program should keep attempting to connect to the data source
        /// if it cannot connect when the program first starts.
        /// </summary>
        private bool _AutoReconnectAtStartup;
        public bool AutoReconnectAtStartup
        {
            get { return _AutoReconnectAtStartup; }
            set { SetField(ref _AutoReconnectAtStartup, value, () => AutoReconnectAtStartup); }
        }

        /// <summary>
        /// Gets or sets the address of the source of data to listen to.
        /// </summary>
        private string _Address;
        public string Address
        {
            get { return _Address; }
            set { SetField(ref _Address, value, () => Address); }
        }

        /// <summary>
        /// Gets or sets the port of the source of data to listen to.
        /// </summary>
        private int _Port;
        public int Port
        {
            get { return _Port; }
            set { SetField(ref _Port, value, () => Port); }
        }

        /// <summary>
        /// Gets or sets the COM port to listen to.
        /// </summary>
        private string _ComPort;
        public string ComPort
        {
            get { return _ComPort; }
            set { SetField(ref _ComPort, value, () => ComPort); }
        }

        /// <summary>
        /// Gets or sets the baud rate to use.
        /// </summary>
        private int _BaudRate;
        public int BaudRate
        {
            get { return _BaudRate; }
            set { SetField(ref _BaudRate, value, () => BaudRate); }
        }

        /// <summary>
        /// Gets or sets the data bits to use.
        /// </summary>
        private int _DataBits;
        public int DataBits
        {
            get { return _DataBits; }
            set { SetField(ref _DataBits, value, () => DataBits); }
        }

        /// <summary>
        /// Gets or sets the stop bits to use.
        /// </summary>
        private StopBits _StopBits;
        public StopBits StopBits
        {
            get { return _StopBits; }
            set { SetField(ref _StopBits, value, () => StopBits); }
        }

        /// <summary>
        /// Gets or sets the parity to use.
        /// </summary>
        private Parity _Parity;
        public Parity Parity
        {
            get { return _Parity; }
            set { SetField(ref _Parity, value, () => Parity); }
        }

        /// <summary>
        /// Gets or sets the handshake protocol to use.
        /// </summary>
        private Handshake _Handshake;
        public Handshake Handshake
        {
            get { return _Handshake; }
            set { SetField(ref _Handshake, value, () => Handshake); }
        }

        /// <summary>
        /// Gets or sets the text to send across the COM port on startup - a null or empty string will disable the
        /// feature. Can contain \r and \n.
        /// </summary>
        private string _StartupText;
        public string StartupText
        {
            get { return _StartupText; }
            set { SetField(ref _StartupText, value, () => StartupText); }
        }

        /// <summary>
        /// Gets or sets the text to send across the COM port on shutdown - a null or empty string will disable the
        /// feature. Can contain \r and \n.
        /// </summary>
        private string _ShutdownText;
        public string ShutdownText
        {
            get { return _ShutdownText; }
            set { SetField(ref _ShutdownText, value, () => ShutdownText); }
        }

        /// <summary>
        /// Gets or sets the identifier of the receiever location record associated with the receiver.
        /// </summary>
        private int _ReceiverLocationId;
        public int ReceiverLocationId
        {
            get { return _ReceiverLocationId; }
            set { SetField(ref _ReceiverLocationId, value, () => ReceiverLocationId); }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if(PropertyChanged != null) PropertyChanged(this, args);
        }

        /// <summary>
        /// Sets the field's value and raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="selectorExpression"></param>
        /// <returns></returns>
        protected bool SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
        {
            if(EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;

            if(selectorExpression == null) throw new ArgumentNullException("selectorExpression");
            MemberExpression body = selectorExpression.Body as MemberExpression;
            if(body == null) throw new ArgumentException("The body must be a member expression");
            OnPropertyChanged(new PropertyChangedEventArgs(body.Member.Name));

            return true;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Receiver()
        {
            Enabled = true;
            AutoReconnectAtStartup = true;
            DataSource = DataSource.Port30003;
            ConnectionType = ConnectionType.TCP;
            Address = "127.0.0.1";
            Port = 30003;
            BaudRate = 115200;
            DataBits = 8;
            StopBits = StopBits.One;
            Parity = Parity.None;
            Handshake = Handshake.None;
            StartupText = "#43-02\\r";
            ShutdownText = "#43-00\\r";
        }
    }
}
