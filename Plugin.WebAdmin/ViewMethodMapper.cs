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
            public WebAdminView WebAdminView { get; set; }
            public IView View { get; set; }
            public Dictionary<string, MethodInfo> ExposedMethods { get; private set; }

            public MappedView()
            {
                ExposedMethods = new Dictionary<string,MethodInfo>();
            }
        }

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
        /// The object that can format up responses for us.
        /// </summary>
        private IResponder _Responder;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ViewMethodMapper()
        {
            _Responder = Factory.Singleton.Resolve<IResponder>();
        }

        /// <summary>
        /// Records a request for a <see cref="WebAdminView"/>.
        /// </summary>
        /// <param name="webAdminView"></param>
        public void ViewRequested(WebAdminView webAdminView)
        {
            MappedView mappedView;

            lock(_SyncLock) {
                var pathAndFileWithoutExtension = RemoveExtension(webAdminView.PathAndFile);
                var key = NormaliseString(pathAndFileWithoutExtension);
                if(!_SharedViewInstances.TryGetValue(key, out mappedView)) {
                    mappedView = new MappedView() {
                        WebAdminView = webAdminView,
                        View = webAdminView.CreateView(),
                    };
                    FindExposedMethods(mappedView.View.GetType(), mappedView.ExposedMethods);
                    mappedView.View.ShowView();

                    var newMap = CollectionHelper.ShallowCopy(_SharedViewInstances);
                    newMap.Add(key, mappedView);
                    _SharedViewInstances = newMap;
                }
            }
        }

        /// <summary>
        /// Pulls out all of the exposed methods from the view into a dictionary, indexed by method name. This does
        /// mean that we can't support overloading of a function, each exposed method must be unique.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private void FindExposedMethods(Type type, Dictionary<string, MethodInfo> methodMap)
        {
            foreach(var method in type.GetMethods()) {
                var webAdminMethodAttribute = method.GetCustomAttributes(inherit: true).OfType<WebAdminMethodAttribute>().SingleOrDefault();
                if(webAdminMethodAttribute != null) {
                    var key = NormaliseString(method.Name);
                    if(methodMap.ContainsKey(key)) {
                        throw new NotImplementedException("Function overloading is not supported by the view mapper");
                    }
                    methodMap.Add(key, method);
                }
            }
        }

        /// <summary>
        /// Handles a potential request for a JSON file.
        /// </summary>
        /// <returns></returns>
        public bool ProcessJsonRequest(RequestReceivedEventArgs args)
        {
            var result = false;

            var sharedViewInstances = _SharedViewInstances;
            var normalisedPath = NormaliseString(args.Path);

            MappedView mappedView;
            if(sharedViewInstances.TryGetValue(normalisedPath, out mappedView)) {
                var normalisedFile = NormaliseString(args.File);
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
                    _Responder.SendText(args.Request, args.Response, jsonText, Encoding.UTF8, MimeType.Json);

                    result = true;
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
    }
}
