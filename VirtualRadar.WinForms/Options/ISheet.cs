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
using System.Windows.Forms;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// The interface that Sheet&lt;T&gt; implements. It makes it easier to use a non-generic interface
    /// when we want to deal with sheets in the abstract.
    /// </summary>
    public interface ISheet
    {
        /// <summary>
        /// Gets a collection of pages that are children of this sheet. The pages are containers for variable length collections
        /// of child sheets.
        /// </summary>
        List<ParentPage> Pages { get; }

        /// <summary>
        /// Gets the title of the sheet, the value shown in the sheet selector control that represents this sheet.
        /// </summary>
        string SheetTitle { get; }

        /// <summary>
        /// Gets or sets a tag against the sheet that is never shown to the user or serialised to the configuration options.
        /// </summary>
        object Tag { get; set; }

        /// <summary>
        /// Takes a copy of the current values of the object.
        /// </summary>
        void SetInitialValues();
    }
}
