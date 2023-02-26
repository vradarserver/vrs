// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaiC:\Source\VirtualRadar\Source\VirtualRadar.Interface\View\mer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.ModeS
{
    /// <summary>
    /// An enumeration of the different possible values for the AF field in DF19 messages.
    /// </summary>
    public enum ApplicationField : byte
    {
        /// <summary>
        /// The message content is as per DF17 extended squitter.
        /// </summary>
        /// <remarks>
        /// There appears to be some disagreement over whether or not this is ever used. It is laid down in some
        /// versions of Annex 10 Vol IV but other papers note that military aircraft participating in ADS-B will
        /// use DF17 and that no restrictions, including the use of AF0 to carry ADS-B information, is to be
        /// placed on DF19 - and that the use of this field cannot be assumed to indicate that the rest of the
        /// message carries ADS-B information.
        /// </remarks>
        ADSB = 0,

        /// <summary>
        /// Reserved for military applications.
        /// </summary>
        AF1 = 1,

        /// <summary>
        /// Reserved for military applications.
        /// </summary>
        AF2 = 2,

        /// <summary>
        /// Reserved for military applications.
        /// </summary>
        AF3 = 3,

        /// <summary>
        /// Reserved for military applications.
        /// </summary>
        AF4 = 4,

        /// <summary>
        /// Reserved for military applications.
        /// </summary>
        AF5 = 5,

        /// <summary>
        /// Reserved for military applications.
        /// </summary>
        AF6 = 6,

        /// <summary>
        /// Reserved for military applications.
        /// </summary>
        AF7 = 7,
    }
}
