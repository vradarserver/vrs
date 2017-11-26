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
using System.Linq;
using System.Text;
using System.Data;
using VirtualRadar.Interface.Database;

namespace VirtualRadar.Database
{
    /// <summary>
    /// A helper class for database objects that implement <see cref="ITransactionable"/>.
    /// </summary>
    public class TransactionHelper
    {
        /// <summary>
        /// See <see cref="ITransactionable.PerformInTransaction(Func{bool})"/>.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="inTransaction"></param>
        /// <param name="allowNestedTransaction"></param>
        /// <param name="recordTransaction"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool PerformInTransaction(IDbConnection connection, bool inTransaction, bool allowNestedTransaction, Action<IDbTransaction> recordTransaction, Func<bool> action)
        {
            if(inTransaction && !allowNestedTransaction) {
                throw new InvalidOperationException("An attempt was made to start a nested transaction");
            }

            using(var transaction = connection.BeginTransaction()) {
                try {
                    recordTransaction?.Invoke(transaction);

                    var result = action();
                    if(result) {
                        transaction.Commit();
                    } else {
                        transaction.Rollback();
                    }

                    return result;
                } catch {
                    transaction.Rollback();
                    throw;
                } finally {
                    try {
                        recordTransaction?.Invoke(null);
                    } catch {
                    }
                }
            }
        }
    }
}
