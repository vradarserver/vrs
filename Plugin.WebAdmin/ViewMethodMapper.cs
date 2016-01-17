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
        /// Describes a web admin view and the object that handles the JSON requests from the HTML.
        /// </summary>
        class MappedView
        {
            public WebAdminView                     WebAdminView { get; private set; }
            public IView                            View { get; private set; }
            public string                           ViewId { get; set; }
            public Dictionary<string, MethodInfo>   ExposedMethods { get; private set; }
            public DateTime                         LastContactUtc { get; set; }

            public MappedView(WebAdminView webAdminView, IView view)
            {
                WebAdminView = webAdminView;
                View = view;
                ExposedMethods = new Dictionary<string,MethodInfo>();
                LastContactUtc = DateTime.UtcNow;
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
        /// The name of the fake method that browsers should periodically call to prevent their view from getting zapped.
        /// </summary>
        private const string ViewHeartbeatMethod = "BrowserHeartbeat";

        /// <summary>
        /// The number of seconds that have to elapse with no message from a multi-instance view before that view is disposed.
        /// </summary>
        private const int ViewInactivitySeconds = 120;

        /// <summary>
        /// The number of seconds to wait between each check for inactive views.
        /// </summary>
        private const int InactiveViewCheckIntervalSeconds = 30;

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
        /// Creates a new object.
        /// </summary>
        public ViewMethodMapper()
        {
            Factory.Singleton.Resolve<IHeartbeatService>().Singleton.SlowTick += Heartbeat_SlowTick;
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
        /// <returns></returns>
        private void FindExposedMethods(Type type, Dictionary<string, MethodInfo> methodMap)
        {
            var reservedMethodName = NormaliseString(ViewHeartbeatMethod);

            foreach(var method in type.GetMethods()) {
                var webAdminMethodAttribute = method.GetCustomAttributes(inherit: true).OfType<WebAdminMethodAttribute>().SingleOrDefault();
                if(webAdminMethodAttribute != null) {
                    var key = NormaliseString(method.Name);

                    if(key == reservedMethodName) {
                        throw new InvalidOperationException(String.Format("Views cannot expose a method called {0}, it has been reserved for use by the mapper", reservedMethodName));
                    }
                    if(methodMap.ContainsKey(key)) {
                        throw new InvalidOperationException("Function overloading is not supported by the view mapper");
                    }

                    methodMap.Add(key, method);
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
                } else {
                    MethodInfo exposedMethod;
                    if(mappedView.ExposedMethods.TryGetValue(normalisedFile, out exposedMethod)) {
                        var response = new JsonResponse();

                        try {
                            var parameters = MapParameters(exposedMethod, args);
                            response.Response = exposedMethod.Invoke(mappedView.View, parameters);
                        } catch(Exception ex) {
                            try {
                                var log = Factory.Singleton.Resolve<ILog>().Singleton;
                                log.WriteLine("Caught exception during processing of request for {0}: {1}", args.Request.RawUrl, ex);
                            } catch {}
                            response.Exception = ex.Message;
                            response.Response = null;
                        }

                        var jsonText = JsonConvert.SerializeObject(response);
                        responder.SendText(args.Request, args.Response, jsonText, Encoding.UTF8, MimeType.Json);

                        result = true;
                    }
                }
            }

            return result;
        }

        private MappedView FindMappedView(RequestReceivedEventArgs args)
        {
            var sharedViewInstances = _SharedViewInstances;
            var multiViewInstances = _MultiViewInstances;

            var normalisedPath = NormaliseString(args.Path);

            MappedView result = null;
            if(!sharedViewInstances.TryGetValue(normalisedPath, out result)) {
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
        private object[] MapParameters(MethodInfo methodInfo, RequestReceivedEventArgs args)
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

            foreach(var parameterInfo in methodInfo.GetParameters()) {
                var name = NormaliseString(parameterInfo.Name);

                if(values.ContainsKey(name)) {
                    result.Add(ParseTextValue(parameterInfo, values[name]));
                } else {
                    if(parameterInfo.IsOptional) {
                        result.Add(parameterInfo.DefaultValue);
                    } else {
                        throw new InvalidOperationException(String.Format("The parameter {0} is required but was not supplied", parameterInfo.Name));
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
                        var log = Factory.Singleton.Resolve<ILog>().Singleton;
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
                }
            }
        }
    }
}
