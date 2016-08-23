// Copyright © 2012 onwards, Andrew Whewell
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
using System.Runtime.Serialization;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Threading;
using System.Globalization;
using Newtonsoft.Json;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// An object that can serialise to JSON.
    /// </summary>
    public class JsonSerialiser
    {
        private bool _Initialised;
        private static Encoding _UTF8NoBOM = new UTF8Encoding(false, true);         // Same encoding that you get by default in StreamWriter

        /// <summary>
        /// Initialises the serialiser.
        /// </summary>
        /// <param name="type"></param>
        public void Initialise(Type type)
        {
            // This was used when I had my own implementation but now that I'm using NewtonSoft's library it's no longer required.
            // All of the code here is just for the benefit of the unit tests written for the original version.
            if(type == null) throw new ArgumentNullException("type");
            if(!type.GetCustomAttributes(typeof(DataContractAttribute), true).Any()) {
                throw new InvalidOperationException("Types passed to Initialise must be tagged with DataContract");
            }
            _Initialised = true;
        }

        /// <summary>
        /// Serialises the object to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        public void WriteObject(Stream stream, object obj)
        {
            if(!_Initialised) throw new InvalidOperationException("Not initialised");

            var currentCulture = Thread.CurrentThread.CurrentCulture;
            try {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                if(stream == null) throw new ArgumentNullException("stream");

                using(var streamWriter = new StreamWriter(stream, _UTF8NoBOM, 1024, leaveOpen: true)) {
                    using(var jsonWriter = new JsonTextWriter(streamWriter) { CloseOutput = false, Formatting = Formatting.None, DateFormatHandling = DateFormatHandling.MicrosoftDateFormat }) {
                        var serialiser = new JsonSerializer();
                        serialiser.Serialize(jsonWriter, obj);
                    }
                }
            } finally {
                Thread.CurrentThread.CurrentCulture = currentCulture;
            }
        }
    }
}
