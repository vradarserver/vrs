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

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// The interface that all implementations of screens that support presenting
    /// and modifying lists of users must support.
    /// </summary>
    public interface IUsersView : IView, IListDetailEditorView<IUser>
    {
        /// <summary>
        /// Gets or sets the name of the user manager that's in use.
        /// </summary>
        string UserManagerName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the selected record is enabled.
        /// </summary>
        bool UserEnabled { get; set; }

        /// <summary>
        /// Gets or sets the login name from the selected record.
        /// </summary>
        string LoginName { get; set; }

        /// <summary>
        /// Gets or sets the user name from the selected record.
        /// </summary>
        string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password from the selected record.
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the control to create new records is enabled.
        /// </summary>
        bool NewEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the control to remove records is enabled.
        /// </summary>
        bool DeleteEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the control to edit the Enabled value is enabled.
        /// </summary>
        bool UserEnabledEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the control to edit the LoginName value is enabled.
        /// </summary>
        bool LoginNameEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the control to edit the UserName value is enabled.
        /// </summary>
        bool UserNameEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the control to edit the password is enabled.
        /// </summary>
        bool PasswordEnabled { get; set; }
    }
}
