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
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.Settings;
using InterfaceFactory;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// The default implementation of <see cref="IUsersListPresenter"/>.
    /// </summary>
    class UsersListPresenter : Presenter<IUsersListView>, IUsersListPresenter
    {
        #region Fields
        private IUserManager _UserManager;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanListAllUsers { get { return _UserManager == null ? false : _UserManager.CanListUsers; } }
        #endregion

        #region Initialise
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="view"></param>
        public override void Initialise(IUsersListView view)
        {
            base.Initialise(view);

            _UserManager = Factory.Singleton.Resolve<IUserManager>().Singleton;
        }
        #endregion

        #region GetUserList, GetUserByLoginName
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public List<IUser> GetUserList()
        {
            var result = new List<IUser>();

            if(_UserManager != null) {
                var busyState = _View.ShowBusy(true, null);
                try {
                    var users = _UserManager.CanListUsers ? _UserManager.GetUsers() : _UserManager.GetUsersByUniqueId(_View.UserIds);
                    result.AddRange(users.OrderBy(r => r.LoginName));
                } finally {
                    _View.ShowBusy(false, busyState);
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public IUser GetUserByLoginName(string loginName)
        {
            IUser result = null;

            if(_UserManager != null) result = _UserManager.GetUserByLoginName(loginName);

            return result;
        }
        #endregion
    }
}
