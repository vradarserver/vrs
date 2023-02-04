// Copyright � 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The interface for objects that can load and save <see cref="Configuration"/> objects.
    /// </summary>
    /// <remarks>
    /// This is the configuration that the .NET Framework versions used. The intention is to use the options
    /// pattern for .NET Core. I've kept this hanging around for now but nothing will use it, the intention
    /// is to port settings over to a JSON file once development is well underway.
    /// </remarks>
    public interface IConfigurationStorage
    {
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
