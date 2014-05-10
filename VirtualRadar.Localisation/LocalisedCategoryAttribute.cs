// Copyright © 2012 onwards, Andrew Whewell
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
using System.ComponentModel;

namespace VirtualRadar.Localisation
{
    /// <summary>
    /// A localisable version of the ComponentModel <see cref="CategoryAttribute"/>.
    /// </summary>
    public class LocalisedCategoryAttribute : CategoryAttribute
    {
        /// <summary>
        /// Gets the prefix assigned to the category.
        /// </summary>
        public string Prefix { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="key">The index of the string to look up.</param>
        public LocalisedCategoryAttribute(string key) : base(Localise.Lookup(key))
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="displayOrder"></param>
        /// <param name="totalCategories"></param>
        public LocalisedCategoryAttribute(string key, int displayOrder, int totalCategories) : base(AddLeadingSortText(key, BuildPrefix(displayOrder, totalCategories)))
        {
            Prefix = BuildPrefix(displayOrder, totalCategories);
        }

        /// <summary>
        /// Adds leading text to the category to force the sort order in a property grid.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static string AddLeadingSortText(string key, string prefix)
        {
            return String.Format("{0} {1}", prefix, Localise.Lookup(key));
        }

        /// <summary>
        /// Creates the prefix for the category.
        /// </summary>
        /// <param name="displayOrder"></param>
        /// <param name="totalCategories"></param>
        /// <returns></returns>
        private static string BuildPrefix(int displayOrder, int totalCategories)
        {
            return String.Format(String.Format("{{0:{0}}}.", totalCategories > 9 ? "00" : "0"), displayOrder + 1);
        }
    }
}
