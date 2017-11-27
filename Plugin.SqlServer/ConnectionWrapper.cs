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
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Plugin.SqlServer
{
    /// <summary>
    /// Manages the lifetime of a connection and exposes the associated transaction.
    /// </summary>
    /// <remarks><para>
    /// SQLite database interfaces in VRS assume, not surprisingly, that the underlying
    /// database is SQLite. In SQLite there is no multithreading so you generally create
    /// a single connection and reuse it for each call, blocking while each call runs.
    /// </para><para>
    /// With database engines you get multithreading support, and one of the big attractions
    /// with using database engines is to be able to take advantage of that. But we have a
    /// problem with the database interfaces, they're all assuming a SQLite world and I don't
    /// want to have to rewrite all of the database handling.
    /// </para><para>
    /// For calls outside of a transaction there isn't any problem - each call allocates a
    /// connection from the pool as per normal. But when you are inside a transaction it gets
    /// troublesome. The transactions are per-thread and the connection associated with the
    /// transaction needs to live for longer than a single call.
    /// </para><para>
    /// This is where this class comes in. Its constructor is passed a connection and a
    /// transaction and it is disposable. If the transaction is null then the dispose method
    /// will dispose of the constructor, if it is non-null then the dispose method does
    /// nothing.
    /// </para><para>
    /// Database handling code can create the class within a normal using statement and not
    /// have to worry about whether they need to dispose of transactions or not. Code that
    /// creates transactions can handle the lifetime of both the connection and the transaction
    /// without having to worry about calls within the transaction using the wrong connection
    /// or destroying the connection.
    /// </para></remarks>
    class ConnectionWrapper : IDisposable
    {
        /// <summary>
        /// Gets the connection to use.
        /// </summary>
        public IDbConnection Connection { get; }

        /// <summary>
        /// Gets a value indicating whether the connection is present. It will return false when
        /// configuration errors mean that there's no connection to the database and database calls
        /// should give up.
        /// </summary>
        public bool HasConnection { get => Connection != null; }

        /// <summary>
        /// Gets the transaction to use.
        /// </summary>
        public IDbTransaction Transaction { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        public ConnectionWrapper(IDbConnection connection, IDbTransaction transaction)
        {
            Connection = connection;
            Transaction = transaction;
        }

        /// <summary>
        /// Finalises a connection wrapper.
        /// </summary>
        ~ConnectionWrapper()
        {
            Dispose(false);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var transactionState = Transaction == null ? "open" : "closed";
            return $"Connection {Connection.State}, Transaction: {transactionState}";
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                if(Transaction == null && HasConnection) {
                    Connection.Dispose();
                }
            }
        }
    }
}
