// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using System.Net;
using System.Text;
using VirtualRadar.Interface;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Test.Framework
{
    public class MockHttpClient : IHttpClientService
    {
        public Dictionary<string, byte[]> CaseSensitiveUrlContent = new();

        public List<string> DownloadedUrls { get; } = new();

        public void AddUrlContent(string caseSensitiveUrl, byte[] content)
        {
            CaseSensitiveUrlContent[caseSensitiveUrl] = content;
        }

        public void AddUrlContent(string caseSensitiveUrl, string text)
        {
            AddUrlContent(caseSensitiveUrl, Encoding.UTF8.GetBytes(text));
        }

        private void ThrowIfNotFound(string url)
        {
            if(!CaseSensitiveUrlContent.ContainsKey(url)) {
                throw new HttpRequestException($"Cannot find {url}", null, HttpStatusCode.NotFound);
            }
        }

        public Stream GetStream(string requestUri, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfNotFound(requestUri);
            DownloadedUrls.Add(requestUri);
            return new MemoryStream(CaseSensitiveUrlContent[requestUri]);
        }

        public async Task<Stream> GetStreamAsync(string requestUri, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetStream(requestUri, cancellationToken);
        }

        public string GetString(string requestUri, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfNotFound(requestUri);
            DownloadedUrls.Add(requestUri);
            return Encoding.UTF8.GetString(
                CaseSensitiveUrlContent[requestUri]
            );
        }

        public async Task<string> GetStringAsync(string requestUri, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetString(requestUri, cancellationToken);
        }
    }
}
