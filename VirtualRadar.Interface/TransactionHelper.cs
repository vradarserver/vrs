// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A generic transaction class that can help when dealing with nested transactions of some kind.
    /// </summary>
    public class TransactionHelper : ITransactionable
    {
        /// <summary>
        /// The 1-based level of transaction. Transactions are not committed
        /// or rolled back until the nested level returns to 1.
        /// </summary>
        public int NestedLevel { get; private set; }

        /// <summary>
        /// Initialised to true before the first level of transaction occurs, if
        /// this is set to false then no further transactionable actions will be
        /// called and the rollback callback will be called once the nested level
        /// returns to 1.
        /// </summary>
        public bool WillCommitTransaction { get; private set; }

        /// <summary>
        /// The function that is called to begin the transaction.
        /// </summary>
        public Action BeginCallback { get; }

        /// <summary>
        /// The function that is called to commit the transaction.
        /// </summary>
        public Action CommitCallback { get; }

        /// <summary>
        /// The function that is called to rollback the transaction.
        /// </summary>
        public Action RollbackCallback { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="beginCallback"></param>
        /// <param name="commitCallback"></param>
        /// <param name="rollbackCallback"></param>
        public TransactionHelper(Action beginCallback, Action commitCallback, Action rollbackCallback)
        {
            BeginCallback = beginCallback;
            CommitCallback = commitCallback;
            RollbackCallback = rollbackCallback;
        }

        /// <inheritdoc/>
        public bool PerformInTransaction(Func<bool> action)
        {
            if(++NestedLevel == 1) {
                WillCommitTransaction = true;
            }

            var result = false;
            try {
                if(NestedLevel == 1) {
                    BeginCallback?.Invoke();
                }
                WillCommitTransaction = WillCommitTransaction && (action?.Invoke() ?? false);
                result = WillCommitTransaction;
            } finally {
                if(--NestedLevel == 0) {
                    try {
                        if(result) {
                            CommitCallback?.Invoke();
                        } else {
                            RollbackCallback?.Invoke();
                        }
                    } finally {
                        WillCommitTransaction = false;
                    }
                }
            }

            return result;
        }
    }
}
