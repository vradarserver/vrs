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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;

namespace Test.VirtualRadar.Library.Presenter
{
    [TestClass]
    public class StatisticsPresenterTests
    {
        public TestContext TestContext { get; set; }

        private IClassFactory _ClassFactorySnapshot;
        private IStatisticsPresenter _Presenter;
        private Mock<IStatisticsView> _View;
        private Mock<IStatistics> _Statistics;
        private Mock<IHeartbeatService> _HeartbeatService;
        private ClockMock _Clock;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _Statistics = StatisticsHelper.CreateLockableStatistics();
            _HeartbeatService = TestUtilities.CreateMockSingleton<IHeartbeatService>();

            _Clock = new ClockMock();
            Factory.Singleton.RegisterInstance<IClock>(_Clock.Object);

            _Presenter = Factory.Singleton.Resolve<IStatisticsPresenter>();
            _View = TestUtilities.CreateMockInstance<IStatisticsView>();

            _View.Setup(r => r.Statistics).Returns(_Statistics.Object);
            _View.Setup(r => r.AdsbMessageTypeCount).Returns(new long[256]);
            _View.Setup(r => r.ModeSDFStatistics).Returns(new ModeSDFStatistics[32]);
            _View.Setup(r => r.AdsbMessageFormatCount).Returns(new long[Enum.GetValues(typeof(MessageFormat)).OfType<MessageFormat>().Select(r => (int)r).Max() + 1]);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }

        [TestMethod]
        public void StatisticsPresenter_Initialise_Calls_UpdateCounters_On_View()
        {
            _Presenter.Initialise(_View.Object);
            _View.Verify(r => r.UpdateCounters(), Times.Once());
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "StatisticsView$")]
        public void StatisticsPresenter_HeartbeatTick_Updates_View_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var viewProperty = worksheet.String("ViewProperty");
            var valueType = worksheet.String("ValueType");
            var statistic1 = worksheet.String("Statistic1");
            var statistic2 = worksheet.String("Statistic2");

            _Presenter.Initialise(_View.Object);

            switch(valueType) {
                case "Counter":
                    SetStatistic(statistic1, 6000000000L);

                    _HeartbeatService.Raise(r => r.FastTick += null, EventArgs.Empty);

                    Assert.AreEqual(6000000000L, GetViewLong(viewProperty), viewProperty);
                    _View.Verify(r => r.UpdateCounters(), Times.Exactly(2));
                    break;
                case "Duration":
                    var startTimeDuration = new DateTime(2013, 1, 1, 11, 17, 42);
                    _Clock.UtcNowValue = new DateTime(2013, 1, 2, 12, 0, 0);
                    var expectedDuration = _Clock.UtcNowValue - startTimeDuration;
                    SetStatistic(statistic1, startTimeDuration);

                    _HeartbeatService.Raise(r => r.FastTick += null, EventArgs.Empty);

                    Assert.AreEqual(expectedDuration, GetViewTimeSpan(viewProperty), viewProperty);
                    _View.Verify(r => r.UpdateCounters(), Times.Exactly(2));

                    // Confirm that a null connection time is handled correctly
                    SetStatistic(statistic1, null);
                    _Clock.UtcNowValue = _Clock.UtcNowValue.AddSeconds(1);
                    _HeartbeatService.Raise(r => r.FastTick += null, EventArgs.Empty);
                    Assert.AreEqual(TimeSpan.Zero, GetViewTimeSpan(viewProperty), viewProperty);
                    break;
                case "ListCounters":
                    var statCounters = (long[])typeof(IStatistics).GetProperty(statistic1).GetValue(_Statistics.Object, null);
                    var viewCounters = GetViewListCounters(viewProperty);

                    var length = statCounters.Count();
                    for(var i = 0;i < length;++i) {
                        statCounters[i] = (i + 1) * 4;
                    }

                    _HeartbeatService.Raise(r => r.FastTick += null, EventArgs.Empty);

                    for(var i = 0;i < length;++i) {
                        Assert.AreEqual((i + 1) * 4, viewCounters[i], "{0}[{1}]", viewProperty, i);
                    }
                    _View.Verify(r => r.UpdateCounters(), Times.Exactly(2));
                    break;
                case "ListObjects":
                    var statList = (IList)typeof(IStatistics).GetProperty(statistic1).GetValue(_Statistics.Object, null);
                    var viewList = GetViewListObjects(viewProperty);

                    var listLength = statList.Count;
                    Assert.IsTrue(listLength > 0);
                    var elementType = statList.GetType().GetElementType();
                    for(var i = 0;i < listLength;++i) {
                        if(elementType == typeof(ModeSDFStatistics)) {
                            statList[i] = new ModeSDFStatistics() {
                                DF = (DownlinkFormat)i,
                                BadParityPI = i + 1,
                                MessagesReceived = i - 1,
                            };
                        } else {
                            throw new NotImplementedException();
                        }
                    }

                    _HeartbeatService.Raise(r => r.FastTick += null, EventArgs.Empty);

                    for(var i = 0;i < listLength;++i) {
                        Assert.AreNotEqual(statList[i], viewList[i], "The view object must be a clone of the statistic object, not the actual statistic object");
                        if(elementType == typeof(ModeSDFStatistics)) {
                            var viewElement = (ModeSDFStatistics)viewList[i];
                            Assert.AreEqual((DownlinkFormat)i, viewElement.DF);
                            Assert.AreEqual(i + 1, viewElement.BadParityPI);
                            Assert.AreEqual(i - 1, viewElement.MessagesReceived);
                        } else {
                            throw new NotImplementedException();
                        }
                    }
                    break;
                case "Ratio":
                    SetStatistic(statistic1, 400L);
                    SetStatistic(statistic2, 100L);

                    _HeartbeatService.Raise(r => r.FastTick += null, EventArgs.Empty);

                    Assert.AreEqual(0.25, GetViewDouble(viewProperty), viewProperty);
                    _View.Verify(r => r.UpdateCounters(), Times.Exactly(2));

                    // Assert that a denominator of zero doesn't cause the ratio calculation to throw a divide by zero exception or produce a result of Infinity
                    SetStatistic(statistic1, 0L);
                    _Clock.UtcNowValue = _Clock.UtcNowValue.AddSeconds(1);
                    _HeartbeatService.Raise(r => r.FastTick += null, EventArgs.Empty);
                    Assert.AreEqual(0.0, GetViewDouble(viewProperty), viewProperty);

                    break;
                case "Throughput":
                    SetStatistic(statistic1, 2239488L);
                    var startTimeThroughput = new DateTime(2013, 1, 2, 12, 0, 0);
                    _Clock.UtcNowValue = new DateTime(2013, 1, 2, 12, 1, 0);
                    SetStatistic(statistic2, startTimeThroughput);

                    _HeartbeatService.Raise(r => r.FastTick += null, EventArgs.Empty);

                    Assert.AreEqual(36.45, GetViewDouble(viewProperty), viewProperty);
                    _View.Verify(r => r.UpdateCounters(), Times.Exactly(2));
                    break;
                default:
                    Assert.Fail("Unknown value type {0}", valueType);
                    break;
            }
        }

        private void SetStatistic(string propertyName, object value)
        {
            var property = typeof(IStatistics).GetProperty(propertyName);
            property.SetValue(_Statistics.Object, value, null);
        }

        private long GetViewLong(string propertyName)
        {
            var property = typeof(IStatisticsView).GetProperty(propertyName);
            return (long)property.GetValue(_View.Object, null);
        }

        private TimeSpan GetViewTimeSpan(string propertyName)
        {
            var property = typeof(IStatisticsView).GetProperty(propertyName);
            return (TimeSpan)property.GetValue(_View.Object, null);
        }

        private double GetViewDouble(string propertyName)
        {
            var property = typeof(IStatisticsView).GetProperty(propertyName);
            return (double)property.GetValue(_View.Object, null);
        }

        private long[] GetViewListCounters(string propertyName)
        {
            var property = typeof(IStatisticsView).GetProperty(propertyName);
            return (long[])property.GetValue(_View.Object, null);
        }

        private IList GetViewListObjects(string propertyName)
        {
            var property = typeof(IStatisticsView).GetProperty(propertyName);
            return (IList)property.GetValue(_View.Object, null);
        }

        [TestMethod]
        public void StatisticsPresenter_ResetCountersClicked_Resets_Counters_Correctly()
        {
            _Presenter.Initialise(_View.Object);

            _View.Raise(r => r.ResetCountersClicked += null, EventArgs.Empty);

            _Statistics.Verify(r => r.ResetMessageCounters(), Times.Once());
            _View.Verify(r => r.UpdateCounters(), Times.Exactly(2));
        }

        [TestMethod]
        public void StatisticsPresenter_CloseClicked_Disconnects_From_Heartbeat_Service()
        {
            _Presenter.Initialise(_View.Object);
            _Statistics.Object.AdsbCount = 1;

            _View.Raise(r => r.CloseClicked += null, EventArgs.Empty);
            _HeartbeatService.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(0L, _View.Object.AdsbMessages);
        }
    }
}
