// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace Test.Framework
{
    /// <summary>
    /// An object that can be hooked to an event to determine that it has been raised and record the parameters passed to the event.
    /// </summary>
    /// <remarks>
    /// This only works with standard events that pass two parameters, a sender object and an args based on <see cref="EventArgs"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class ObjectUnderTest()
    /// {
    ///     public event EventHandler TheEvent;
    ///
    ///     public void RaiseEvent()
    ///     {
    ///         if(TheEvent != null) TheEvent(this, EventArgs.Empty);
    ///     }
    /// }
    /// 
    /// [TestMethod]
    /// public void Check_That_RaiseEvent_Raises_TheEvent()
    /// {
    ///     var objectUnderTest = new ObjectUnderTest();
    ///     EventRecorder&lt;EventArgs&gt; eventRecorder = new EventRecorder&lt;EventArgs&gt;();
    ///
    ///     objectUnderTest.TheEvent += eventRecorder.Handler;
    ///     objectUnderTest.RaiseEvent();
    ///     
    ///     Assert.AreEqual(1, eventRecorder.CallCount);
    ///     Assert.AreSame(objectUnderTest, eventRecorder.Sender);
    ///     Assert.IsNotNull(eventRecorder.Args);
    /// }
    /// </code>
    /// </example>
    public class EventRecorder<T>
        where T: EventArgs
    {
        /// <summary>
        /// Gets the number of times the event has been raised.
        /// </summary>
        public int CallCount { get; private set; }

        /// <summary>
        /// Gets the sender parameter from the last time the event was raised.
        /// </summary>
        public object Sender { get; private set; }

        /// <summary>
        /// Gets the args parameter from the last time the event was raised.
        /// </summary>
        public T Args { get; private set; }

        /// <summary>
        /// Gets a list of every sender parameter passed to the event. There will be <see cref="CallCount"/>
        /// entries in the list.
        /// </summary>
        public IList<object> AllSenders { get; } = new List<object>();

        /// <summary>
        /// Gets a list of every args parameter passed to the event. There will be <see cref="CallCount"/>
        /// entries in the list.
        /// </summary>
        public IList<T> AllArgs { get; } = new List<T>();

        /// <summary>
        /// Raised by <see cref="Handler"/> whenever the event is raised. Can be used to test the state of
        /// objects when the event was raised.
        /// </summary>
        /// <remarks>
        /// The sender passed to the event is the EventRecorder, <em>not</em> the sender of the original event.
        /// By the time the event is raised the EventRecorder's <see cref="Sender"/> property will be set to the
        /// sender of the original event.
        /// </remarks>
        /// <example>
        /// This snippet shows how to check the state of an object when the event is raised. In this case it
        /// is testing that when some hypothetical object raises <em>CountChanged</em> its <em>Count</em>
        /// property is set to 2.
        /// <code>
        /// EventRecorder&lt;SomeArgs&gt; eventRecorder = new EventRecorder&lt;SomeArgs&gt;();
        /// myObject.CountChanged += eventRecorder.Handler;
        /// recorder.EventRaised += (s, a) => { Assert.AreEqual(2, myObject.Count); };
        /// 
        /// myObject.DoSomeWork();
        /// 
        /// Assert.AreEqual(1, recorder.CallCount);
        /// </code>
        /// Similarly this code will test what happens when an event handler throws an exception during processing:
        /// <code>
        /// EventRecorder&lt;SomeArgs&gt; eventRecorder = new EventRecorder&lt;SomeArgs&gt;();
        /// myObject.CountChanged += eventRecorder.Handler;
        /// recorder.EventRaised += (s, a) => { throw new InvalidOperation(); };
        /// 
        /// myObject.DoSomeWork();
        /// </code>
        /// Finally, this code checks that the sender is the same as the myObject value during processing of the event.
        /// Note that the sender passed to the event is the EventRecorder that is raising the event, not the original
        /// sender, so the test must use the EventRecorder's <see cref="Sender"/> property.
        /// <code>
        /// EventRecorder&lt;SomeArgs&gt; eventRecorder = new EventRecorder&lt;SomeArgs&gt;();
        /// myObject.CountChanged += eventRecorder.Handler;
        /// recorder.EventRaised += (s, a) => {
        ///     // This just illustrates that the sender is the EventRecorder and to get the original
        ///     // sender you need to use the appropriate property on the event recorder.
        ///     Assert.AreSame(s, eventRecorder);
        ///     Assert.AreSame(eventRecorder.Sender, myObject);
        /// };
        /// </code>
        /// </example>
        public event EventHandler<T> EventRaised;

        /// <summary>
        /// Raises <see cref="EventRaised"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnEventRaised(T args) => EventRaised?.Invoke(this, args);

        /// <summary>
        /// An event handler matching the EventHandler and/or EventHandler&lt;&gt; delegate that can be attached
        /// to an event and record the parameters passed by the code that raises the event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <example>
        /// Many of the examples already illustrate this, but just to emphasise that this class needs to be hooked
        /// to the event that you want to test, this is how you do it:
        /// <code>
        /// ObservableCollection&lt;int&gt; collection = new ObservableCollection&lt;int&gt;();
        /// EventRecorder&lt;NotifyCollectionChangedEventArgs&gt; eventRecorder = new EventRecorder&lt;NotifyCollectionChangedEventArgs&gt;();
        /// 
        /// // Change the collection without attaching the event recorder:
        /// collection.Add(987);
        /// Assert.AreEqual(0, eventRecorder.CallCount);
        ///
        /// // Change the collection after attaching the event recorder:
        /// collection.CollectionChanged += eventRecorder.Handler;
        /// collection.Add(643);
        /// Assert.AreEqual(1, eventRecorder.CallCount);
        /// </code>
        /// </example>
        public virtual void Handler(object sender, T args)
        {
            ++CallCount;
            Sender = sender;
            Args = args;

            AllSenders.Add(sender);
            AllArgs.Add(args);

            OnEventRaised(args);
        }
    }
}
