﻿// Copyright © 2020 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface.StateHistory
{
    /// <summary>
    /// The interface for objects that can store and retrieve state history records.
    /// </summary>
    /// <remarks>
    /// State history repositories are not expected to be thread-safe. The code that calls them
    /// will ensure that concurrent calls won't happen.
    /// </remarks>
    public interface IStateHistoryRepository
    {
        /// <summary>
        /// Retrieves the latest database version record.
        /// </summary>
        /// <returns></returns>
        DatabaseVersion DatabaseVersion_GetLatest();

        /// <summary>
        /// Saves the database record passed across.
        /// </summary>
        /// <param name="record"></param>
        void DatabaseVersion_Save(DatabaseVersion record);

        /// <summary>
        /// Updates the schema to the latest version. The application is expected to run this once at
        /// program startup.
        /// </summary>
        void Schema_Update();

        /// <summary>
        /// Creates a new session record and fills in its ID.
        /// </summary>
        /// <param name="session"></param>
        void VrsSession_Insert(VrsSession session);
    }
}
