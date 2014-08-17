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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// The default implementation of <see cref="ICidrEditPresenter"/>.
    /// </summary>
    class CidrEditPresenter : Presenter<ICidrEditView>, ICidrEditPresenter
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CidrIsValid
        {
            get {
                Cidr cidr;
                return !String.IsNullOrEmpty(_View.Cidr) && Cidr.TryParse(_View.Cidr, out cidr);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="view"></param>
        public override void Initialise(ICidrEditView view)
        {
            base.Initialise(view);

            _View.CidrChanged += View_CidrChanged;
            ShowCidrAddresses();
        }

        /// <summary>
        /// Fills the first and last address on the view.
        /// </summary>
        private void ShowCidrAddresses()
        {
            Cidr cidr = null;
            var isEmpty = String.IsNullOrEmpty(_View.Cidr);
            var isValid = !isEmpty && Cidr.TryParse(_View.Cidr, out cidr);

            _View.FirstMatchingAddress = isEmpty ? "" : !isValid ? Strings.CidrInvalid : cidr.FirstMatchingAddress.ToString();
            _View.LastMatchingAddress =  isEmpty ? "" : !isValid ? Strings.CidrInvalid : cidr.LastMatchingAddress.ToString();
        }

        /// <summary>
        /// Called when the user changes the CIDR.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void View_CidrChanged(object sender, EventArgs e)
        {
            ShowCidrAddresses();
        }
    }
}
