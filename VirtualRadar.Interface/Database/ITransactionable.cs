// Copyright © 2010 onwards, Andrew Whewell
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
using System.Data;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface.Database
{
    /// <summary>
    /// The interface that should be implemented by database objects that support transactions.
    /// </summary>
    public interface ITransactionable
    {
        /// <summary>
        /// Perform an action within a transaction. If the action returns true then the transaction is
        /// committed, if an exception is thrown or it returns false then it is rolled back.
        /// </summary>
        /// <param name="action"></param>
        /// <returns>
        /// True if the transaction was committed, false if it was rolled back. Note that any exception
        /// that causes a rollback will be rethrown so this is actually mirroring the return value from
        /// the action.
        /// </returns>
        /// <remarks>
        /// Implementations do not have to ensure that transactions started here are valid across all
        /// threads. Do not perform database work on a background thread in <paramref name="action"/>.
        /// </remarks>
        bool PerformInTransaction(Func<bool> action);
    }
}
