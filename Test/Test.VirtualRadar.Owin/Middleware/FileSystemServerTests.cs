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
using System.Net;
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
        private MockFileSystemProvider _FileSystem;
        private Mock<IFileSystemConfiguration> _ServerConfiguration;
        private Dictionary<string, string> _SiteRoots;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();
            _ServerConfiguration = TestUtilities.CreateMockSingleton<IFileSystemConfiguration>();

            _SiteRoots = new Dictionary<string, string>();
            _ServerConfiguration.Setup(r => r.GetSiteRootFolders()).Returns(() => {
                return _SiteRoots.Keys.ToList();
            });

            _Server = Factory.Singleton.Resolve<IFileSystemServer>();
            _FileSystem = new MockFileSystemProvider();
            _Server.FileSystemProvider = _FileSystem;

            _Environment = new MockOwinEnvironment();
            _Pipeline = new MockOwinPipeline();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        private void AddSiteRoot(string folder)
        {
            if(!_SiteRoots.ContainsKey(folder)) {
                _SiteRoots.Add(folder, null);
            }
        }

        private void AddSiteRootAndFile(string siteRoot, string pathFromSiteRoot, byte[] content)
        {
            AddSiteRoot(siteRoot);
            var fullPath = Path.Combine(siteRoot, pathFromSiteRoot);
            _FileSystem.AddFile(fullPath, content);
        }

        private void AddSiteRootAndFile(string siteRoot, string pathFromSiteRoot, string content)
        {
            AddSiteRoot(siteRoot);
            var fullPath = Path.Combine(siteRoot, pathFromSiteRoot);
            _FileSystem.AddFile(fullPath, Encoding.UTF8.GetBytes(content ?? ""));
        }

        private void ConfigureRequest(string path)
        {
            _Environment.RequestPath = path;
        }

        private void AssertFileReturned(string mimeType, byte[] content)
        {
            var filePath = Path.Combine(TestContext.TestDeploymentDir, "FileSystemServerTests");

            Assert.AreEqual(content.Length, _Environment.Response.ContentLength);
            Assert.AreEqual(mimeType, _Environment.Response.ContentType);
            Assert.IsTrue(content.SequenceEqual(_Environment.ResponseBodyBytes));
            Assert.AreEqual(200, _Environment.Response.StatusCode);
        }

        private void AssertFileReturned(string mimeType, string content)
        {
            AssertFileReturned(mimeType, Encoding.UTF8.GetBytes(content));
        }

        private void AssertNoFileReturned()
        {
            Assert.IsNull(_Environment.Response.ContentLength);
            Assert.IsNull(_Environment.Response.ContentType);
            Assert.AreEqual(0, _Environment.ResponseBodyBytes.Length);
        }

        [TestMethod]
        public void FileSystemServer_Constructor_Initialises_Properties_To_Known_Values()
        {
            var server = Factory.Singleton.Resolve<IFileSystemServer>();
            Assert.IsNotNull(server.FileSystemProvider);
        }

        [TestMethod]
        public void FileSystemServer_FileSystemProvider_Remains_Consistent()
        {
            var server = Factory.Singleton.Resolve<IFileSystemServer>();
            var provider = server.FileSystemProvider;
            Assert.AreSame(provider, server.FileSystemProvider);
        }

        [TestMethod]
        public void FileSystemServer_Serves_Files_In_SiteRoot()
        {
            var content = "Hello World!";
            AddSiteRootAndFile(@"c:\web\root", "Hello World.txt", content);
            ConfigureRequest("/Hello World.txt");

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            AssertFileReturned(MimeType.Text, content);
            Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        public void FileSystemServer_Ignores_Requests_That_Do_Not_Match_Filenames()
        {
            AddSiteRootAndFile(@"c:\web\root", "Exists.txt", "I am here");
            ConfigureRequest("/Hello World.txt");

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            AssertNoFileReturned();
            Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        public void FileSystemServer_Ignores_Requests_That_Attempt_To_Move_Out_Of_Root()
        {
            AddSiteRootAndFile(@"c:\web\root", "Allowed.txt", "Allowed");
            _FileSystem.AddFile(@"c:\web\NotAllowed.txt", Encoding.UTF8.GetBytes("NotAllowed"));
            ConfigureRequest("/../NotAllowed.txt");

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            AssertNoFileReturned();
            Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        public void FileSystemServer_Ignores_Requests_For_Root()
        {
            AddSiteRootAndFile(@"c:\web\root", "Content.txt", "Content");
            ConfigureRequest("/");

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            AssertNoFileReturned();
            Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        public void FileSystemServer_Ignores_Requests_For_Folders_With_Same_Name_As_File()
        {
            AddSiteRootAndFile(@"c:\web\root", "Content.txt", "Content");
            ConfigureRequest("/Content.txt/");

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            AssertNoFileReturned();
            Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        public void FileSystemServer_Infers_Mime_Type_From_File_Extension()
        {
            foreach(var extension in MimeType.GetKnownExtensions()) {
                TestCleanup();
                TestInitialise();

                AddSiteRootAndFile(@"c:\web\root", $"file.{extension}", extension);
                ConfigureRequest($"/file.{extension}");

                _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

                var expectedMimeType = MimeType.GetForExtension(extension);
                AssertFileReturned(expectedMimeType, extension);
            }
        }

        [TestMethod]
        public void FileSystemServer_Files_With_No_Extension_Are_Sent_As_Byte_Stream()
        {
            var content = Encoding.UTF8.GetBytes("Content");
            AddSiteRootAndFile(@"c:\web\root", "NoExtension", content);
            ConfigureRequest("/NoExtension");

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            AssertFileReturned("application/octet-stream", content);
        }
    }
}
