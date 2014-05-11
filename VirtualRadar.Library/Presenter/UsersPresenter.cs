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
    /// The default implementation for <see cref="IUsersPresenter"/>.
    /// </summary>
    class UsersPresenter : ListDetailEditorPresenter<IUsersView, IUser>, IUsersPresenter
    {
        private static readonly string _PasswordPlaceholder = "\t\t\t\t\t\t\t\t";

        public override void Initialise(IUsersView view)
        {
            base.Initialise(view);

            var userManager = Factory.Singleton.Resolve<IUserManager>().Singleton;
            view.UserManagerName = userManager.Name;

            if(!userManager.CanChangeEnabledState)  view.UserEnabledEnabled = false;
            if(!userManager.CanChangePassword)      view.PasswordEnabled = false;
            if(!userManager.CanCreateUsers)         view.NewEnabled = false;
            if(!userManager.CanDeleteUsers)         view.DeleteEnabled = false;
            if(!userManager.CanEditUsers)           view.LoginNameEnabled = view.UserNameEnabled = false;
        }

        protected override List<IUser> DoLoadRecords()
        {
            var result = new List<IUser>();

            var userManager = Factory.Singleton.Resolve<IUserManager>().Singleton;
            var users = userManager.CanListUsers ? userManager.GetUsers() : null;
            if(users != null) result.AddRange(users);

            foreach(var user in result) {
                user.UIPassword = _PasswordPlaceholder;
            }

            return result;
        }

        protected override void DoSaveRecords(List<IUser> records)
        {
            var userManager = Factory.Singleton.Resolve<IUserManager>().Singleton;
            if(userManager.CanListUsers) {
                var existing = userManager.GetUsers();
                var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
                var configuration = configurationStorage.Load();

                // Update existing records
                foreach(var record in records) {
                    var current = existing.FirstOrDefault(r => r.UniqueId == record.UniqueId);
                    if(current != null) {
                        if(!userManager.CanEditUsers) {
                            record.Name = current.Name;
                            record.LoginName = current.LoginName;
                        }
                        if(!userManager.CanChangeEnabledState) record.Enabled = current.Enabled;
                        if(!userManager.CanChangePassword) record.UIPassword = _PasswordPlaceholder;

                        if(record.Enabled !=    current.Enabled ||
                           record.LoginName !=  current.LoginName ||
                           record.Name !=       current.Name ||
                           record.UIPassword != _PasswordPlaceholder
                        ) {
                            userManager.UpdateUser(record, record.UIPassword == _PasswordPlaceholder ? null : record.UIPassword);
                        }
                    }
                }

                // Insert new records
                if(userManager.CanCreateUsers) {
                    foreach(var record in records.Where(r => !r.IsPersisted)) {
                        if(record.UIPassword == _PasswordPlaceholder) record.UIPassword = null;
                        userManager.CreateUser(record);
                        record.UIPassword = _PasswordPlaceholder;
                    }
                }

                // Delete missing records
                if(userManager.CanDeleteUsers) {
                    foreach(var missing in existing.Where(r => !records.Any(i => i.IsPersisted && i.UniqueId == r.UniqueId))) {
                        userManager.DeleteUser(missing);

                        var webServerIndex = configuration.WebServerSettings.BasicAuthenticationUserIds.IndexOf(missing.UniqueId);
                        if(webServerIndex != -1) configuration.WebServerSettings.BasicAuthenticationUserIds.RemoveAt(webServerIndex);
                    }
                }

                // Save the configuration to force any cached users to be reloaded, and to save any changes we've made
                configurationStorage.Save(configuration);
            }
        }

        protected override IUser DoCreateNewRecord()
        {
            var result = Factory.Singleton.Resolve<IUser>();
            result.Enabled = true;

            return result;
        }

        protected override void DoCopyRecordToEditFields(IUser record)
        {
            _View.UserEnabled = record == null ? false : record.Enabled;
            _View.LoginName =   record == null ? "" : record.LoginName ?? "";
            _View.UserName =    record == null ? "" : record.Name ?? "";
            _View.Password =    record == null ? "" : record.UIPassword ?? "";
        }

        protected override void DoCopyEditFieldsToRecord(IUser record)
        {
            record.Enabled =        _View.UserEnabled;
            record.LoginName =      _View.LoginName;
            record.Name =           _View.UserName;
            record.UIPassword =     _View.Password;
        }

        protected override bool? DoValidation(List<ValidationResult> results, IUser record, IUser currentRecord)
        {
            var userManager = Factory.Singleton.Resolve<IUserManager>().Singleton;
            userManager.ValidateUser(results, record, currentRecord, _View.Records);

            return null;
        }
    }
}
