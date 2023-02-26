// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;

namespace VirtualRadar.Interface.Adsb
{
    /// <summary>
    /// Gets or sets the content of an identifier and category message.
    /// </summary>
    public class IdentifierAndCategoryMessage
    {
        /// <summary>
        /// Gets or sets the emitter category from the message. Note that reserved values do not have an enum value, they are a plain byte.
        /// </summary>
        public EmitterCategory EmitterCategory { get; set; }

        /// <summary>
        /// Gets or sets the identification code transmitted by the vehicle.
        /// </summary>
        /// <remarks>
        /// This is the aircraft identification as filed on the flight plan, or the registration of the aircraft if it is either not flying
        /// against a flight plan or the aircraft identification is not known, or a surface vehicle's radio call sign.
        /// </remarks>
        public string Identification { get; set; }

        /// <summary>
        /// Returns an English description of the message.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = new StringBuilder("IDC");
            result.AppendFormat(" EC:{0}", (int)EmitterCategory);
            result.AppendFormat(" ID:{0}", Identification);

            return result.ToString();
        }
    }
}
