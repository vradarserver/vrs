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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InterfaceFactory;

namespace Test.InterfaceFactory
{
    [TestClass]
    public class ClassFactoryTests
    {
        #region Interfaces and implementations for testing
        interface IX { int Foo { get; set; } }

        interface IY { int Bar { get; set; } }

        [Singleton]
        interface ISi { int Goo { get; set; } }

        class X : IX { public int Foo { get; set; } }

        class XX : IX { public int Foo { get; set; } }

        class Y : IY { public int Bar { get; set; } }

        class Si : ISi { public int Goo { get; set; } }
        #endregion

        #region Fields, Properties, TestInitialise & TestCleanup
        private IClassFactory _ClassFactory;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactory = Factory.Singleton.Resolve<IClassFactory>();
        }
        #endregion

        #region Register (on its own)
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ClassFactory_Register_Throws_If_InterfaceType_Is_Null()
        {
            _ClassFactory.Register(null, typeof(X));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ClassFactory_Register_Throws_If_ImplementationType_Is_Null()
        {
            _ClassFactory.Register(typeof(IX), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ClassFactoryException))]
        public void ClassFactory_Register_Throws_If_InterfaceType_Is_Not_An_Interface_Type()
        {
            _ClassFactory.Register(typeof(X), typeof(X));
        }

        [TestMethod]
        [ExpectedException(typeof(ClassFactoryException))]
        public void ClassFactory_Register_Throws_If_ImplementationType_Is_An_Interface_Type()
        {
            _ClassFactory.Register(typeof(IX), typeof(IX));
        }

        [TestMethod]
        [ExpectedException(typeof(ClassFactoryException))]
        public void ClassFactory_Register_Throws_If_ImplementationType_Does_Not_Implement_InterfaceType()
        {
            _ClassFactory.Register(typeof(IX), typeof(Y));
        }
        #endregion

        #region Register (generic version)
        [TestMethod]
        [ExpectedException(typeof(ClassFactoryException))]
        public void ClassFactory_RegisterGeneric_Throws_If_InterfaceType_Is_Not_An_Interface_Type()
        {
            _ClassFactory.Register<X, X>();
        }

        [TestMethod]
        [ExpectedException(typeof(ClassFactoryException))]
        public void ClassFactory_RegisterGeneric_Throws_If_ImplementationType_Is_An_Interface_Type()
        {
            _ClassFactory.Register<IX, IX>();
        }
        #endregion

        #region Register (callback version)
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ClassFactory_RegisterCallback_Throws_If_Callback_Is_Null()
        {
            _ClassFactory.Register<X>(null);
        }
        #endregion

        #region RegisterInstance (on its own)
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ClassFactory_RegisterInstance_Throws_If_InterfaceType_Is_Null()
        {
            _ClassFactory.RegisterInstance(null, new X());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ClassFactory_RegisterInstance_Throws_If_Instance_Is_Null()
        {
            _ClassFactory.RegisterInstance(typeof(IX), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ClassFactoryException))]
        public void ClassFactory_RegisterInstance_Throws_If_Instance_Does_Not_Implement_InterfaceType()
        {
            _ClassFactory.RegisterInstance(typeof(IX), new Y());
        }
        #endregion

        #region Resolve (on its own)
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ClassFactory_Resolve_Throws_If_InterfaceType_Is_Null()
        {
            _ClassFactory.Resolve(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ClassFactoryException))]
        public void ClassFactory_Resolve_Throws_If_Type_Is_Not_Interface()
        {
            _ClassFactory.Resolve(typeof(X));
        }

        [TestMethod]
        [ExpectedException(typeof(ClassFactoryException))]
        public void ClassFactory_ResolveGeneric_Throws_If_Type_Is_Not_Interface()
        {
            _ClassFactory.Resolve<X>();
        }
        #endregion

        #region Register and Resolve
        [TestMethod]
        [ExpectedException(typeof(ClassFactoryException))]
        public void ClassFactory_Resolve_Throws_If_Interface_Has_Not_Been_Registered()
        {
            _ClassFactory.Resolve<IX>();
        }

        [TestMethod]
        public void ClassFactory_Resolve_Returns_New_Instances_Of_Registered_Implementations()
        {
            _ClassFactory.Register<IX, X>();

            IX x1 = _ClassFactory.Resolve<IX>();
            Assert.IsNotNull(x1);

            IX x2 = _ClassFactory.Resolve<IX>();
            Assert.IsNotNull(x2);

            Assert.AreNotSame(x1, x2);
        }

        [TestMethod]
        public void ClassFactory_Resolve_Returns_Instances_From_Callback()
        {
            IX x = null;
            _ClassFactory.Register<IX>(() => {
                x = new X();
                return x;
            });

            IX x1 = _ClassFactory.Resolve<IX>();
            Assert.AreSame(x, x1);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void ClassFactory_Resolve_Throws_If_Callback_Returns_Null()
        {
            _ClassFactory.Register<IX>(() => (IX)null);
            _ClassFactory.Resolve<IX>();
        }

        [TestMethod]
        public void ClassFactory_Resolve_Returns_Latest_Implementation_Registered_1()
        {
            _ClassFactory.Register<IX, X>();
            _ClassFactory.Register<IX, XX>();
            Assert.IsInstanceOfType(_ClassFactory.Resolve<IX>(), typeof(XX));
        }

        [TestMethod]
        public void ClassFactory_Resolve_Returns_Latest_Implementation_Registered_2()
        {
            _ClassFactory.Register<IX, X>();
            _ClassFactory.Register<IX>(() => new XX());
            Assert.IsInstanceOfType(_ClassFactory.Resolve<IX>(), typeof(XX));
        }
        [TestMethod]
        public void ClassFactory_Resolve_Returns_Latest_Implementation_Registered_3()
        {
            _ClassFactory.Register<IX>(() => new X());
            _ClassFactory.Register<IX, XX>();
            Assert.IsInstanceOfType(_ClassFactory.Resolve<IX>(), typeof(XX));
        }

        [TestMethod]
        public void ClassFactory_Resolve_Returns_Same_Instance_Of_Implementations_Registered_With_RegisterInstance()
        {
            X x = new X();
            _ClassFactory.RegisterInstance<IX>(x);
            Assert.AreSame(x, _ClassFactory.Resolve<IX>());
            Assert.AreSame(x, _ClassFactory.Resolve<IX>());
        }

        [TestMethod]
        public void ClassFactory_Resolve_Returns_Registered_Instance_In_Preference_To_New_Instance()
        {
            _ClassFactory.Register<IX, X>();
            XX singleton = new XX();
            _ClassFactory.RegisterInstance<IX>(singleton);
            Assert.AreSame(singleton, _ClassFactory.Resolve<IX>());
        }

        [TestMethod]
        public void ClassFactory_Resolve_Returns_Last_Registered_Instance()
        {
            X x1 = new X();
            X x2 = new X();
            _ClassFactory.RegisterInstance<IX>(x1);
            _ClassFactory.RegisterInstance<IX>(x2);
            Assert.AreSame(x2, _ClassFactory.Resolve<IX>());
        }

        [TestMethod]
        public void ClassFactory_Resolve_Returns_Singletons_For_Instances_Marked_As_Such()
        {
            _ClassFactory.Register<ISi, Si>();

            var instance1 = _ClassFactory.Resolve<ISi>();
            var instance2 = _ClassFactory.Resolve<ISi>();

            Assert.AreSame(instance1, instance2);
        }
        #endregion

        #region ResolveSingleton
        [TestMethod]
        public void ClassFactory_ResolveSingleton_Resolves_Singletons()
        {
            _ClassFactory.Register<ISi, Si>();

            var instance1 = _ClassFactory.ResolveSingleton<ISi>();
            var instance2 = _ClassFactory.ResolveSingleton<ISi>();

            Assert.AreSame(instance1, instance2);
        }

        [TestMethod]
        [ExpectedException(typeof(ClassFactoryException))]
        public void ClassFactory_ResolveSingleton_Does_Not_Resolve_Non_Singletons()
        {
            _ClassFactory.Register<IX, X>();
            _ClassFactory.ResolveSingleton<IX>();
        }

        [TestMethod]
        public void ClassFactory_ResolveSingleton_NonGeneric_Resolves_Singletons()
        {
            _ClassFactory.Register<ISi, Si>();

            var instance1 = (ISi)_ClassFactory.ResolveSingleton(typeof(ISi));
            var instance2 = (ISi)_ClassFactory.ResolveSingleton(typeof(ISi));

            Assert.AreSame(instance1, instance2);
        }

        [TestMethod]
        [ExpectedException(typeof(ClassFactoryException))]
        public void ClassFactory_ResolveSingleton_NonGeneric_Does_Not_Resolve_Non_Singletons()
        {
            _ClassFactory.Register<IX, X>();
            _ClassFactory.ResolveSingleton(typeof(IX));
        }
        #endregion

        #region ResolveNewInstance
        [TestMethod]
        public void ClassFactory_ResolveNewInstance_Returns_New_Instances_Of_Singletons()
        {
            _ClassFactory.Register<ISi, Si>();

            var instance1 = _ClassFactory.ResolveNewInstance(typeof(ISi));
            var instance2 = _ClassFactory.ResolveNewInstance(typeof(ISi));

            Assert.AreNotSame(instance1, instance2);
        }

        [TestMethod]
        [ExpectedException(typeof(ClassFactoryException))]
        public void ClassFactory_ResolveNewInstance_Throws_If_Interface_Is_Not_Singleton()
        {
            _ClassFactory.Register<IX, X>();
            _ClassFactory.ResolveNewInstance(typeof(IX));
        }

        [TestMethod]
        public void ClassFactory_ResolveNewInstance_Does_Not_Affect_Resolve()
        {
            _ClassFactory.Register<ISi, Si>();

            var instance1 = _ClassFactory.Resolve<ISi>();
            var instance2 = _ClassFactory.ResolveNewInstance<ISi>();
            var instance3 = _ClassFactory.Resolve<ISi>();

            Assert.AreNotSame(instance1, instance2);
            Assert.AreNotSame(instance2, instance3);
            Assert.AreSame(instance1, instance3);

        }
        #endregion

        #region CreateChildFactory
        [TestMethod]
        public void ClassFactory_CreateChildFactory_Returns_Child_That_Contains_Same_Implementations_And_Singletons()
        {
            X x = new X();
            _ClassFactory.RegisterInstance<IX>(x);
            _ClassFactory.Register<IY, Y>();

            IClassFactory child = _ClassFactory.CreateChildFactory();
            Assert.AreSame(x, child.Resolve<IX>());
            Assert.IsInstanceOfType(child.Resolve<IY>(), typeof(Y));
        }

        [TestMethod]
        public void ClassFactory_CreateChildFactory_Returns_Child_That_Can_Be_Modified_Independently_Of_Parent()
        {
            _ClassFactory.Register<IX, X>();
            IClassFactory child = _ClassFactory.CreateChildFactory();
            child.Register<IX, XX>();

            Assert.IsInstanceOfType(_ClassFactory.Resolve<IX>(), typeof(X));
            Assert.IsInstanceOfType(child.Resolve<IX>(), typeof(XX));
        }
        #endregion
    }
}
