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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Library;
using InterfaceFactory;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Library.Presenter
{
    [TestClass]
    public class AboutPresenterTests
    {
        IClassFactory _OriginalFactory;
        IAboutPresenter _Presenter;
        Mock<IAboutView> _View;
        Mock<IApplicationInformation> _ApplicationInformation;
        Mock<IConfigurationStorage> _ConfigurationStorage;
        Mock<IRuntimeEnvironment> _RuntimeEnvironment;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _View = new Mock<IAboutView>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _ApplicationInformation = TestUtilities.CreateMockImplementation<IApplicationInformation>();
            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();

            _Presenter = Factory.Singleton.Resolve<IAboutPresenter>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);
        }

        [TestMethod]
        public void AboutPresenter_Initialise_Copies_Initial_Values_From_Model_To_View()
        {
            _ApplicationInformation.Setup(m => m.Copyright).Returns("The copyright");
            _ApplicationInformation.Setup(m => m.Description).Returns("The description");
            _ApplicationInformation.Setup(m => m.ApplicationName).Returns("The name");
            _ApplicationInformation.Setup(m => m.FullVersion).Returns("The version");
            _ApplicationInformation.Setup(m => m.ProductName).Returns("The product name");
            _ConfigurationStorage.Setup(m => m.Folder).Returns("The config folder");
            _RuntimeEnvironment.Setup(m => m.IsMono).Returns(true);

            _Presenter.Initialise(_View.Object);

            Assert.AreEqual("The copyright", _View.Object.Copyright);
            Assert.AreEqual("The description", _View.Object.Description);
            Assert.AreEqual("The name", _View.Object.Caption);
            Assert.AreEqual("The version", _View.Object.Version);
            Assert.AreEqual("The config folder", _View.Object.ConfigurationFolder);
            Assert.AreEqual("The product name", _View.Object.ProductName);
            Assert.AreEqual(true, _View.Object.IsMono);
        }

        [TestMethod]
        public void AboutPresenter_Asks_View_To_Show_Configuration_Folder_Content_When_User_Requests_It()
        {
            _Presenter.Initialise(_View.Object);

            _View.Raise(m => m.OpenConfigurationFolderClicked += null, EventArgs.Empty);
            _View.Verify(m => m.ShowConfigurationFolderContents(), Times.Once());
        }
    }
}
