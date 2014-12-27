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

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// A class that can help when filling DTO objects for transfer to views.
    /// </summary>
    /// <typeparam name="T">The type of the DTO.</typeparam>
    class ViewDtoHelper<T>
    {
        /// <summary>
        /// Gets or sets the data version to set when a value has been updated.
        /// </summary>
        public long DataVersion { get; set; }

        /// <summary>
        /// Gets a delegate that can assign a data version value.
        /// </summary>
        public Action<T, long> SetDataVersion { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="setDataVersion"></param>
        public ViewDtoHelper(Action<T, long> setDataVersion)
        {
            SetDataVersion = setDataVersion;
        }

        /// <summary>
        /// Updates a value on the DTO.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dto"></param>
        /// <param name="dtoValue"></param>
        /// <param name="newValue"></param>
        /// <param name="setValue"></param>
        public void Update<TValue>(T dto, TValue dtoValue, TValue newValue, Action<T, TValue> setValue)
            where TValue: struct
        {
            if(!Object.Equals(dtoValue, newValue)) {
                setValue(dto, newValue);
                SetDataVersion(dto, DataVersion);
            }
        }

        /// <summary>
        /// Updates an integer value on the DTO.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="dtoValue"></param>
        /// <param name="newValue"></param>
        /// <param name="setValue"></param>
        public void Update(T dto, bool dtoValue, bool newValue, Action<T, bool> setValue)
        {
            if(dtoValue != newValue) {
                setValue(dto, newValue);
                SetDataVersion(dto, DataVersion);
            }
        }

        /// <summary>
        /// Updates an integer value on the DTO.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="dtoValue"></param>
        /// <param name="newValue"></param>
        /// <param name="setValue"></param>
        public void Update(T dto, int dtoValue, int newValue, Action<T, int> setValue)
        {
            if(dtoValue != newValue) {
                setValue(dto, newValue);
                SetDataVersion(dto, DataVersion);
            }
        }

        /// <summary>
        /// Updates a string value on the DTO.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="dtoValue"></param>
        /// <param name="newValue"></param>
        /// <param name="setValue"></param>
        public void Update(T dto, string dtoValue, string newValue, Action<T, string> setValue)
        {
            if(dtoValue != newValue) {
                setValue(dto, newValue);
                SetDataVersion(dto, DataVersion);
            }
        }
    }
}
