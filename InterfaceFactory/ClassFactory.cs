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

            public object CreateInstance()
            {
                var result = Callback == null ? Activator.CreateInstance(Type) : Callback();
                if(result == null) throw new NullReferenceException(String.Format("Callback returned null when creating a {0}", Type.FullName));

                return result;
            }
        }

        /// <summary>
        /// A map of interface types to implementation types.
        /// </summary>
        private Dictionary<Type, Implementation> _ImplementationMap = new Dictionary<Type,Implementation>();

        /// <summary>
        /// A map of registered singleton objects.
        /// </summary>
        private Dictionary<Type, object> _SingletonMap = new Dictionary<Type,object>();

        /// <summary>
        /// Records an implementation of an interface
        /// </summary>
        /// <typeparam name="TI">Interface type</typeparam>
        /// <typeparam name="TM">Implementation type</typeparam>
        public void Register<TI, TM>()
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
            if(!interfaceType.IsInterface) throw new ClassFactoryException(String.Format("{0} is not an interface", interfaceType.Name));
            if(implementationType.IsInterface) throw new ClassFactoryException(String.Format("{0} is an interface", implementationType.Name));
            if(!interfaceType.IsAssignableFrom(implementationType)) throw new ClassFactoryException(String.Format("{0} does not implement {1}", implementationType.Name, interfaceType.Name));

            var implementation = new Implementation() { Type = implementationType };
            AddImplementation(interfaceType, implementation);
        }

        private void AddImplementation(Type interfaceType, Implementation implementation)
        {
            if(_ImplementationMap.ContainsKey(interfaceType)) _ImplementationMap[interfaceType] = implementation;
            else _ImplementationMap.Add(interfaceType, implementation);
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
            if(!interfaceType.IsAssignableFrom(instance.GetType())) throw new ClassFactoryException(String.Format("Instances of {0} do not implement {1}", instance.GetType().Name, interfaceType.Name));

            if(_SingletonMap.ContainsKey(interfaceType)) _SingletonMap[interfaceType] = instance;
            else _SingletonMap.Add(interfaceType, instance);
        }

        /// <summary>
        /// Returns an instance of an implementation of the interface type passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>()
            where T: class
        {
            return (T)Resolve(typeof(T));
        }

        /// <summary>
        /// Returns an implementation of the interface type passed across.
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public object Resolve(Type interfaceType)
        {
            if(interfaceType == null) throw new ArgumentNullException("interfaceType");
            if(!interfaceType.IsInterface) throw new ClassFactoryException(String.Format("{0} is not an interface", interfaceType.Name));
            if(!_ImplementationMap.ContainsKey(interfaceType) && !_SingletonMap.ContainsKey(interfaceType)) throw new ClassFactoryException(String.Format("{0} has not had an implementation registered for it", interfaceType.Name));

            object result;
            if(!_SingletonMap.TryGetValue(interfaceType, out result)) result = _ImplementationMap[interfaceType].CreateInstance();

            return result;
        }

        /// <summary>
        /// Returns an instance of a factory that contains the same definitions and singleton declarations as this one
        /// but which can be modified without affecting this one.
        /// </summary>
        /// <returns></returns>
        public IClassFactory CreateChildFactory()
        {
            ClassFactory result = new ClassFactory();

            foreach(KeyValuePair<Type, Implementation> valuePair in _ImplementationMap) {
                result._ImplementationMap.Add(valuePair.Key, valuePair.Value);
            }

            foreach(KeyValuePair<Type, object> valuePair in _SingletonMap) {
                result._SingletonMap.Add(valuePair.Key, valuePair.Value);
            }

            return result;
        }
    }
}
