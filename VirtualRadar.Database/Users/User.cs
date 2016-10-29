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
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Settings;
using System.Linq.Expressions;
using System.ComponentModel;
using VirtualRadar.Interface;

namespace VirtualRadar.Database.Users
{
    /// <summary>
    /// The default implementation of <see cref="IUser"/>.
    /// </summary>
    class User : IUser
    {
        private long _Id;
        /// <summary>
        /// Gets or sets the unique identifier of the user.
        /// </summary>
        public long Id
        {
            get { return _Id; }
            set
            {
                var oldUniqueId = UniqueId;
                var oldIsPersisted = IsPersisted;
                SetField(ref _Id, value, nameof(Id));
                if(UniqueId != oldUniqueId) {
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(UniqueId)));
                }
                if(oldIsPersisted != IsPersisted) {
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsPersisted)));
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UniqueId
        {
            get { return Id.ToString(); }
            set { Id = String.IsNullOrEmpty(value) ? 0L : long.Parse(value); }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsPersisted { get { return Id > 0; } }

        private bool _Enabled;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool Enabled
        {
            get { return _Enabled; }
            set { SetField(ref _Enabled, value, nameof(Enabled)); }
        }

        private string _LoginName;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string LoginName
        {
            get { return _LoginName; }
            set { SetField(ref _LoginName, value, nameof(LoginName)); }
        }

        private string _Name;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { SetField(ref _Name, value, nameof(Name)); }
        }

        private string _UIPassword;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UIPassword
        {
            get { return _UIPassword; }
            set { SetField(ref _UIPassword, value, nameof(UIPassword)); }
        }

        private int _PasswordHashVersion;
        /// <summary>
        /// Gets or sets the format of the hashing function used to generate the stored hash.
        /// </summary>
        public int PasswordHashVersion
        {
            get { return _PasswordHashVersion; }
            set { SetField(ref _PasswordHashVersion, value, nameof(PasswordHashVersion)); }
        }

        private byte[] _PasswordHash;
        /// <summary>
        /// Gets or sets the stored hash of the password.
        /// </summary>
        public byte[] PasswordHash
        {
            get { return _PasswordHash; }
            set { SetField(ref _PasswordHash, value, nameof(PasswordHash)); }
        }

        private DateTime _CreatedUtc;
        /// <summary>
        /// Gets or sets the date and time that the record was created.
        /// </summary>
        public DateTime CreatedUtc
        {
            get { return _CreatedUtc; }
            set { SetField(ref _CreatedUtc, value, nameof(CreatedUtc)); }
        }

        private DateTime _UpdatedUtc;
        /// <summary>
        /// Gets or sets the date and time that the record was updated.
        /// </summary>
        public DateTime UpdatedUtc
        {
            get { return _UpdatedUtc; }
            set { SetField(ref _UpdatedUtc, value, nameof(UpdatedUtc)); }
        }

        private Hash _Hash;
        /// <summary>
        /// Gets the password hash object.
        /// </summary>
        public Hash Hash
        {
            get
            {
                if(_Hash == null) {
                    _Hash = new Hash(PasswordHashVersion);
                    _Hash.Buffer.AddRange(PasswordHash);
                }
                return _Hash;
            }
        }

        private object _Tag;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public object Tag
        {
            get { return _Tag; }
            set { SetField(ref _Tag, value, nameof(Tag)); }
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
    }
}
