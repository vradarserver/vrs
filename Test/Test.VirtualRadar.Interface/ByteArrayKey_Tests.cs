// Copyright © 2020 onwards, Andrew Whewell
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
using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class ByteArrayKey_Tests
    {
        [TestMethod]
        public void Array_Ctor_Initialises_Array_Property()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { SourceArray = new byte[] { 1, 2, 3 }, },
                new { SourceArray = new byte[0] },
                new { SourceArray = (byte[])null },
            }, row => {
                var key = new ByteArrayKey(row.SourceArray);
                if(row.SourceArray == null) {
                    Assert.IsNull(key.Array);
                } else {
                    Assert.IsTrue(row.SourceArray.SequenceEqual(key.Array));
                }
            });
        }

        [TestMethod]
        public void Equals_Compares_Arrays_Correctly()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { Array1 = (byte[])null,            Array2 = (byte[])null,                                  ExpectEquals = true, },
                new { Array1 = new byte[0],             Array2 = (byte[])null,                                  ExpectEquals = false, },
                new { Array1 = new byte[0],             Array2 = new byte[0],                                   ExpectEquals = true, },
                new { Array1 = new byte[] { 1, 2, 3 },  Array2 = new byte[] { 3, 2, 1 }.Reverse().ToArray(),    ExpectEquals = true, },     // avoid two refs to the same array
                new { Array1 = new byte[] { 1, 2, 3 },  Array2 = new byte[] { 2, 3, 1 },                        ExpectEquals = false, },
                new { Array1 = new byte[] { 1, 2, 3 },  Array2 = new byte[] { 1, 2 },                           ExpectEquals = false, },
            }, row => {
                var key1 = new ByteArrayKey(row.Array1);
                var key2 = new ByteArrayKey(row.Array2);

                Assert.AreEqual(key1, key1);
                Assert.AreEqual(key2, key2);
                Assert.AreEqual(row.ExpectEquals, key1.Equals(key2));
                Assert.AreEqual(row.ExpectEquals, key2.Equals(key1));
            });
        }

        [TestMethod]
        public void GetHashCode_Returns_Reasonable_Hashcodes()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { Array = (byte[])null,                                                         Expected = int.MinValue, },
                new { Array = new byte[0],                                                          Expected = 0L.GetHashCode(), },
                new { Array = new byte[] { 0x01 },                                                  Expected = 1L.GetHashCode(), },
                new { Array = new byte[] { 0x01, 0x2f, 0x30, 0x41, 0x52, 0x63, 0x7e,  },            Expected = 0x012f304152637eL.GetHashCode(), },
                new { Array = new byte[] { 0x01, 0x2f, 0x30, 0x41, 0x52, 0x63, 0x7e, 0x87,  },      Expected = 0x012f304152637e87L.GetHashCode(), },
                new { Array = new byte[] { 0x01, 0x2f, 0x30, 0x41, 0x52, 0x63, 0x7e, 0x87, 0x98  }, Expected = 0x012f304152637e87L.GetHashCode(), },
            }, row => {
                var key = new ByteArrayKey(row.Array);
                var actual = key.GetHashCode();

                Assert.AreEqual(row.Expected, actual);
            });
        }
    }
}
