// Copyright © 2016 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Reflection;
using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class EventHelper_Tests
    {
        event EventHandler _Raise_Raises_Event;
        [TestMethod]
        public void Raise_Raises_Event()
        {
            var recorder = new EventRecorder<EventArgs>();
            _Raise_Raises_Event += recorder.Handler;

            var sender = new object();
            var args = new EventArgs();
            EventHelper.Raise(_Raise_Raises_Event, sender, args);

            Assert.AreEqual(1, recorder.CallCount);
            Assert.AreSame(sender, recorder.Sender);
            Assert.AreSame(args, recorder.Args);
        }

        event EventHandler _Raise_Copes_With_Null_Event;
        [TestMethod]
        public void Raise_Copes_With_Null_Event()
        {
            EventHelper.Raise(_Raise_Copes_With_Null_Event, new object(), EventArgs.Empty);
        }

        event EventHandler _Raise_Raises_Event_To_Multiple_Handlers;
        [TestMethod]
        public void Raise_Raises_Event_To_Multiple_Handlers()
        {
            var recorder1 = new EventRecorder<EventArgs>();
            var recorder2 = new EventRecorder<EventArgs>();
            _Raise_Raises_Event_To_Multiple_Handlers += recorder1.Handler;
            _Raise_Raises_Event_To_Multiple_Handlers += recorder2.Handler;

            EventHelper.Raise(_Raise_Raises_Event_To_Multiple_Handlers, new object(), EventArgs.Empty);

            Assert.AreEqual(1, recorder1.CallCount);
            Assert.AreEqual(1, recorder2.CallCount);
        }

        event EventHandler _Raise_Exception_Thrown_In_Handler_Does_Not_Stop_Event_Propagation_When_Exception_Callback_Supplied;
        [TestMethod]
        public void Raise_Exception_Thrown_In_Handler_Does_Not_Stop_Event_Propagation_When_Exception_Callback_Supplied()
        {
            var recorder1 = new EventRecorder<EventArgs>();
            var recorder2 = new EventRecorder<EventArgs>();
            _Raise_Exception_Thrown_In_Handler_Does_Not_Stop_Event_Propagation_When_Exception_Callback_Supplied += recorder1.Handler;
            _Raise_Exception_Thrown_In_Handler_Does_Not_Stop_Event_Propagation_When_Exception_Callback_Supplied += recorder2.Handler;

            var exception1 = new Exception("1");
            var exception2 = new Exception("2");
            recorder1.EventRaised += (sender, args) => { throw exception1; };
            recorder2.EventRaised += (sender, args) => { throw exception2; };

            var seenException1 = false;
            var seenException2 = false;
            EventHelper.Raise(_Raise_Exception_Thrown_In_Handler_Does_Not_Stop_Event_Propagation_When_Exception_Callback_Supplied, new object(), EventArgs.Empty, ex => {
                if(Object.ReferenceEquals(ex, exception1))      seenException1 = true;
                else if(Object.ReferenceEquals(ex, exception2)) seenException2 = true;
            });

            Assert.IsTrue(seenException1);
            Assert.IsTrue(seenException2);
            Assert.AreEqual(1, recorder1.CallCount);
            Assert.AreEqual(1, recorder2.CallCount);
        }

        event EventHandler _Raise_Exception_Thrown_In_ExceptionCallback_Exposed_As_EventHandlerException;
        [TestMethod]
        public void Raise_Exception_Thrown_In_ExceptionCallback_Exposed_As_EventHandlerException()
        {
            var recorder1 = new EventRecorder<EventArgs>();
            var recorder2 = new EventRecorder<EventArgs>();
            _Raise_Exception_Thrown_In_ExceptionCallback_Exposed_As_EventHandlerException += recorder1.Handler;
            _Raise_Exception_Thrown_In_ExceptionCallback_Exposed_As_EventHandlerException += recorder2.Handler;

            recorder1.EventRaised += (sender, args) => { throw new NotImplementedException(); };
            recorder2.EventRaised += (sender, args) => { throw new NotImplementedException(); };

            var exceptionWrapperBubbled = false;
            try {
                EventHelper.Raise(_Raise_Exception_Thrown_In_ExceptionCallback_Exposed_As_EventHandlerException, new object(), EventArgs.Empty, ex => {
                    throw new InvalidOperationException();
                });
            } catch(EventHelperException ex) {
                exceptionWrapperBubbled = true;
                Assert.AreEqual(2, ex.HandlerExceptions.Length);
                Assert.AreEqual(2, ex.HandlerExceptions.Where(r => r is InvalidOperationException).Count());
            }

            Assert.IsTrue(exceptionWrapperBubbled);
            Assert.AreEqual(1, recorder1.CallCount);
            Assert.AreEqual(1, recorder2.CallCount);
        }

        event EventHandler _Raise_Does_Not_Call_All_Handlers_When_ThrowEventHelperException_Is_False;
        [TestMethod]
        public void Raise_Does_Not_Call_All_Handlers_When_ThrowEventHelperException_Is_False()
        {
            var recorder1 = new EventRecorder<EventArgs>();
            var recorder2 = new EventRecorder<EventArgs>();
            _Raise_Does_Not_Call_All_Handlers_When_ThrowEventHelperException_Is_False += recorder1.Handler;
            _Raise_Does_Not_Call_All_Handlers_When_ThrowEventHelperException_Is_False += recorder2.Handler;

            recorder1.EventRaised += (sender, args) => { throw new NotImplementedException(); };
            recorder2.EventRaised += (sender, args) => { throw new NotImplementedException(); };

            var exceptionBubbled = false;
            try {
                EventHelper.Raise(_Raise_Does_Not_Call_All_Handlers_When_ThrowEventHelperException_Is_False, new object(), EventArgs.Empty, null, false);
            } catch(NotImplementedException) {
                exceptionBubbled = true;
            }

            Assert.IsTrue(exceptionBubbled);
            Assert.AreEqual(1, recorder1.CallCount + recorder2.CallCount);
        }

        event EventHandler _Raise_Calls_All_Handlers_When_ThrowEventHelperException_Is_True;
        [TestMethod]
        public void Raise_Calls_All_Handlers_When_ThrowEventHelperException_Is_True()
        {
            var recorder1 = new EventRecorder<EventArgs>();
            var recorder2 = new EventRecorder<EventArgs>();
            _Raise_Calls_All_Handlers_When_ThrowEventHelperException_Is_True += recorder1.Handler;
            _Raise_Calls_All_Handlers_When_ThrowEventHelperException_Is_True += recorder2.Handler;

            var exception1 = new NotImplementedException("1");
            var exception2 = new NotImplementedException("2");
            recorder1.EventRaised += (sender, args) => { throw exception1; };
            recorder2.EventRaised += (sender, args) => { throw exception2; };

            var exceptionBubbled = false;
            var exceptionWrapperBubbled = false;
            try {
                EventHelper.Raise(_Raise_Calls_All_Handlers_When_ThrowEventHelperException_Is_True, new object(), EventArgs.Empty, null, true);
            } catch(NotImplementedException) {
                exceptionBubbled = true;
            } catch(EventHelperException ex) {
                exceptionWrapperBubbled = true;
                Assert.AreEqual(2, ex.HandlerExceptions.Length);
                Assert.IsTrue(ex.HandlerExceptions.Any(r => Object.ReferenceEquals(r, exception1)));
                Assert.IsTrue(ex.HandlerExceptions.Any(r => Object.ReferenceEquals(r, exception2)));
            }

            Assert.IsFalse(exceptionBubbled);
            Assert.IsTrue(exceptionWrapperBubbled);
            Assert.AreEqual(1, recorder1.CallCount);
            Assert.AreEqual(1, recorder2.CallCount);
        }

        event EventHandler _Raise_Handler_Exceptions_Have_Correct_Stack_Trace;
        [TestMethod]
        public void Raise_Handler_Exceptions_Have_Correct_Stack_Trace()
        {
            var recorder = new EventRecorder<EventArgs>();
            _Raise_Handler_Exceptions_Have_Correct_Stack_Trace += recorder.Handler;

            recorder.EventRaised += (sender, args) => { throw new TargetInvocationException(new NotImplementedException()); };

            var exceptionBubbled = false;
            try {
                EventHelper.Raise(_Raise_Handler_Exceptions_Have_Correct_Stack_Trace, this, EventArgs.Empty);
            } catch(NotImplementedException ex) {
                exceptionBubbled = true;
                Assert.IsTrue(ex.StackTrace.Contains("Raise_Handler_Exceptions_Have_Correct_Stack_Trace"));
            }

            Assert.IsTrue(exceptionBubbled);
        }

        event EventHandler<EventArgs> _RaiseQuickly_Raises_Event;
        [TestMethod]
        public void RaiseQuickly_Raises_Event()
        {
            var recorder = new EventRecorder<EventArgs>();
            _RaiseQuickly_Raises_Event += recorder.Handler;

            var sender = new object();
            var args = new EventArgs();
            EventHelper.RaiseQuickly(_RaiseQuickly_Raises_Event, sender, args);

            Assert.AreEqual(1, recorder.CallCount);
            Assert.AreSame(sender, recorder.Sender);
            Assert.AreSame(args, recorder.Args);
        }

        event EventHandler<EventArgs> _RaiseQuickly_Copes_With_Null_Event;
        [TestMethod]
        public void RaiseQuickly_Copes_With_Null_Event()
        {
            EventHelper.RaiseQuickly(_RaiseQuickly_Copes_With_Null_Event, new object(), EventArgs.Empty);
        }

        event EventHandler<EventArgs> _RaiseQuickly_Raises_Event_To_Multiple_Handlers;
        [TestMethod]
        public void RaiseQuickly_Raises_Event_To_Multiple_Handlers()
        {
            var recorder1 = new EventRecorder<EventArgs>();
            var recorder2 = new EventRecorder<EventArgs>();
            _RaiseQuickly_Raises_Event_To_Multiple_Handlers += recorder1.Handler;
            _RaiseQuickly_Raises_Event_To_Multiple_Handlers += recorder2.Handler;

            EventHelper.RaiseQuickly(_RaiseQuickly_Raises_Event_To_Multiple_Handlers, new object(), EventArgs.Empty);

            Assert.AreEqual(1, recorder1.CallCount);
            Assert.AreEqual(1, recorder2.CallCount);
        }

        event EventHandler<EventArgs> _RaiseQuickly_Exceptions_In_Handlers_Stop_Propagation;
        [TestMethod]
        public void RaiseQuickly_Exceptions_In_Handlers_Stop_Propagation()
        {
            var recorder1 = new EventRecorder<EventArgs>();
            var recorder2 = new EventRecorder<EventArgs>();
            _RaiseQuickly_Exceptions_In_Handlers_Stop_Propagation += recorder1.Handler;
            _RaiseQuickly_Exceptions_In_Handlers_Stop_Propagation += recorder2.Handler;

            recorder1.EventRaised += (sender, args) => { throw new NotImplementedException("1"); };
            recorder2.EventRaised += (sender, args) => { throw new NotImplementedException("2"); };

            var exceptionBubbled = false;
            try {
                EventHelper.RaiseQuickly(_RaiseQuickly_Exceptions_In_Handlers_Stop_Propagation, new object(), EventArgs.Empty);
            } catch(NotImplementedException) {
                exceptionBubbled = true;
            }

            Assert.IsTrue(exceptionBubbled);
            Assert.AreEqual(1, recorder1.CallCount + recorder2.CallCount);
        }

        event EventHandler<EventArgs> _RaiseQuickly_ArgsBuilder_Raises_Event;
        [TestMethod]
        public void RaiseQuickly_ArgsBuilder_Raises_Event()
        {
            var recorder = new EventRecorder<EventArgs>();
            _RaiseQuickly_ArgsBuilder_Raises_Event += recorder.Handler;

            var sender = new object();
            EventArgs args = null;
            EventHelper.RaiseQuickly(_RaiseQuickly_ArgsBuilder_Raises_Event, sender, () => {;
                args = new EventArgs();
                return args;
            });

            Assert.IsNotNull(args);
            Assert.AreEqual(1, recorder.CallCount);
            Assert.AreSame(sender, recorder.Sender);
            Assert.AreSame(args, recorder.Args);
        }

        event EventHandler<EventArgs> _RaiseQuickly_ArgsBuilder_Copes_With_Null_Event;
        [TestMethod]
        public void RaiseQuickly_ArgsBuilder_Copes_With_Null_Event()
        {
            var countBuilderCalls = 0;
            EventHelper.RaiseQuickly(_RaiseQuickly_ArgsBuilder_Copes_With_Null_Event, new object(), () => {
                ++countBuilderCalls;
                return EventArgs.Empty;
            });

            Assert.AreEqual(0, countBuilderCalls);
        }

        event EventHandler<EventArgs> _RaiseQuickly_ArgsBuilder_Raises_Event_To_Multiple_Handlers;
        [TestMethod]
        public void RaiseQuickly_ArgsBuilder_Raises_Event_To_Multiple_Handlers()
        {
            var recorder1 = new EventRecorder<EventArgs>();
            var recorder2 = new EventRecorder<EventArgs>();
            _RaiseQuickly_ArgsBuilder_Raises_Event_To_Multiple_Handlers += recorder1.Handler;
            _RaiseQuickly_ArgsBuilder_Raises_Event_To_Multiple_Handlers += recorder2.Handler;

            var countBuilderCalls = 0;
            EventHelper.RaiseQuickly(_RaiseQuickly_ArgsBuilder_Raises_Event_To_Multiple_Handlers, new object(), () => {
                ++countBuilderCalls;
                return EventArgs.Empty;
            });

            Assert.AreEqual(1, countBuilderCalls);
            Assert.AreEqual(1, recorder1.CallCount);
            Assert.AreEqual(1, recorder2.CallCount);
        }

        event EventHandler<EventArgs> _RaiseQuickly_ArgsBuilder_Exceptions_In_Handlers_Stop_Propagation;
        [TestMethod]
        public void RaiseQuickly_ArgsBuilder_Exceptions_In_Handlers_Stop_Propagation()
        {
            var recorder1 = new EventRecorder<EventArgs>();
            var recorder2 = new EventRecorder<EventArgs>();
            _RaiseQuickly_ArgsBuilder_Exceptions_In_Handlers_Stop_Propagation += recorder1.Handler;
            _RaiseQuickly_ArgsBuilder_Exceptions_In_Handlers_Stop_Propagation += recorder2.Handler;

            recorder1.EventRaised += (sender, args) => { throw new NotImplementedException("1"); };
            recorder2.EventRaised += (sender, args) => { throw new NotImplementedException("2"); };

            var exceptionBubbled = false;
            try {
                EventHelper.RaiseQuickly(_RaiseQuickly_ArgsBuilder_Exceptions_In_Handlers_Stop_Propagation, new object(), () => EventArgs.Empty);
            } catch(NotImplementedException) {
                exceptionBubbled = true;
            }

            Assert.IsTrue(exceptionBubbled);
            Assert.AreEqual(1, recorder1.CallCount + recorder2.CallCount);
        }
    }
}
