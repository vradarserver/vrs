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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class UriHelperTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void UriHelper_NormalisePathFromRoot_Returns_Correct_Value()
        {
            Assert.AreEqual("/", UriHelper.NormalisePathFromRoot(""));
            Assert.AreEqual("/", UriHelper.NormalisePathFromRoot(null));
            Assert.AreEqual("/A/", UriHelper.NormalisePathFromRoot("A"));
            Assert.AreEqual("/A/", UriHelper.NormalisePathFromRoot("A/"));
            Assert.AreEqual("/A/", UriHelper.NormalisePathFromRoot("/A"));
            Assert.AreEqual("/A/", UriHelper.NormalisePathFromRoot("/A/"));
        }

        [TestMethod]
        public void UriHelper_NormalisePathFromRoot_Can_Convert_To_Lowercase()
        {
            Assert.AreEqual("/a/", UriHelper.NormalisePathFromRoot("A", convertToLowerCase: true));
            Assert.AreEqual("/a/", UriHelper.NormalisePathFromRoot("A/", convertToLowerCase: true));
            Assert.AreEqual("/a/", UriHelper.NormalisePathFromRoot("/A", convertToLowerCase: true));
            Assert.AreEqual("/a/", UriHelper.NormalisePathFromRoot("/A/", convertToLowerCase: true));
        }

        [TestMethod]
        public void UriHelper_RelativePathToFullPath_Returns_Correct_Value()
        {
            Assert.AreEqual("", UriHelper.RelativePathToFull(null, null));
            Assert.AreEqual("", UriHelper.RelativePathToFull(null, ""));
            Assert.AreEqual("", UriHelper.RelativePathToFull("", null));
            Assert.AreEqual("a", UriHelper.RelativePathToFull(null, "a"));
            Assert.AreEqual("a", UriHelper.RelativePathToFull("", "a"));

            Assert.AreEqual("/relative", UriHelper.RelativePathToFull("/", "relative"));
            Assert.AreEqual("/relative", UriHelper.RelativePathToFull("/file", "relative"));
            Assert.AreEqual("/folder/relative", UriHelper.RelativePathToFull("/folder/", "relative"));
            Assert.AreEqual("/folder/relative/sub", UriHelper.RelativePathToFull("/folder/", "relative/sub"));
            Assert.AreEqual("/relative", UriHelper.RelativePathToFull("/folder/subfolder/", "/relative"));
            Assert.AreEqual("relative", UriHelper.RelativePathToFull("folder", "relative"));
            Assert.AreEqual("/folder/subfolder/../relative", UriHelper.RelativePathToFull("/folder/subfolder/", "../relative"));
        }
    }
}
