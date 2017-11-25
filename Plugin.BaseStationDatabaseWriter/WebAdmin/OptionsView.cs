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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Plugin.BaseStationDatabaseWriter.WebAdmin
{
    public class OptionsView : IOptionsView
    {
        private OptionsPresenter _Presenter;
        private ViewModel _ViewModel;
        private string _CreateDatabaseOutcomeTitle;
        private string _CreateDatabaseOutcomeMessage;

        private bool _PluginEnabled;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool PluginEnabled
        {
            get { return _PluginEnabled; }
            set { SetField(ref _PluginEnabled, value, nameof(PluginEnabled)); }
        }

        private bool _AllowUpdateOfOtherDatabases;
        public bool AllowUpdateOfOtherDatabases
        {
            get { return _AllowUpdateOfOtherDatabases; }
            set { SetField(ref _AllowUpdateOfOtherDatabases, value, nameof(AllowUpdateOfOtherDatabases)); }
        }

        private string _DatabaseFileName;
        public string DatabaseFileName
        {
            get { return _DatabaseFileName; }
            set { SetField(ref _DatabaseFileName, value, nameof(DatabaseFileName)); }
        }

        private int _ReceiverId;
        public int ReceiverId
        {
            get { return _ReceiverId; }
            set { SetField(ref _ReceiverId, value, nameof(ReceiverId)); }
        }

        private bool _SaveDownloadedAircraftDetails;
        public bool SaveDownloadedAircraftDetails
        {
            get { return _SaveDownloadedAircraftDetails; }
            set { SetField(ref _SaveDownloadedAircraftDetails, value, nameof(SaveDownloadedAircraftDetails)); }
        }

        private bool _RefreshOutOfDateAircraft;
        public bool RefreshOutOfDateAircraft
        {
            get { return _RefreshOutOfDateAircraft; }
            set { SetField(ref _RefreshOutOfDateAircraft, value, nameof(RefreshOutOfDateAircraft)); }
        }

        private List<CombinedFeed> _CombinedFeeds = new List<CombinedFeed>();
        public IList<CombinedFeed> CombinedFeeds
        {
            get { return _CombinedFeeds; }
        }

        private string _OnlineLookupWriteActionNotice;
        public string OnlineLookupWriteActionNotice
        {
            get { return _OnlineLookupWriteActionNotice; }
            set { SetField(ref _OnlineLookupWriteActionNotice, value, nameof(OnlineLookupWriteActionNotice)); }
        }

        public event EventHandler SaveClicked;
        protected virtual void OnSaveClicked(EventArgs args)
        {
            EventHelper.Raise(SaveClicked, this, args);
        }

        public event EventHandler UseDefaultFileNameClicked;
        protected virtual void OnUseDefaultFileNameClicked(EventArgs args)
        {
            EventHelper.Raise(UseDefaultFileNameClicked, this, args);
        }

        public event EventHandler CreateDatabaseClicked;
        protected virtual void OnCreateDatabaseClicked(EventArgs args)
        {
            EventHelper.Raise(CreateDatabaseClicked, this, args);
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

        public DialogResult ShowView()
        {
            _ViewModel = new ViewModel(this);
            _Presenter = new OptionsPresenter();
            _Presenter.Initialise(this);

            return DialogResult.OK;
        }

        public void Dispose()
        {
            ;
        }

        public void ShowCreateDatabaseOutcome(string message, string title)
        {
            _CreateDatabaseOutcomeMessage = message;
            _CreateDatabaseOutcomeTitle = title;
        }

        [WebAdminMethod]
        public ViewModel GetState()
        {
            _ViewModel.RefreshFromView(this);
            return _ViewModel;
        }

        [WebAdminMethod(/*DeferExecution=true*/)]
        public SaveOutcomeModel Save(ViewModel viewModel)
        {
            var outcome = "";

            viewModel.CopyToView(this);
            try {
                OnSaveClicked(EventArgs.Empty);
                outcome = "Saved";
            } catch(ConflictingUpdateException) {
                outcome = "ConflictingUpdate";
                _Presenter.ReloadOptions();
            }
            _ViewModel.RefreshFromView(this);

            return new SaveOutcomeModel(outcome, _ViewModel);
        }

        [WebAdminMethod]
        public CreateDatabaseOutcomeModel CreateDatabase(ViewModel viewModel)
        {
            _CreateDatabaseOutcomeTitle = "";
            _CreateDatabaseOutcomeMessage = "";

            viewModel.CopyToView(this);
            OnCreateDatabaseClicked(EventArgs.Empty);
            _ViewModel.RefreshFromView(this);

            return new CreateDatabaseOutcomeModel(_CreateDatabaseOutcomeTitle, _CreateDatabaseOutcomeMessage, _ViewModel);
        }

        [WebAdminMethod]
        public ViewModel UseDefaultFileName(ViewModel viewModel)
        {
            viewModel.CopyToView(this);
            OnUseDefaultFileNameClicked(EventArgs.Empty);
            _ViewModel.RefreshFromView(this);

            return _ViewModel;
        }
    }
}
