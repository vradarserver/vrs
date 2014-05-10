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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for a wrapper around the .NET SpeechSynthesizer class.
    /// </summary>
    /// <remarks>
    /// There are two default implementations for this, one that is used when the program runs under .NET
    /// and another that is used when the program runs under Mono. Mono does not support the SpeechSynthesizer
    /// class and any attempt to instantiate it sends the program into hyperspace, so this just wraps the parts
    /// of the .NET class that the application uses.
    /// </remarks>
    public interface ISpeechSynthesizerWrapper : IDisposable
    {
        /// <summary>
        /// Gets the default voice name.
        /// </summary>
        string DefaultVoiceName { get; }

        /// <summary>
        /// Gets or sets the text-to-speech rate.
        /// </summary>
        int Rate { get; set; }

        /// <summary>
        /// Returns a collection of the name of every installed voice.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetInstalledVoiceNames();

        /// <summary>
        /// Selects a voice by name.
        /// </summary>
        /// <param name="name"></param>
        void SelectVoice(string name);

        /// <summary>
        /// Sends the output to the default audio device.
        /// </summary>
        void SetOutputToDefaultAudioDevice();

        /// <summary>
        /// Converts the text to speech asynchronously.
        /// </summary>
        /// <param name="text"></param>
        void SpeakAsync(string text);
    }
}
