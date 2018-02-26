using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;

namespace Test.InterfaceFactory
{
    [TestClass]
    public class FactoryTests
    {
        public interface IX { int Foo { get; set; } }
        public class X : IX { public int Foo { get; set; } }

        IClassFactory _Snapshot;
        Mock<IClassFactory> _ClassFactory;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();
            _ClassFactory = TestUtilities.CreateMockInstance<IClassFactory>();
            Factory.RestoreSnapshot(_ClassFactory.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        public void Factory_Exposes_Singleton_Through_IClassFactory_Static_Methods()
        {
            foreach(var method in typeof(IClassFactory).GetMethods()) {
                TestCleanup();
                TestInitialise();

                switch(method.Name) {
                    case nameof(IClassFactory.CreateChildFactory):
                        Factory.CreateChildFactory();
                        _ClassFactory.Verify(r => r.CreateChildFactory(), Times.Once());
                        break;
                    case nameof(IClassFactory.HasImplementation):
                        if(method.IsGenericMethod) {
                            Factory.HasImplementation<string>();
                            _ClassFactory.Verify(r => r.HasImplementation<string>(), Times.Once());
                        } else {
                            Factory.HasImplementation(typeof(string));
                            _ClassFactory.Verify(r => r.HasImplementation(typeof(string)), Times.Once());
                        }
                        break;
                    case nameof(IClassFactory.Register):
                        if(method.IsGenericMethod) {
                            if(method.GetGenericArguments().Length == 1) {
                                Func<string> callback = () => "1";
                                Factory.Register<string>(callback);
                                _ClassFactory.Verify(r => r.Register<string>(callback), Times.Once());
                            } else {
                                Factory.Register<IX, X>();
                                _ClassFactory.Verify(r => r.Register<IX, X>(), Times.Once());
                            }
                        } else {
                            Factory.Register(typeof(string), typeof(int));
                            _ClassFactory.Verify(r => r.Register(typeof(string), typeof(int)), Times.Once());
                        }
                        break;
                    case nameof(IClassFactory.RegisterInstance):
                        var x = new X();
                        if(method.IsGenericMethod) {
                            Factory.RegisterInstance<IX>(x);
                            _ClassFactory.Verify(r => r.RegisterInstance<IX>(x), Times.Once());
                        } else {
                            Factory.RegisterInstance(typeof(IX), x);
                            _ClassFactory.Verify(r => r.RegisterInstance(typeof(IX), x), Times.Once());
                        }
                        break;
                    case nameof(IClassFactory.Resolve):
                        if(method.IsGenericMethod) {
                            Factory.Resolve<IX>();
                            _ClassFactory.Verify(r => r.Resolve<IX>(), Times.Once());
                        } else {
                            Factory.Resolve(typeof(IX));
                            _ClassFactory.Verify(r => r.Resolve(typeof(IX)), Times.Once());
                        }
                        break;
                    case nameof(IClassFactory.ResolveNewInstance):
                        if(method.IsGenericMethod) {
                            Factory.ResolveNewInstance<IX>();
                            _ClassFactory.Verify(r => r.ResolveNewInstance<IX>(), Times.Once());
                        } else {
                            Factory.ResolveNewInstance(typeof(IX));
                            _ClassFactory.Verify(r => r.ResolveNewInstance(typeof(IX)), Times.Once());
                        }
                        break;
                    case nameof(IClassFactory.ResolveSingleton):
                        if(method.IsGenericMethod) {
                            Factory.ResolveSingleton<IX>();
                            _ClassFactory.Verify(r => r.ResolveSingleton<IX>(), Times.Once());
                        } else {
                            Factory.ResolveSingleton(typeof(IX));
                            _ClassFactory.Verify(r => r.ResolveSingleton(typeof(IX)), Times.Once());
                        }
                        break;
                    default:
                        throw new NotImplementedException(method.Name);
                }
            }
        }
    }
}
