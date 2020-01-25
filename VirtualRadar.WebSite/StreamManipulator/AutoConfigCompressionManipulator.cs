// Copyright © 2020 onwards, Andrew Whewell
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
using System.Threading.Tasks;
using AWhewell.Owin.Interface;
using InterfaceFactory;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.WebSite.StreamManipulator
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// See interface docs.
    /// </summary>
    class AutoConfigCompressionManipulator : IAutoConfigCompressionManipulator, ISharedConfigurationSubscriber
    {
        /// <summary>
        /// The Owin library manipulator that does all the work.
        /// </summary>
        private ICompressResponseManipulator _Wrapped;

        /// <summary>
        /// The singleton shared configuration that we're listening to.
        /// </summary>
        private ISharedConfiguration _SharedConfiguration;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AutoConfigCompressionManipulator()
        {
            _SharedConfiguration = Factory.ResolveSingleton<ISharedConfiguration>();

            _Wrapped = Factory.Resolve<ICompressResponseManipulator>();
            SharedConfigurationChanged(_SharedConfiguration);

            _SharedConfiguration.AddWeakSubscription(this);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public AppFunc AppFuncBuilder(AppFunc next) => _Wrapped.AppFuncBuilder(next);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="sharedConfiguration"></param>
        public void SharedConfigurationChanged(ISharedConfiguration sharedConfiguration)
        {
            _Wrapped.Enabled = sharedConfiguration
                .Get()
                .GoogleMapSettings
                .EnableCompression;
        }
    }
}
