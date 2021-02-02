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
    public class AircraftSnapshot_Tests
    {
        [TestMethod]
        public void TakeFingerprint_Uses_Correct_Properties()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { Snapshot = new AircraftSnapshot() {
                          AircraftSnapshotID =  1,
                          CreatedUtc =          new DateTime(2020, 1, 1),
                          Fingerprint =         new byte[] { 1 },
                          Icao =                "Abc",
                          Registration =        "Def",
                          ModelSnapshotID =     2,
                          ConstructionNumber =  "Ghi",
                          YearBuilt =           "Jkl",
                          OperatorSnapshotID =  4,
                          CountrySnapshotID =   5,
                          IsMilitary =          true,
                          IsInteresting =       false,
                          UserNotes =           "Mno",
                          UserTag =             "Pqr",
                      },
                      Expected = "9667f4913c62e7e52c6f14424fbaa81d982d0f1d"     // Abc\nDef\n2\nGhi\nJkl\n4\n5\nTrue\nFalse\nMno\nPqr\n
                }
            }, row => {
                var componentFingerprint = AircraftSnapshot.TakeFingerprint(
                    row.Snapshot.Icao,
                    row.Snapshot.Registration,
                    row.Snapshot.ModelSnapshotID,
                    row.Snapshot.ConstructionNumber,
                    row.Snapshot.YearBuilt,
                    row.Snapshot.OperatorSnapshotID,
                    row.Snapshot.CountrySnapshotID,
                    row.Snapshot.IsMilitary,
                    row.Snapshot.IsInteresting,
                    row.Snapshot.UserNotes,
                    row.Snapshot.UserTag
                );

                row.Snapshot.TakeFingerprint();

                Assert.AreEqual(row.Expected, row.Snapshot.FingerprintHex, ignoreCase: true);
                Assert.IsTrue(row.Snapshot.Fingerprint.SequenceEqual(componentFingerprint));
            });
        }
    }
}
