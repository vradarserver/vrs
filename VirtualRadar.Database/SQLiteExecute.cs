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
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface.SQLite;

namespace VirtualRadar.Database
{
    /// <summary>
    /// A class that can perform some action on a SQLite command repeatedly until the command no longer throws a locked exception.
    /// </summary>
    public class SQLiteExecute
    {
        /// <summary>
        /// Gets or sets a value indicating that the action should be retried if the database is locked.
        /// </summary>
        public bool RetryIfLocked { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds that must elapse before the code gives up retrying an action that is failing due
        /// to a database lock.
        /// </summary>
        public int RetryIfLockedTimeout { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public SQLiteExecute()
        {
            RetryIfLocked = true;
            RetryIfLockedTimeout = 60000;
        }

        /// <summary>
        /// Performs the ExecuteNonQuery operation on the command, returning the result. Will retry the command if
        /// the attempt fails due to a locked database.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(IDbCommand command)
        {
            var result = 0;
            CallActionUntilUnlocked(() => {
                result = command.ExecuteNonQuery();
            });

            return result;
        }

        /// <summary>
        /// Performs the ExecuteReader operation on the command, returning the result. Will retry the command if
        /// the attempt fails due to a locked database.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public IDataReader ExecuteReader(IDbCommand command)
        {
            IDataReader result = null;
            CallActionUntilUnlocked(() => {
                result = command.ExecuteReader();
            });

            return result;
        }

        /// <summary>
        /// Performs the ExecuteReader operation on the command, returning the result. Will retry the command if
        /// the attempt fails due to a locked database.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandBehaviour"></param>
        /// <returns></returns>
        public IDataReader ExecuteReader(IDbCommand command, CommandBehavior commandBehaviour)
        {
            IDataReader result = null;
            CallActionUntilUnlocked(() => {
                result = command.ExecuteReader(commandBehaviour);
            });

            return result;
        }

        /// <summary>
        /// Performs the ExecuteReader operation on the command, returning the result. Will retry the command if
        /// the attempt fails due to a locked database.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandBehaviour"></param>
        /// <returns></returns>
        public object ExecuteScalar(IDbCommand command)
        {
            object result = null;
            CallActionUntilUnlocked(() => {
                result = command.ExecuteScalar();
            });

            return result;
        }

        /// <summary>
        /// Performs the Read operation on a data reader, retrying if the read throws a locked exception.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public bool Read(IDataReader reader)
        {
            var result = false;
            CallActionUntilUnlocked(() => {
                result = reader.Read();
            });

            return result;
        }

        /// <summary>
        /// Calls the action passed across. If the action throws a SQLite locked exception then it retries
        /// the action.
        /// </summary>
        /// <param name="action"></param>
        public void CallActionUntilUnlocked(Action action)
        {
            var executed = false;
            DateTime firstFailureTime = DateTime.MinValue;

            do {
                try {
                    action();
                    executed = true;
                } catch(Exception ex) {
                    var sqliteException = Factory.Singleton.Resolve<ISQLiteException>();
                    sqliteException.Initialise(ex);
                    if(!sqliteException.IsSQLiteException || sqliteException.ErrorCode != SQLiteErrorCode.Locked || !RetryIfLocked) throw;

                    if(firstFailureTime == DateTime.MinValue) firstFailureTime = DateTime.UtcNow;
                    else if(firstFailureTime.AddMilliseconds(RetryIfLockedTimeout) >= DateTime.UtcNow) throw;

                    Thread.Sleep(1);
                }
            } while(!executed);
        }
    }
}
