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
using System.IO;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class StreamHelperTests
    {
        private MemoryStream _SourceStream;
        private MemoryStream _DestinationStream;

        [TestInitialize]
        public void TestInitialise()
        {
            _SourceStream = new MemoryStream();
            _DestinationStream = new MemoryStream();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if(_SourceStream != null) _SourceStream.Dispose();
            if(_DestinationStream != null) _DestinationStream.Dispose();
            _SourceStream = _DestinationStream = null;
        }

        private void CopyTextToStream(Stream stream, string text, bool resetPosition = true)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
            if(resetPosition) stream.Position = 0;
        }

        private string ReadTextFromMemoryStream(MemoryStream stream, bool fromCurrentPosition = false)
        {
            if(!fromCurrentPosition) stream.Position = 0;
            var bytes = stream.ToArray();
            return Encoding.UTF8.GetString(bytes);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StreamHelper_CopyStream_Throws_If_The_Source_Stream_Is_Null()
        {
            StreamHelper.CopyStream(null, _DestinationStream);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StreamHelper_CopyStream_Throws_If_The_Destination_Stream_Is_Null()
        {
            StreamHelper.CopyStream(_SourceStream, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void StreamHelper_CopyStream_Throws_If_Buffer_Size_Is_Zero()
        {
            StreamHelper.CopyStream(_SourceStream, _DestinationStream, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void StreamHelper_CopyStream_Throws_If_Buffer_Size_Is_Negative()
        {
            StreamHelper.CopyStream(_SourceStream, _DestinationStream, -1);
        }

        [TestMethod]
        public void StreamHelper_CopyStream_Copies_Source_To_Destination()
        {
            CopyTextToStream(_SourceStream, "Hello");
            StreamHelper.CopyStream(_SourceStream, _DestinationStream);
            Assert.AreEqual("Hello", ReadTextFromMemoryStream(_DestinationStream));
        }

        [TestMethod]
        public void StreamHelper_CopyStream_Returns_The_Total_Number_Of_Bytes_Read()
        {
            CopyTextToStream(_SourceStream, "Hello");
            Assert.AreEqual(5, StreamHelper.CopyStream(_SourceStream, _DestinationStream, 2));
        }
    }
}
