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
using VirtualRadar.Interface.Presenter;
using Moq;
using VirtualRadar.Interface.View;
using InterfaceFactory;
using VirtualRadar.Interface;
using Test.Framework;

namespace Test.VirtualRadar.Library.Presenter
{
    [TestClass]
    public class InvalidPluginsPresenterTests
    {
        public TestContext TestContext { get; set; }

        private IClassFactory _ClassFactorySnapshot;
        private IInvalidPluginsPresenter _Presenter;
        private Mock<IInvalidPluginsView> _View;
        private Mock<IPluginManager> _PluginManager;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _Presenter = Factory.Singleton.Resolve<IInvalidPluginsPresenter>();
            _View = new Mock<IInvalidPluginsView>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _PluginManager = TestUtilities.CreateMockSingleton<IPluginManager>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }

        [TestMethod]
        public void InvalidPluginsPresenter_Initialise_Copies_Ignored_Plugins_To_View()
        {
            IDictionary<string, string> viewPlugins = null;
            _View.Setup(v => v.ShowInvalidPlugins(It.IsAny<IDictionary<string, string>>())).Callback((IDictionary<string, string> d) => { viewPlugins = d; });

            var ignoredPlugins = new Dictionary<string, string>() { { "a", "b" }, { "c", "d" } };
            _PluginManager.Setup(p => p.IgnoredPlugins).Returns(ignoredPlugins);

            _Presenter.Initialise(_View.Object);

            Assert.AreEqual(2, viewPlugins.Count);
            Assert.AreEqual("b", viewPlugins["a"]);
            Assert.AreEqual("d", viewPlugins["c"]);
        }
    }
}
