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
        /// Gets or sets the user's unique identifier.
        /// </summary>
        /// <remarks>
        /// The Unique ID should ideally be a value that you can guarantee will only
        /// be assigned to a single user and, if that user is deleted, will not be
        /// re-used for another user. However, if your user repository cannot make
        /// that guarantee then it would be acceptable to return the LoginName here,
        /// there would be side-effects but they would not be too surprising.
        /// </remarks>
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
        /// Gets or sets the user's login name. This should be unique to a user.
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
        /// This is only ever filled by the user interface - the IUserManager should not store
        /// passwords and it must never return passwords when loading users.
        /// </remarks>
        string UIPassword { get; set; }
    }
}
