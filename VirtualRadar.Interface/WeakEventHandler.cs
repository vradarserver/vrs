// Copyright © 2007 onwards, Dustin Campbell (with, it appears, portions copyright © Joe Duffy)
// All rights reserved.
//
// All based on / copied from Dustin's page on weak event handlers at:
// http://diditwith.net/2007/03/23/SolvingTheProblemWithEventsWeakEventHandlers.aspx

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The delegate for a method that will automatically unregister an event handler when the owning object is garbage collected.
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <param name="eventHandler"></param>
    public delegate void UnregisterCallback<E>(EventHandler<E> eventHandler) where E: EventArgs;

    /// <summary>
    /// The interface for event handlers that can hold a weak reference to a target.
    /// </summary>
    /// <typeparam name="E"></typeparam>
    public interface IWeakEventHandler<E>
      where E: EventArgs
    {
        /// <summary>
        /// The event handler.
        /// </summary>
        EventHandler<E> Handler { get; }
    }

    /// <summary>
    /// The default implementation of IWeakEventHandler.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    public class WeakEventHandler<T, E> : IWeakEventHandler<E>
        where T: class
        where E: EventArgs
    {
        private delegate void OpenEventHandler(T @this, object sender, E e);

        private WeakReference _WeakReference;
        private OpenEventHandler _OpenEventHandler;
        private EventHandler<E> _EventHandler;
        private UnregisterCallback<E> _UnregisterCallback;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <param name="unregister"></param>
        public WeakEventHandler(EventHandler<E> eventHandler, UnregisterCallback<E> unregister)
        {
            _WeakReference = new WeakReference(eventHandler.Target);
            _OpenEventHandler = (OpenEventHandler)Delegate.CreateDelegate(typeof(OpenEventHandler), null, eventHandler.Method);
            _EventHandler = Invoke;
            _UnregisterCallback = unregister;
        }

        /// <summary>
        /// Invokes the event handler held by the weak target.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Invoke(object sender, E e)
        {
            T target = (T)_WeakReference.Target;

            if(target != null) _OpenEventHandler.Invoke(target, sender, e);
            else if(_UnregisterCallback != null) {
                _UnregisterCallback(_EventHandler);
                _UnregisterCallback = null;
            }
        }

        /// <summary>
        /// Gets the event handler.
        /// </summary>
        public EventHandler<E> Handler
        {
            get { return _EventHandler; }
        }

        /// <summary>
        /// Gets the weak event handler's event handler.
        /// </summary>
        /// <param name="weakEventHandler"></param>
        /// <returns></returns>
        public static implicit operator EventHandler<E>(WeakEventHandler<T, E> weakEventHandler)
        {
            return weakEventHandler._EventHandler;
        }
    }

    /// <summary>
    /// A static class that makes it easier to use weak event handlers.
    /// </summary>
    public static class EventHandlerUtils
    {
        /// <summary>
        /// Creates a weak event handler for an event.
        /// </summary>
        /// <typeparam name="E">The type of EventArgs that the event uses.</typeparam>
        /// <param name="eventHandler">The delegate that will be called when the event is raised.</param>
        /// <param name="unregister">A delegate that will unregister the event from the event handler when the object is garbage collected.</param>
        /// <returns>The event handler for the event.</returns>
        public static EventHandler<E> MakeWeak<E>(EventHandler<E> eventHandler, UnregisterCallback<E> unregister)
          where E: EventArgs
        {
            if(eventHandler == null) throw new ArgumentNullException("eventHandler");
            if(eventHandler.Method.IsStatic || eventHandler.Target == null) {
                throw new ArgumentException("Only instance methods are supported.", "eventHandler");
            }

            Type wehType = typeof(WeakEventHandler<,>).MakeGenericType(eventHandler.Method.DeclaringType, typeof(E));
            ConstructorInfo wehConstructor = wehType.GetConstructor(new Type[] { typeof(EventHandler<E>), typeof(UnregisterCallback<E>) });

            IWeakEventHandler<E> weh = (IWeakEventHandler<E>)wehConstructor.Invoke(new object[] { eventHandler, unregister });

            return weh.Handler;
        }
    }
}
