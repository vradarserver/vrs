// Copyright © 2013 onwards, Andrew Whewell
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
using System.ComponentModel;
using System.Globalization;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// A type converter between either a MergedFeed or a Recevier and a unique ID from all available receivers and merged feeds.
    /// </summary>
    class FeedTypeConverter : Int32Converter
    {
        /// <summary>
        /// A class that describes the common details between receivers and merged feeds.
        /// </summary>
        public class Feed
        {
            public int UniqueId { get; set; }

            public string Name { get; set; }

            public Feed(int uniqueId, string name)
            {
                UniqueId = uniqueId;
                Name = name;
            }
        }

        /// <summary>
        /// Builds a list of all unique IDs across receivers and merged feeds.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<int> BuildAllIds()
        {
            var result = ReceiverTypeConverter.Receivers.Select(r => r.UniqueId);
            result = result.Concat(MergedFeedTypeConverter.MergedFeeds.Select(r => r.UniqueId));

            return result;
        }

        /// <summary>
        /// Builds a summary of all receivers and merged feeds.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Feed> BuildAllFeeds()
        {
            var result = ReceiverTypeConverter.Receivers.Select(r => new Feed(r.UniqueId, r.Name));
            result = result.Concat(MergedFeedTypeConverter.MergedFeeds.Select(r => new Feed(r.UniqueId, r.Name)));

            return result;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(BuildAllIds().ToList());
        }

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
            var name = (string)value;
            return BuildAllFeeds().Where(r => r.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).Select(r => r.UniqueId).FirstOrDefault();
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
            var uniqueId = (int)value;
            return BuildAllFeeds().Where(r => r.UniqueId == uniqueId).Select(r => r.Name).FirstOrDefault() ?? "";
        }
    }
}
