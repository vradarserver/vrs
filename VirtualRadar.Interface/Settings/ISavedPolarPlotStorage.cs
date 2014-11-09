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
using VirtualRadar.Interface.Listener;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// Manages the loading and saving of polar plots.
    /// </summary>
    public interface ISavedPolarPlotStorage : ISingleton<ISavedPolarPlotStorage>
    {
        /// <summary>
        /// Uses the FeedManager to retrieve all feeds and saves polar plots for those that have them.
        /// </summary>
        void Save();

        /// <summary>
        /// Saves the polar plot for a single feed.
        /// </summary>
        /// <param name="feed"></param>
        void Save(IFeed feed);

        /// <summary>
        /// Loads the saved polar plot for a feed. If there is no saved polar plot, or if the feed's properties
        /// have changed since the plot was last saved, then null is returned.
        /// </summary>
        /// <param name="feed"></param>
        /// <returns></returns>
        SavedPolarPlot Load(IFeed feed);
    }
}
