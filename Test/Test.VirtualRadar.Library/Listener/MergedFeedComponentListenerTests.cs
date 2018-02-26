// Copyright © 2015 onwards, Andrew Whewell
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
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Library.Listener
{
    [TestClass]
    public class MergedFeedComponentListenerTests
    {
        public TestContext TestContext;
        private IMergedFeedComponentListener _Component;
        private Mock<IListener> _Listener;

        [TestInitialize]
        public void TestInitialise()
        {
            _Component = Factory.Resolve<IMergedFeedComponentListener>();
            _Listener = TestUtilities.CreateMockInstance<IListener>();
        }

        [TestMethod]
        public void MergedFeedComponentListener_Ctor_Initialises_To_Known_State()
        {
            Assert.IsNull(_Component.Listener);
            Assert.AreEqual(false, _Component.IsMlatFeed);
        }

        [TestMethod]
        public void MergedFeedComponentListener_SetListener_Sets_Properties()
        {
            _Component.SetListener(_Listener.Object, true);
            Assert.AreSame(_Listener.Object, _Component.Listener);
            Assert.AreEqual(true, _Component.IsMlatFeed);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MergedFeedComponentListener_SetListener_Throws_If_Called_Twice()
        {
            _Component.SetListener(_Listener.Object, true);
            _Component.SetListener(_Listener.Object, true);
        }

        [TestMethod]
        public void MergedFeedComponentListener_Equals_Returns_True_If_ReferenceEquals()
        {
            _Component.SetListener(_Listener.Object, true);

            Assert.IsTrue(_Component.Equals(_Component));
        }

        [TestMethod]
        public void MergedFeedComponentListener_Equals_Returns_True_If_Two_Objects_Are_Same()
        {
            var other = Factory.Resolve<IMergedFeedComponentListener>();
            _Component.SetListener(_Listener.Object, true);
            other.SetListener(_Listener.Object, true);

            Assert.IsTrue(_Component.Equals(other));
        }

        [TestMethod]
        public void MergedFeedComponentListener_Equals_Returns_False_If_Listener_Is_Different()
        {
            var other = Factory.Resolve<IMergedFeedComponentListener>();
            _Component.SetListener(_Listener.Object, true);
            other.SetListener(TestUtilities.CreateMockInstance<IListener>().Object, true);

            Assert.IsFalse(_Component.Equals(other));
        }

        [TestMethod]
        public void MergedFeedComponentListener_Equals_Returns_False_If_MLAT_Feed_Type_Is_Different()
        {
            var other = Factory.Resolve<IMergedFeedComponentListener>();
            _Component.SetListener(_Listener.Object, true);
            other.SetListener(_Listener.Object, false);

            Assert.IsFalse(_Component.Equals(other));
        }
    }
}
