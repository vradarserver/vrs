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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// An interface for objects that may be utilising background threads and want to allow exceptions
    /// on those threads to bubble up to the GUI thread.
    /// </summary>
    /// <remarks><para>
    /// Many interfaces need to perform tasks on a background thread. In general an exception on a background thread
    /// will end up going to the unhandled exception handler, which will log it and show it to the user. However
    /// leaving an exception on a background thread unhandled can cause problems, particularly if the exception is
    /// raised by an event handler. It may prevent other event handlers from seeing that event.
    /// </para><para>
    /// In many cases the background thread would like to catch exceptions and have them displayed to the user on the
    /// GUI thread, or logged by the normal unhandled exception handler, without having to actually get involved in
    /// how this might be done.
    /// </para><para>
    /// The idea here is that interfaces whose implementation may involve work on a background thread will include this
    /// interface. If the implementation does use background threads then they should catch exceptions raised on those
    /// threads and raise an <see cref="ExceptionCaught"/> event in response. Some other bit of code will then take over
    /// and do whatever is necessary to report or log the exception.
    /// </para><para>
    /// Background threads will usually be sent a ThreadAbortException when the thread is shut down - they should not
    /// send those through <see cref="ExceptionCaught"/>.
    /// </para></remarks>
    /// <example>
    /// <code>
    /// public event EventHandler&lt;EventArgs&lt;Exception&gt;&gt; ExceptionCaught;
    /// 
    /// protected virtual void OnExceptionCaught(EventArgs&lt;Exception&gt; args)
    /// {
    ///     if(ExceptionCaught != null) ExceptionCaught(this, args);
    /// }
    /// 
    /// private void BackgroundThreadMethod()
    /// {
    ///     try {
    ///         // Do some work here...
    ///     } catch(ThreadAbortException) {
    ///         // This is raised if the thread is shutting down, you don't need to pass these on to
    ///         // the user. Whatever you do in this catch block it will always end with the framework
    ///         // throwing another ThreadAbort at the end of the catch.
    ///     } catch(Exception ex) {
    ///         OnExceptionCaught(new EventArgs&lt;Exception&gt;(ex));
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IBackgroundThreadExceptionCatcher
    {
        /// <summary>
        /// Raised when an exception is caught on the background thread. The background thread should not
        /// pass ThreadAbortException through this.
        /// </summary>
        event EventHandler<EventArgs<Exception>> ExceptionCaught;
    }
}
