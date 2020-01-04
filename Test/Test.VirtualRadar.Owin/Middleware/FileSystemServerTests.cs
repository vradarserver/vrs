// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private Mock<IFileSystemServerConfiguration> _ServerConfiguration;
        private Dictionary<string, string> _SiteRoots;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();
            _ServerConfiguration = TestUtilities.CreateMockSingleton<IFileSystemServerConfiguration>();

            _SiteRoots = new Dictionary<string, string>();
            _ServerConfiguration.Setup(r => r.GetSiteRootFolders()).Returns(() => {
                return _SiteRoots.Keys.ToList();
            });
            _ServerConfiguration.Setup(r => r.IsFileUnmodified(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>())).Returns(
                (string siteRootFolder, string requestPath, byte[] fileContent) => {
                    return true;
                }
            );

            _Server = Factory.Resolve<IFileSystemServer>();
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

            Assert.AreEqual(content.Length, _Environment.ResponseHeaders.ContentLength);
            Assert.AreEqual(mimeType, _Environment.ResponseHeaders.ContentType);
            Assert.IsTrue(content.SequenceEqual(_Environment.ResponseBodyBytes));
            Assert.AreEqual(200, _Environment.ResponseStatusCode);
        }

        private void AssertFileReturned(string mimeType, string content)
        {
            AssertFileReturned(mimeType, Encoding.UTF8.GetBytes(content));
        }

        private void AssertNoFileReturned()
        {
            Assert.IsNull(_Environment.ResponseHeaders.ContentLength);
            Assert.IsNull(_Environment.ResponseHeaders.ContentType);
            Assert.AreEqual(0, _Environment.ResponseBodyBytes.Length);
        }

        [TestMethod]
        public void FileSystemServer_Constructor_Initialises_Properties_To_Known_Values()
        {
            var server = Factory.Resolve<IFileSystemServer>();
            Assert.IsNotNull(server.FileSystemProvider);
        }

        [TestMethod]
        public void FileSystemServer_FileSystemProvider_Remains_Consistent()
        {
            var server = Factory.Resolve<IFileSystemServer>();
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
        }

        [TestMethod]
        public void FileSystemServer_Stops_Pipeline_If_File_Served()
        {
            AddSiteRootAndFile(@"c:\web\root", "Distant.mp3", "Past");
            ConfigureRequest("/Distant.mp3");

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            Assert.IsFalse(_Pipeline.NextMiddlewareCalled);
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
        public void FileSystemServer_Ignores_Requests_Containing_Invalid_Path_Characters()
        {
            AddSiteRoot(@"c:\web");

            foreach(var invalidPathChar in Path.GetInvalidPathChars()) {
                var badRequest = $"/web/-{invalidPathChar}-/File.txt";
                ConfigureRequest(badRequest);

                _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

                AssertNoFileReturned();
                Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
            }
        }

        [TestMethod]
        public void FileSystemServer_Ignores_Requests_Containing_Invalid_Filename_Characters()
        {
            AddSiteRoot(@"c:\web");

            foreach(var invalidFileNameChar in Path.GetInvalidFileNameChars()) {
                var badRequest = $"/web/-{invalidFileNameChar}-.txt";
                ConfigureRequest(badRequest);

                _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

                AssertNoFileReturned();
                Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
            }
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

        [TestMethod]
        public void FileSystemServer_Files_That_Fail_Checksum_Are_Not_Served()
        {
            var content = Encoding.UTF8.GetBytes("Sticky Fingers");
            AddSiteRootAndFile(@"c:\web\root", "file.txt", content);
            _ServerConfiguration.Setup(r => r.IsFileUnmodified(@"c:\web\root", "/file.txt", It.IsAny<byte[]>())).Returns(false);

            ConfigureRequest("/file.txt");
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            Assert.AreEqual(400, _Environment.ResponseStatusCode);
            Assert.AreEqual(0, _Environment.ResponseBodyBytes.Length);
        }

        [TestMethod]
        public void FileSystemServer_Html_Files_That_Fail_Checksum_Return_Error_Message()
        {
            var content = Encoding.UTF8.GetBytes("Opportunity Three");
            AddSiteRootAndFile(@"c:\web\root", "file.html", content);
            _ServerConfiguration.Setup(r => r.IsFileUnmodified(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>())).Returns(false);

            ConfigureRequest("/file.html");
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            var expectedResponse = "<HTML><HEAD><TITLE>No</TITLE></HEAD><BODY>VRS will not serve content that has been tampered with. Install the custom content plugin if you want to alter the site's files.</BODY></HTML>";
            var expectedBytes = Encoding.UTF8.GetBytes(expectedResponse);
            Assert.AreEqual(200, _Environment.ResponseStatusCode);
            Assert.IsTrue(_Environment.ResponseBodyBytes.SequenceEqual(expectedBytes));
        }

        [TestMethod]
        public void FileSystemServer_If_File_Exists_In_Two_Roots_Then_First_Root_Returned_By_Configuration_Is_Used()
        {
            var content = Encoding.UTF8.GetBytes("Sproston Green");
            AddSiteRootAndFile(@"c:\first",  "file.html", content);
            AddSiteRootAndFile(@"c:\second", "file.html", "DO NOT USE THIS ONE");

            ConfigureRequest("/file.html");
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            AssertFileReturned(MimeType.Html, content);
        }

        [TestMethod]
        public void FileSystemServer_Tests_Flattened_Paths_For_Checksums()
        {
            var content = Encoding.UTF8.GetBytes("Wild Horses");
            AddSiteRootAndFile(@"c:\web\root", "file.txt", content);
            _FileSystem.AddFile(@"c:\web\root\subfolder\otherfile.txt", new byte[0]);
            _ServerConfiguration.Setup(r => r.IsFileUnmodified(@"c:\web\root", "/file.txt", It.IsAny<byte[]>())).Returns(false);

            ConfigureRequest("/subfolder/../file.txt");
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            Assert.AreEqual(400, _Environment.ResponseStatusCode);
            Assert.AreEqual(0, _Environment.ResponseBodyBytes.Length);
        }

        [TestMethod]
        public void FileSystemServer_TextLoadedFromFile_Raised_For_Supported_Mime_Types()
        {
            foreach(var kvp in new Dictionary<string, string>() {
                { ".css",   MimeType.Css },
                { ".html",  MimeType.Html },
                { ".js",    MimeType.Javascript },
                { ".txt",   MimeType.Text }
            }) {
                TestCleanup();
                TestInitialise();

                var extension = kvp.Key;
                var mimeType = kvp.Value;
                var errorMessage = $"extension={extension}, mimeType={mimeType}";

                AddSiteRootAndFile(@"c:\web\root", $"Folder/File{extension}", "Hello");
                ConfigureRequest($"/Folder/File{extension}");

                TextContentEventArgs args = null;
                _ServerConfiguration.Setup(r => r.RaiseTextLoadedFromFile(It.IsAny<TextContentEventArgs>())).Callback((TextContentEventArgs e) => {
                    args = e;
                });

                _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

                _ServerConfiguration.Verify(r => r.RaiseTextLoadedFromFile(It.IsAny<TextContentEventArgs>()), Times.Once(), errorMessage);
                Assert.AreEqual($"/Folder/File{extension}", args.PathAndFile, errorMessage);
                Assert.AreEqual("Hello", args.Content, errorMessage);
                Assert.AreEqual(Encoding.UTF8.EncodingName, args.Encoding.EncodingName, errorMessage);
                Assert.AreEqual(mimeType, args.MimeType, errorMessage);
            }
        }

        [TestMethod]
        public void FileSystemServer_TextLoadedFromFile_Not_Raised_For_Other_Mime_Types()
        {
            AddSiteRootAndFile(@"c:\web\root", "Folder/File.ts", "Hello");
            ConfigureRequest($"/Folder/File.ts");

            TextContentEventArgs args = null;
            _ServerConfiguration.Setup(r => r.RaiseTextLoadedFromFile(It.IsAny<TextContentEventArgs>())).Callback((TextContentEventArgs e) => {
                args = e;
            });

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            _ServerConfiguration.Verify(r => r.RaiseTextLoadedFromFile(It.IsAny<TextContentEventArgs>()), Times.Never());
        }

        [TestMethod]
        public void FileSystemServer_TextLoadedFromFile_Reports_Encoding_Correctly()
        {
            var content = "£1.23";
            var bytes = Encoding.UTF32.GetPreamble().Concat(Encoding.UTF32.GetBytes(content)).ToArray();
            AddSiteRootAndFile(@"c:\web\root", "Folder/File.html", bytes);
            ConfigureRequest("/Folder/File.html");

            TextContentEventArgs args = null;
            _ServerConfiguration.Setup(r => r.RaiseTextLoadedFromFile(It.IsAny<TextContentEventArgs>())).Callback((TextContentEventArgs e) => {
                args = e;
            });

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            _ServerConfiguration.Verify(r => r.RaiseTextLoadedFromFile(It.IsAny<TextContentEventArgs>()), Times.Once());
            Assert.AreEqual("/Folder/File.html", args.PathAndFile);
            Assert.AreEqual("£1.23", args.Content);
            Assert.AreEqual(Encoding.UTF32.EncodingName, args.Encoding.EncodingName);
            Assert.AreEqual(MimeType.Html, args.MimeType);
        }

        [TestMethod]
        public void FileSystemServer_TextLoadedFromFile_Can_Change_Content()
        {
            AddSiteRootAndFile(@"c:\web\root", "Folder/File.html", "Hello");
            ConfigureRequest("/Folder/File.html");

            _ServerConfiguration.Setup(r => r.RaiseTextLoadedFromFile(It.IsAny<TextContentEventArgs>())).Callback((TextContentEventArgs e) => {
                e.Content = "New Content";
            });

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            AssertFileReturned(MimeType.Html, "New Content");
        }

        [TestMethod]
        public void FileSystemServer_TextLoadedFromFile_Preserves_Encoding()
        {
            var content = "Hello";
            var bytes = Encoding.UTF32.GetPreamble().Concat(Encoding.UTF32.GetBytes(content)).ToArray();
            AddSiteRootAndFile(@"c:\web\root", "Folder/File.html", bytes);
            ConfigureRequest("/Folder/File.html");

            _ServerConfiguration.Setup(r => r.RaiseTextLoadedFromFile(It.IsAny<TextContentEventArgs>())).Callback((TextContentEventArgs e) => {
                e.Content = "New Content";
            });

            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            var expectedBytes = Encoding.UTF32.GetPreamble().Concat(Encoding.UTF32.GetBytes("New Content")).ToArray();
            AssertFileReturned(MimeType.Html, expectedBytes);
        }

        [TestMethod]
        public void FileSystemServer_TextLoadedFromFile_Modifications_Do_Not_Affect_Checksum_Test()
        {
            var content = Encoding.UTF8.GetBytes("Can't You Hear Me Knocking");
            AddSiteRootAndFile(@"c:\web\root", "file.txt", content);
            byte[] checksumTestContent = null;

            _ServerConfiguration.Setup(r => r.IsFileUnmodified(@"c:\web\root", "/file.txt", It.IsAny<byte[]>())).Returns(
                (string siteRootFolder, string requestPath, byte[] fileContent) => {
                checksumTestContent = fileContent;
                    return true;
                }
            );
            _ServerConfiguration.Setup(r => r.RaiseTextLoadedFromFile(It.IsAny<TextContentEventArgs>())).Callback((TextContentEventArgs e) => {
                e.Content = "You Gotta Move";
            });

            ConfigureRequest("/file.txt");
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            Assert.IsTrue(content.SequenceEqual(checksumTestContent));
        }
    }
}
