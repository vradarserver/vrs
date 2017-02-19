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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.ModeS;

namespace Test.Framework
{
    /// <summary>
    /// A static class that can help when testing Statistics objects.
    /// </summary>
    public static class StatisticsHelper
    {
        /// <summary>
        /// Creates a mock <see cref="IStatistics"/> object.
        /// </summary>
        /// <returns></returns>
        public static Mock<IStatistics> CreateLockableStatistics()
        {
            var mock = TestUtilities.CreateMockImplementation<IStatistics>();

            var lockObject = new object();
            var adsbTypeCount = new long[256];
            var modeSDFStatistics = new ModeSDFStatistics[32];
            var adsbMessageFormatCount = new long[Enum.GetValues(typeof(MessageFormat)).OfType<MessageFormat>().Select(r => (int)r).Max() + 1];

            for(var i = 0;i < modeSDFStatistics.Length;++i) {
                modeSDFStatistics[i] = new ModeSDFStatistics() {
                    DF = (DownlinkFormat)i
                };
            }

            mock.Setup(r => r.Lock(It.IsAny<Action<IStatistics>>())).Callback((Action<IStatistics> del) => {
                del(mock.Object);
            });
            mock.Setup(r => r.AdsbTypeCount).Returns(adsbTypeCount);
            mock.Setup(r => r.ModeSDFStatistics).Returns(modeSDFStatistics);
            mock.Setup(r => r.AdsbMessageFormatCount).Returns(adsbMessageFormatCount);

            return mock;
        }
    }
}
