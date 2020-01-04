// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Linq;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;

namespace Test.VirtualRadar.Owin.Middleware
{
    [TestClass]
    public class AudioServerTests
    {
        public TestContext TestContext { get; set; }
        private IClassFactory _Snapshot;

        private IAudioServer _Server;
        private MockOwinEnvironment _Environment;
        private MockOwinPipeline _Pipeline;
        private global::VirtualRadar.Interface.Settings.Configuration _Configuration;
        private Mock<ISharedConfiguration> _SharedConfiguration;
        private Mock<IAudio> _Audio;
        private byte[] _SomeBytes;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _Configuration = new global::VirtualRadar.Interface.Settings.Configuration();
            _SharedConfiguration = TestUtilities.CreateMockSingleton<ISharedConfiguration>();
            _SharedConfiguration.Setup(r => r.Get()).Returns(_Configuration);
            _Audio = TestUtilities.CreateMockImplementation<IAudio>();
            _Audio.Setup(r => r.SpeechToWavBytes(It.IsAny<string>())).Returns(new byte[0]);
            _SomeBytes = new byte[] { 0x01, 0x02 };

            _Server = Factory.Resolve<IAudioServer>();

            _Environment = new MockOwinEnvironment();
            _Environment.ServerRemoteIpAddress = "127.0.0.1";
            _Pipeline = new MockOwinPipeline();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        private void AssertAudioReturned(string mimeType, byte[] content)
        {
            Assert.AreEqual(content.Length, _Environment.ResponseHeaders.ContentLength);
            Assert.AreEqual(mimeType, _Environment.ResponseHeaders.ContentType);
            Assert.IsTrue(content.SequenceEqual(_Environment.ResponseBodyBytes));
            Assert.AreEqual(200, _Environment.ResponseStatusCode);
            Assert.IsFalse(_Pipeline.NextMiddlewareCalled);
        }

        private void AssertAudioNotReturned()
        {
            Assert.IsNull(_Environment.ResponseHeaders.ContentLength);
            Assert.AreEqual(0, _Environment.ResponseBodyBytes.Length);
            Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        public void AudioServer_Responds_To_Request_For_Speech_To_Text()
        {
            _Audio.Setup(r => r.SpeechToWavBytes("Hello")).Returns(_SomeBytes);
            _Environment.SetRequestUrl("/Audio", new [,] {
                { "cmd",  "say" },
                { "line", "Hello" },
            });

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            AssertAudioReturned(MimeType.WaveAudio, _SomeBytes);
        }

        [TestMethod]
        public void AudioServer_Command_Is_Not_Case_Sensitive()
        {
            _Audio.Setup(r => r.SpeechToWavBytes("Hello")).Returns(_SomeBytes);
            _Environment.SetRequestUrl("/Audio", new [,] {
                { "cmd",  "Say" },
                { "line", "Hello" },
            });

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            AssertAudioReturned(MimeType.WaveAudio, _SomeBytes);
        }

        [TestMethod]
        public void AudioServer_Does_Not_Respond_To_Requests_For_Other_Pages()
        {
            _Audio.Setup(r => r.SpeechToWavBytes("Hello")).Returns(_SomeBytes);
            _Environment.SetRequestUrl("/NotAudio", new [,] {
                { "cmd",  "say" },
                { "line", "Hello" },
            });

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            AssertAudioNotReturned();
        }

        [TestMethod]
        public void AudioServer_Does_Not_Respond_If_Command_Is_Missing()
        {
            _Audio.Setup(r => r.SpeechToWavBytes("Hello")).Returns(_SomeBytes);
            _Environment.SetRequestUrl("/Audio", new [,] {
                { "line", "Hello" },
            });

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            AssertAudioNotReturned();
        }

        [TestMethod]
        public void AudioServer_Does_Not_Respond_To_Requests_For_Unknown_Commands()
        {
            _Audio.Setup(r => r.SpeechToWavBytes("Hello")).Returns(_SomeBytes);
            _Environment.SetRequestUrl("/Audio", new [,] {
                { "cmd",  "notSay" },
                { "line", "Hello" },
            });

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            AssertAudioNotReturned();
        }

        [TestMethod]
        public void AudioServer_Does_Not_Respond_To_Say_If_Line_Is_Missing()
        {
            _Audio.Setup(r => r.SpeechToWavBytes(null)).Returns(_SomeBytes);
            _Audio.Setup(r => r.SpeechToWavBytes("")).Returns(_SomeBytes);
            _Environment.SetRequestUrl("/Audio", new [,] {
                { "cmd",  "say" },
            });

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            AssertAudioNotReturned();
        }

        [TestMethod]
        public void AudioServer_Honours_InternetClient_Permission_Settings()
        {
            foreach(var isInternetRequest in new [] { true, false }) {
                foreach(var internetAudioAllowed in new [] { true, false }) {
                    TestCleanup();
                    TestInitialise();

                    _Audio.Setup(r => r.SpeechToWavBytes("Permissions")).Returns(_SomeBytes);
                    _Environment.SetRequestUrl("/Audio", new [,] {
                        { "cmd",  "Say" },
                        { "line", "Permissions" },
                    });

                    _Environment.ServerRemoteIpAddress = isInternetRequest ? "1.2.3.4" : "127.0.0.1";
                    _Configuration.InternetClientSettings.CanPlayAudio = internetAudioAllowed;

                    _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

                    var expectedResult = !isInternetRequest || internetAudioAllowed;
                    var actualResult = _Environment.ResponseBodyBytes.Length > 0;
                    Assert.AreEqual(expectedResult, actualResult, $"Expected {expectedResult} when {nameof(isInternetRequest)}={isInternetRequest} and {nameof(internetAudioAllowed)}={internetAudioAllowed}, got {actualResult}");
                }
            }
        }
    }
}
