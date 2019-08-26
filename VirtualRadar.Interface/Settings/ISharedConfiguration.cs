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
using System.Threading;
using InterfaceFactory;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// Exposes the current configuration.
    /// </summary>
    /// <remarks><para>
    /// For a long time if you wanted to get the current configuration you had to
    /// hook the ConfigurationChanged event on IConfigurationStorage and call Load
    /// when the configuration changed. This was fine, it worked, but there were
    /// two problems with it:
    /// </para>
    /// <list type="number">
    /// <item><description>
    /// Hooking the event was awkward for objects with a limited lifetime. It meant
    /// that they had to be disposable so that there was a point where the unhook
    /// could take place.
    /// </description></item>
    /// <item><description>
    /// When the configuration changes dozens of objects jump in and call Load. Load
    /// reads the configuration off disk, so you had dozens of redundant reads.
    /// </description></item>
    /// </list>
    /// <para>
    /// This class aims to get around these problems. When a configuration value is
    /// required a class can call <see cref="Get"/>. The configuration returned is
    /// guaranteed to be current and consistent - if the configuration is changed
    /// after <see cref="Get"/> is called it won't affect the configuration returned
    /// by the method.
    /// </para><para>
    /// The drawback is that the configuration is shared. Any changes that you make
    /// to the configuration will affect everything using this object. To protect
    /// against bugs introduced by this kind of behaviour the object listens for
    /// changes to the configuration and throws an exception when they're detected.
    /// </para></remarks>
    [Singleton]
    public interface ISharedConfiguration : ISingleton<ISharedConfiguration>
    {
        /// <summary>
        /// Raised when the configuration returned by <see cref="Get"/> has changed.
        /// </summary>
        /// <remarks>
        /// Basically the same as IConfigurationStorage.ConfigurationChanged except
        /// that calling <see cref="Get"/> to get the new configuration doesn't involve
        /// a fresh load from disk.
        /// </remarks>
        event EventHandler ConfigurationChanged;

        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// I didn't make this a property because the configuration it returns can
        /// change between calls, and I didn't want people calling it repeatedly within
        /// a function and getting inconsistencies.
        /// </remarks>
        Configuration Get();
    }
}
