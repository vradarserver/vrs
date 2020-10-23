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
using VirtualRadar.Interface.StateHistory;

namespace Test.VirtualRadar.Interface.StateHistory
{
    [TestClass]
    public class ReceiverSnapshot_Tests
    {
        [TestMethod]
        public void TakeFingerprint_Uses_Correct_Properties()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { Snapshot = new ReceiverSnapshot() {
                        ReceiverSnapshotID =    1,
                        ReceiverID =            2,
                        Key =                   Guid.Parse("dfad14f3-6491-41fe-a51f-9900c5c1e8b6"),
                        ReceiverName =          "Po",
                        CreatedUtc =            new DateTime(2020, 1, 1),
                        Fingerprint =           new byte[] { 1 },
                    },
                    Expected = "48c2e875fe9de43536687aae9abc74e73182e3ac"     // 2\nDFAD14F3-6491-41FE-A51F-9900C5C1E8B6\nPo\n
                }
            }, row => {
                var componentFingerprint = ReceiverSnapshot.TakeFingerprint(
                    row.Snapshot.ReceiverID,
                    row.Snapshot.Key,
                    row.Snapshot.ReceiverName
                );

                row.Snapshot.TakeFingerprint();

                Assert.AreEqual(row.Expected, row.Snapshot.FingerprintHex, ignoreCase: true);
                Assert.IsTrue(row.Snapshot.Fingerprint.SequenceEqual(componentFingerprint));
            });
        }
    }
}
