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
using System.Linq;
using System.Text;
using VirtualRadar.Interface;

namespace VirtualRadar.Plugin.BaseStationDatabaseWriter.WebAdmin
{
    /// <summary>
    /// Carries the view's properties to the web page.
    /// </summary>
    public class ViewModel
    {
        public bool PluginEnabled { get; set; }

        public bool AllowUpdateOfOtherDatabases { get; set; }

        public string DatabaseFileName { get; set; }

        public int ReceiverId { get; set; }

        public bool SaveDownloadedAircraftDetails { get; set; }

        public bool RefreshOutOfDateAircraft { get; set; }

        public IList<CombinedFeed> CombinedFeeds { get; private set; }

        public string OnlineLookupWriteActionNotice { get; set; }

        public ViewModel()
        {
            CombinedFeeds = new List<CombinedFeed>();
        }

        public ViewModel(OptionsView view) : this()
        {
            RefreshFromView(view);
        }

        public void RefreshFromView(OptionsView view)
        {
            PluginEnabled =                 view.PluginEnabled;
            AllowUpdateOfOtherDatabases =   view.AllowUpdateOfOtherDatabases;
            DatabaseFileName =              view.DatabaseFileName;
            ReceiverId =                    view.ReceiverId;
            SaveDownloadedAircraftDetails = view.SaveDownloadedAircraftDetails;
            RefreshOutOfDateAircraft =      view.RefreshOutOfDateAircraft;
            OnlineLookupWriteActionNotice = view.OnlineLookupWriteActionNotice;

            CollectionHelper.OverwriteDestinationWithSource(view.CombinedFeeds, CombinedFeeds);
        }

        public void CopyToView(OptionsView view)
        {
            view.PluginEnabled =                    PluginEnabled;
            view.AllowUpdateOfOtherDatabases =      AllowUpdateOfOtherDatabases;
            view.DatabaseFileName =                 DatabaseFileName;
            view.ReceiverId =                       ReceiverId;
            view.SaveDownloadedAircraftDetails =    SaveDownloadedAircraftDetails;
            view.RefreshOutOfDateAircraft =         RefreshOutOfDateAircraft;
            view.OnlineLookupWriteActionNotice =    OnlineLookupWriteActionNotice;
        }
    }

    public class CreateDatabaseOutcomeModel
    {
        public string Title { get; set; }

        public string Message { get; set; }

        public ViewModel ViewModel { get; set; }

        public CreateDatabaseOutcomeModel(string title, string message, ViewModel viewModel)
        {
            Title = title;
            Message = message;
            ViewModel = ViewModel;
        }
    }
}
