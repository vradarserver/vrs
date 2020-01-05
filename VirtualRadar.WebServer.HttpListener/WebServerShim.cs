// Copyright © 2017 onwards, Andrew Whewell
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
using System.Threading;
using System.Threading.Tasks;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.WebServer.HttpListener
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// A shim that glues OWIN and the old web site interface together.
    /// </summary>
    /// <remarks>
    /// The intention is that this will be short-lived and will not appear in the final form of
    /// the OWIN version of the web server. I just need it so that the program continues to work
    /// while everything is being ported.
    /// </remarks>
    class WebServerShim
    {
        /// <summary>
        /// A web request that is waiting for an OnRequestFinished to be raised for it.
        /// </summary>
        class FinishedWebRequest
        {
            public long RequestId;
            public DateTime RaiseEventTimeUtc;
        }

        /// <summary>
        /// A list of finished web requests.
        /// </summary>
        private LinkedList<FinishedWebRequest> _FinishedWebRequests = new LinkedList<FinishedWebRequest>();

        /// <summary>
        /// The object used to control access to the fields.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The web server that we expose OWIN stuff through.
        /// </summary>
        public OwinWebServer WebServer { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="webServer"></param>
        public WebServerShim(OwinWebServer webServer)
        {
            WebServer = webServer;
        }

        /// <summary>
        /// The shim's OWIN middleware.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public AppFunc ShimMiddleware(AppFunc next)
        {
            AppFunc appFunc = async(IDictionary<string, object> environment) => {
                var startTime = WebServer.Provider.UtcNow;
                var context = new Context(environment);
                var requestArgs = new RequestReceivedEventArgs(context.Request, context.Response, WebServer.Root);

                RaiseRequestReceivedEvents(context, requestArgs);

                if(!requestArgs.Handled) {
                    await next.Invoke(environment);
                }

                RaiseRequestCompletedEvents(context, requestArgs, startTime);
            };

            return appFunc;
        }

        /// <summary>
        /// Raises events that old web site code would use to listen for requests.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requestArgs"></param>
        private void RaiseRequestReceivedEvents(Context context, RequestReceivedEventArgs requestArgs)
        {
            try {
                WebServer.OnBeforeRequestReceived(requestArgs);
                WebServer.OnRequestReceived(requestArgs);
                WebServer.OnAfterRequestReceived(requestArgs);
            } catch(ThreadAbortException) {
                ;
            } catch(Exception ex) {
                // The HttpListener version doesn't log these as there can be lot of them, but given that
                // this stuff is all brand new I think I'd like to see what exceptions are being thrown
                // during processing.
                var log = Factory.ResolveSingleton<ILog>();
                log.WriteLine("Caught exception in general request handling event handlers: {0}", ex);
            }
        }

        /// <summary>
        /// Raises events that used to occur after a request had been processed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requestArgs"></param>
        /// <param name="startTime"></param>
        private void RaiseRequestCompletedEvents(Context context, RequestReceivedEventArgs requestArgs, DateTime startTime)
        {
            var requestReceivedEventArgsId = requestArgs.UniqueId;

            try {
                var request = context.Request;
                var response = context.Response;

                var fullClientAddress = String.Format("{0}:{1}", requestArgs.ClientAddress, request.RemoteEndPoint.Port);

                var responseArgs = new ResponseSentEventArgs(
                    requestArgs.PathAndFile,
                    fullClientAddress,
                    requestArgs.ClientAddress,
                    response.ContentLength,
                    MimeType.GetContentClassification(response.MimeType),
                    request,
                    (int)response.StatusCode,
                    (int)(WebServer.Provider.UtcNow - startTime).TotalMilliseconds,
                    context.BasicUserName
                );
                WebServer.OnResponseSent(responseArgs);
            } catch(Exception ex) {
                // The HttpListener version doesn't log these as there can be lot of them, but given that
                // this stuff is all brand new I think I'd like to see what exceptions are being thrown
                // during processing.
                var log = Factory.ResolveSingleton<ILog>();
                log.WriteLine("Caught exception in general request handling event handlers: {0}", ex);
            }

            // The request finished event has to be raised after the response has been sent. This is
            // a bit tricky with OWIN because we don't know when the response has finished. So we're
            // just going to wait a couple of seconds on a background thread, raise the event and
            // hope for the best. In practise the event is used by web admin views to do things that
            // might reset the connection, they're not common.
            lock(_SyncLock) {
                _FinishedWebRequests.AddLast(new FinishedWebRequest() {
                    RequestId =         requestArgs.UniqueId,
                    RaiseEventTimeUtc = DateTime.UtcNow.AddSeconds(2),
                });
            }
        }

        /// <summary>
        /// Called about once a second by the parent <see cref="OwinWebServer"/> to raise outstanding RequestFinished events.
        /// </summary>
        public void RaiseRequestFinishedEvents()
        {
            var now = DateTime.UtcNow;

            for(var loop = true;loop;) {
                FinishedWebRequest finishedWebRequest = null;
                lock(_SyncLock) {
                    finishedWebRequest = _FinishedWebRequests.First?.Value;
                    loop = finishedWebRequest?.RaiseEventTimeUtc <= now;
                    if(loop) {
                        _FinishedWebRequests.RemoveFirst();
                        loop = _FinishedWebRequests.First?.Value.RaiseEventTimeUtc <= now;
                    }
                }

                if(finishedWebRequest != null) {
                    try {
                        WebServer.OnRequestFinished(new EventArgs<long>(finishedWebRequest.RequestId));
                    } catch(Exception ex) {
                        var log = Factory.ResolveSingleton<ILog>();
                        log.WriteLine($"Caught exception in RequestFinished event handler: {ex}");
                    }
                }
            }
        }
    }
}
