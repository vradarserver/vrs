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
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;

// Models that are common between plugins.
namespace VirtualRadar.Interface.WebSite.WebAdminModels
{
    /// <summary>
    /// Describes the properties of an <see cref="Access"/> object.
    /// </summary>
    public class AccessModel
    {
        /// <summary>
        /// Gets or sets the numeric value of a <see cref="VirtualRadar.Interface.Settings.DefaultAccess"/> enum.
        /// </summary>
        public int DefaultAccess { get; set; }

        /// <summary>
        /// Gets a list of addresses.
        /// </summary>
        public List<CidrModel> Addresses { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AccessModel()
        {
            Addresses = new List<CidrModel>();
        }

        /// <summary>
        /// Creates an object from an existing <see cref="Access"/>.
        /// </summary>
        /// <param name="settings"></param>
        public AccessModel(Access settings) : this()
        {
            RefreshFromSettings(settings);
        }

        /// <summary>
        /// Copies the values of an existing <see cref="Access"/>.
        /// </summary>
        /// <param name="settings"></param>
        public void RefreshFromSettings(Access settings)
        {
            DefaultAccess = (int)settings.DefaultAccess;
            CollectionHelper.OverwriteDestinationWithSource(settings.Addresses.Select(r => new CidrModel(r)).ToList(), Addresses);
        }

        /// <summary>
        /// Overwrites an <see cref="Access"/> with values from the model's properties.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public Access CopyToSettings(Access settings)
        {
            settings.DefaultAccess = EnumModel.CastFromInt<DefaultAccess>(DefaultAccess);
            CollectionHelper.OverwriteDestinationWithSource(Addresses.Select(r => r.Cidr).ToList(), settings.Addresses);

            return settings;
        }
    }

    /// <summary>
    /// Represents a CIDR in an <see cref="AccessModel"/>.
    /// </summary>
    public class CidrModel
    {
        /// <summary>
        /// Gets or sets the CIDR.
        /// </summary>
        public string Cidr { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public CidrModel()
        {
        }

        /// <summary>
        /// Creates an object from a CIDR.
        /// </summary>
        /// <param name="address"></param>
        public CidrModel(string address)
        {
            Cidr = address;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Cidr;
        }
    }
}
