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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Moq;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;

namespace Test.VirtualRadar.Library.Listener
{
    /// <summary>
    /// A class that wraps a mock <see cref="IListenerProvider"/>.
    /// </summary>
    class MockListenerProvider : Mock<IListenerProvider>
    {
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public MockListenerProvider() : base()
        {
            DefaultValue = DefaultValue.Mock;
            SetupAllProperties();
            Setup(p => p.EndConnect(It.IsAny<IAsyncResult>())).Returns(true);
            Setup(p => p.Sleep(It.IsAny<int>())).Callback((int milliseconds) => { Thread.Sleep(milliseconds); });
        }

        /// <summary>
        /// Configures the provider for a Begin/End Connect set of calls.
        /// </summary>
        /// <param name="repeatCount"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        /// <remarks>
        /// Note that the way this is implemented means that the callback remains on the same thread as the BeginConnect.
        /// This greatly simplifies testing but it is inaccurate, in real life they would be on different threads. This
        /// makes it impossible to accurately test some situations.
        /// </remarks>
        public IAsyncResult ConfigureForConnect(int repeatCount = 1, Action beginReadCallback = null)
        {
            IAsyncResult asyncResult = new Mock<IAsyncResult>().Object;

            int callCount = 0;
            Setup(m => m.BeginConnect(It.IsAny<AsyncCallback>()))
                .Returns((AsyncCallback callback) =>
                {
                    if(beginReadCallback != null) beginReadCallback();  // <-- can't use normal Callback here as it won't be called until AFTER the 'async' callback runs
                    if(callCount++ < repeatCount) {
                        if(callback != null) callback(asyncResult);
                    }
                    return asyncResult;
                });

            return asyncResult;
        }

        /// <summary>
        /// Configures the provider for a Begin/End ReadStream set of calls where the stream content will be interpreted as text.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="reportReadLength"></param>
        /// <param name="subsequentReads"></param>
        /// <returns></returns>
        /// <remarks>
        /// Note that this all executes on a single thread, simplifying testing but preventing some situations from being testable.
        /// </remarks>
        public IAsyncResult ConfigureForReadStream(string text, int reportReadLength = int.MinValue, IEnumerable<string> subsequentReads = null)
        {
            IAsyncResult asyncResult = new Mock<IAsyncResult>().Object;

            var readTexts = new List<string>();
            readTexts.Add(text);
            if(subsequentReads != null) readTexts.AddRange(subsequentReads);

            int beginReadCount = 0;
            Setup(m => m.BeginRead(It.IsAny<AsyncCallback>())).Returns(
                (AsyncCallback callback) =>
                {
                    if(beginReadCount++ < readTexts.Count) {
                        if(callback != null) callback(asyncResult);
                    }
                    return asyncResult;
                });

            int endReadCount = 0;
            Setup(m => m.EndRead(asyncResult)).Returns(() => {
                var readBuffer = Encoding.ASCII.GetBytes(readTexts[endReadCount]);
                Setup(p => p.ReadBuffer).Returns(readBuffer);
                return endReadCount++ == 0 && reportReadLength != int.MinValue ? reportReadLength : readBuffer.Length;
            });

            return asyncResult;
        }

        /// <summary>
        /// Configures the provider for a Begin/End ReadStream set of calls where the stream content will be a sequence of plain bytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="reportReadLength"></param>
        /// <param name="subsequentReads"></param>
        /// <returns></returns>
        /// <remarks>
        /// Note that this all executes on a single thread, simplifying testing but preventing some situations from being testable.
        /// </remarks>
        public IAsyncResult ConfigureForReadStream(IEnumerable<byte> bytes, int reportReadLength = int.MinValue, List<IEnumerable<byte>> subsequentReads = null)
        {
            IAsyncResult asyncResult = new Mock<IAsyncResult>().Object;

            var readBuffers = new List<IEnumerable<byte>>();
            readBuffers.Add(bytes);
            if(subsequentReads != null) readBuffers.AddRange(subsequentReads);

            int beginReadCount = 0;
            Setup(m => m.BeginRead(It.IsAny<AsyncCallback>())).Returns(
                (AsyncCallback callback) =>
                {
                    if(beginReadCount++ < readBuffers.Count) {
                        if(callback != null) callback(asyncResult);
                    }
                    return asyncResult;
                });

            int endReadCount = 0;
            Setup(m => m.EndRead(asyncResult)).Returns(() => {
                var readBuffer = readBuffers[endReadCount].ToArray();
                Setup(p => p.ReadBuffer).Returns(readBuffer);
                return endReadCount++ == 0 && reportReadLength != int.MinValue ? reportReadLength : readBuffer.Length;
            });

            return asyncResult;
        }
    }
}
