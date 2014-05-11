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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// A type converter that can convert from a list of user IDs to a string and back.
    /// </summary>
    class UsersListTypeConverter : TypeConverter
    {
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var list = context.Instance as List<string>;
            if(list != null) {
                list.Clear();
                var userManager = Factory.Singleton.Resolve<IUserManager>().Singleton;

                foreach(var chunk in ((string)value).Split(',', ' ')) {
                    var loginName = chunk.Trim();
                    if(!String.IsNullOrEmpty(loginName)) {
                        var user = userManager.GetUserByLoginName(loginName);
                        if(user != null) list.Add(user.UniqueId);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var result = "";
            var ids = value as List<string>;
            if(ids != null && ids.Count > 0) {
                var userManager = Factory.Singleton.Resolve<IUserManager>().Singleton;
                var buffer = new StringBuilder();
                foreach(var id in ids) {
                    var user = userManager.GetUserByLoginName(id);
                    if(user != null) {
                        if(buffer.Length > 0) buffer.Append(", ");
                        buffer.Append(user.LoginName);
                    }
                }
                result = buffer.ToString();
            }

            return result;
        }
    }
}
