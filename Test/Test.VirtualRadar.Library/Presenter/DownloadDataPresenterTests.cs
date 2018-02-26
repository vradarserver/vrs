// Copyright © 2010 onwards, Andrew Whewell
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
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;

namespace Test.VirtualRadar.Library.Presenter
{
    [TestClass]
    public class DownloadDataPresenterTests
    {
        #region TestContext, Fields, TestInitialise
        public TestContext TestContext { get; set; }

        private IClassFactory _ClassFactorySnapshot;
        private IDownloadDataPresenter _Presenter;
        private Mock<IDownloadDataView> _View;
        private Mock<IStandingDataManager> _StandingDataManager;
        private Mock<IStandingDataUpdater> _StandingDataUpdater;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _StandingDataManager = TestUtilities.CreateMockSingleton<IStandingDataManager>();
            _StandingDataUpdater = TestUtilities.CreateMockImplementation<IStandingDataUpdater>();

            _Presenter = Factory.Resolve<IDownloadDataPresenter>();
            _View = new Mock<IDownloadDataView>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }
        #endregion

        #region Initialise
        [TestMethod]
        public void DownloadDataPresenter_Initialise_Sets_Initial_Status()
        {
            _StandingDataManager.Setup(s => s.RouteStatus).Returns("The route status");
            _Presenter.Initialise(_View.Object);

            Assert.AreEqual("The route status", _View.Object.Status);
        }
        #endregion

        #region DownloadButtonClicked
        [TestMethod]
        public void DownloadDataPresenter_DownloadButtonClicked_Instantiates_Updater_And_Calls_It()
        {
            _Presenter.Initialise(_View.Object);
            _View.Raise(v => v.DownloadButtonClicked += null, EventArgs.Empty);

            _StandingDataUpdater.Verify(u => u.Update(), Times.Once());
        }

        [TestMethod]
        public void DownloadDataPresenter_DownloadButtonClicked_Reloads_Data_After_Update_Has_Finished()
        {
            _StandingDataManager.Setup(m => m.Load()).Callback(() => {
                _StandingDataUpdater.Verify(u => u.Update(), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _View.Raise(v => v.DownloadButtonClicked += null, EventArgs.Empty);

            _StandingDataManager.Verify(m => m.Load(), Times.Once());
        }

        [TestMethod]
        public void DownloadDataPresenter_DownloadButtonClicked_Sets_View_Status_Before_Update()
        {
            _StandingDataUpdater.Setup(u => u.Update()).Callback(() => {
                Assert.AreEqual(Strings.DownloadingPleaseWait, _View.Object.Status);
            });

            _Presenter.Initialise(_View.Object);
            _View.Raise(v => v.DownloadButtonClicked += null, EventArgs.Empty);
        }

        [TestMethod]
        public void DownloadDataPresenter_DownloadButtonClicked_Sets_View_Status_After_Load()
        {
            _StandingDataManager.Setup(m => m.Load()).Callback(() => {
                _StandingDataManager.Setup(m => m.RouteStatus).Returns("New status");
            });

            _Presenter.Initialise(_View.Object);
            _View.Raise(v => v.DownloadButtonClicked += null, EventArgs.Empty);

            Assert.AreEqual("New status", _View.Object.Status);
        }

        [TestMethod]
        public void DownloadDataPresenter_DownloadButtonClicked_Indicates_To_User_That_UI_Will_Be_Unresponsive()
        {
            var busyState = new Object();
            _View.Setup(v => v.ShowBusy(It.IsAny<bool>(), It.IsAny<object>())).Returns((bool busy, object state) => { return busyState; }).Callback((bool busy, object state) => {
                if(busy)    _StandingDataUpdater.Verify(u => u.Update(), Times.Never());
                if(!busy)   _StandingDataUpdater.Verify(u => u.Update(), Times.Once());
            });

            _Presenter.Initialise(_View.Object);
            _View.Raise(v => v.DownloadButtonClicked += null, EventArgs.Empty);

            _View.Verify(v => v.ShowBusy(It.IsAny<bool>(), It.IsAny<object>()), Times.Exactly(2));
            _View.Verify(v => v.ShowBusy(true, null), Times.Once());
            _View.Verify(v => v.ShowBusy(false, busyState), Times.Once());
        }

        [TestMethod]
        public void DownloadDataPresenter_DownloadButtonClicked_Restores_Busy_Indicator_Even_If_Downloader_Throws()
        {
            _StandingDataUpdater.Setup(u => u.Update()).Callback(() => { throw new InvalidOperationException(); });

            _Presenter.Initialise(_View.Object);
            try {
                _View.Raise(v => v.DownloadButtonClicked += null, EventArgs.Empty);
            } catch {
            }

            _View.Verify(v => v.ShowBusy(It.IsAny<bool>(), It.IsAny<object>()), Times.Exactly(2));
        }
        #endregion
    }
}
