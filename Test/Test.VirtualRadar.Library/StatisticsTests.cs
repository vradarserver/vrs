// Copyright © 2012 onwards, Andrew Whewell
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
using VirtualRadar.Interface;
using InterfaceFactory;
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Interface.Adsb;
using Test.Framework;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class StatisticsTests
    {
        private IStatistics _Statistics;

        [TestInitialize]
        public void TestInitialise()
        {
            _Statistics = Factory.Singleton.Resolve<IStatistics>();
        }

        [TestMethod]
        public void Statistics_Initialise_Initialises_To_Known_State_And_Properties_Work()
        {
            _Statistics.Initialise();

            foreach(DownlinkFormat df in Enum.GetValues(typeof(DownlinkFormat))) {
                Assert.AreEqual(0, _Statistics.ModeSDFCount[(int)df]);
            }

            for(var i = 0;i < 256;++i) {
                Assert.AreEqual(0, _Statistics.AdsbTypeCount[i]);
            }

            foreach(MessageFormat mf in Enum.GetValues(typeof(MessageFormat))) {
                Assert.AreEqual(0, _Statistics.AdsbMessageFormatCount[(int)mf]);
            }

            TestUtilities.TestProperty(_Statistics, r => r.AdsbAircraftTracked, 0L, 1L);
            TestUtilities.TestProperty(_Statistics, r => r.AdsbCount, 0L, 1L);
            TestUtilities.TestProperty(_Statistics, r => r.AdsbPositionsExceededSpeedCheck, 0L, 1L);
            TestUtilities.TestProperty(_Statistics, r => r.AdsbPositionsOutsideRange, 0L, 1L);
            TestUtilities.TestProperty(_Statistics, r => r.AdsbPositionsReset, 0L, 1L);
            TestUtilities.TestProperty(_Statistics, r => r.AdsbRejected, 0L, 1L);
            TestUtilities.TestProperty(_Statistics, r => r.BaseStationBadFormatMessagesReceived, 0L, 1L);
            TestUtilities.TestProperty(_Statistics, r => r.BaseStationMessagesReceived, 0L, 1L);
            TestUtilities.TestProperty(_Statistics, r => r.BytesReceived, 0L, 1L);
            TestUtilities.TestProperty(_Statistics, r => r.ConnectionTimeUtc, null, DateTime.UtcNow);
            TestUtilities.TestProperty(_Statistics, r => r.CurrentBufferSize, 0L, 1L);
            TestUtilities.TestProperty(_Statistics, r => r.FailedChecksumMessages, 0L, 1L);
            TestUtilities.TestProperty(_Statistics, r => r.ModeSLongFrameMessagesReceived, 0L, 1L);
            TestUtilities.TestProperty(_Statistics, r => r.ModeSMessagesReceived, 0L, 1L);
            TestUtilities.TestProperty(_Statistics, r => r.ModeSNotAdsbCount, 0L, 1L);
            TestUtilities.TestProperty(_Statistics, r => r.ModeSShortFrameMessagesReceived, 0L, 1L);
            TestUtilities.TestProperty(_Statistics, r => r.ModeSShortFrameWithoutLongFrameMessagesReceived, 0L, 1L);
            TestUtilities.TestProperty(_Statistics, r => r.ModeSWithBadParityPIField, 0L, 1L);
            TestUtilities.TestProperty(_Statistics, r => r.ModeSWithPIField, 0L, 1L);
        }

        [TestMethod]
        public void Statistics_Initialise_Does_Nothing_When_Called_Twice()
        {
            _Statistics.Initialise();
            _Statistics.AdsbTypeCount[0] = 1;
            _Statistics.Initialise();
            Assert.AreEqual(1, _Statistics.AdsbTypeCount[0]);
        }

        [TestMethod]
        public void Statistics_ResetMessageCounters_Resets_Counters()
        {
            _Statistics.Initialise();

            foreach(var property in typeof(IStatistics).GetProperties()) {
                switch(property.Name) {
                    case "AdsbAircraftTracked":                             _Statistics.AdsbAircraftTracked = 1; break;
                    case "AdsbCount":                                       _Statistics.AdsbCount = 1; break;
                    case "AdsbMessageFormatCount":                          _Statistics.AdsbMessageFormatCount[0] = 1; break;
                    case "AdsbPositionsExceededSpeedCheck":                 _Statistics.AdsbPositionsExceededSpeedCheck = 1; break;
                    case "AdsbPositionsOutsideRange":                       _Statistics.AdsbPositionsOutsideRange = 1; break;
                    case "AdsbPositionsReset":                              _Statistics.AdsbPositionsReset = 1; break;
                    case "AdsbRejected":                                    _Statistics.AdsbRejected = 1; break;
                    case "AdsbTypeCount":                                   _Statistics.AdsbTypeCount[0] = 1; break;
                    case "BaseStationBadFormatMessagesReceived":            _Statistics.BaseStationBadFormatMessagesReceived = 1; break;
                    case "BaseStationMessagesReceived":                     _Statistics.BaseStationMessagesReceived = 1; break;
                    case "BytesReceived":                                   _Statistics.BytesReceived = 1; break;
                    case "ConnectionTimeUtc":                               _Statistics.ConnectionTimeUtc = DateTime.UtcNow; break;
                    case "CurrentBufferSize":                               _Statistics.CurrentBufferSize = 1; break;
                    case "FailedChecksumMessages":                          _Statistics.FailedChecksumMessages = 1; break;
                    case "ModeSDFCount":                                    _Statistics.ModeSDFCount[0] = 1; break;
                    case "ModeSLongFrameMessagesReceived":                  _Statistics.ModeSLongFrameMessagesReceived = 1; break;
                    case "ModeSMessagesReceived":                           _Statistics.ModeSMessagesReceived = 1; break;
                    case "ModeSNotAdsbCount":                               _Statistics.ModeSNotAdsbCount = 1; break;
                    case "ModeSShortFrameMessagesReceived":                 _Statistics.ModeSShortFrameMessagesReceived = 1; break;
                    case "ModeSShortFrameWithoutLongFrameMessagesReceived": _Statistics.ModeSShortFrameWithoutLongFrameMessagesReceived = 1; break;
                    case "ModeSWithBadParityPIField":                       _Statistics.ModeSWithBadParityPIField = 1; break;
                    case "ModeSWithPIField":                                _Statistics.ModeSWithPIField = 1; break;
                    case "Singleton":                                       continue;
                    default:                                                throw new NotImplementedException();
                }
            }

            _Statistics.ResetMessageCounters();

            foreach(var property in typeof(IStatistics).GetProperties()) {
                switch(property.Name) {
                    case "AdsbAircraftTracked":                             Assert.AreEqual(0L, _Statistics.AdsbAircraftTracked); break;
                    case "AdsbCount":                                       Assert.AreEqual(0L, _Statistics.AdsbCount); break;
                    case "AdsbMessageFormatCount":                          Assert.AreEqual(0L, _Statistics.AdsbMessageFormatCount[0]); break;
                    case "AdsbPositionsExceededSpeedCheck":                 Assert.AreEqual(0L, _Statistics.AdsbPositionsExceededSpeedCheck); break;
                    case "AdsbPositionsOutsideRange":                       Assert.AreEqual(0L, _Statistics.AdsbPositionsOutsideRange); break;
                    case "AdsbPositionsReset":                              Assert.AreEqual(0L, _Statistics.AdsbPositionsReset); break;
                    case "AdsbRejected":                                    Assert.AreEqual(0L, _Statistics.AdsbRejected); break;
                    case "AdsbTypeCount":                                   Assert.AreEqual(0L, _Statistics.AdsbTypeCount[0]); break;
                    case "BaseStationBadFormatMessagesReceived":            Assert.AreEqual(0L, _Statistics.BaseStationBadFormatMessagesReceived); break;
                    case "BaseStationMessagesReceived":                     Assert.AreEqual(0L, _Statistics.BaseStationMessagesReceived); break;
                    case "BytesReceived":                                   Assert.AreEqual(1L, _Statistics.BytesReceived); break;
                    case "ConnectionTimeUtc":                               Assert.IsNotNull(_Statistics.ConnectionTimeUtc); break;
                    case "CurrentBufferSize":                               Assert.AreEqual(0L, _Statistics.CurrentBufferSize); break;
                    case "FailedChecksumMessages":                          Assert.AreEqual(0L, _Statistics.FailedChecksumMessages); break;
                    case "ModeSDFCount":                                    Assert.AreEqual(0L, _Statistics.ModeSDFCount[0]); break;
                    case "ModeSLongFrameMessagesReceived":                  Assert.AreEqual(0L, _Statistics.ModeSLongFrameMessagesReceived); break;
                    case "ModeSMessagesReceived":                           Assert.AreEqual(0L, _Statistics.ModeSMessagesReceived); break;
                    case "ModeSNotAdsbCount":                               Assert.AreEqual(0L, _Statistics.ModeSNotAdsbCount); break;
                    case "ModeSShortFrameMessagesReceived":                 Assert.AreEqual(0L, _Statistics.ModeSShortFrameMessagesReceived); break;
                    case "ModeSShortFrameWithoutLongFrameMessagesReceived": Assert.AreEqual(0L, _Statistics.ModeSShortFrameWithoutLongFrameMessagesReceived); break;
                    case "ModeSWithBadParityPIField":                       Assert.AreEqual(0L, _Statistics.ModeSWithBadParityPIField); break;
                    case "ModeSWithPIField":                                Assert.AreEqual(0L, _Statistics.ModeSWithPIField); break;
                    case "Singleton":                                       continue;
                    default:                                                throw new NotImplementedException();
                }
            }
        }

        [TestMethod]
        public void Statistics_ResetConnectionStatistics_Resets_Connection_Statistics()
        {
            _Statistics.Initialise();

            foreach(var property in typeof(IStatistics).GetProperties()) {
                switch(property.Name) {
                    case "AdsbAircraftTracked":                             _Statistics.AdsbAircraftTracked = 1; break;
                    case "AdsbCount":                                       _Statistics.AdsbCount = 1; break;
                    case "AdsbMessageFormatCount":                          _Statistics.AdsbMessageFormatCount[0] = 1; break;
                    case "AdsbPositionsExceededSpeedCheck":                 _Statistics.AdsbPositionsExceededSpeedCheck = 1; break;
                    case "AdsbPositionsOutsideRange":                       _Statistics.AdsbPositionsOutsideRange = 1; break;
                    case "AdsbPositionsReset":                              _Statistics.AdsbPositionsReset = 1; break;
                    case "AdsbRejected":                                    _Statistics.AdsbRejected = 1; break;
                    case "AdsbTypeCount":                                   _Statistics.AdsbTypeCount[0] = 1; break;
                    case "BaseStationBadFormatMessagesReceived":            _Statistics.BaseStationBadFormatMessagesReceived = 1; break;
                    case "BaseStationMessagesReceived":                     _Statistics.BaseStationMessagesReceived = 1; break;
                    case "BytesReceived":                                   _Statistics.BytesReceived = 1; break;
                    case "ConnectionTimeUtc":                               _Statistics.ConnectionTimeUtc = DateTime.UtcNow; break;
                    case "CurrentBufferSize":                               _Statistics.CurrentBufferSize = 1L; break;
                    case "FailedChecksumMessages":                          _Statistics.FailedChecksumMessages = 1; break;
                    case "ModeSDFCount":                                    _Statistics.ModeSDFCount[0] = 1; break;
                    case "ModeSLongFrameMessagesReceived":                  _Statistics.ModeSLongFrameMessagesReceived = 1; break;
                    case "ModeSMessagesReceived":                           _Statistics.ModeSMessagesReceived = 1; break;
                    case "ModeSNotAdsbCount":                               _Statistics.ModeSNotAdsbCount = 1; break;
                    case "ModeSShortFrameMessagesReceived":                 _Statistics.ModeSShortFrameMessagesReceived = 1; break;
                    case "ModeSShortFrameWithoutLongFrameMessagesReceived": _Statistics.ModeSShortFrameWithoutLongFrameMessagesReceived = 1; break;
                    case "ModeSWithBadParityPIField":                       _Statistics.ModeSWithBadParityPIField = 1; break;
                    case "ModeSWithPIField":                                _Statistics.ModeSWithPIField = 1; break;
                    case "Singleton":                                       continue;
                    default:                                                throw new NotImplementedException();
                }
            }

            _Statistics.ResetConnectionStatistics();

            foreach(var property in typeof(IStatistics).GetProperties()) {
                switch(property.Name) {
                    case "AdsbAircraftTracked":                             Assert.AreEqual(1L, _Statistics.AdsbAircraftTracked); break;
                    case "AdsbCount":                                       Assert.AreEqual(1L, _Statistics.AdsbCount); break;
                    case "AdsbMessageFormatCount":                          Assert.AreEqual(1L, _Statistics.AdsbMessageFormatCount[0]); break;
                    case "AdsbPositionsExceededSpeedCheck":                 Assert.AreEqual(1L, _Statistics.AdsbPositionsExceededSpeedCheck); break;
                    case "AdsbPositionsOutsideRange":                       Assert.AreEqual(1L, _Statistics.AdsbPositionsOutsideRange); break;
                    case "AdsbPositionsReset":                              Assert.AreEqual(1L, _Statistics.AdsbPositionsReset); break;
                    case "AdsbRejected":                                    Assert.AreEqual(1L, _Statistics.AdsbRejected); break;
                    case "AdsbTypeCount":                                   Assert.AreEqual(1L, _Statistics.AdsbTypeCount[0]); break;
                    case "BaseStationBadFormatMessagesReceived":            Assert.AreEqual(1L, _Statistics.BaseStationBadFormatMessagesReceived); break;
                    case "BaseStationMessagesReceived":                     Assert.AreEqual(1L, _Statistics.BaseStationMessagesReceived); break;
                    case "BytesReceived":                                   Assert.AreEqual(0L, _Statistics.BytesReceived); break;
                    case "ConnectionTimeUtc":                               Assert.IsNull(_Statistics.ConnectionTimeUtc); break;
                    case "CurrentBufferSize":                               Assert.AreEqual(1L, _Statistics.CurrentBufferSize); break;
                    case "FailedChecksumMessages":                          Assert.AreEqual(1L, _Statistics.FailedChecksumMessages); break;
                    case "ModeSDFCount":                                    Assert.AreEqual(1L, _Statistics.ModeSDFCount[0]); break;
                    case "ModeSLongFrameMessagesReceived":                  Assert.AreEqual(1L, _Statistics.ModeSLongFrameMessagesReceived); break;
                    case "ModeSMessagesReceived":                           Assert.AreEqual(1L, _Statistics.ModeSMessagesReceived); break;
                    case "ModeSNotAdsbCount":                               Assert.AreEqual(1L, _Statistics.ModeSNotAdsbCount); break;
                    case "ModeSShortFrameMessagesReceived":                 Assert.AreEqual(1L, _Statistics.ModeSShortFrameMessagesReceived); break;
                    case "ModeSShortFrameWithoutLongFrameMessagesReceived": Assert.AreEqual(1L, _Statistics.ModeSShortFrameWithoutLongFrameMessagesReceived); break;
                    case "ModeSWithBadParityPIField":                       Assert.AreEqual(1L, _Statistics.ModeSWithBadParityPIField); break;
                    case "ModeSWithPIField":                                Assert.AreEqual(1L, _Statistics.ModeSWithPIField); break;
                    case "Singleton":                                       continue;
                    default:                                                throw new NotImplementedException();
                }
            }
        }

        [TestMethod]
        public void Statistics_ResetMessageCounters_Does_Not_Crash_If_Called_Before_Initialise()
        {
            _Statistics.ResetMessageCounters();
        }

        [TestMethod]
        public void Statistics_ResetConnectionStatistics_Does_Not_Crash_If_Called_Before_Initialise()
        {
            _Statistics.ResetConnectionStatistics();
        }
    }
}
