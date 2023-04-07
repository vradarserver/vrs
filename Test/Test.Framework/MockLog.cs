// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Moq;
using VirtualRadar.Interface;

namespace Test.Framework
{
    public class MockLog : ILog
    {
        /// <summary>
        /// The output of all of the log calls.
        /// </summary>
        public List<string> Output { get; } = new();

        public Mock<ILog> Mock { get; } = new();

        public MockLog()
        {
            Mock.Setup(r => r.Flush())
                .Callback(() => Output.Clear());
            Mock.Setup(r => r.FlushAsync())
                .Callback(() => Output.Clear());
            Mock.Setup(r => r.WriteLine(It.IsAny<string>()))
                .Callback((string line) => Output.Add(line));
            Mock.Setup(r => r.WriteLineAsync(It.IsAny<string>()))
                .Callback((string line) => Output.Add(line));

            Mock.Setup(r => r.GetContent())
                .Returns(() => Array.Empty<LogMessage>());
        }

        public string FileName => Mock.Object.FileName;

        public void Flush() => Mock.Object.Flush();

        public async Task FlushAsync() => await Mock.Object.FlushAsync();

        public IReadOnlyList<LogMessage> GetContent() => Mock.Object.GetContent();

        public void WriteLine(string message) => Mock.Object.WriteLine(message);

        public async Task WriteLineAsync(string message) => await Mock.Object.WriteLineAsync(message);
    }
}
