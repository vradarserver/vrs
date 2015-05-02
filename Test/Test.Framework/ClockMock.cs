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
using VirtualRadar.Interface;
using Moq;

namespace Test.Framework
{
    /// <summary>
    /// A mock implementation of <see cref="IClock"/>.
    /// </summary>
    public class ClockMock : Mock<IClock>
    {
        /// <summary>
        /// Gets or sets the value of the UtcNow property.
        /// </summary>
        public DateTime UtcNowValue { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ClockMock() : base()
        {
            DefaultValue = DefaultValue.Mock;
            SetupAllProperties();

            UtcNowValue = DateTime.UtcNow;
            Setup(r => r.UtcNow).Returns(() => { return UtcNowValue; });
        }

        /// <summary>
        /// Advances <see cref="UtcNowValue"/> by so-many milliseconds.
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public DateTime AddMilliseconds(int milliseconds)
        {
            var result = UtcNowValue.AddMilliseconds(milliseconds);
            UtcNowValue = result;

            return result;
        }
    }
}
