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
using System.Text;
using InterfaceFactory;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The interface for objects that can load and save <see cref="Configuration"/> objects.
    /// </summary>
    /// <example>
    /// To read all of the application settings in one go:
    /// <code>
    /// IConfigurationStorage storage = Factory.Resolve&lt;IConfigurationStorage&gt;().Singleton;
    /// Configuration configuration = storage.Load();
    /// </code>
    /// To change a setting, save it and automatically raise <see cref="ConfigurationChanged"/>:
    /// <code>
    /// IConfigurationStorage storage = Factory.Resolve&lt;IConfigurationStorage&gt;().Singleton;
    /// Configuration configuration = storage.Load();
    /// configuration.AudioSettings.Enabled = false;
    /// storage.Save(configuration);
    /// </code>
    /// And to have an event handler that is raised whenever something changes the configuration:
    /// <code>
    /// private void SetupMyObject()
    /// {
    ///     IConfigurationStorage storage = Factory.Resolve&lt;IConfigurationStorage&gt;().Singleton;
    ///     storage.ConfigurationChanged += ConfigurationStorage_ConfigurationChanged;
    /// }
    /// 
    /// private void ConfigurationStorage_ConfigurationChanged(object sender, EventArgs args)
    /// {
    ///     // reload and apply new configuration here
    /// }
    /// </code>
    /// </example>
    [Singleton]
    public interface IConfigurationStorage
    {
        /// <summary>
        /// Gets or sets a value that abstracts away the environment for testing purposes.
        /// </summary>
        IConfigurationStorageProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets the folder the contains the configuration and log files.
        /// </summary>
        string Folder { get; set; }

        /// <summary>
        /// Raised after <see cref="Save"/> has saved a new configuration to disk or <see cref="Erase"/> has deleted the user's configuration.
        /// </summary>
        event EventHandler<EventArgs> ConfigurationChanged;

        /// <summary>
        /// Deletes all of the configuration settings, resetting everything back to factory settings.
        /// </summary>
        void Erase();

        /// <summary>
        /// Loads the current configuration for the user.
        /// </summary>
        /// <returns></returns>
        Configuration Load();

        /// <summary>
        /// Saves the configuration for the user.
        /// </summary>
        /// <param name="configuration"></param>
        void Save(Configuration configuration);
    }
}
