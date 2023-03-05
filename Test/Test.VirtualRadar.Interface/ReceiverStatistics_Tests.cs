// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.ModeS;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class ReceiverStatistics_Tests
    {
        private ReceiverStatistics _Statistics;

        [TestInitialize]
        public void TestInitialise()
        {
            _Statistics = new ReceiverStatistics();
        }

        [TestMethod]
        public void Ctor_Initialises_To_Known_State_And_Properties_Work()
        {
            Assert.IsTrue(_Statistics.ModeSDFStatistics.Length == 32);
            foreach(DownlinkFormat df in Enum.GetValues(typeof(DownlinkFormat))) {
                var modeSDFStatistic = _Statistics.ModeSDFStatistics[(int)df];
                Assert.AreEqual(df, modeSDFStatistic.DF);
            }

            for(var i = 0;i < 256;++i) {
                Assert.AreEqual(0, _Statistics.AdsbTypeCount[i]);
            }

            foreach(MessageFormat mf in Enum.GetValues(typeof(MessageFormat))) {
                Assert.AreEqual(0, _Statistics.AdsbMessageFormatCount[(int)mf]);
            }
        }

        [TestMethod]
        public void ResetMessageCounters_Resets_Counters()
        {
            foreach(var property in typeof(ReceiverStatistics).GetProperties()) {
                switch(property.Name) {
                    case nameof(ReceiverStatistics.AdsbAircraftTracked):                                _Statistics.AdsbAircraftTracked = 1; break;
                    case nameof(ReceiverStatistics.AdsbCount):                                          _Statistics.AdsbCount = 1; break;
                    case nameof(ReceiverStatistics.AdsbMessageFormatCount):                             _Statistics.AdsbMessageFormatCount[0] = 1; break;
                    case nameof(ReceiverStatistics.AdsbPositionsExceededSpeedCheck):                    _Statistics.AdsbPositionsExceededSpeedCheck = 1; break;
                    case nameof(ReceiverStatistics.AdsbPositionsOutsideRange):                          _Statistics.AdsbPositionsOutsideRange = 1; break;
                    case nameof(ReceiverStatistics.AdsbPositionsReset):                                 _Statistics.AdsbPositionsReset = 1; break;
                    case nameof(ReceiverStatistics.AdsbRejected):                                       _Statistics.AdsbRejected = 1; break;
                    case nameof(ReceiverStatistics.AdsbTypeCount):                                      _Statistics.AdsbTypeCount[0] = 1; break;
                    case nameof(ReceiverStatistics.BaseStationBadFormatMessagesReceived):               _Statistics.BaseStationBadFormatMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.BaseStationMessagesReceived):                        _Statistics.BaseStationMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.BytesReceived):                                      _Statistics.BytesReceived = 1; break;
                    case nameof(ReceiverStatistics.ConnectionTimeUtc):                                  _Statistics.ConnectionTimeUtc = DateTime.UtcNow; break;
                    case nameof(ReceiverStatistics.CurrentBufferSize):                                  _Statistics.CurrentBufferSize = 1; break;
                    case nameof(ReceiverStatistics.FailedChecksumMessages):                             _Statistics.FailedChecksumMessages = 1; break;
                    case nameof(ReceiverStatistics.ModeSDFStatistics):                                  _Statistics.ModeSDFStatistics[0].MessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.ModeSLongFrameMessagesReceived):                     _Statistics.ModeSLongFrameMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.ModeSMessagesReceived):                              _Statistics.ModeSMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.ModeSNotAdsbCount):                                  _Statistics.ModeSNotAdsbCount = 1; break;
                    case nameof(ReceiverStatistics.ModeSShortFrameMessagesReceived):                    _Statistics.ModeSShortFrameMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.ModeSShortFrameWithoutLongFrameMessagesReceived):    _Statistics.ModeSShortFrameWithoutLongFrameMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.ModeSWithBadParityPIField):                          _Statistics.ModeSWithBadParityPIField = 1; break;
                    case nameof(ReceiverStatistics.ModeSWithPIField):                                   _Statistics.ModeSWithPIField = 1; break;
                    default:                                                                            throw new NotImplementedException();
                }
            }

            _Statistics.ResetMessageCounters();

            foreach(var property in typeof(ReceiverStatistics).GetProperties()) {
                switch(property.Name) {
                    case nameof(ReceiverStatistics.AdsbAircraftTracked):                                Assert.AreEqual(0L, _Statistics.AdsbAircraftTracked); break;
                    case nameof(ReceiverStatistics.AdsbCount):                                          Assert.AreEqual(0L, _Statistics.AdsbCount); break;
                    case nameof(ReceiverStatistics.AdsbMessageFormatCount):                             Assert.AreEqual(0L, _Statistics.AdsbMessageFormatCount[0]); break;
                    case nameof(ReceiverStatistics.AdsbPositionsExceededSpeedCheck):                    Assert.AreEqual(0L, _Statistics.AdsbPositionsExceededSpeedCheck); break;
                    case nameof(ReceiverStatistics.AdsbPositionsOutsideRange):                          Assert.AreEqual(0L, _Statistics.AdsbPositionsOutsideRange); break;
                    case nameof(ReceiverStatistics.AdsbPositionsReset):                                 Assert.AreEqual(0L, _Statistics.AdsbPositionsReset); break;
                    case nameof(ReceiverStatistics.AdsbRejected):                                       Assert.AreEqual(0L, _Statistics.AdsbRejected); break;
                    case nameof(ReceiverStatistics.AdsbTypeCount):                                      Assert.AreEqual(0L, _Statistics.AdsbTypeCount[0]); break;
                    case nameof(ReceiverStatistics.BaseStationBadFormatMessagesReceived):               Assert.AreEqual(0L, _Statistics.BaseStationBadFormatMessagesReceived); break;
                    case nameof(ReceiverStatistics.BaseStationMessagesReceived):                        Assert.AreEqual(0L, _Statistics.BaseStationMessagesReceived); break;
                    case nameof(ReceiverStatistics.BytesReceived):                                      Assert.AreEqual(1L, _Statistics.BytesReceived); break;
                    case nameof(ReceiverStatistics.ConnectionTimeUtc):                                  Assert.IsNotNull(_Statistics.ConnectionTimeUtc); break;
                    case nameof(ReceiverStatistics.CurrentBufferSize):                                  Assert.AreEqual(0L, _Statistics.CurrentBufferSize); break;
                    case nameof(ReceiverStatistics.FailedChecksumMessages):                             Assert.AreEqual(0L, _Statistics.FailedChecksumMessages); break;
                    case nameof(ReceiverStatistics.ModeSDFStatistics):                                  Assert.AreEqual(0L, _Statistics.ModeSDFStatistics[0].MessagesReceived); break;
                    case nameof(ReceiverStatistics.ModeSLongFrameMessagesReceived):                     Assert.AreEqual(0L, _Statistics.ModeSLongFrameMessagesReceived); break;
                    case nameof(ReceiverStatistics.ModeSMessagesReceived):                              Assert.AreEqual(0L, _Statistics.ModeSMessagesReceived); break;
                    case nameof(ReceiverStatistics.ModeSNotAdsbCount):                                  Assert.AreEqual(0L, _Statistics.ModeSNotAdsbCount); break;
                    case nameof(ReceiverStatistics.ModeSShortFrameMessagesReceived):                    Assert.AreEqual(0L, _Statistics.ModeSShortFrameMessagesReceived); break;
                    case nameof(ReceiverStatistics.ModeSShortFrameWithoutLongFrameMessagesReceived):    Assert.AreEqual(0L, _Statistics.ModeSShortFrameWithoutLongFrameMessagesReceived); break;
                    case nameof(ReceiverStatistics.ModeSWithBadParityPIField):                          Assert.AreEqual(0L, _Statistics.ModeSWithBadParityPIField); break;
                    case nameof(ReceiverStatistics.ModeSWithPIField):                                   Assert.AreEqual(0L, _Statistics.ModeSWithPIField); break;
                    default:                                                                            throw new NotImplementedException();
                }
            }
        }

        [TestMethod]
        public void ResetConnectionStatistics_Resets_Connection_Statistics()
        {
            foreach(var property in typeof(ReceiverStatistics).GetProperties()) {
                switch(property.Name) {
                    case nameof(ReceiverStatistics.AdsbAircraftTracked):                                _Statistics.AdsbAircraftTracked = 1; break;
                    case nameof(ReceiverStatistics.AdsbCount):                                          _Statistics.AdsbCount = 1; break;
                    case nameof(ReceiverStatistics.AdsbMessageFormatCount):                             _Statistics.AdsbMessageFormatCount[0] = 1; break;
                    case nameof(ReceiverStatistics.AdsbPositionsExceededSpeedCheck):                    _Statistics.AdsbPositionsExceededSpeedCheck = 1; break;
                    case nameof(ReceiverStatistics.AdsbPositionsOutsideRange):                          _Statistics.AdsbPositionsOutsideRange = 1; break;
                    case nameof(ReceiverStatistics.AdsbPositionsReset):                                 _Statistics.AdsbPositionsReset = 1; break;
                    case nameof(ReceiverStatistics.AdsbRejected):                                       _Statistics.AdsbRejected = 1; break;
                    case nameof(ReceiverStatistics.AdsbTypeCount):                                      _Statistics.AdsbTypeCount[0] = 1; break;
                    case nameof(ReceiverStatistics.BaseStationBadFormatMessagesReceived):               _Statistics.BaseStationBadFormatMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.BaseStationMessagesReceived):                        _Statistics.BaseStationMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.BytesReceived):                                      _Statistics.BytesReceived = 1; break;
                    case nameof(ReceiverStatistics.ConnectionTimeUtc):                                  _Statistics.ConnectionTimeUtc = DateTime.UtcNow; break;
                    case nameof(ReceiverStatistics.CurrentBufferSize):                                  _Statistics.CurrentBufferSize = 1L; break;
                    case nameof(ReceiverStatistics.FailedChecksumMessages):                             _Statistics.FailedChecksumMessages = 1; break;
                    case nameof(ReceiverStatistics.ModeSDFStatistics):                                  _Statistics.ModeSDFStatistics[0].MessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.ModeSLongFrameMessagesReceived):                     _Statistics.ModeSLongFrameMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.ModeSMessagesReceived):                              _Statistics.ModeSMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.ModeSNotAdsbCount):                                  _Statistics.ModeSNotAdsbCount = 1; break;
                    case nameof(ReceiverStatistics.ModeSShortFrameMessagesReceived):                    _Statistics.ModeSShortFrameMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.ModeSShortFrameWithoutLongFrameMessagesReceived):    _Statistics.ModeSShortFrameWithoutLongFrameMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.ModeSWithBadParityPIField):                          _Statistics.ModeSWithBadParityPIField = 1; break;
                    case nameof(ReceiverStatistics.ModeSWithPIField):                                   _Statistics.ModeSWithPIField = 1; break;
                    default:                                                                            throw new NotImplementedException();
                }
            }

            _Statistics.ResetConnectionStatistics();

            foreach(var property in typeof(ReceiverStatistics).GetProperties()) {
                switch(property.Name) {
                    case nameof(ReceiverStatistics.AdsbAircraftTracked):                                Assert.AreEqual(1L, _Statistics.AdsbAircraftTracked); break;
                    case nameof(ReceiverStatistics.AdsbCount):                                          Assert.AreEqual(1L, _Statistics.AdsbCount); break;
                    case nameof(ReceiverStatistics.AdsbMessageFormatCount):                             Assert.AreEqual(1L, _Statistics.AdsbMessageFormatCount[0]); break;
                    case nameof(ReceiverStatistics.AdsbPositionsExceededSpeedCheck):                    Assert.AreEqual(1L, _Statistics.AdsbPositionsExceededSpeedCheck); break;
                    case nameof(ReceiverStatistics.AdsbPositionsOutsideRange):                          Assert.AreEqual(1L, _Statistics.AdsbPositionsOutsideRange); break;
                    case nameof(ReceiverStatistics.AdsbPositionsReset):                                 Assert.AreEqual(1L, _Statistics.AdsbPositionsReset); break;
                    case nameof(ReceiverStatistics.AdsbRejected):                                       Assert.AreEqual(1L, _Statistics.AdsbRejected); break;
                    case nameof(ReceiverStatistics.AdsbTypeCount):                                      Assert.AreEqual(1L, _Statistics.AdsbTypeCount[0]); break;
                    case nameof(ReceiverStatistics.BaseStationBadFormatMessagesReceived):               Assert.AreEqual(1L, _Statistics.BaseStationBadFormatMessagesReceived); break;
                    case nameof(ReceiverStatistics.BaseStationMessagesReceived):                        Assert.AreEqual(1L, _Statistics.BaseStationMessagesReceived); break;
                    case nameof(ReceiverStatistics.BytesReceived):                                      Assert.AreEqual(0L, _Statistics.BytesReceived); break;
                    case nameof(ReceiverStatistics.ConnectionTimeUtc):                                  Assert.IsNull(_Statistics.ConnectionTimeUtc); break;
                    case nameof(ReceiverStatistics.CurrentBufferSize):                                  Assert.AreEqual(1L, _Statistics.CurrentBufferSize); break;
                    case nameof(ReceiverStatistics.FailedChecksumMessages):                             Assert.AreEqual(1L, _Statistics.FailedChecksumMessages); break;
                    case nameof(ReceiverStatistics.ModeSDFStatistics):                                  Assert.AreEqual(1L, _Statistics.ModeSDFStatistics[0].MessagesReceived); break;
                    case nameof(ReceiverStatistics.ModeSLongFrameMessagesReceived):                     Assert.AreEqual(1L, _Statistics.ModeSLongFrameMessagesReceived); break;
                    case nameof(ReceiverStatistics.ModeSMessagesReceived):                              Assert.AreEqual(1L, _Statistics.ModeSMessagesReceived); break;
                    case nameof(ReceiverStatistics.ModeSNotAdsbCount):                                  Assert.AreEqual(1L, _Statistics.ModeSNotAdsbCount); break;
                    case nameof(ReceiverStatistics.ModeSShortFrameMessagesReceived):                    Assert.AreEqual(1L, _Statistics.ModeSShortFrameMessagesReceived); break;
                    case nameof(ReceiverStatistics.ModeSShortFrameWithoutLongFrameMessagesReceived):    Assert.AreEqual(1L, _Statistics.ModeSShortFrameWithoutLongFrameMessagesReceived); break;
                    case nameof(ReceiverStatistics.ModeSWithBadParityPIField):                          Assert.AreEqual(1L, _Statistics.ModeSWithBadParityPIField); break;
                    case nameof(ReceiverStatistics.ModeSWithPIField):                                   Assert.AreEqual(1L, _Statistics.ModeSWithPIField); break;
                    default:                                                                            throw new NotImplementedException();
                }
            }
        }

        [TestMethod]
        public void Clone_Creates_Copy_Of_Original()
        {
            var now = DateTime.UtcNow;

            foreach(var property in typeof(ReceiverStatistics).GetProperties()) {
                switch(property.Name) {
                    case nameof(ReceiverStatistics.AdsbAircraftTracked):                                _Statistics.AdsbAircraftTracked = 1; break;
                    case nameof(ReceiverStatistics.AdsbCount):                                          _Statistics.AdsbCount = 1; break;
                    case nameof(ReceiverStatistics.AdsbMessageFormatCount):                             _Statistics.AdsbMessageFormatCount[0] = 1; break;
                    case nameof(ReceiverStatistics.AdsbPositionsExceededSpeedCheck):                    _Statistics.AdsbPositionsExceededSpeedCheck = 1; break;
                    case nameof(ReceiverStatistics.AdsbPositionsOutsideRange):                          _Statistics.AdsbPositionsOutsideRange = 1; break;
                    case nameof(ReceiverStatistics.AdsbPositionsReset):                                 _Statistics.AdsbPositionsReset = 1; break;
                    case nameof(ReceiverStatistics.AdsbRejected):                                       _Statistics.AdsbRejected = 1; break;
                    case nameof(ReceiverStatistics.AdsbTypeCount):                                      _Statistics.AdsbTypeCount[0] = 1; break;
                    case nameof(ReceiverStatistics.BaseStationBadFormatMessagesReceived):               _Statistics.BaseStationBadFormatMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.BaseStationMessagesReceived):                        _Statistics.BaseStationMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.BytesReceived):                                      _Statistics.BytesReceived = 1; break;
                    case nameof(ReceiverStatistics.ConnectionTimeUtc):                                  _Statistics.ConnectionTimeUtc = now; break;
                    case nameof(ReceiverStatistics.CurrentBufferSize):                                  _Statistics.CurrentBufferSize = 1; break;
                    case nameof(ReceiverStatistics.FailedChecksumMessages):                             _Statistics.FailedChecksumMessages = 1; break;
                    case nameof(ReceiverStatistics.ModeSDFStatistics):                                  _Statistics.ModeSDFStatistics[0].MessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.ModeSLongFrameMessagesReceived):                     _Statistics.ModeSLongFrameMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.ModeSMessagesReceived):                              _Statistics.ModeSMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.ModeSNotAdsbCount):                                  _Statistics.ModeSNotAdsbCount = 1; break;
                    case nameof(ReceiverStatistics.ModeSShortFrameMessagesReceived):                    _Statistics.ModeSShortFrameMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.ModeSShortFrameWithoutLongFrameMessagesReceived):    _Statistics.ModeSShortFrameWithoutLongFrameMessagesReceived = 1; break;
                    case nameof(ReceiverStatistics.ModeSWithBadParityPIField):                          _Statistics.ModeSWithBadParityPIField = 1; break;
                    case nameof(ReceiverStatistics.ModeSWithPIField):                                   _Statistics.ModeSWithPIField = 1; break;
                    default:                                                                            throw new NotImplementedException();
                }
            }

            var clone = _Statistics.Clone();

            foreach(var property in typeof(ReceiverStatistics).GetProperties()) {
                switch(property.Name) {
                    case nameof(ReceiverStatistics.AdsbAircraftTracked):                                Assert.AreEqual(1L, clone.AdsbAircraftTracked); break;
                    case nameof(ReceiverStatistics.AdsbCount):                                          Assert.AreEqual(1L, clone.AdsbCount); break;
                    case nameof(ReceiverStatistics.AdsbMessageFormatCount):                             Assert.AreEqual(1L, clone.AdsbMessageFormatCount[0]); break;
                    case nameof(ReceiverStatistics.AdsbPositionsExceededSpeedCheck):                    Assert.AreEqual(1L, clone.AdsbPositionsExceededSpeedCheck); break;
                    case nameof(ReceiverStatistics.AdsbPositionsOutsideRange):                          Assert.AreEqual(1L, clone.AdsbPositionsOutsideRange); break;
                    case nameof(ReceiverStatistics.AdsbPositionsReset):                                 Assert.AreEqual(1L, clone.AdsbPositionsReset); break;
                    case nameof(ReceiverStatistics.AdsbRejected):                                       Assert.AreEqual(1L, clone.AdsbRejected); break;
                    case nameof(ReceiverStatistics.AdsbTypeCount):                                      Assert.AreEqual(1L, clone.AdsbTypeCount[0]); break;
                    case nameof(ReceiverStatistics.BaseStationBadFormatMessagesReceived):               Assert.AreEqual(1L, clone.BaseStationBadFormatMessagesReceived); break;
                    case nameof(ReceiverStatistics.BaseStationMessagesReceived):                        Assert.AreEqual(1L, clone.BaseStationMessagesReceived); break;
                    case nameof(ReceiverStatistics.BytesReceived):                                      Assert.AreEqual(1L, clone.BytesReceived); break;
                    case nameof(ReceiverStatistics.ConnectionTimeUtc):                                  Assert.AreEqual(now, clone.ConnectionTimeUtc); break;
                    case nameof(ReceiverStatistics.CurrentBufferSize):                                  Assert.AreEqual(1L, clone.CurrentBufferSize); break;
                    case nameof(ReceiverStatistics.FailedChecksumMessages):                             Assert.AreEqual(1L, clone.FailedChecksumMessages); break;
                    case nameof(ReceiverStatistics.ModeSDFStatistics):                                  Assert.AreEqual(1L, clone.ModeSDFStatistics[0].MessagesReceived); break;
                    case nameof(ReceiverStatistics.ModeSLongFrameMessagesReceived):                     Assert.AreEqual(1L, clone.ModeSLongFrameMessagesReceived); break;
                    case nameof(ReceiverStatistics.ModeSMessagesReceived):                              Assert.AreEqual(1L, clone.ModeSMessagesReceived); break;
                    case nameof(ReceiverStatistics.ModeSNotAdsbCount):                                  Assert.AreEqual(1L, clone.ModeSNotAdsbCount); break;
                    case nameof(ReceiverStatistics.ModeSShortFrameMessagesReceived):                    Assert.AreEqual(1L, clone.ModeSShortFrameMessagesReceived); break;
                    case nameof(ReceiverStatistics.ModeSShortFrameWithoutLongFrameMessagesReceived):    Assert.AreEqual(1L, clone.ModeSShortFrameWithoutLongFrameMessagesReceived); break;
                    case nameof(ReceiverStatistics.ModeSWithBadParityPIField):                          Assert.AreEqual(1L, clone.ModeSWithBadParityPIField); break;
                    case nameof(ReceiverStatistics.ModeSWithPIField):                                   Assert.AreEqual(1L, clone.ModeSWithPIField); break;
                    default:                                                                            throw new NotImplementedException();
                }
            }
        }

        [TestMethod]
        public void Clone_Deep_Copies_The_Mode_S_DF_Statistics()
        {
            for(var idx = 0;idx < _Statistics.ModeSDFStatistics.Length;++idx) {
                var stats = _Statistics.ModeSDFStatistics[idx];
                stats.BadParityPI = 100 + idx;
                stats.MessagesReceived = 200 + idx;
            }

            var clone = _Statistics.Clone();

            Assert.AreEqual(_Statistics.ModeSDFStatistics.Length, clone.ModeSDFStatistics.Length);
            for(var idx = 0;idx < _Statistics.ModeSDFStatistics.Length;++idx) {
                var target = clone.ModeSDFStatistics[idx];
                foreach(var property in typeof(ModeSDFStatistics).GetProperties()) {
                    switch(property.Name) {
                        case nameof(ModeSDFStatistics.BadParityPI):         Assert.AreEqual(100 + idx, target.BadParityPI); break;
                        case nameof(ModeSDFStatistics.DF):                  Assert.AreEqual((DownlinkFormat)idx, target.DF); break;
                        case nameof(ModeSDFStatistics.MessagesReceived):    Assert.AreEqual(200 + idx, target.MessagesReceived); break;
                        default:                                            throw new NotImplementedException();
                    }
                }
            }
        }
    }
}
