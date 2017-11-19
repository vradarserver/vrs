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

namespace InterfaceFactory
{
    /// <summary>
    /// A very simple factory that records implementations of interfaces and creates them.
    /// </summary>
    /// <remarks>
    /// For use in situations where a full-blown IoC container would be overkill.
    /// </remarks>
    class ClassFactory : IClassFactory
    {
        class Implementation
        {
            public Type Type;
            public Func<object> Callback;
            public bool IsSingleton;

            public object CreateInstance()
            {
                var result = Callback == null ? Activator.CreateInstance(Type) : Callback();
                if(result == null) {
                    throw new NullReferenceException($"Callback returned null when creating a {Type.FullName}");
                }

                return result;
            }
        }

        /// <summary>
        /// A map of interface types to implementation types.
        /// </summary>
        private Dictionary<Type, Implementation> _ImplementationMap = new Dictionary<Type, Implementation>();

        /// <summary>
        /// A map of registered singleton objects. Always take a reference to this when reading, never write to the
        /// dictionary... rather, overwrite the map instead within a <see cref="_SyncLock"/>.
        /// </summary>
        private Dictionary<Type, object> _SingletonMap = new Dictionary<Type,object>();

        /// <summary>
        /// Locks writes to _SingletonMap.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// Records an implementation of an interface
        /// </summary>
        /// <typeparam name="TI">Interface type</typeparam>
        /// <typeparam name="TM">Implementation type</typeparam>
        public void Register<TI, TM>()
            where TI: class
            where TM: class, TI
        {
            Register(typeof(TI), typeof(TM));
        }

        /// <summary>
        /// Records an implementation of an interface.
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="implementationType"></param>
        public void Register(Type interfaceType, Type implementationType)
        {
            if(interfaceType == null) throw new ArgumentNullException("interfaceType");
            if(implementationType == null) throw new ArgumentNullException("implementationType");
            if(!interfaceType.IsInterface) throw new ClassFactoryException($"{interfaceType.Name} is not an interface");
            if(implementationType.IsInterface) throw new ClassFactoryException($"{implementationType.Name} is an interface");
            if(!interfaceType.IsAssignableFrom(implementationType)) throw new ClassFactoryException($"{implementationType.Name} does not implement {interfaceType.Name}");

            var implementation = new Implementation() { Type = implementationType };
            AddImplementation(interfaceType, implementation);
        }

        private void AddImplementation(Type interfaceType, Implementation implementation)
        {
            implementation.IsSingleton = interfaceType.GetCustomAttributes(true).OfType<SingletonAttribute>().Any();

            if(_ImplementationMap.ContainsKey(interfaceType)) {
                _ImplementationMap[interfaceType] = implementation;
            } else {
                _ImplementationMap.Add(interfaceType, implementation);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        public void Register<T>(Func<T> callback)
        {
            if(callback == null) throw new ArgumentNullException("callback");

            var implementation = new Implementation() { Callback = () => callback() };
            AddImplementation(typeof(T), implementation);
        }

        /// <summary>
        /// Records an instance to always return when an implementation of the interface type is asked for.
        /// </summary>
        /// <typeparam name="TI"></typeparam>
        /// <param name="instance"></param>
        public void RegisterInstance<TI>(TI instance)
        {
            RegisterInstance(typeof(TI), instance);
        }

        /// <summary>
        /// Records an instance to always return when an implementation of the interface type is asked for.
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="instance"></param>
        public void RegisterInstance(Type interfaceType, object instance)
        {
            if(interfaceType == null) throw new ArgumentNullException("interfaceType");
            if(instance == null) throw new ArgumentNullException("instance");
            if(!interfaceType.IsAssignableFrom(instance.GetType())) throw new ClassFactoryException($"Instances of {instance.GetType().Name} do not implement {interfaceType.Name}");

            lock(_SyncLock) {
                var newMap = CopySingletonMapWithinLock();
                if(newMap.ContainsKey(interfaceType)) {
                    newMap[interfaceType] = instance;
                } else {
                    newMap.Add(interfaceType, instance);
                }

                _SingletonMap = newMap;
            }
        }

        /// <summary>
        /// Creates a shallow copy of <see cref="_SingletonMap"/>. Must be called within a lock on <see cref="_SyncLock"/>.
        /// Do not call class constructors while holding the lock.
        /// </summary>
        /// <returns></returns>
        private Dictionary<Type, object> CopySingletonMapWithinLock()
        {
            var result = new Dictionary<Type, object>();

            foreach(var kvp in _SingletonMap) {
                result.Add(kvp.Key, kvp.Value);
            }

            return result;
        }

        /// <summary>
        /// Returns an instance of an implementation of the interface type passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>()
            where T : class
        {
            return (T)Resolve(typeof(T));
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ResolveSingleton<T>()
            where T: class
        {
            return (T)ResolveSingleton(typeof(T));
        }

        /// <summary>
        /// Returns an implementation of the interface type passed across.
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public object Resolve(Type interfaceType)
        {
            return Resolve(interfaceType, out Implementation unused);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public object ResolveSingleton(Type interfaceType)
        {
            var result = Resolve(interfaceType, out Implementation implementation);
            var isSingleton = implementation?.IsSingleton ?? interfaceType.GetCustomAttributes(typeof(SingletonAttribute), inherit: true).Length > 0;
            if(!isSingleton) {
                throw new ClassFactoryException($"{interfaceType?.FullName} is not a singleton");
            }

            return result;
        }

        private object Resolve(Type interfaceType, out Implementation implementation)
        {
            var singletonMap = _SingletonMap;

            if(interfaceType == null) {
                throw new ArgumentNullException("interfaceType");
            }
            if(!interfaceType.IsInterface) {
                throw new ClassFactoryException(String.Format("{0} is not an interface", interfaceType.Name));
            }
            if(!_ImplementationMap.TryGetValue(interfaceType, out implementation) && !singletonMap.ContainsKey(interfaceType)) {
                throw new ClassFactoryException($"{interfaceType.Name} has not had an implementation registered for it");
            }

            if(!FetchSingleton(interfaceType, out object result)) {
                result = implementation.CreateInstance();

                if(implementation.IsSingleton) {
                    lock(_SyncLock) {
                        var newObject = result;
                        if(!FetchSingleton(interfaceType, out result)) {
                            result = newObject;
                            RegisterInstance(interfaceType, newObject);
                            singletonMap = null;
                        }
                    }
                }
            }

            return result;
        }

        private bool FetchSingleton(Type interfaceType, out object result)
        {
            var singletonMap = _SingletonMap;
            return singletonMap.TryGetValue(interfaceType, out result);
        }

        /// <summary>
        /// Returns an instance of a factory that contains the same definitions and singleton declarations as this one
        /// but which can be modified without affecting this one.
        /// </summary>
        /// <returns></returns>
        public IClassFactory CreateChildFactory()
        {
            var result = new ClassFactory();

            foreach(var valuePair in _ImplementationMap) {
                result._ImplementationMap.Add(valuePair.Key, valuePair.Value);
            }

            var singletonMap = _SingletonMap;
            foreach(var valuePair in singletonMap) {
                result._SingletonMap.Add(valuePair.Key, valuePair.Value);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ResolveNewInstance<T>()
            where T: class
        {
            return (T)ResolveNewInstance(typeof(T));
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public object ResolveNewInstance(Type interfaceType)
        {
            if(interfaceType == null) {
                throw new ArgumentNullException("interfaceType");
            }
            if(!interfaceType.IsInterface) {
                throw new ClassFactoryException(String.Format("{0} is not an interface", interfaceType.Name));
            }

            if(!_ImplementationMap.TryGetValue(interfaceType, out Implementation implementation)) {
                throw new ClassFactoryException($"{interfaceType.Name} has not had an implementation registered for it");
            }
            if(!implementation.IsSingleton) {
                throw new ClassFactoryException($"{interfaceType.Name} has not been tagged with the Singleton attribute");
            }

            return implementation.CreateInstance();
        }
    }
}
