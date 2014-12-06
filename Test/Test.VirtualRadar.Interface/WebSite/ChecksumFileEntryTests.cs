// Copyright © 2013 onwards, Andrew Whewell
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
using VirtualRadar.Interface.WebSite;
using Test.Framework;
using System.IO;

namespace Test.VirtualRadar.Interface.WebSite
{
    [TestClass]
    public class ChecksumFileEntryTests
    {
        #region Test initialise
        public TestContext TestContext { get; set; }
        private ChecksumFileEntry _Instance;

        [TestInitialize]
        public void TestInitialise()
        {
            _Instance = new ChecksumFileEntry();
        }
        #endregion

        #region Constructors
        [TestMethod]
        public void ChecksumFileEntry_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            TestUtilities.TestProperty(_Instance, r => r.Checksum, null, "ABC");
            TestUtilities.TestProperty(_Instance, r => r.FileName, null, "xyz");
            TestUtilities.TestProperty(_Instance, r => r.FileSize, 0L, 100L);
        }
        #endregion

        #region GenerateChecksum
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ChecksumFileEntry_GenerateChecksum_Throws_If_FileName_Is_Null()
        {
            ChecksumFileEntry.GenerateChecksum(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ChecksumFileEntry_GenerateChecksum_Throws_If_File_Is_Missing()
        {
            ChecksumFileEntry.GenerateChecksum("c:\\hoobawhatevertibbergibberoo");
        }

        [TestMethod]
        public void ChecksumFileEntry_GenerateChecksum_Generates_Correct_Checksum_For_Known_Content()
        {
            var fileName = Path.Combine(TestContext.TestDeploymentDir, "Dummy-Whatever-DeleteMe.txt");
            File.WriteAllText(fileName, "Don't change this");
            try {
                var checksum = ChecksumFileEntry.GenerateChecksum(fileName);
                Assert.AreEqual("4A71A99D584C95CD", checksum);
            } finally {
                File.Delete(fileName);
            }
        }
        #endregion

        #region GetFileSize
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ChecksumFileEntry_GetFileSize_Throws_If_FileName_Is_Null()
        {
            ChecksumFileEntry.GetFileSize(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ChecksumFileEntry_GetFileSize_Throws_If_File_Is_Missing()
        {
            ChecksumFileEntry.GetFileSize("c:\\hoobawhatevertibbergibberoo");
        }

        [TestMethod]
        public void ChecksumFileEntry_GetFileSize_Returns_Correct_Length_For_File()
        {
            var fileName = Path.Combine(TestContext.TestDeploymentDir, "Dummy-Whatever-DeleteMe.txt");
            File.WriteAllText(fileName, "Don't change this");
            try {
                var length = ChecksumFileEntry.GetFileSize(fileName);
                Assert.AreEqual(17, length);
            } finally {
                File.Delete(fileName);
            }
        }
        #endregion

        #region GetFileNameFromFullPath
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ChecksumFileEntry_GetFileNameFromFullPath_Throws_If_RootFolder_Is_Null()
        {
            ChecksumFileEntry.GetFileNameFromFullPath(null, "c:\\whatever");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ChecksumFileEntry_GetFileNameFromFullPath_Throws_If_FileName_Does_Not_Start_With_RootFolder()
        {
            ChecksumFileEntry.GetFileNameFromFullPath(@"c:\root", @"c:\rootatootoot\file");
        }

        [TestMethod]
        public void ChecksumFileEntry_GetFileNameFromFullPath_Returns_Correct_FileName()
        {
            var entry = ChecksumFileEntry.GetFileNameFromFullPath(@"c:\root", @"c:\root\file");
            Assert.AreEqual(@"\file", entry);
        }

        [TestMethod]
        public void ChecksumFileEntry_GetFileNameFromFullPath_Translates_Forward_Slashes_In_RootFolder()
        {
            var entry = ChecksumFileEntry.GetFileNameFromFullPath(@"/root", @"\root\file");
            Assert.AreEqual(@"\file", entry);
        }

        [TestMethod]
        public void ChecksumFileEntry_GetFileNameFromFullPath_Translates_Forward_Slashes_In_FileName()
        {
            var entry = ChecksumFileEntry.GetFileNameFromFullPath(@"\root", @"/root/file");
            Assert.AreEqual(@"\file", entry);
        }
        #endregion

        #region GetFullPathFromRoot
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ChecksumFileEntry_GetFullPathFromRoot_Throws_If_Root_Folder_Is_Null()
        {
            _Instance.FileName = @"\Hello.txt";
            _Instance.GetFullPathFromRoot(null);
        }

        [TestMethod]
        public void ChecksumFileEntry_GetFullPathFromRoot_Returns_Null_If_FileName_Is_Uninitialised()
        {
            Assert.IsNull(_Instance.GetFullPathFromRoot(@"c:\tmp"));
        }

        [TestMethod]
        public void ChecksumFileEntry_GetFullPathFromRoot_Joins_FileName_To_Root_Folder()
        {
            _Instance.FileName = @"\Hello.txt";
            Assert.AreEqual(@"c:\tmp\Hello.txt", _Instance.GetFullPathFromRoot(@"c:\tmp"));
        }
        #endregion
    }
}
