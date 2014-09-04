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
using System.ComponentModel;
using System.Linq.Expressions;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The settings for the rebroadcast server.
    /// </summary>
    [Serializable]
    public class RebroadcastSettings : INotifyPropertyChanged
    {
        private int _UniqueId;
        /// <summary>
        /// Gets or sets the unique identifier for the server. This cannot be zero.
        /// </summary>
        public int UniqueId
        {
            get { return _UniqueId; }
            set { SetField(ref _UniqueId, value, () => UniqueId); }
        }

        private string _Name;
        /// <summary>
        /// Gets or sets the unique name for the server.
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { SetField(ref _Name, value, () => Name); }
        }

        private bool _Enabled;
        /// <summary>
        /// Gets or sets a value indicating whether the rebroadcast server is enabled.
        /// </summary>
        public bool Enabled
        {
            get { return _Enabled; }
            set { SetField(ref _Enabled, value, () => Enabled); }
        }

        private int _ReceiverId;
        /// <summary>
        /// Gets or sets the receiver that the rebroadcast server will rebroadcast.
        /// </summary>
        public int ReceiverId
        {
            get { return _ReceiverId; }
            set { SetField(ref _ReceiverId, value, () => ReceiverId); }
        }

        private RebroadcastFormat _Format;
        /// <summary>
        /// Gets or sets the format in which to rebroadcast the receiver's messages.
        /// </summary>
        public RebroadcastFormat Format
        {
            get { return _Format; }
            set { SetField(ref _Format, value, () => Format); }
        }

        private bool _IsTransmitter;
        /// <summary>
        /// Gets or sets a value that indicates whether the rebroadcast server is active (it
        /// transmits the feed to another machine) or passive (it accepts multiple connections
        /// from other machines).
        /// </summary>
        public bool IsTransmitter
        {
            get { return _IsTransmitter; }
            set { SetField(ref _IsTransmitter, value, () => IsTransmitter); }
        }

        private string _TransmitAddress;
        /// <summary>
        /// Gets or sets the address of the machine to send the feed to in Transmitter mode.
        /// </summary>
        public string TransmitAddress
        {
            get { return _TransmitAddress; }
            set { SetField(ref _TransmitAddress, value, () => TransmitAddress); }
        }

        private int _Port;
        /// <summary>
        /// Gets or sets the port number to rebroadcast the receiver's messages on or the
        /// port to transmit the feed to.
        /// </summary>
        public int Port
        {
            get { return _Port; }
            set { SetField(ref _Port, value, () => Port); }
        }

        private bool _UseKeepAlive;
        /// <summary>
        /// Gets or sets a value indicating that the network connection should use KeepAlive packets.
        /// </summary>
        public bool UseKeepAlive
        {
            get { return _UseKeepAlive; }
            set { SetField(ref _UseKeepAlive, value, () => UseKeepAlive); }
        }

        private int _IdleTimeoutMilliseconds;
        /// <summary>
        /// Gets or sets the period of time that the receiving side has to accept a message within before
        /// the connection is dropped.
        /// </summary>
        public int IdleTimeoutMilliseconds
        {
            get { return _IdleTimeoutMilliseconds; }
            set { SetField(ref _IdleTimeoutMilliseconds, value, () => IdleTimeoutMilliseconds); }
        }

        private int _StaleSeconds;
        /// <summary>
        /// Gets or sets the threshold for discarding buffered messages that are too old.
        /// </summary>
        public int StaleSeconds
        {
            get { return _StaleSeconds; }
            set { SetField(ref _StaleSeconds, value, () => StaleSeconds); }
        }

        private Access _Access;
        /// <summary>
        /// Gets or sets the access settings for the rebroadcast server.
        /// </summary>
        public Access Access
        {
            get { return _Access; }
            set { SetField(ref _Access, value, () => Access); }
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
        public RebroadcastSettings()
        {
            Access = new Access();
            StaleSeconds = 3;
            IdleTimeoutMilliseconds = 30000;
        }
    }
}
