// Copyright © 2010 onwards, Andrew Whewell
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
using System.Text;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Plugin.BaseStationDatabaseWriter
{
    /// <summary>
    /// The interface that options views have to implement.
    /// </summary>
    public interface IOptionsView : IView, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets a value indicating that the plugin has been switched on by the user.
        /// </summary>
        bool PluginEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the plugin is allowed to update databases that it did not create.
        /// </summary>
        bool AllowUpdateOfOtherDatabases { get; set; }

        /// <summary>
        /// Gets or sets the name of the database file that the plugin will write to.
        /// </summary>
        string DatabaseFileName { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the receiver that the plugin will listen to.
        /// </summary>
        int ReceiverId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the downloaded aircraft details should be saved to BaseStation.sqb.
        /// </summary>
        bool SaveDownloadedAircraftDetails { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that details for out-of-date aircraft should be fetched and saved to the database.
        /// </summary>
        bool RefreshOutOfDateAircraft { get; set; }

        /// <summary>
        /// Gets a list of combined feeds to show to the user.
        /// </summary>
        IList<CombinedFeed> CombinedFeeds { get; }

        /// <summary>
        /// Gets or sets a notice that it shown to users that describes what aircraft lookup information, if any, will be
        /// written to BaseStation.sqb by the plugin.
        /// </summary>
        string OnlineLookupWriteActionNotice { get; set; }

        /// <summary>
        /// Raised when the user wants to save their changes to the options.
        /// </summary>
        event EventHandler SaveClicked;

        /// <summary>
        /// Raised when the user clicks the 'Use default filename' button.
        /// </summary>
        event EventHandler UseDefaultFileNameClicked;

        /// <summary>
        /// Raised when the user clicks the 'Create database' button.
        /// </summary>
        event EventHandler CreateDatabaseClicked;

        /// <summary>
        /// Reports the outcome of an attempt to create a database to the user.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        void ShowCreateDatabaseOutcome(string message, string title);
    }
}
