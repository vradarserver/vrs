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
using VirtualRadar.Localisation;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The interface for objects that can localise HTML using string resources.
    /// </summary>
    public interface IHtmlLocaliser
    {
        /// <summary>
        /// Gets the localised strings that the localiser is using.
        /// </summary>
        LocalisedStringsMap LocalisedStringsMap { get; }

        /// <summary>
        /// Initialises the localiser with the Virtual Radar Server application strings.
        /// </summary>
        void Initialise();

        /// <summary>
        /// Initialises the localiser with the strings from the compiled resource object passed across.
        /// </summary>
        void Initialise(Type resourceStringsType);

        /// <summary>
        /// Adds resource strings to the localised strings map.
        /// </summary>
        /// <param name="resourceStringsType"></param>
        void AddResourceStrings(Type resourceStringsType);

        /// <summary>
        /// Replaces all instances of ::STRING:: with the localised text.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        string Html(string html, Encoding encoding);

        /// <summary>
        /// Escapes HTML in the string, converts linebreaks to break tags and so on.
        /// </summary>
        /// <param name="resourceString"></param>
        /// <returns></returns>
        string ConvertResourceStringToHtmlString(string resourceString);

        /// <summary>
        /// Escapes quotes in the string, converts linebreaks to \r or \n etc.
        /// </summary>
        /// <param name="resourceString"></param>
        /// <param name="stringDelimiter"></param>
        /// <returns></returns>
        string ConvertResourceStringToJavaScriptString(string resourceString, char stringDelimiter);
    }
}
