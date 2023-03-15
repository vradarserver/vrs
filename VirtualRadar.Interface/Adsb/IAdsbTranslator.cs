// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface.Feeds;
using VirtualRadar.Interface.ModeS;

namespace VirtualRadar.Interface.Adsb
{
    /// <summary>
    /// The interface for that classes that can translate ME message blocks into ADS-B messages.
    /// </summary>
    public interface IAdsbTranslator
    {
        /// <summary>
        /// Gets or sets the statistics to update when translating messages.
        /// </summary>
        ReceiverStatistics Statistics { get; set; }

        /// <summary>
        /// Translates the payload of the <see cref="ModeSMessage.ExtendedSquitterMessage"/> property into an ADS-B message.
        /// </summary>
        /// <param name="modeSMessage"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method will return null if the Mode-S message does not contain an ADS-B message or if the ADS-B message could
        /// not be translated from the Mode-S message. Callers should test for a return of null even if they pass in what appears
        /// to be a Mode-S message carrying a valid extended squitter message block.
        /// </remarks>
        AdsbMessage Translate(ModeSMessage modeSMessage);
    }
}
