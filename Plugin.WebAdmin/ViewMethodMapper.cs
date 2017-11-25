// Copyright © 2016 onwards, Andrew Whewell
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using InterfaceFactory;
using Newtonsoft.Json;
using VirtualRadar.Interface;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Plugin.WebAdmin
{
    /// <summary>
    /// A class that manages the relationship between a view and the web pages.
    /// </summary>
    class ViewMethodMapper
    {
        /// <summary>
        /// Describes an exposed method.
        /// </summary>
        class ExposedMethod
        {
            public WebAdminMethodAttribute WebAdminMethod { get; private set; }
            public MethodInfo MethodInfo { get; private set; }

            public ExposedMethod(WebAdminMethodAttribute attribute, MethodInfo methodInfo)
            {
                WebAdminMethod = attribute;
                MethodInfo = methodInfo;
            }
        }

        /// <summary>
        /// Describes a web admin view and the object that handles the JSON requests from the HTML.
        /// </summary>
        class MappedView
        {
            public WebAdminView                         WebAdminView { get; private set; }
            public IView                                View { get; private set; }
            public string                               ViewId { get; set; }
            public Dictionary<string, ExposedMethod>    ExposedMethods { get; private set; }
            public DateTime                             LastContactUtc { get; set; }

            public MappedView(WebAdminView webAdminView, IView view)
            {
                WebAdminView = webAdminView;
                View = view;
                ExposedMethods = new Dictionary<string,ExposedMethod>();
                LastContactUtc = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Describes a method that is to be called after the original request has had a response sent back for it.
        /// </summary>
        class DeferredMethod
        {
            public string           JobId { get; private set; }
            public DateTime         CreatedUtc { get; private set; }
            public long             RequestId { get; private set; }
            public MappedView       MappedView { get; private set; }
            public ExposedMethod    ExposedMethod { get; private set; }
            public object[]         Parameters { get; private set; }

            public DeferredMethod(long requestId, MappedView mappedView, ExposedMethod exposedMethod, object[] parameters)
            {
                JobId = Guid.NewGuid().ToString();
                CreatedUtc = DateTime.UtcNow;
                RequestId = requestId;
                MappedView = mappedView;
                ExposedMethod = exposedMethod;
                Parameters = parameters;
            }
        }

        /// <summary>
        /// Holds the response to a deferred method.
        /// </summary>
        class DeferredMethodResponse
        {
            public DateTime CreatedUtc { get; private set; }
            public string   JobId { get; private set; }
            public string   JsonResponseText { get; private set; }

            public DeferredMethodResponse(string jobId, string jsonResponseText)
            {
                CreatedUtc = DateTime.UtcNow;
                JobId = jobId;
                JsonResponseText = jsonResponseText;
            }
        }

        /// <summary>
        /// The substitution marker in HTML for the generated view identifier.
        /// </summary>
        private const string ViewIdMarker = "@view-id@";

        /// <summary>
        /// The name of the parameter that identifies which view instance a JSON request should be passed to.
        /// </summary>
        private const string ViewIdParameterName = "__ViewId";

        /// <summary>
        /// The name of the parameter that holds the deferred execution job ID.
        /// </summary>
        private const string JobIdParameterName = "JobId";

        /// <summary>
        /// The name of the fake method that browsers should periodically call to prevent their view from getting zapped.
        /// </summary>
        private const string ViewHeartbeatMethod = "BrowserHeartbeat";

        /// <summary>
        /// The name of the fake method that browsers periodically call when they're asking for the response to a deferred execution.
        /// </summary>
        private const string GetDeferredResponseMethod = "GetDeferredResponse";

        /// <summary>
        /// The number of seconds that have to elapse with no message from a multi-instance view before that view is disposed.
        /// </summary>
        private const int ViewInactivitySeconds = 120;

        /// <summary>
        /// The number of seconds to wait between each check for inactive views.
        /// </summary>
        private const int InactiveViewCheckIntervalSeconds = 30;

        /// <summary>
        /// The number of minutes that orphaned deferred executions and results can hang around before they're removed.
        /// </summary>
        private const int CleanUpDeferredExecutionsIntervalMinutes = 1;

        /// <summary>
        /// The number of minutes that deferred execution responses will be kept until they're flushed.
        /// </summary>
        private const int CleanUpDeferredExecutionResponseMinutes = 1;

        /// <summary>
        /// Used to manage multi-threaded access to the fields.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// A dictionary of <see cref="MappedView"/>s indexed by the normalised path and file of the HTML page.
        /// </summary>
        /// <remarks>
        /// All of the views in this map share a single instance of an <see cref="IView"/> between all of
        /// the web pages running for the view.
        /// </remarks>
        private Dictionary<string, MappedView> _SharedViewInstances = new Dictionary<string,MappedView>();

        /// <summary>
        /// A dictionary mapping normalised HTML path and files to an inner map of view identifiers to mapped views.
        /// </summary>
        private Dictionary<string, Dictionary<string, MappedView>> _MultiViewInstances = new Dictionary<string,Dictionary<string,MappedView>>();

        /// <summary>
        /// Views that are scheduled for termination.
        /// </summary>
        private List<MappedView> _DetentionBlockAA23 = new List<MappedView>();

        /// <summary>
        /// The date and time of the last check for inactive forms.
        /// </summary>
        private DateTime _LastInactiveViewCheckUtc = DateTime.UtcNow;

        /// <summary>
        /// The deferred methods waiting to be executed.
        /// </summary>
        private Dictionary<long, DeferredMethod> _DeferredMethods = new Dictionary<long,DeferredMethod>();

        /// <summary>
        /// The responses to deferred methods that have finished executing.
        /// </summary>
        private Dictionary<string, DeferredMethodResponse> _DeferredResponses = new Dictionary<string,DeferredMethodResponse>();

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ViewMethodMapper()
        {
            Factory.Singleton.ResolveSingleton<IHeartbeatService>().SlowTick += Heartbeat_SlowTick;
        }

        /// <summary>
        /// Records a request for a <see cref="WebAdminView"/>.
        /// </summary>
        /// <param name="webAdminView"></param>
        /// <param name="args"></param>
        public void ViewRequested(WebAdminView webAdminView, TextContentEventArgs args)
        {
            lock(_SyncLock) {
                var pathAndFileWithoutExtension = RemoveExtension(webAdminView.PathAndFile);
                var key = NormaliseString(pathAndFileWithoutExtension);
                var isMultiInstance = args.Content.Contains(ViewIdMarker);

                if(!isMultiInstance) {
                    StartSingleViewInstance(webAdminView, key);
                } else {
                    var viewId = StartMultiViewInstance(webAdminView, key, args);
                    args.Content = args.Content.Replace(ViewIdMarker, viewId);
                }
            }
        }

        private void StartSingleViewInstance(WebAdminView webAdminView, string key)
        {
            MappedView mappedView;

            if(!_SharedViewInstances.TryGetValue(key, out mappedView)) {
                mappedView = CreateView(webAdminView);

                var newMap = CollectionHelper.ShallowCopy(_SharedViewInstances);
                newMap.Add(key, mappedView);
                _SharedViewInstances = newMap;
            }
        }

        private string StartMultiViewInstance(WebAdminView webAdminView, string key, TextContentEventArgs args)
        {
            Dictionary<string, MappedView> runningInstances;

            if(!_MultiViewInstances.TryGetValue(key, out runningInstances)) {
                runningInstances = new Dictionary<string,MappedView>();
            }

            string viewId;
            do {
                viewId = Guid.NewGuid().ToString();
            } while(runningInstances.ContainsKey(viewId));

            MappedView mappedView = CreateView(webAdminView);
            mappedView.ViewId = viewId;

            var newInnerMap = CollectionHelper.ShallowCopy(runningInstances);
            newInnerMap.Add(viewId, mappedView);

            var newOuterMap = CollectionHelper.ShallowCopy(_MultiViewInstances);
            if(newOuterMap.ContainsKey(key)) {
                newOuterMap[key] = newInnerMap;
            } else {
                newOuterMap.Add(key, newInnerMap);
            }
            _MultiViewInstances = newOuterMap;

            return viewId;
        }

        private MappedView CreateView(WebAdminView webAdminView)
        {
            var result = new MappedView(webAdminView, webAdminView.CreateView());
            FindExposedMethods(result.View.GetType(), result.ExposedMethods);
            result.View.ShowView();

            return result;
        }

        /// <summary>
        /// Pulls out all of the exposed methods from the view into a dictionary, indexed by method name. This does
        /// mean that we can't support overloading of a function, each exposed method must be unique.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodMap"></param>
        /// <returns></returns>
        private void FindExposedMethods(Type type, Dictionary<string, ExposedMethod> methodMap)
        {
            var reservedMethodNames = new string[] {
                NormaliseString(ViewHeartbeatMethod),
                NormaliseString(GetDeferredResponseMethod),
            };

            foreach(var method in type.GetMethods()) {
                var webAdminMethodAttribute = method.GetCustomAttributes(inherit: true).OfType<WebAdminMethodAttribute>().SingleOrDefault();
                if(webAdminMethodAttribute != null) {
                    var key = NormaliseString(method.Name);

                    if(reservedMethodNames.Contains(key)) {
                        throw new InvalidOperationException($"Views cannot expose a method called {method.Name}, it has been reserved for use by the mapper");
                    }
                    if(methodMap.ContainsKey(key)) {
                        throw new InvalidOperationException("Function overloading is not supported by the view mapper");
                    }

                    var exposedMethod = new ExposedMethod(webAdminMethodAttribute, method);
                    methodMap.Add(key, exposedMethod);
                }
            }
        }

        /// <summary>
        /// Handles a potential request for a JSON file.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="responder"></param>
        /// <returns></returns>
        public bool ProcessJsonRequest(RequestReceivedEventArgs args, IResponder responder)
        {
            var result = false;

            var mappedView = FindMappedView(args);
            if(mappedView != null) {
                mappedView.LastContactUtc = DateTime.UtcNow;

                var normalisedFile = NormaliseString(args.File);
                if(normalisedFile == NormaliseString(ViewHeartbeatMethod)) {
                    result = true;
                } else if(normalisedFile == NormaliseString(GetDeferredResponseMethod)) {
                    ProcessRequestForDeferredResponse(args, responder);
                    result = true;
                } else {
                    if(mappedView.ExposedMethods.TryGetValue(normalisedFile, out var exposedMethod)) {
                        var response = new JsonResponse();

                        try {
                            var parameters = MapParameters(exposedMethod, args);

                            //if(!exposedMethod.WebAdminMethod.DeferExecution) {        <-- see comments against WebAdminMethod.DeferExecution, flag is retired until OWIN gives us enough control to support it
                                response.Response = exposedMethod.MethodInfo.Invoke(mappedView.View, parameters);
                            //} else {
                            //    var deferredMethod = new DeferredMethod(args.UniqueId, mappedView, exposedMethod, parameters);
                            //    lock(_SyncLock) {
                            //        var newMethods = CollectionHelper.ShallowCopy(_DeferredMethods);
                            //        newMethods.Add(deferredMethod.RequestId, deferredMethod);
                            //        _DeferredMethods = newMethods;
                            //    }
                            //
                            //    response.Response = new DeferredExecutionResult() {
                            //        JobId = deferredMethod.JobId,
                            //    };
                            //}
                        } catch(Exception ex) {
                            try {
                                var log = Factory.Singleton.ResolveSingleton<ILog>();
                                log.WriteLine("Caught exception during processing of request for {0}: {1}", args.Request.RawUrl, ex);
                            } catch { }
                            response.Exception = ex.Message;
                            response.Response = null;
                        }

                        var jsonText = JsonConvert.SerializeObject(response);
                        responder.SendText(args.Request, args.Response, jsonText, Encoding.UTF8, MimeType.Json, cacheSeconds: 0);

                        result = true;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Processes the deferred method for the unique ID passed across.
        /// </summary>
        /// <param name="requestUniqueId"></param>
        public void ExecuteDeferredMethodForRequestId(long requestUniqueId)
        {
            var deferredMethods = _DeferredMethods;
            if(deferredMethods.Count > 0) {
                if(deferredMethods.TryGetValue(requestUniqueId, out var deferredMethod)) {
                    lock(_SyncLock) {
                        if(_DeferredMethods.ContainsKey(requestUniqueId)) {
                            var newMap = CollectionHelper.ShallowCopy(_DeferredMethods);
                            newMap.Remove(requestUniqueId);
                            _DeferredMethods = newMap;
                        }
                    }

                    var response = new JsonResponse();
                    try {
                        response.Response = deferredMethod.ExposedMethod.MethodInfo.Invoke(deferredMethod.MappedView.View, deferredMethod.Parameters);
                    } catch(Exception ex) {
                        try {
                            var log = Factory.Singleton.ResolveSingleton<ILog>();
                            log.WriteLine("Caught exception during processing of deferred request for {0}.{1}: {2}",
                                deferredMethod.MappedView.View.GetType().Name,
                                deferredMethod.ExposedMethod.MethodInfo.Name,
                                ex
                            );
                        } catch { }
                        response.Exception = ex.Message;
                        response.Response = null;
                    }

                    var jsonText = JsonConvert.SerializeObject(response);
                    var deferredResponse = new DeferredMethodResponse(deferredMethod.JobId, jsonText);
                    lock(_SyncLock) {
                        var newMap = CollectionHelper.ShallowCopy(_DeferredResponses);
                        newMap.Add(deferredResponse.JobId, deferredResponse);
                        _DeferredResponses = newMap;
                    }
                }
            }
        }

        /// <summary>
        /// Handles requests for deferred responses.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="responder"></param>
        private void ProcessRequestForDeferredResponse(RequestReceivedEventArgs args, IResponder responder)
        {
            var jobId = args.QueryString[JobIdParameterName] ?? args.Request.FormValues[JobIdParameterName];
            if(!String.IsNullOrEmpty(jobId)) {
                string jsonText;

                var deferredResponses = _DeferredResponses;
                if(deferredResponses.TryGetValue(jobId, out DeferredMethodResponse response)) {
                    jsonText = response.JsonResponseText;
                } else {
                    var jsonResponse = new JsonResponse();
                    jsonResponse.Response = new DeferredExecutionResult() {
                        JobId = jobId,
                    };
                    jsonText = JsonConvert.SerializeObject(jsonResponse);
                }

                responder.SendText(args.Request, args.Response, jsonText, Encoding.UTF8, MimeType.Json, cacheSeconds: 0);
            }
        }

        private MappedView FindMappedView(RequestReceivedEventArgs args)
        {
            var sharedViewInstances = _SharedViewInstances;
            var multiViewInstances = _MultiViewInstances;

            var normalisedPath = NormaliseString(args.Path);

            if(!sharedViewInstances.TryGetValue(normalisedPath, out MappedView result)) {
                Dictionary<string, MappedView> viewInstances;
                if(multiViewInstances.TryGetValue(normalisedPath, out viewInstances)) {
                    var viewId = args.QueryString[ViewIdParameterName] ?? args.Request.FormValues[ViewIdParameterName];
                    if(viewId != null) {
                        viewInstances.TryGetValue(viewId, out result);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Builds the parameters for an exposed method from the request passed in.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private object[] MapParameters(ExposedMethod exposedMethod, RequestReceivedEventArgs args)
        {
            var result = new List<object>();

            var values = new Dictionary<string, string>();
            foreach(var queryStringKey in args.QueryString.AllKeys) {
                var name = NormaliseString(queryStringKey);
                var textValue = args.QueryString[queryStringKey];
                values.Add(name, textValue);
            }
            foreach(var formKey in args.Request.FormValues.AllKeys) {
                var name = NormaliseString(formKey);
                var textValue = args.Request.FormValues[formKey];
                values.Add(name, textValue);
            }

            if(!String.IsNullOrEmpty(exposedMethod.WebAdminMethod.User)) {
                var userParameter = exposedMethod.WebAdminMethod.User;
                if(values.ContainsKey(userParameter)) {
                    values.Remove(userParameter);
                }
                values.Add(userParameter, args.UserName);
            }

            foreach(var parameterInfo in exposedMethod.MethodInfo.GetParameters()) {
                var name = NormaliseString(parameterInfo.Name);

                if(values.ContainsKey(name)) {
                    result.Add(ParseTextValue(parameterInfo, values[name]));
                } else {
                    if(parameterInfo.IsOptional) {
                        result.Add(parameterInfo.DefaultValue);
                    } else {
                        throw new InvalidOperationException($"The parameter {parameterInfo.Name} is required but was not supplied");
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Parses a text value into an object of the type required by the parameter.
        /// </summary>
        /// <param name="parameterInfo"></param>
        /// <param name="textValue"></param>
        /// <returns></returns>
        private object ParseTextValue(ParameterInfo parameterInfo, string textValue)
        {
            object result = null;

            if(parameterInfo.ParameterType == typeof(string)) {
                result = textValue;
            } else if(parameterInfo.ParameterType.IsClass) {
                if(!String.IsNullOrEmpty(textValue)) {
                    result = JsonConvert.DeserializeObject(textValue, parameterInfo.ParameterType);
                }
            } else {
                result = CustomConvert.ChangeType(textValue, parameterInfo.ParameterType, CultureInfo.InvariantCulture);
            }

            return result;
        }

        /// <summary>
        /// Normalises strings.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string NormaliseString(string text)
        {
            return (text ?? "").ToLower();
        }

        /// <summary>
        /// Returns the path and file passed across without the extension.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <returns></returns>
        private string RemoveExtension(string pathAndFile)
        {
            var result = pathAndFile;

            if(!String.IsNullOrEmpty(pathAndFile)) {
                var dotIndex = pathAndFile.LastIndexOf('.');
                if(dotIndex > -1) {
                    result = pathAndFile.Substring(0, dotIndex);
                }
            }

            return result;
        }

        /// <summary>
        /// Disposes of views that were removed in the last search for inactive views.
        /// </summary>
        /// <remarks>
        /// We mark views for termination when no browser has called them in n-seconds, but we don't
        /// dispose of them straight away because something might have called them while the removal
        /// code is running, and we don't want to dispose of it while it's in use. They get disposed
        /// in the next pass, and it's this function that does the disposing.
        /// </remarks>
        private void DisposeOfViewsFoundInLastPass()
        {
            foreach(var mappedView in _DetentionBlockAA23) {
                try {
                    mappedView.View.Dispose();
                    System.Diagnostics.Debug.WriteLine(String.Format("View {0} terminated", mappedView.ViewId));
                } catch {
                    try {
                        var log = Factory.Singleton.ResolveSingleton<ILog>();
                        log.WriteLine("Caught exception while disposing of multi-instance view {0}", mappedView.View.GetType().Name);
                    } catch {
                    }
                }
            }

            _DetentionBlockAA23.Clear();
        }

        /// <summary>
        /// Removes views that have not been called for a while.
        /// </summary>
        private void RemoveInactiveViews()
        {
            var threshold = DateTime.UtcNow.AddSeconds(-ViewInactivitySeconds);
            if(_MultiViewInstances.Any(r => r.Value.Any(i => i.Value.LastContactUtc < threshold))) {
                var newOuterMap = CollectionHelper.ShallowCopy(_MultiViewInstances);
                foreach(var kvpOuter in newOuterMap.ToArray()) {
                    var pathAndFile = kvpOuter.Key;
                    var newInnerMap = CollectionHelper.ShallowCopy(kvpOuter.Value);

                    foreach(var kvpInner in newInnerMap.Where(r => r.Value.LastContactUtc < threshold).ToArray()) {
                        var viewId = kvpInner.Key;
                        var mappedView = kvpInner.Value;

                        newInnerMap.Remove(viewId);
                        _DetentionBlockAA23.Add(mappedView);

                        System.Diagnostics.Debug.WriteLine(String.Format("View {0} scheduled for termination", viewId));
                    }

                    if(newInnerMap.Values.Count == 0) {
                        newOuterMap.Remove(pathAndFile);
                    } else {
                        newOuterMap[pathAndFile] = newInnerMap;
                    }
                }

                _MultiViewInstances = newOuterMap;
            }
        }

        /// <summary>
        /// Removes deferred executions that are still waiting for a request to finish after so-many minutes have elapsed.
        /// </summary>
        /// <remarks>
        /// This should not happen in real life, any deferred executions that are still queued after several minutes
        /// have passed would indicate a bug somewhere.
        /// </remarks>
        private void RemoveOrpanedDeferredExecutions()
        {
            var threshold = DateTime.UtcNow.AddMinutes(-CleanUpDeferredExecutionsIntervalMinutes);
            var removeList = _DeferredMethods.Where(r => r.Value.CreatedUtc <= threshold).Select(r => r.Key).ToArray();
            if(removeList.Length > 0) {
                var log = Factory.Singleton.ResolveSingleton<ILog>();

                foreach(var removeKey in removeList) {
                    var value = _DeferredMethods[removeKey];
                    _DeferredMethods.Remove(removeKey);

                    log.WriteLine("Removed deferred execution for {0}.{1}, waiting on request {2} for {3} minutes",
                        value.MappedView.View.GetType().Name,
                        value.ExposedMethod.MethodInfo.Name,
                        value.RequestId,
                        CleanUpDeferredExecutionsIntervalMinutes
                    );
                }
            }
        }

        /// <summary>
        /// Removes deferred execution responses that no browser has fetched. These can happen in real life, the browser
        /// page might be closed before the response can be retrieved.
        /// </summary>
        private void RemoveUnclaimedDeferredExecutionResponses()
        {
            var threshold = DateTime.UtcNow.AddMinutes(-CleanUpDeferredExecutionResponseMinutes);
            var removeList = _DeferredResponses.Where(r => r.Value.CreatedUtc <= threshold).Select(r => r.Key).ToArray();

            foreach(var removeKey in removeList) {
                _DeferredResponses.Remove(removeKey);
            }
        }

        /// <summary>
        /// Called on every slow timer tick from the heartbeat service.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Heartbeat_SlowTick(object sender, EventArgs e)
        {
            if(_LastInactiveViewCheckUtc.AddSeconds(InactiveViewCheckIntervalSeconds) <= DateTime.UtcNow) {
                lock(_SyncLock) {
                    _LastInactiveViewCheckUtc = DateTime.UtcNow;
                    DisposeOfViewsFoundInLastPass();
                    RemoveInactiveViews();
                    RemoveOrpanedDeferredExecutions();
                    RemoveUnclaimedDeferredExecutionResponses();
                }
            }
        }
    }
}
