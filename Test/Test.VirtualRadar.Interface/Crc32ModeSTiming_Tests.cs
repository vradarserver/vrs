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

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class Crc32ModeSTiming_Tests
    {
        [TestMethod]
        public void Crc32ModeS_Timing_Test()
        {
            var count = 1000000;

            var fastShortTiming = DoTimingRun(true, false, count);
            var fastLongTiming = DoTimingRun(true, true, count);

            var slowShortTiming = DoTimingRun(false, false, count);
            var slowLongTiming = DoTimingRun(false, true, count);

            Assert.Inconclusive("Timings on {0:N0} messages: 'Fast' and short: {1}, 'Fast' and long: {2}, 'Slow' and short: {3}, 'Slow' and long: {4}",
                count,
                fastShortTiming, fastLongTiming,
                slowShortTiming, slowLongTiming
            );
        }

        private TimeSpan DoTimingRun(bool useFastMethod, bool useLongMessage, int count)
        {
            var message = new byte[useLongMessage ? 11 : 4];
            var random = new Random();

            DateTime start, finish;
            if(useFastMethod) {
                start = DateTime.UtcNow;
                for(var i = 0;i < count;++i) {
                    random.NextBytes(message);
                    Crc32ModeS.ComputeChecksumBytes(message, 0, message.Length);
                }
                finish = DateTime.UtcNow;
            } else if(useLongMessage) {
                start = DateTime.UtcNow;
                for(var i = 0;i < count;++i) {
                    random.NextBytes(message);
                    Crc32ModeS.ComputeChecksumBytesTraditional88(message);
                }
                finish = DateTime.UtcNow;
            } else {
                start = DateTime.UtcNow;
                for(var i = 0;i < count;++i) {
                    random.NextBytes(message);
                    Crc32ModeS.ComputeChecksumBytesTraditional32(message);
                }
                finish = DateTime.UtcNow;
            }

            return finish - start;
        }
    }
}
