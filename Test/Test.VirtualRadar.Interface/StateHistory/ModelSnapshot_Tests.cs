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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.StateHistory;

namespace Test.VirtualRadar.Interface.StateHistory
{
    [TestClass]
    public class ModelSnapshot_Tests
    {
        [TestMethod]
        public void TakeFingerprint_Uses_Correct_Properties()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { Snapshot = new ModelSnapshot() {
                          ModelSnapshotID =                 1,
                          CreatedUtc =                      new DateTime(2020, 1, 1),
                          Fingerprint =                     new byte[] { 1 },
                          Icao =                            "Abc",
                          ModelName =                       "Def",
                          NumberOfEngines =                 "C",
                          ManufacturerSnapshotID =          11,
                          WakeTurbulenceCodeSnapshotID =    21,
                          EngineTypeSnapshotID =            37,
                          EnginePlacementSnapshotID =       4312,
                          SpeciesSnapshotID =               5,
                      },
                      Expected = "04a26e717ea4f3fc3bbfe4c16a9b68ee215d7445"     // Abc\nDef\nC\n11\n21\n37\n4312\n5\n
                }
            }, row => {
                var componentFingerprint = ModelSnapshot.TakeFingerprint(
                    row.Snapshot.Icao,
                    row.Snapshot.ModelName,
                    row.Snapshot.NumberOfEngines,
                    row.Snapshot.ManufacturerSnapshotID,
                    row.Snapshot.WakeTurbulenceCodeSnapshotID,
                    row.Snapshot.EngineTypeSnapshotID,
                    row.Snapshot.EnginePlacementSnapshotID,
                    row.Snapshot.SpeciesSnapshotID
                );

                row.Snapshot.TakeFingerprint();

                Assert.AreEqual(row.Expected, row.Snapshot.FingerprintHex, ignoreCase: true);
                Assert.IsTrue(row.Snapshot.Fingerprint.SequenceEqual(componentFingerprint));
            });
        }
    }
}
