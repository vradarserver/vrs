// Copyright © 2015 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;

namespace VirtualRadar.Localisation
{
    /// <summary>
    /// A class that can extract strings from a resource for a specific culture info.
    /// </summary>
    public class ResourceStrings
    {
        /// <summary>
        /// A map of strings read from the resource for the culture info specified.
        /// </summary>
        private Dictionary<string, string> _Strings = new Dictionary<string, string>();

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="resourceSource"></param>
        /// <param name="cultureName"></param>
        public ResourceStrings(Type resourceSource, string cultureName)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture(cultureName);
            ReadResourceStrings(resourceSource, cultureInfo);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="resourceSource"></param>
        /// <param name="cultureInfo"></param>
        public ResourceStrings(Type resourceSource, CultureInfo cultureInfo)
        {
            ReadResourceStrings(resourceSource, cultureInfo);
        }

        /// <summary>
        /// Loads the strings for the specified culture.
        /// </summary>
        /// <param name="resourceSource"></param>
        /// <param name="cultureInfo"></param>
        private void ReadResourceStrings(Type resourceSource, CultureInfo cultureInfo)
        {
            var resourceManager = new ResourceManager(resourceSource);
            var resourceSet = resourceManager.GetResourceSet(cultureInfo, createIfNotExists: true, tryParents: false);

            // We don't want to dispose of the resource set, we just let the resource manager take care of it. Other threads
            // might be reading the same resource set.

            // We also assume that resourceSet is not null. If it's null then the wrong resources were released with the application.

            foreach(DictionaryEntry kvp in resourceSet) {
                var value = kvp.Value as String;
                if(value != null) {
                    _Strings.Add((string)kvp.Key, value);
                }
            }
        }

        /// <summary>
        /// Returns an array of every string name in the map.
        /// </summary>
        /// <returns></returns>
        public string[] GetStringNames()
        {
            return _Strings.Keys.ToArray();
        }

        /// <summary>
        /// Returns the localised string for the name passed across.
        /// </summary>
        /// <param name="stringName"></param>
        /// <returns></returns>
        public string GetLocalisedString(string stringName)
        {
            return _Strings[stringName];
        }

        /// <summary>
        /// Tries to get the localised string for the name passed across.
        /// </summary>
        /// <param name="stringName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetLocalisedString(string stringName, out string value)
        {
            return _Strings.TryGetValue(stringName, out value);
        }
    }
}
