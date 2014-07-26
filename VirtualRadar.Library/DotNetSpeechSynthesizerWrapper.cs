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
using VirtualRadar.Interface;
#if !__MonoCS__
using System.Speech.Synthesis;
#endif

namespace VirtualRadar.Library
{
    /// <summary>
    /// The .NET default implementation of <see cref="ISpeechSynthesizerWrapper"/>.
    /// </summary>
    sealed class DotNetSpeechSynthesizerWrapper : ISpeechSynthesizerWrapper
    {
        /// <summary>
        /// The speech synthesizer that this class wraps.
        /// </summary>
        #if !__MonoCS__
        private SpeechSynthesizer _SpeechSynthesizer;
        #else
        private IDisposable _SpeechSynthesizer;
        #endif

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string DefaultVoiceName
        {
            #if !__MonoCS__
            get { return _SpeechSynthesizer != null ? _SpeechSynthesizer.Voice.Name : ""; }
            #else
            get { return "SpeechSynthesisNotSupported"; }
            #endif
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Rate
        {
            #if !__MonoCS__
            get { return _SpeechSynthesizer != null ? _SpeechSynthesizer.Rate : 0; }
            set { if(_SpeechSynthesizer != null) _SpeechSynthesizer.Rate = value; }
            #else
            get { return 0; }
            set { ; }
            #endif
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public DotNetSpeechSynthesizerWrapper()
        {
            #if !__MonoCS__
            try {
                _SpeechSynthesizer = new SpeechSynthesizer();
            } catch {
                // On some Windows installs there's no speech synthesizer.
                _SpeechSynthesizer = null;
            }
            #endif
         }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~DotNetSpeechSynthesizerWrapper()
        {
            Dispose(false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object. Note that that the class is sealed.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if(disposing) {
                if(_SpeechSynthesizer != null) _SpeechSynthesizer.Dispose();
                _SpeechSynthesizer = null;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetInstalledVoiceNames()
        {
            #if !__MonoCS__
            return _SpeechSynthesizer == null ? new string[0] : _SpeechSynthesizer.GetInstalledVoices().Where(s => s.Enabled).Select(v => v.VoiceInfo.Name);
            #else
            return new string[]{};
            #endif
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        public void SelectVoice(string name)
        {
            #if !__MonoCS__
            if(_SpeechSynthesizer != null) _SpeechSynthesizer.SelectVoice(name);
            #endif
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void SetOutputToDefaultAudioDevice()
        {
            #if !__MonoCS__
            if(_SpeechSynthesizer != null) _SpeechSynthesizer.SetOutputToDefaultAudioDevice();
            #endif
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="text"></param>
        public void SpeakAsync(string text)
        {
            #if !__MonoCS__
            if(_SpeechSynthesizer != null) _SpeechSynthesizer.SpeakAsync(text);
            #endif
        }
    }
}
