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
        private bool _Enabled;
        /// <summary>
        /// Gets or sets a value indicating that the receiver is to be used.
        /// </summary>
        public bool Enabled
        {
            get { return _Enabled; }
            set { SetField(ref _Enabled, value, nameof(Enabled)); }
        }

        private int _UniqueId;
        /// <summary>
        /// Gets or sets a unique identifier for the receiever. This is unique across receivers and merged feeds. It cannot be zero.
        /// </summary>
        public int UniqueId
        {
            get { return _UniqueId; }
            set { SetField(ref _UniqueId, value, nameof(UniqueId)); }
        }

        private string _Name;
        /// <summary>
        /// Gets or sets the unique name of the receiver.
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { SetField(ref _Name, value, nameof(Name)); }
        }

        private string _DataSource;
        /// <summary>
        /// Gets or sets the unique ID of the IReceiverFormatProvider that will handle
        /// the feed from the receiever.
        /// </summary>
        public string DataSource
        {
            get { return _DataSource; }
            set { SetField(ref _DataSource, value, nameof(DataSource)); }
        }

        private ConnectionType _ConnectionType;
        /// <summary>
        /// Gets or sets the mechanism to use to connect to the data source.
        /// </summary>
        public ConnectionType ConnectionType
        {
            get { return _ConnectionType; }
            set { SetField(ref _ConnectionType, value, nameof(ConnectionType)); }
        }

        private bool _AutoReconnectAtStartup;
        /// <summary>
        /// Obsolete since version 2.0.3. Network connections now always reconnect until you disable them.
        /// </summary>
        public bool AutoReconnectAtStartup
        {
            get { return _AutoReconnectAtStartup; }
            set { SetField(ref _AutoReconnectAtStartup, value, nameof(AutoReconnectAtStartup)); }
        }

        private bool _IsSatcomFeed;
        /// <summary>
        /// Gets or sets a value indicating that the feed is coming off a satellite.
        /// </summary>
        /// <remarks>
        /// Aero Satcom feeds have much longer intervals between transmissions so they are subject to a
        /// separate set of display and tracking timeouts.
        /// </remarks>
        public bool IsSatcomFeed
        {
            get { return _IsSatcomFeed; }
            set { SetField(ref _IsSatcomFeed, value, nameof(IsSatcomFeed)); }
        }

        private bool _IsPassive;
        /// <summary>
        /// Gets or sets a value indicating that the receiver will not connect to the source but will instead
        /// wait for the source to connect to it.
        /// </summary>
        /// <remarks>
        /// Only used when the <see cref="ConnectionType"/> is TCP. When a receiver is in passive mode the 
        /// <see cref="Address"/> is ignored. Only one source can connect to a passive receiver at a time,
        /// once a connection is established the receiver stops listening.
        /// </remarks>
        public bool IsPassive
        {
            get { return _IsPassive; }
            set { SetField(ref _IsPassive, value, nameof(IsPassive)); }
        }

        private Access _Access;
        /// <summary>
        /// Gets or sets the access settings to use when the receiver is in passive mode.
        /// </summary>
        public Access Access
        {
            get { return _Access; }
            set { SetField(ref _Access, value, nameof(Access)); }
        }

        private string _Address;
        /// <summary>
        /// Gets or sets the address of the source of data to listen to.
        /// </summary>
        public string Address
        {
            get { return _Address; }
            set { SetField(ref _Address, value, nameof(Address)); }
        }

        private int _Port;
        /// <summary>
        /// Gets or sets the port of the source of data to listen to.
        /// </summary>
        public int Port
        {
            get { return _Port; }
            set { SetField(ref _Port, value, nameof(Port)); }
        }

        private bool _UseKeepAlive;
        /// <summary>
        /// Gets or sets a value indicating that the network connection should use KeepAlive packets.
        /// </summary>
        public bool UseKeepAlive
        {
            get { return _UseKeepAlive; }
            set { SetField(ref _UseKeepAlive, value, nameof(UseKeepAlive)); }
        }

        private int _IdleTimeoutMilliseconds;
        /// <summary>
        /// Gets or sets the period of time that must elapse with no received content before the network
        /// connection is reset.
        /// </summary>
        public int IdleTimeoutMilliseconds
        {
            get { return _IdleTimeoutMilliseconds; }
            set { SetField(ref _IdleTimeoutMilliseconds, value, nameof(IdleTimeoutMilliseconds)); }
        }

        private string _Passphrase;
        /// <summary>
        /// Gets or sets the passphrase that the other side is expecting us to send in order to authenticate.
        /// </summary>
        /// <remarks>
        /// If this is null or empty then no passphrase is sent.
        /// </remarks>
        public string Passphrase
        {
            get { return _Passphrase; }
            set { SetField(ref _Passphrase, value, nameof(Passphrase)); }
        }

        private string _ComPort;
        /// <summary>
        /// Gets or sets the COM port to listen to.
        /// </summary>
        public string ComPort
        {
            get { return _ComPort; }
            set { SetField(ref _ComPort, value, nameof(ComPort)); }
        }

        private int _BaudRate;
        /// <summary>
        /// Gets or sets the baud rate to use.
        /// </summary>
        public int BaudRate
        {
            get { return _BaudRate; }
            set { SetField(ref _BaudRate, value, nameof(BaudRate)); }
        }

        private int _DataBits;
        /// <summary>
        /// Gets or sets the data bits to use.
        /// </summary>
        public int DataBits
        {
            get { return _DataBits; }
            set { SetField(ref _DataBits, value, nameof(DataBits)); }
        }

        private StopBits _StopBits;
        /// <summary>
        /// Gets or sets the stop bits to use.
        /// </summary>
        public StopBits StopBits
        {
            get { return _StopBits; }
            set { SetField(ref _StopBits, value, nameof(StopBits)); }
        }

        private Parity _Parity;
        /// <summary>
        /// Gets or sets the parity to use.
        /// </summary>
        public Parity Parity
        {
            get { return _Parity; }
            set { SetField(ref _Parity, value, nameof(Parity)); }
        }

        private Handshake _Handshake;
        /// <summary>
        /// Gets or sets the handshake protocol to use.
        /// </summary>
        public Handshake Handshake
        {
            get { return _Handshake; }
            set { SetField(ref _Handshake, value, nameof(Handshake)); }
        }

        private string _StartupText;
        /// <summary>
        /// Gets or sets the text to send across the COM port on startup - a null or empty string will disable the
        /// feature. Can contain \r and \n.
        /// </summary>
        public string StartupText
        {
            get { return _StartupText; }
            set { SetField(ref _StartupText, value, nameof(StartupText)); }
        }

        private string _ShutdownText;
        /// <summary>
        /// Gets or sets the text to send across the COM port on shutdown - a null or empty string will disable the
        /// feature. Can contain \r and \n.
        /// </summary>
        public string ShutdownText
        {
            get { return _ShutdownText; }
            set { SetField(ref _ShutdownText, value, nameof(ShutdownText)); }
        }

        private string _WebAddress;
        /// <summary>
        /// Gets or sets the HTTP address to fetch data from.
        /// </summary>
        public string WebAddress
        {
            get { return _WebAddress; }
            set { SetField(ref _WebAddress, value, nameof(WebAddress)); }
        }

        private int _FetchIntervalMillseconds;
        /// <summary>
        /// Gets or sets the interval between fetches in milliseconds.
        /// </summary>
        public int FetchIntervalMilliseconds
        {
            get { return _FetchIntervalMillseconds; }
            set { SetField(ref _FetchIntervalMillseconds, value, nameof(FetchIntervalMilliseconds)); }
        }

        private int _ReceiverLocationId;
        /// <summary>
        /// Gets or sets the identifier of the receiever location record associated with the receiver.
        /// </summary>
        public int ReceiverLocationId
        {
            get { return _ReceiverLocationId; }
            set { SetField(ref _ReceiverLocationId, value, nameof(ReceiverLocationId)); }
        }

        private ReceiverUsage _ReceiverUsage;
        /// <summary>
        /// Gets or sets a value indicating how the receiver should be used by the system.
        /// </summary>
        public ReceiverUsage ReceiverUsage
        {
            get { return _ReceiverUsage; }
            set { SetField(ref _ReceiverUsage, value, nameof(ReceiverUsage)); }
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
            var handler = PropertyChanged;
            if(handler != null) {
                handler(this, args);
            }
        }

        /// <summary>
        /// Sets the field's value and raises <see cref="PropertyChanged"/>, but only when the value has changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="fieldName"></param>
        /// <returns>True if the value was set because it had changed, false if the value did not change and the event was not raised.</returns>
        protected bool SetField<T>(ref T field, T value, string fieldName)
        {
            var result = !EqualityComparer<T>.Default.Equals(field, value);
            if(result) {
                field = value;
                OnPropertyChanged(new PropertyChangedEventArgs(fieldName));
            }

            return result;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Receiver()
        {
            Enabled = true;
            AutoReconnectAtStartup = true;
            DataSource = VirtualRadar.Interface.Settings.DataSource.Port30003;
            ConnectionType = ConnectionType.TCP;
            Access = new Access();
            Address = "127.0.0.1";
            Port = 30003;
            UseKeepAlive = true;
            IdleTimeoutMilliseconds = 60000;
            BaudRate = 115200;
            DataBits = 8;
            StopBits = StopBits.One;
            Parity = Parity.None;
            Handshake = Handshake.None;
            StartupText = "#43-02\\r";
            ShutdownText = "#43-00\\r";
            FetchIntervalMilliseconds = 1000;
        }

        /// <summary>
        /// See base class docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name ?? "<no name>";
        }
    }
}
