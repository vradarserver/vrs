// Copyright © 2013 onwards, Andrew Whewell
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
using VirtualRadar.Interface.WebSite;
using Test.Framework;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Interface.WebSite
{
    [TestClass]
    public class ServerReceiverJsonTests
    {
        [TestMethod]
        public void ServerReceiverJson_Constructor_Initialises_To_Known_Values_And_Properties_Work()
        {
            var json = new ServerReceiverJson();

            TestUtilities.TestProperty(json, r => r.Name, null, "Abc");
            TestUtilities.TestProperty(json, r => r.UniqueId, 0, 123);
        }

        [TestMethod]
        public void ServerReceiverJson_ToModel_Receiver_Returns_Model_Based_On_Receiver()
        {
            var receiver = new Receiver() { UniqueId = 7, Name = "Hello" };

            var model = ServerReceiverJson.ToModel(receiver);

            Assert.AreEqual(7, model.UniqueId);
            Assert.AreEqual("Hello", model.Name);
        }

        [TestMethod]
        public void ServerReceiverJson_ToModel_Receiver_Returns_Null_If_Receiver_Is_Null()
        {
            Assert.IsNull(ServerReceiverJson.ToModel((Receiver)null));
        }

        [TestMethod]
        public void ServerReceiverJson_ToModel_Receiver_Returns_Null_If_Receiver_Is_MergeOnly()
        {
            var receiver = new Receiver() { UniqueId = 7, Name = "Hello", ReceiverUsage = ReceiverUsage.MergeOnly };

            Assert.IsNull(ServerReceiverJson.ToModel(receiver));
        }

        [TestMethod]
        public void ServerReceiverJson_ToModel_Receiver_Returns_Null_If_Receiver_Is_HideFromWebSite()
        {
            var receiver = new Receiver() { UniqueId = 7, Name = "Hello", ReceiverUsage = ReceiverUsage.HideFromWebSite };

            Assert.IsNull(ServerReceiverJson.ToModel(receiver));
        }

        [TestMethod]
        public void ServerReceiverJson_ToModel_MergedFeed_Returns_Model_Based_On_MergedFeed()
        {
            var mergedFeed = new MergedFeed() { UniqueId = 7, Name = "Hello" };

            var model = ServerReceiverJson.ToModel(mergedFeed);

            Assert.AreEqual(7, model.UniqueId);
            Assert.AreEqual("Hello", model.Name);
        }

        [TestMethod]
        public void ServerReceiverJson_ToModel_MergedFeed_Returns_Null_If_MergedFeed_Is_Null()
        {
            Assert.IsNull(ServerReceiverJson.ToModel((MergedFeed)null));
        }

        [TestMethod]
        public void ServerMergedFeedJson_ToModel_MergedFeed_Returns_Null_If_MergedFeed_Is_MergeOnly()
        {
            var mergedFeed = new MergedFeed() { UniqueId = 7, Name = "Hello", ReceiverUsage = ReceiverUsage.MergeOnly };

            Assert.IsNull(ServerReceiverJson.ToModel(mergedFeed));
        }

        [TestMethod]
        public void ServerMergedFeedJson_ToModel_MergedFeed_Returns_Null_If_MergedFeed_Is_HideFromWebSite()
        {
            var mergedFeed = new MergedFeed() { UniqueId = 7, Name = "Hello", ReceiverUsage = ReceiverUsage.HideFromWebSite };

            Assert.IsNull(ServerReceiverJson.ToModel(mergedFeed));
        }
    }
}
