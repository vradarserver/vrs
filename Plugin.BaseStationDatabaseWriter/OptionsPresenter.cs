// Copyright © 2016 onwards, Andrew Whewell
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
using System.IO;
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Plugin.BaseStationDatabaseWriter
{
    /// <summary>
    /// Handles the business logic for the options view.
    /// </summary>
    public class OptionsPresenter
    {
        /// <summary>
        /// The view that is being controlled by the presenter.
        /// </summary>
        private IOptionsView _View;

        /// <summary>
        /// The options that are being edited by the view.
        /// </summary>
        private Options _Options;

        /// <summary>
        /// The configuration that is being edited by the view (the plugin does not hold its own copy of the
        /// database filename, it shares it with the configuration screens).
        /// </summary>
        private Configuration _Configuration;

        /// <summary>
        /// Initialises the presenter and the view.
        /// </summary>
        /// <param name="view"></param>
        public void Initialise(IOptionsView view)
        {
            _View = view;

            ReloadOptions();

            _View.CreateDatabaseClicked += View_CreateDatabaseClicked;
            _View.PropertyChanged += View_PropertyChanged;
            _View.SaveClicked += View_SaveClicked;
            _View.UseDefaultFileNameClicked += View_UseDefaultFileNameClicked;
        }

        public void ReloadOptions()
        {
            var optionsStorage = new OptionsStorage();
            _Options = optionsStorage.Load();

            var configurationStorage = Factory.ResolveSingleton<IConfigurationStorage>();
            _Configuration = configurationStorage.Load();

            _View.CombinedFeeds.Clear();
            _View.CombinedFeeds.AddRange(
                _Configuration.Receivers.Select(r =>   new CombinedFeed() { UniqueId = r.UniqueId, Name = r.Name })
                .Concat(_Configuration.MergedFeeds.Select(r => new CombinedFeed() { UniqueId = r.UniqueId, Name = r.Name }))
            );

            _View.PluginEnabled =                    _Options.Enabled;
            _View.AllowUpdateOfOtherDatabases =      _Options.AllowUpdateOfOtherDatabases;
            _View.DatabaseFileName =                 _Configuration.BaseStationSettings.DatabaseFileName;
            _View.ReceiverId =                       _Options.ReceiverId;
            _View.SaveDownloadedAircraftDetails =    _Options.SaveDownloadedAircraftDetails;
            _View.RefreshOutOfDateAircraft =         _Options.RefreshOutOfDateAircraft;

            RefreshWriteNotice();
        }

        /// <summary>
        /// Updates the information shown on the view about what the plugin will do regarding saving online
        /// aircraft lookup details.
        /// </summary>
        private void RefreshWriteNotice()
        {
            _View.OnlineLookupWriteActionNotice = _View.RefreshOutOfDateAircraft ? PluginStrings.WriteOnlineLookupsNoticeAllAircraft : PluginStrings.WriteOnlineLookupsNoticeJustNew;
        }

        /// <summary>
        /// Called when a property on the view changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if(args.PropertyName == PropertyHelper.ExtractName<IOptionsView>(r => r.RefreshOutOfDateAircraft)) {
                RefreshWriteNotice();
            }
        }

        /// <summary>
        /// Called when the user indicates that they want to use the default filename.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_UseDefaultFileNameClicked(object sender, EventArgs args)
        {
            var configurationStorage = Factory.ResolveSingleton<IConfigurationStorage>();
            _View.DatabaseFileName = Path.Combine(configurationStorage.Folder, "BaseStation.sqb");
        }

        /// <summary>
        /// Called when the user indicates that they want to create a database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_CreateDatabaseClicked(object sender, EventArgs args)
        {
            var databaseFileName = _View.DatabaseFileName;

            if(!String.IsNullOrEmpty(databaseFileName)) {
                bool fileExists = File.Exists(databaseFileName);
                bool zeroLength = fileExists && new FileInfo(databaseFileName).Length == 0;
                if(fileExists && !zeroLength) {
                    _View.ShowCreateDatabaseOutcome(PluginStrings.DatabaseFileAlreadyExists, PluginStrings.CannotCreateDatabaseFileTitle);
                } else {
                    var databaseService = Factory.Resolve<IBaseStationDatabase>();
                    databaseService.CreateDatabaseIfMissing(_View.DatabaseFileName);

                    _View.ShowCreateDatabaseOutcome(String.Format(PluginStrings.CreatedDatabaseFile, databaseFileName), PluginStrings.CreatedDatabaseFileTitle);
                }
            }
        }

        /// <summary>
        /// Called when the user indicates that they want to save their changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_SaveClicked(object sender, EventArgs args)
        {
            _Options.Enabled =                          _View.PluginEnabled;
            _Options.AllowUpdateOfOtherDatabases =      _View.AllowUpdateOfOtherDatabases;
            _Options.ReceiverId =                       _View.ReceiverId;
            _Options.SaveDownloadedAircraftDetails =    _View.SaveDownloadedAircraftDetails;
            _Options.RefreshOutOfDateAircraft =         _View.RefreshOutOfDateAircraft;

            var configurationStorage = Factory.ResolveSingleton<IConfigurationStorage>();
            _Configuration.BaseStationSettings.DatabaseFileName = _View.DatabaseFileName;
            configurationStorage.Save(_Configuration);

            var optionsStorage = new OptionsStorage();
            optionsStorage.Save(_Options);
        }
    }
}
