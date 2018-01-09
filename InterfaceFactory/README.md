# InterfaceFactory
The interface factory is responsible for creating instances of interfaces.

## ClassFactory
ClassFactory manages the registration of classes that implement interfaces.
Each library usually exposes a static class called *Implementations* that has a
method called *Register*. The program calls these Register methods early on in
its run. Each Register method is passed a ClassFactory to populate with
implementations, usually *Factory.Singleton*, and the Register method just
calls **Register** on the ClassFactory for each of the interfaces that it implements.

Implementations can be changed while the program is running. In VRS it is common
for plugins to override the default implementation of one or two interfaces with
their own implementations.

When the program wants to instantiate an interface it calls one of the **Resolve**
methods on ClassFactory.

The ClassFactory guarantees that it will always return the same instance for
interfaces that are tagged with the **Singleton** attribute. It will also do this
if an instance is registered with ClassFactory using **RegisterInstance**.

Any of the Resolve calls (except **ResolveNewInstance**) will honour the Singleton
attribute, but for the sake of self-documenting code it's preferrable to call
**ResolveSingleton** when resolving singleton interfaces. This method is just the
same as Resolve, the only difference is that it throws an exception if the interface
isn't marked as a Singleton.

## Factory
Factory is a static class that exposes a property called **Singleton**, which is
the instance of ClassFactory that the application is expected to use.

There are two methods, **TakeSnapshot** and **RestoreSnapshot**, that let you
copy a class factory and restore it respectively. These are only intended for use
in unit tests, they should not be called in production code.

## Examples
The interface:
```c#
public interface IMyInterface
{
    string GenerateName();
}
```

The implementation:
```c#
// Note that the implementation is usually private to the library
class MyImplementation : IMyInterface
{
    public string GenerateName()
    {
        return "A Name";
    }
}
```

Registering the implementation - this need only be done once:
```c#
Factory.Singleton.Register<IMyInterface, MyImplementation>();
```

Creating a new instance of an implementation:
```c#
IMyInterface instance = Factory.Singleton.Resolve<IMyInterface>();
Console.WriteLine(instance.GenerateName());
```
