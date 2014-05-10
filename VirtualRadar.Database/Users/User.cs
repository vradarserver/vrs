using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Database.Users
{
    /// <summary>
    /// The default implementation of <see cref="IUser"/>.
    /// </summary>
    class User : IUser
    {
        /// <summary>
        /// Gets or sets the unique identifier of the user.
        /// </summary>
        public long Id { get; set; }

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

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UIPassword { get; set; }

        /// <summary>
        /// Gets or sets the format of the hashing function used to generate the stored hash.
        /// </summary>
        public int PasswordHashVersion { get; set; }

        /// <summary>
        /// Gets or sets the stored hash of the password.
        /// </summary>
        public byte[] PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the record was created.
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the record was updated.
        /// </summary>
        public DateTime UpdatedUtc { get; set; }
    }
}
