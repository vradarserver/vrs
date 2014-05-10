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
using VirtualRadar.Interface.View;
using Moq;
using InterfaceFactory;
using VirtualRadar.Interface;
using Test.Framework;
using VirtualRadar.Interface.Presenter;

namespace Test.VirtualRadar.Library.Presenter
{
    [TestClass]
    public class PluginsPresenterTests
    {
        #region TestContext, Fields, TestInitialise etc.
        public TestContext TestContext { get; set; }

        private IClassFactory _ClassFactorySnapshot;
        private IPluginsPresenter _Presenter;
        private Mock<IPluginsView> _View;
        private Mock<IPluginManager> _PluginManager;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _PluginManager = TestUtilities.CreateMockSingleton<IPluginManager>();
            _View = new Mock<IPluginsView>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();

            _Presenter = Factory.Singleton.Resolve<IPluginsPresenter>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }
        #endregion

        #region Initialise
        [TestMethod]
        public void PluginsPresenter_Initialise_Shows_All_Loaded_Plugins()
        {
            var plugin1 = new Mock<IPlugin>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            var plugin2 = new Mock<IPlugin>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            _PluginManager.Setup(p => p.LoadedPlugins).Returns(new IPlugin[] { plugin1, plugin2 });

            _View.Setup(v => v.ShowPlugins(It.IsAny<IEnumerable<IPlugin>>())).Callback((IEnumerable<IPlugin> plugins) => {
                Assert.AreEqual(2, plugins.Count());
                Assert.IsTrue(plugins.Contains(plugin1));
                Assert.IsTrue(plugins.Contains(plugin2));
            });

            _Presenter.Initialise(_View.Object);

            _View.Verify(v => v.ShowPlugins(It.IsAny<IEnumerable<IPlugin>>()), Times.Once());
        }

        [TestMethod]
        public void PluginsPresenter_Initialise_Sets_The_Count_Of_Invalid_Plugins()
        {
            _PluginManager.Setup(p => p.IgnoredPlugins).Returns(new Dictionary<string, string>() { { "v", "w" }, { "x", "y" } });

            _Presenter.Initialise(_View.Object);

            Assert.AreEqual(2, _View.Object.InvalidPluginsCount);
        }
        #endregion
    }
}
