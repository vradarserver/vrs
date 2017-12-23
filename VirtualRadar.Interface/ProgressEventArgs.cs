// Copyright © 2017 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Carries progress information.
    /// </summary>
    public class ProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the progress bar's caption.
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets the element number of the item being processed.
        /// </summary>
        public long CurrentItem { get; set; }

        /// <summary>
        /// Gets or sets the total number of items being processed.
        /// </summary>
        public long TotalItems { get; set; }

        /// <summary>
        /// Gets a value indicating that TotalItems is negative one, which represents the unknown quantity.
        /// </summary>
        public bool IsTotalItemsUnknown { get => TotalItems == -1; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ProgressEventArgs() : this("")
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="currentItem"></param>
        /// <param name="totalItems"></param>
        public ProgressEventArgs(string caption, long currentItem = -1, long totalItems = -1)
        {
            Caption = caption ?? "";
            TotalItems = totalItems;
            CurrentItem = Math.Min(currentItem, TotalItems);
        }
    }
}
