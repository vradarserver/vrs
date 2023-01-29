// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Database.SQLite.Users
{
    /// <summary>
    /// The default implementation of <see cref="IUser"/>.
    /// </summary>
    class User : IUser
    {
        /// <summary>
        /// Gets or sets the unique identifier of the user.
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UniqueId
        {
            get => Id.ToString();
            set {
                long.TryParse(value, out var parsed);
                Id = parsed;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsPersisted => Id > 0;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        [Required]
        public string LoginName { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        [NotMapped]
        public string UIPassword { get; set; }

        /// <summary>
        /// Gets or sets the format of the hashing function used to generate the stored hash.
        /// </summary>
        [Required]
        public int PasswordHashVersion { get; set; }

        /// <summary>
        /// Gets or sets the stored hash of the password.
        /// </summary>
        [Required]
        public byte[] PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the record was created.
        /// </summary>
        [Column("Created")]
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the record was updated.
        /// </summary>
        [Column("Updated")]
        public DateTime UpdatedUtc { get; set; }

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

        /// <summary>
        /// See interface docs.
        /// </summary>
        [NotMapped]
        public object Tag { get; set; }
    }
}
