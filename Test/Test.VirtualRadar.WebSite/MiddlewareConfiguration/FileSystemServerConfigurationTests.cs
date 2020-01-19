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
using System.IO;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.WebSite.MiddlewareConfiguration
{
    [TestClass]
    public class FileSystemServerConfigurationTests
    {
        public TestContext TestContext { get; set; }
        private IClassFactory _Snapshot;
        private MockFileSystemProvider _FileSystemProvider;
        private IFileSystemServerConfiguration _Configuration;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _FileSystemProvider = new MockFileSystemProvider();
            Factory.RegisterInstance<IFileSystemProvider>(_FileSystemProvider);

            _Configuration = Factory.ResolveNewInstance<IFileSystemServerConfiguration>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileSystemConfiguration_AddSiteRoot_Throws_If_SiteRoot_Is_Null()
        {
            _Configuration.AddSiteRoot(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FileSystemConfiguration_AddSiteRoot_Throws_If_SiteRoot_Folder_Is_Null()
        {
            _Configuration.AddSiteRoot(new SiteRoot() { Folder = null });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FileSystemConfiguration_AddSiteRoot_Throws_If_SiteRoot_Folder_Is_Empty()
        {
            _Configuration.AddSiteRoot(new SiteRoot() { Folder = "" });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FileSystemConfiguration_AddSiteRoot_Throws_If_Folder_Does_Not_Exist()
        {
            _Configuration.AddSiteRoot(new SiteRoot() { Folder = @"C:\DOES-NOT-EXIST" });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FileSystemConfiguration_AddSiteRoot_Throws_If_Folder_Is_Relative()
        {
            _Configuration.AddSiteRoot(new SiteRoot() { Folder = "." });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FileSystemConfiguration_AddSiteRoot_Throws_If_Folder_Is_Already_A_Site_Root()
        {
            _FileSystemProvider.AddFolder(@"c:\web");

            _Configuration.AddSiteRoot(new SiteRoot() { Folder = @"c:\web" });
            _Configuration.AddSiteRoot(new SiteRoot() { Folder = @"c:\web" });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FileSystemConfiguration_AddSiteRoot_Is_Not_Case_Sensitive_When_Searching_For_Duplicate_Folders()
        {
            _FileSystemProvider.AddFolder(@"c:\web");

            _Configuration.AddSiteRoot(new SiteRoot() { Folder = @"c:\web" });
            _Configuration.AddSiteRoot(new SiteRoot() { Folder = @"C:\WEB" });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FileSystemConfiguration_AddSiteRoot_Flattens_Traversal_Folders_When_Searching_For_Duplicate_Folders()
        {
            _FileSystemProvider.AddFolder(@"c:\web");
            _FileSystemProvider.AddFolder(@"c:\web\subfolder");

            _Configuration.AddSiteRoot(new SiteRoot() { Folder = @"c:\web" });
            _Configuration.AddSiteRoot(new SiteRoot() { Folder = @"c:\web\subfolder\.." });
        }

        [TestMethod]
        public void FileSystemConfiguration_AddSiteRoot_Adds_Trailing_Directory_Separator_To_Folder()
        {
            var folder = @"c:\web";
            _FileSystemProvider.AddFolder(folder);

            _Configuration.AddSiteRoot(new SiteRoot() { Folder = folder });

            var folders = _Configuration.GetSiteRootFolders();
            var siteRootFolder = folders[0];
            Assert.AreEqual(Path.DirectorySeparatorChar, siteRootFolder[siteRootFolder.Length - 1]);
        }

        [TestMethod]
        public void FileSystemConfiguration_RemoveSiteRoot_Does_Nothing_If_Passed_Null()
        {
            _Configuration.RemoveSiteRoot(null);
            Assert.AreEqual(0, _Configuration.GetSiteRootFolders().Count);
        }

        [TestMethod]
        public void FileSystemConfiguration_RemoveSiteRoot_Removes_Added_Site()
        {
            _FileSystemProvider.AddFolder(@"c:\web");
            var siteRoot = new SiteRoot() { Folder = @"c:\web" };
            _Configuration.AddSiteRoot(siteRoot);
            _Configuration.RemoveSiteRoot(siteRoot);

            Assert.IsFalse(_Configuration.IsSiteRootActive(siteRoot, false));
            Assert.AreEqual(0, _Configuration.GetSiteRootFolders().Count);
        }

        [TestMethod]
        public void FileSystemConfiguration_RemoveSiteRoot_Ignores_Sites_Not_Added()
        {
            _FileSystemProvider.AddFolder(@"c:\web");
            var siteRoot = new SiteRoot() { Folder = @"c:\web" };
            _Configuration.RemoveSiteRoot(siteRoot);
            Assert.IsFalse(_Configuration.IsSiteRootActive(siteRoot, false));
        }

        [TestMethod]
        public void FileSystemConfiguration_IsSiteRootActive_Returns_False_If_Passed_Null()
        {
            Assert.IsFalse(_Configuration.IsSiteRootActive(null, false));
        }

        [TestMethod]
        public void FileSystemConfiguration_IsSiteRootActive_Returns_False_If_SiteRoot_Not_Added()
        {
            var siteRoot = new SiteRoot() { Folder = TestContext.TestDeploymentDir };
            Assert.IsFalse(_Configuration.IsSiteRootActive(siteRoot, false));
        }

        [TestMethod]
        public void FileSystemConfiguration_IsSiteRootActive_Returns_True_If_SiteRoot_Added()
        {
            _FileSystemProvider.AddFolder(@"c:\web");
            var siteRoot = new SiteRoot() { Folder = @"c:\web" };
            _Configuration.AddSiteRoot(siteRoot);

            Assert.IsTrue(_Configuration.IsSiteRootActive(siteRoot, false));
        }

        [TestMethod]
        public void FileSystemConfiguration_IsSiteRootActive_Returns_True_If_SiteRoot_Folder_Changed_And_Folders_Are_Ignored()
        {
            _FileSystemProvider.AddFolder(@"c:\web");
            _FileSystemProvider.AddFolder(@"c:\other");

            var siteRoot = new SiteRoot() { Folder = @"c:\web" };
            _Configuration.AddSiteRoot(siteRoot);

            siteRoot.Folder = @"c:\other";

            Assert.IsTrue(_Configuration.IsSiteRootActive(siteRoot, false));
        }

        [TestMethod]
        public void FileSystemConfiguration_IsSiteRootActive_Returns_True_If_SiteRoot_Folder_Changed_And_Folders_Are_Significant()
        {
            _FileSystemProvider.AddFolder(@"c:\web");
            _FileSystemProvider.AddFolder(@"c:\other");

            var siteRoot = new SiteRoot() { Folder = @"c:\web" };
            _Configuration.AddSiteRoot(siteRoot);

            siteRoot.Folder = @"c:\other";

            Assert.IsFalse(_Configuration.IsSiteRootActive(siteRoot, true));
        }

        [TestMethod]
        public void FileSystemConfiguration_IsSiteRootActive_Returns_True_If_Changed_Folder_Points_To_Original_Folder()
        {
            _FileSystemProvider.AddFolder(@"c:\web");
            _FileSystemProvider.AddFolder(@"c:\web\subfolder");

            var siteRoot = new SiteRoot() { Folder = @"c:\web" };
            _Configuration.AddSiteRoot(siteRoot);

            siteRoot.Folder = @"c:\web\subfolder\..";

            Assert.IsTrue(_Configuration.IsSiteRootActive(siteRoot, true));
        }

        [TestMethod]
        public void FileSystemConfiguration_GetSiteRootFolders_Does_Not_Include_Default_Folder()
        {
            var folders = _Configuration.GetSiteRootFolders();
            Assert.AreEqual(0, folders.Count);
        }

        [TestMethod]
        public void FileSystemConfiguration_GetSiteRootFolders_Includes_Sites_Added_In_Order_Of_Priority()
        {
            var abcFolder = @"c:\web\abc";
            var xyzFolder = @"c:\web\xyz";

            _FileSystemProvider.AddFolder(abcFolder);
            _FileSystemProvider.AddFolder(xyzFolder);

            for(var i = 0;i < 2;++i) {
                var expectAbcFirst = i % 2 == 0;
                var abcSiteRoot = new SiteRoot() { Folder = abcFolder, Priority = expectAbcFirst ? -1 : 1 };
                var xyzSiteRoot = new SiteRoot() { Folder = xyzFolder, Priority = expectAbcFirst ? 1 : -1 };
                _Configuration.AddSiteRoot(abcSiteRoot);
                _Configuration.AddSiteRoot(xyzSiteRoot);

                var folders = _Configuration.GetSiteRootFolders();
                Assert.AreEqual(2, folders.Count);
                var abcIsFirst = folders[0].ToLower().TrimEnd('\\', '/') == abcFolder.ToLower();

                Assert.AreEqual(expectAbcFirst, abcIsFirst);
                _Configuration.RemoveSiteRoot(abcSiteRoot);
                _Configuration.RemoveSiteRoot(xyzSiteRoot);
            }
        }

        [TestMethod]
        public void FileSystemConfiguration_RaiseTextLoadedFromFile_Raises_TextLoadedFromFile()
        {
            var eventRecorder = new EventRecorder<TextContentEventArgs>();
            _Configuration.TextLoadedFromFile += eventRecorder.Handler;

            var args = new TextContentEventArgs(null, null, null, null);
            _Configuration.RaiseTextLoadedFromFile(args);

            Assert.AreEqual(1, eventRecorder.CallCount);
            Assert.AreSame(_Configuration, eventRecorder.Sender);
            Assert.AreSame(args, eventRecorder.Args);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileSystemConfiguration_IsFileUnmodified_Throws_If_SiteRootFolder_Is_Null()
        {
            _Configuration.IsFileUnmodified(null, "/", new byte[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileSystemConfiguration_IsFileUnmodified_Throws_If_RequestPath_Is_Null()
        {
            _Configuration.IsFileUnmodified(@"c:\web", null, new byte[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileSystemConfiguration_IsFileUnmodified_Throws_If_Content_Is_Null()
        {
            _Configuration.IsFileUnmodified(@"c:\web", "/", null);
        }

        private ChecksumFileEntry CreateChecksummedFile(string webRoot, string pathFromWebRoot, byte[] content)
        {
            var fileName = Path.Combine(webRoot, pathFromWebRoot);
            _FileSystemProvider.AddOrOverwriteFile(fileName, content);

            var fullPathFromRoot = (pathFromWebRoot[0] == '\\' ? pathFromWebRoot : "\\" + pathFromWebRoot);

            var result = new ChecksumFileEntry() {
                FileName = fullPathFromRoot,
                FileSize = content.Length,
                Checksum = ChecksumFileEntry.GenerateChecksum(content),
            };

            return result;
        }

        private ChecksumFileEntry CreateChecksummedFile(string webRoot, string pathFromWebRoot, string content)
        {
            return CreateChecksummedFile(webRoot, pathFromWebRoot, Encoding.UTF8.GetBytes(content));
        }

        [TestMethod]
        public void FileSystemConfiguration_IsFileUnmodified_Returns_False_If_File_Modified()
        {
            var webRoot = @"c:\web";

            _Configuration.AddSiteRoot(new SiteRoot() {
                Folder = webRoot,
                Checksums = {
                    CreateChecksummedFile(webRoot, "File.txt", "Old Content"),
                }
            });

            var newBytes = Encoding.UTF8.GetBytes("New content");
            _FileSystemProvider.OverwriteFile($@"{webRoot}\File.txt", newBytes);

            var isUnmodified = _Configuration.IsFileUnmodified(webRoot, "/file.txt", newBytes);
            Assert.IsFalse(isUnmodified);
        }

        [TestMethod]
        public void FileSystemConfiguration_IsFileUnmodified_Returns_True_If_Modified_Was_Not_Checksummed()
        {
            var webRoot = @"c:\web";

            CreateChecksummedFile(webRoot, "File.txt", "Old Content");

            _Configuration.AddSiteRoot(new SiteRoot() {
                Folder = webRoot,
            });

            var newBytes = Encoding.UTF8.GetBytes("New content");
            _FileSystemProvider.OverwriteFile($@"{webRoot}\File.txt", newBytes);

            var isUnmodified = _Configuration.IsFileUnmodified(webRoot, "/file.txt", newBytes);
            Assert.IsTrue(isUnmodified);
        }

        [TestMethod]
        public void FileSystemConfiguration_IsFileUnmodified_Returns_False_If_File_Is_In_Protected_Root_Without_Checksum()
        {
            var webRoot = @"c:\web";

            _Configuration.AddSiteRoot(new SiteRoot() {
                Folder = webRoot,
                Checksums = {
                    // Having a checksum for a file in the web root indicates that all files under the web root
                    // are checksummed. Serving files from the root without a checksum is not allowed.
                    CreateChecksummedFile(webRoot, "SomeOtherFile.txt", "Opportunity Three"),
                }
            });

            var isUnmodified = _Configuration.IsFileUnmodified(webRoot, "/file.txt", Encoding.UTF8.GetBytes("The Only One I Know"));
            Assert.IsFalse(isUnmodified);
        }
    }
}
