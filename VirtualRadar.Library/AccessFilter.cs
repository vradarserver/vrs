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
using System.Net;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IAccessFilter"/>.
    /// </summary>
    class AccessFilter : IAccessFilter
    {
        #region Fields
        /// <summary>
        /// The object that we use to restrict access to the fields to a single thread.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// True if the object has been initialised.
        /// </summary>
        private bool _Initialised;

        /// <summary>
        /// The default access to allow for addresses that are not known.
        /// </summary>
        private DefaultAccess _DefaultAccess;

        /// <summary>
        /// The list of CIDRs that match the addresses on the Access object, as at the time the filter
        /// was initialised.
        /// </summary>
        /// <remarks>
        /// If this was public then I would initialise this so that it could never be null, and so that
        /// the address was fixed over the lifetime of the program. However. This is not public, the only
        /// code that will use this is in this class. Also I want people to be able to call Initialise
        /// and create a new list while the existing list is still being used on another thread that was
        /// in the middle of an <see cref="Allow"/> call. The <see cref="Allow"/> call takes a copy of all
        /// of the fields that <see cref="Initialise"/> can change from within a lock, and I don't want to
        /// have to create a List and populate it every time <see cref="Allow"/> is called. So I treat this
        /// list like a simple reference type - it starts at null, <see cref="Initialise"/> creates new
        /// instances and other methods can use the reference even if Initialise subsequently blats it.
        /// </remarks>
        private List<Cidr> _Cidrs;
        #endregion

        #region Initialise, Allow
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="access"></param>
        public void Initialise(Access access)
        {
            if(access == null) throw new ArgumentNullException("access");

            lock(_SyncLock) {
                _Initialised = true;
                _DefaultAccess = access.DefaultAccess;

                _Cidrs = new List<Cidr>();
                foreach(var address in access.Addresses) {
                    _Cidrs.Add(Cidr.Parse(address));
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool Allow(IPAddress address)
        {
            var defaultAccess = default(DefaultAccess);
            List<Cidr> cidrs = null;

            lock(_SyncLock) {
                if(!_Initialised) throw new InvalidOperationException("Initialise must be called before calling Allow");
                defaultAccess = _DefaultAccess;
                cidrs = _Cidrs;
            }

            bool result;
            switch(defaultAccess) {
                case DefaultAccess.Unrestricted:    result = true; break;
                case DefaultAccess.Allow:           result = !CidrsMatch(cidrs, address); break;
                case DefaultAccess.Deny:            result = CidrsMatch(cidrs, address); break;
                default:                            throw new NotImplementedException();
            }

            return result;
        }

        /// <summary>
        /// Returns true if the address matches at least one of the CIDRs. If there are no CIDRS
        /// then false is returned.
        /// </summary>
        /// <param name="cidrs"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        private bool CidrsMatch(List<Cidr> cidrs, IPAddress address)
        {
            var result = false;

            foreach(var cidr in cidrs) {
                result = cidr.Matches(address);
                if(result) break;
            }

            return result;
        }
        #endregion
    }
}
