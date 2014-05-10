using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The interface that describes a user.
    /// </summary>
    public interface IUser
    {
        /// <summary>
        /// Gets or sets the user's unique identifier. These are recorded in the
        /// configuration, it cannot change between sessions.
        /// </summary>
        string UniqueId { get; set; }

        /// <summary>
        /// Gets a value indicating that this record has been persisted to the
        /// store of users, or has been read from the store of users.
        /// </summary>
        bool IsPersisted { get; }

        /// <summary>
        /// Gets or sets a value indicating that the user account is enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the user's login name.
        /// </summary>
        string LoginName { get; set; }

        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the password as entered at the user interface.
        /// </summary>
        /// <remarks>
        /// This is only ever filled by the user interface - the IUserManager must not store
        /// passwords or return passwords for loaded users.
        /// </remarks>
        string UIPassword { get; set; }
    }
}
