// Copyright © 2017 onwards, Andrew Whewell
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.Owin.Middleware
{
    [TestClass]
    public class FileSystemServerTests
    {
        public TestContext TestContext { get; set; }
        private IClassFactory _Snapshot;

        private IFileSystemServer _Server;
        private MockOwinEnvironment _Environment;
        private MockOwinPipeline _Pipeline;
        private IFileSystemConfiguration _ServerConfiguration;
        private string _WebRoot;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();
            _ServerConfiguration = Factory.Singleton.Resolve<IFileSystemConfiguration>();
            TestUtilities.CreateMockSingletonHost(_ServerConfiguration);

            _Server = Factory.Singleton.Resolve<IFileSystemServer>();
            _Environment = new MockOwinEnvironment();
            _Pipeline = new MockOwinPipeline();

            _WebRoot = $"{TestContext.TestDeploymentDir}\\FileSystemServerTests";
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        private void ConfigureSingleRootWithoutChecksums()
        {
            _ServerConfiguration.AddSiteRoot(new SiteRoot() {
                Folder = _WebRoot
            });
        }

        private void ConfigureRequest(string path)
        {
            _Environment.RequestPath = path;
        }

        private void AssertFileReturned(string mimeType, params string[] filePathParts)
        {
            var filePath = Path.Combine(TestContext.TestDeploymentDir, "FileSystemServerTests");
            foreach(var filePathPart in filePathParts) {
                filePath = Path.Combine(filePath, filePathPart);
            }

            var fileBytes = File.ReadAllBytes(filePath);

            Assert.AreEqual(fileBytes.Length, _Environment.Response.ContentLength);
            Assert.AreEqual(mimeType, _Environment.Response.ContentType);
            Assert.IsTrue(fileBytes.SequenceEqual(_Environment.ResponseBodyBytes));
            Assert.AreEqual(200, _Environment.Response.StatusCode);
        }

        [TestMethod]
        public void FileSystemServer_Calls_Next_Middlware_By_Default()
        {
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        public void FileSystemServer_Serves_Files_In_SiteRoot()
        {
            ConfigureSingleRootWithoutChecksums();
            ConfigureRequest("/Hello World.txt");

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            AssertFileReturned(MimeType.Text, "Hello World.txt");
        }
    }
}
