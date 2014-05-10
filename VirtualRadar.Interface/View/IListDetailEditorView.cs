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
using System.ComponentModel;

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// The interface that views capable of presenting and maintaining a list
    /// of records should implement.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IListDetailEditorView<T> : IValidateView, IConfirmView
    {
        /// <summary>
        /// Gets the list of records to show to the user.
        /// </summary>
        List<T> Records { get; }

        /// <summary>
        /// Gets the currently selected record, if any.
        /// </summary>
        T SelectedRecord { get; set; }

        /// <summary>
        /// Raised when the user has selected a record in the list.
        /// </summary>
        event EventHandler SelectedRecordChanged;

        /// <summary>
        /// Raised when the user has indicated that they want to rollback their changes
        /// to the selected record.
        /// </summary>
        event EventHandler ResetClicked;

        /// <summary>
        /// Raised when the user modifies a value on the selected record.
        /// </summary>
        event EventHandler ValueChanged;

        /// <summary>
        /// Raised when the user indicates that they want to create a new record.
        /// </summary>
        event EventHandler NewRecordClicked;

        /// <summary>
        /// Raised when the user indicates that they want to delete an existing record.
        /// </summary>
        event EventHandler DeleteRecordClicked;

        /// <summary>
        /// Raised when the user indicates that they want to save their changes.
        /// </summary>
        event EventHandler<CancelEventArgs> SaveClicked;

        /// <summary>
        /// Refreshes the list of records shown to the user.
        /// </summary>
        void RefreshRecords();

        /// <summary>
        /// Refreshes the fields that hold the content of the selected record.
        /// </summary>
        void RefreshSelectedRecord();

        /// <summary>
        /// Sets the user's input focus to the first field that represents the
        /// content of the selected record.
        /// </summary>
        void FocusOnEditFields();
    }
}
