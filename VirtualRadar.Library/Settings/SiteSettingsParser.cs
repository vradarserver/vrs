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
using Newtonsoft.Json.Linq;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library.Settings
{
    /// <summary>
    /// The default implementation of <see cref="ISiteSettingsParser"/>.
    /// </summary>
    class SiteSettingsParser : ISiteSettingsParser
    {
        /// <summary>
        /// The JSON last loaded by <see cref="Load"/>.
        /// </summary>
        private string _Json;

        /// <summary>
        /// The JSON last loaded by <see cref="Load"/>, parsed into a Newtonsoft Json.NET JObject.
        /// </summary>
        private JObject _ParsedJson;

        /// <summary>
        /// The version of the settings format last loaded by <see cref="Load"/>.
        /// </summary>
        private int _Version;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Exception LoadException { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="exportedSettings"></param>
        /// <returns></returns>
        public bool Load(string exportedSettings)
        {
            IsValid = false;
            LoadException = null;

            try {
                _Json = exportedSettings;
                _ParsedJson = null;
                if(!String.IsNullOrEmpty(_Json)) {
                    _ParsedJson = JObject.Parse(_Json);
                    var version = (int)_ParsedJson["ver"];
                    if(version == 1) {
                        var values = (object)_ParsedJson["values"];
                        IsValid = values != null;
                    }
                }
            } catch(Exception ex) {
                LoadException = ex;
                IsValid = false;
            }

            return IsValid;
        }
    }
}
