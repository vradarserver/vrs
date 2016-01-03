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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface;
using VirtualRadar.Library;
using InterfaceFactory;
using Moq;
using Test.Framework;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class AudioTests
    {
        public TestContext TestContext { get; set; }

        private IClassFactory _FactorySnapshot;
        private IAudio _Audio;
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;

        [TestInitialize]
        public void TestInitialise()
        {
            _FactorySnapshot = Factory.TakeSnapshot();

            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _RuntimeEnvironment.Setup(e => e.IsMono).Returns(false);
            _Audio = Factory.Singleton.Resolve<IAudio>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_FactorySnapshot);
        }

        [TestMethod]
        public void Audio_IsSupported_Returns_True_If_The_Runtime_Is_Not_Mono()
        {
            Assert.IsTrue(_Audio.IsSupported);
        }

        [TestMethod]
        public void Audio_IsSupported_Returns_False_If_The_Runtime_Is_Mono()
        {
            _RuntimeEnvironment.Setup(e => e.IsMono).Returns(true);
            Assert.IsFalse(_Audio.IsSupported);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Audio_SpeechToWavBytes_Throws_If_SpeechText_Is_Null()
        {
            _Audio.SpeechToWavBytes(null);
        }

        [TestMethod]
        public void Audio_SpeechToWavBytes_Looks_Like_It_Is_Working()
        {
            // Hard to tell whether it actually IS working without listening to the audio so
            // all we can really do is see if the size of the byte array it returns looks OK
            var emptyAudio = _Audio.SpeechToWavBytes("");
            Assert.AreNotEqual(0, emptyAudio.Length);
            Assert.IsTrue(emptyAudio.Length < 100); // Should just be a header

            var letterB = _Audio.SpeechToWavBytes("B");
            Assert.IsTrue(letterB.Length > 24000);

            var letterBB = _Audio.SpeechToWavBytes("BB");
            Assert.IsTrue(letterBB.Length > letterB.Length);
        }

        [TestMethod]
        public void Audio_SpeechToWavBytes_Returns_Empty_Byte_Array_On_Mono()
        {
            _RuntimeEnvironment.Setup(e => e.IsMono).Returns(true);
            Assert.AreEqual(0, _Audio.SpeechToWavBytes("Hello").Length);
        }
    }
}
