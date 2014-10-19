The options screen was rewritten for 2.0.3 and as a part of that I wanted
to use data binding. Unfortunately after having rewritten the screen
(twice) I discovered that Mono's support for data binding is not complete.
This is especially true of older versions of Mono.

The classes here are part of the workaround for the lack of data binding
in Mono. They are pretty basic and not heavily automated in the same way
that .NET's data binding is - I didn't need a reimplementation of data
binding, just something that would cover my use cases and be reasonably
easy to debug.

Binding is managed by a class based on ControlBinder. ControlBinder takes
care of hooking and unhooking the model and the control, and of copying
values between model and control. It is abstract.

For simple "one model property <-> one control property" cases there are
a bunch of binders, all based on SimpleValueBinder. SimpleValueBinder
assumes that the model implements INotifyPropertyChanged. The binders
are:
    CheckBoxBoolBinder
    NumericIntBinder
    TextBoxStringBinder

