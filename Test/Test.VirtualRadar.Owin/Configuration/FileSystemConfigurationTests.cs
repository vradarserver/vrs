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
using Test.Framework;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.Owin.Configuration
{
    [TestClass]
    public class FileSystemConfigurationTests
    {
        public TestContext TestContext { get; set; }
        private IFileSystemConfiguration _Configuration;

        [TestInitialize]
        public void TestInitialise()
        {
            _Configuration = Factory.Singleton.Resolve<IFileSystemConfiguration>();
        }

        [TestMethod]
        public void FileSystemConfiguration_Singleton_Is_Not_Null()
        {
            Assert.IsNotNull(_Configuration.Singleton);
        }

        [TestMethod]
        public void FileSystemConfiguration_Singleton_Is_Same_Instance_Across_All_Instances()
        {
            var other = Factory.Singleton.Resolve<IFileSystemConfiguration>();
            Assert.AreSame(_Configuration.Singleton, other.Singleton);
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
            _Configuration.AddSiteRoot(new SiteRoot() { Folder = TestContext.TestDeploymentDir });
            _Configuration.AddSiteRoot(new SiteRoot() { Folder = TestContext.TestDeploymentDir });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FileSystemConfiguration_AddSiteRoot_Is_Not_Case_Sensitive_When_Searching_For_Duplicate_Folders()
        {
            _Configuration.AddSiteRoot(new SiteRoot() { Folder = TestContext.TestDeploymentDir.ToUpper() });
            _Configuration.AddSiteRoot(new SiteRoot() { Folder = TestContext.TestDeploymentDir.ToLower() });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FileSystemConfiguration_AddSiteRoot_Flattens_Traversal_Folders_When_Searching_For_Duplicate_Folders()
        {
            var anotherWayToFolder = String.Format(@"{0}\..\{1}", TestContext.TestDeploymentDir, Path.GetFileName(TestContext.TestDeploymentDir));
            _Configuration.AddSiteRoot(new SiteRoot() { Folder = TestContext.TestDeploymentDir });
            _Configuration.AddSiteRoot(new SiteRoot() { Folder = anotherWayToFolder });
        }

        [TestMethod]
        public void FileSystemConfiguration_AddSiteRoot_Adds_Trailing_Directory_Separator_To_Folder()
        {
            var folder = TestContext.TestDeploymentDir;
            if(folder[folder.Length - 1] == '\\') folder = folder.Substring(0, folder.Length - 1);

            _Configuration.AddSiteRoot(new SiteRoot() { Folder = folder });

            var folders = _Configuration.GetSiteRootFolders();
            Assert.IsFalse(folders.Any(r => r.ToLower() == folder.ToLower()));
            Assert.IsTrue(folders.Any(r => r.ToLower() == (folder + '\\').ToLower()));
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
            var siteRoot = new SiteRoot() { Folder = TestContext.TestDeploymentDir };
            _Configuration.AddSiteRoot(siteRoot);
            _Configuration.RemoveSiteRoot(siteRoot);

            Assert.IsFalse(_Configuration.IsSiteRootActive(siteRoot, false));
            Assert.AreEqual(0, _Configuration.GetSiteRootFolders().Count);
        }

        [TestMethod]
        public void FileSystemConfiguration_RemoveSiteRoot_Ignores_Sites_Not_Added()
        {
            var siteRoot = new SiteRoot() { Folder = TestContext.TestDeploymentDir };
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
            var siteRoot = new SiteRoot() { Folder = TestContext.TestDeploymentDir };
            _Configuration.AddSiteRoot(siteRoot);
            Assert.IsTrue(_Configuration.IsSiteRootActive(siteRoot, false));
        }

        [TestMethod]
        public void FileSystemConfiguration_IsSiteRootActive_Returns_True_If_SiteRoot_Folder_Changed_And_Folders_Are_Ignored()
        {
            var siteRoot = new SiteRoot() { Folder = TestContext.TestDeploymentDir };
            _Configuration.AddSiteRoot(siteRoot);
            siteRoot.Folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Assert.IsTrue(_Configuration.IsSiteRootActive(siteRoot, false));
        }

        [TestMethod]
        public void FileSystemConfiguration_IsSiteRootActive_Returns_True_If_SiteRoot_Folder_Changed_And_Folders_Are_Significant()
        {
            var siteRoot = new SiteRoot() { Folder = TestContext.TestDeploymentDir };
            _Configuration.AddSiteRoot(siteRoot);
            siteRoot.Folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Assert.IsFalse(_Configuration.IsSiteRootActive(siteRoot, true));
        }

        [TestMethod]
        public void FileSystemConfiguration_IsSiteRootActive_Returns_True_If_Changed_Folder_Points_To_Original_Folder()
        {
            var siteRoot = new SiteRoot() { Folder = TestContext.TestDeploymentDir };
            _Configuration.AddSiteRoot(siteRoot);
            siteRoot.Folder = Path.Combine(siteRoot.Folder, String.Format(@"..\{0}", Path.GetFileName(siteRoot.Folder)));
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
            for(var i = 0;i < 2;++i) {
                var expectAbcFirst = i % 2 == 0;
                var abcFolder = $"{TestContext.TestDeploymentDir}\\StandingDataTest\\";
                var xyzFolder = $"{TestContext.TestDeploymentDir}\\SubFolder\\";
                var abcSiteRoot = new SiteRoot() { Folder = abcFolder, Priority = expectAbcFirst ? -1 : 1 };
                var xyzSiteRoot = new SiteRoot() { Folder = xyzFolder, Priority = expectAbcFirst ? 1 : -1 };
                _Configuration.AddSiteRoot(abcSiteRoot);
                _Configuration.AddSiteRoot(xyzSiteRoot);

                var folders = _Configuration.GetSiteRootFolders();
                Assert.AreEqual(2, folders.Count);
                var abcIsFirst = folders[0].ToLower() == abcFolder.ToLower();

                Assert.AreEqual(expectAbcFirst, abcIsFirst);
                _Configuration.RemoveSiteRoot(abcSiteRoot);
                _Configuration.RemoveSiteRoot(xyzSiteRoot);
            }
        }

        [TestMethod]
        public void FileSystemConfiguration_RaiseHtmlLoadedFromFile_Raises_HtmlLoadedFromFile()
        {
            var eventRecorder = new EventRecorder<TextContentEventArgs>();
            _Configuration.HtmlLoadedFromFile += eventRecorder.Handler;

            var args = new TextContentEventArgs(null, null, null);
            _Configuration.RaiseHtmlLoadedFromFile(args);

            Assert.AreEqual(1, eventRecorder.CallCount);
            Assert.AreSame(_Configuration, eventRecorder.Sender);
            Assert.AreSame(args, eventRecorder.Args);
        }
    }
}
