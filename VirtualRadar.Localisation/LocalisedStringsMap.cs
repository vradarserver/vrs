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
using System.Linq;
using System.Text;
using System.Reflection;

namespace VirtualRadar.Localisation
{
    /// <summary>
    /// A class that uses reflection to find all of the localised strings we've got set up in a generated string map class.
    /// </summary>
    public class LocalisedStringsMap
    {
        /// <summary>
        /// A map of all localised string names to the relevant static property in Strings.
        /// </summary>
        /// <remarks>
        /// You might be wondering why I'm using PropertyInfo instead of just reading the properties to get the strings.
        /// Using PropertyInfo means that I get the strings for the current UI culture and not the UI culture that was
        /// current when the object was initialised.
        /// </remarks>
        private Dictionary<string, PropertyInfo> _LocalisedStrings = new Dictionary<string, PropertyInfo>();

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="stringsType">The type of the generated strings resource object holding all of the strings.</param>
        public LocalisedStringsMap(Type stringsType)
        {
            foreach(PropertyInfo property in stringsType.GetProperties(BindingFlags.Public | BindingFlags.Static)) {
                if(property.PropertyType == typeof(string)) _LocalisedStrings.Add(property.Name, property);
            }
        }

        /// <summary>
        /// Returns the localised string for the name passed across. If there is no such localised string then an exception
        /// is thrown.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetLocalisedString(string name)
        {
            if(name == null) throw new ArgumentNullException("name");

            PropertyInfo property;
            if(!_LocalisedStrings.TryGetValue(name, out property)) throw new NotImplementedException(String.Format(@"There is no localised string called ""{0}""", name));

            return (string)property.GetValue(null, null);
        }

        /// <summary>
        /// Returns the localised string for the name passed across. If there is no such localised string then null is returned.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string TryGetLocalisedString(string name)
        {
            if(name == null) throw new ArgumentNullException("name");

            PropertyInfo property;
            _LocalisedStrings.TryGetValue(name, out property);

            return property == null ? null : (string)property.GetValue(null, null);
        }
    }
}
