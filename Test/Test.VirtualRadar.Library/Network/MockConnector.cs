// Copyright © 2014 onwards, Andrew Whewell
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
using Test.Framework;
using VirtualRadar.Interface.Network;

namespace Test.VirtualRadar.Library.Network
{
    public class MockConnector<TConnector, TConnection> : Mock<TConnector>
        where TConnector: class, IConnector
        where TConnection: class, IConnection
    {
        #region Public fields
        public Mock<TConnection> Connection;

        public List<byte[]> Written = new List<byte[]>();
        #endregion

        #region Ctor
        public MockConnector() : base()
        {
            DefaultValue = DefaultValue.Mock;

            Connection = TestUtilities.CreateMockInstance<TConnection>();
            ConfigureForReadStream(new byte[] {});

            SetupAllProperties();
            Setup(r => r.Connection).Returns((TConnection)null);

            Action<byte[], int> simpleWrite = (buffer, staleOverride) => {
                Written.Add(buffer);
            };
            Action<byte[], int, int, int> fullWrite = (buffer, offset, length, staleOverride) => {
                var excerpt = new byte[length];
                Array.Copy(buffer, offset, excerpt, 0, length);
                Written.Add(excerpt);
            };

            Setup(r => r.Write(It.IsAny<byte[]>(), It.IsAny<int>())).Callback(simpleWrite);
            Connection.Setup(r => r.Write(It.IsAny<byte[]>(), It.IsAny<int>())).Callback(simpleWrite);

            Setup(r => r.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Callback(fullWrite);
            Connection.Setup(r => r.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Callback(fullWrite);
        }
        #endregion

        #region Connect
        public void ConfigureForConnect()
        {
            Setup(r => r.EstablishConnection()).Callback(() => {
                Setup(r => r.Connection).Returns(Connection.Object);
                Raise(r => r.ConnectionEstablished += null, new ConnectionEventArgs(Connection.Object));
            });
        }
        #endregion

        #region Read
        public void ConfigureForReadStream(string text, int reportReadLength = int.MaxValue, IEnumerable<string> subsequentReads = null)
        {
            var content = new List<byte[]>();
            content.Add(Encoding.UTF8.GetBytes(text));
            if(subsequentReads != null) {
                foreach(var line in subsequentReads) {
                    content.Add(Encoding.UTF8.GetBytes(line));
                }
            }

            DoConfigureForReadStream(content, reportReadLength);
        }

        public void ConfigureForReadStream(IEnumerable<byte> bytes, int reportReadLength = int.MaxValue, List<IEnumerable<byte>> subsequentReads = null)
        {
            var content = new List<byte[]>();
            content.Add(bytes.ToArray());
            if(subsequentReads != null) {
                foreach(var moreContent in subsequentReads) {
                    content.Add(moreContent.ToArray());
                }
            }

            DoConfigureForReadStream(content, reportReadLength);
        }

        private void DoConfigureForReadStream(List<byte[]> content, int reportReadLength)
        {
            int callCount = 0;

            Action<byte[], ConnectionReadDelegate> bufferSimpleRead = (buffer, callback) => {
                var thisContent = callCount < content.Count ? content[callCount++] : null;
                MockRead(buffer, 0, buffer.Length, callback, thisContent, reportReadLength);
            };
            Action<byte[], int, int, ConnectionReadDelegate> bufferFullRead = (buffer, offset, length, callback) => {
                var thisContent = callCount < content.Count ? content[callCount++] : null;
                MockRead(buffer, offset, length, callback, thisContent, reportReadLength);
            };

            Setup(r => r.Read(It.IsAny<byte[]>(), It.IsAny<ConnectionReadDelegate>())).Callback(bufferSimpleRead);
            Connection.Setup(r => r.Read(It.IsAny<byte[]>(), It.IsAny<ConnectionReadDelegate>())).Callback(bufferSimpleRead);

            Setup(r => r.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ConnectionReadDelegate>())).Callback(bufferFullRead);
            Connection.Setup(r => r.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ConnectionReadDelegate>())).Callback(bufferFullRead);
        }

        private void MockRead(byte[] buffer, int offset, int length, ConnectionReadDelegate callback, byte[] content, int reportReadLength)
        {
            if(content != null) {
                var bytesRead = Math.Min(content.Length, length);
                bytesRead = Math.Min(bytesRead, reportReadLength);
                Array.Copy(content, 0, buffer, offset, bytesRead);
                callback(Connection.Object, buffer, offset, length, bytesRead);
            }
        }
        #endregion
    }
}
