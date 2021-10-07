# NotifyingPropertyToolkit
A library for cleaner MVVM through view-bindable properties, commands and actions that 
provide 'memoization', cascading updates, highly efficient change notifications and configuration with minimum boilerplate code. Written as a .NET Standard 2.0 library, it is platform, and UI-framework independent - though 
its typical use case is for XAML frameworks that provide data binding like WPF, UWP, Xamarin Forms and MAUI.

Supported frameworks:
- .NET 5 or newer
- .NET Framework 4.7.1, 4.7.2 or 4.8
- .NET Core 1.0 or newer
- Mono 5.4 or newer
- UWP 10 v1709 or newer
- Unity 2018.1 or newer

> NotifyingToolkit is _not_ thread-safe. It is only intended to be used in the context of a single-thread

Copyright &copy; 2021 Rxcle. NotifyingPropertyToolkit is licensed under the [MIT license](LICENSE).

## Source code & Building
The code is written in C# for .NET. The main Toolkit consists of a single Visual Studio library project: `NotifyingPropertyToolkit.csproj`. It is contained in a solution that also contains projects with examples for WPF and UWP and a unittest project (NUnit3).

Simply build `NotifyingPropertyToolkit.csproj` for the architecture of choice and reference the binary into your project. Alternatively you can link in the source code directly.

## Purpose
Typical ViewModels consist of commands and properties with a backing field, a check on whether the property value changed against the backing field and a trigger on the PropertyChanged event when this is the case. Additionally changes to one property might also affect the state of other properties and it might affect the CanExecute state of commands, furthermore when a property changes a certain side-effect (e.g. model update) action may need to be executed.

With NotifyingPropertyToolkit ViewModels can be drastically simplified, dependency between properties can specified explicitly and can be made self-contained all within the constructor of the ViewModel.

## NotifyingContext
The primary class is `NotifyingContext` typically you will create a NotifyingContext per ViewModel. If you use a base class for your ViewModels it is recommended to create an instance of the context class in the constructor and provide the instance as a protected field/property to the derived ViewModels.

On a NotifyingContext you can create/register the following entities:
- Property
    - ReadOnly - `CreateReadonlyProperty()`
    - Writeable - `CreateWritableProperty()`
    - Collection - `CreateCollectionProperty()`
- Command
    - Non-parameterized - `CreateCommand()`
    - Parameterized - `CreateCommand<T>()`
- Action
    - `CreateAction()`

These entities can then be used to bind a view to. Typically the instances returned from the `Create...()` methods are assigned to public C# getter-only properties.

The `NotifyingContext` keeps track of the entities and their dependencies, ensures uniqueness of names, and provides a delegate which triggers on changes. This delegate is typically hooked up to the `PropertyChanged` event of `INotifyPropertyChanged` on your ViewModel (base) class.

> The toolkit does not provide a standard ViewModel base class that does the instantiation of NotifyingContext or hookup to INotifyPropertyChanged because this would tie the library to a specific UI framework. The WPF and UWP example projects contain a simple reference implementation of this though.

### Constructor
```csharp 
NotifyingContext(
    Action<INotifyingProperty> notifyingPropertyChangedAction = null,
    Action<bool, EventHandler> commandRequeryHook = null)
```
 The first argument to the constructor is an action delegate which (if provided) gets called every time a property has a changed value. This delegate is called after the property value has been changed. The changed property is provided as an argument. 
 
 Typical implementations map this delegate to an invocation on of the `PropertyChanged` event on a ViewModel using the `Name` property on `INotifyingProperty`, like so:

 ```csharp
var notifyingContext = new NotifyingContext(np => 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(np.Name)));
```

The second argument to the constructor is typically not needed for straightforward usage, but if provided it can be used to set up an external trigger that updates the CanExecute state of NotifiableCommands registered on the context. For instance, with WPF you can use to to attach Commands to the `RequerySuggested` event on `CommandManager`. The `bool` argument indicates whether the event handler is to be attached or removed. See the chapter on `NotifiableCommand` for more information.

> While the option to do this is provided, it is recommended _not_ to rely on the CommandManager to update the state of Commands, but rather correctly configure dependencies for automatic updates and, where needed, explicit call `Notify()` on Commands (or `NotifyAll()` on the Context) instead.

### ReevaluateAll
```csharp
void ReevaluateAll()
```
Request all notifiable items (ReadonlyProperty, Command, Action) to reevaluate its state. 
Note that this is just a convenience method. You can also call `Reevaluate()` directly on a specific Property, Command or Action.

### BulkUpdate
```csharp
void BulkUpdate(Action updateAction)
```
Update the values of multiple WritableProperties in one go without triggering notifications or cascading to updating dependent items until the specified delegate ends.

#### Example
```csharp
var context = new NotifyingContext();
var firstName = context.CreateWritableProperty("First", string.Empty);
var lastName = context.CreateWritableProperty("Last", string.Empty);
var fullName = context.CreateReadonlyProperty("Full", 
    () => firstName.Value + " " + lastName.Value,
    dependencies: new[] { firstName, lastName });

context.BulkUpdate(() => {
    firstName.Value = "John";
    lastName.Value = "D'oh";
    lastName.Value = "Doe";
});

Console.WriteLine(fullName.Value);
```
Because of the BulkUpdate, the value of `fullName` is only updated at the end of the BulkUpdate action. During the BulkUpdate, changes on `firstName` and `lastName` will send out no change notifications. Additionally, even though `lastName` changed twice, only a single change notification is sent out. 

## NotifyingProperty
NotifyingProperties come in three flavors: Writable, Readonly and Collection.

All NotifyingProperties have the following in common:
- A Generic Type
    - For Readonly and Writable NotifyingProperty this denotes the type of the `Value` property
    - For Collection NotifyingProperties this denotes the type of items in the Collection that is stored in the `Value` property.
- `Name`
    - The unique name of the NotifyingProperty in the context.
    - It is recommended to set this to `nameof(ViewModelPropertyName)`.
- `Value`
    - The current value of the NotifyingProperty.
    - This is read-only for ReadOnly and Collection NotifyingProperties and read/write for Writable NotifyingProperties.
- `IsPrivate`
    - Specifies whether the NotifyingProperty exhibits _private_ or _public_ behavior. 
        - The default is _public_ (thus IsPrivate is `false` by default)
    - Private NotifyingProperties will not trigger the PropertyChanged action on the Context in which it is registered and are typically used for ViewModel internal updates.

NotifyingProperties convert implicitly to the type of the `Value` property. So, given a `notifyingStringProperty`, this will work:
```csharp
notifyingStringProperty.Value = "Hello"
Console.WriteLine(notifyingStringProperty);       // Produces "Hello"
Console.WriteLine(notifyingStringProperty.Value); // Also produces "Hello"
```
Similarly, binding directly on the NotifyingProperty in a View will work fine for one-way bindings. However for two-way bindings and in general, when you want to modify the Value of a NotifyingProperty, you must use the `Value` property.

### NotifyingReadonlyProperty
Readonly NotifyingProperties can only be updated by triggering the `Reevaluate()` method, either automatically through dependencies or manually. When `Reevaluate()` is called then a delegate is executed which determines the new value, if this differs from the previous value then update notifications are triggered.

### NotifyingWritableProperty
Writable NotifyingProperties have a value that can be freely read and set directly. Upon setting the value, change notification are only triggered when the value differs from the previous value.

### NotifyingCollectionProperty
NotifyingCollection Properties have a read-only Value of type `ObservableCollection<T>`. Note that only the `Value` property itself is read-only, the actual wrapped Collection can be changed. Changes on the collection will cause property changed notifications (and will update any dependent properties).

Note that you can also create a `NotifyingReadonlyProperty<ObservableCollection<T>>`, but the difference with that and `NotifyingCollectionProperty<T>` is that the former will not automatically trigger notifications on the NotifyingProperty on changes made to the internal collection while the latter will.

## NotifiableCommand
The classes `NotifiableCommand` and `NotifiableCommand<T>` implement `ICommand`. The action to execute on command invocation and the CanExecute state care supplied to the constructor as delegates. The non-generic class takes no argument when executing (or when determining the CanExecute state), while the generic class does take a (single) argument for these actions. Because the non-generic class can cache its CanExecute state, it is a bit more efficient than the generic variant.

Note that Commands in the Toolkit are _Notifiable_ and not _Notifying_. Commands do not trigger a notification on change, but instead can be notified of changes on properties through configured dependencies - specifically to update its CanExecute state.

Note that NotifiableCommands are not linked to the CommandManager (WPF) by default, which means that unlike some other Command implementations they do not automatically update its state in response to UI actions. For performance this is also the best default choice; Simply configure the dependencies for automatic reevaluation base on NotifyingProperty changes and, where needed, call `Reevaluate()` manually on the Command.

If you want custom automatic reevaluation or attach the NotifiableCommand to a CommandManager you can specify a delegate function as the second argument for the `NotifyingContext` constructor. Two parameters are passed, a boolean indicator that specifies that the Command needs to be add/attached as a listener and
an EventHandler that is to be attached/invoked on changes that require reevaluation of the Command.

In this example (for WPF) the created NotifyingCommand will automatically reevaluate when the CommandManager triggers its `RequerySuggested` event.

```csharp
var notifyingContext = new NotifyingContext(ni => { },
    commandRequeryHook: (add, eventHandler) =>
    {
        if (add)
            CommandManager.RequerySuggested += eventHandler;
        else
            CommandManager.RequerySuggested -= eventHandler;
    });
var command = notifyingContext.CreateCommand(() => {});
```

## NotifiableAction
NotifiableActions are very simple wrapped parameterless Actions that execute when the `Reevaluate()` method is called. They implement the same base interface `INotifiableItem` as `NotifiableCommand` and `NotifyingReadonlyProperty`. When created on the context, dependencies can be specified. When one of the dependencies notifies a change then the action is invoked. If you want certain conditional behavior then this should be written as part of the supplied action delegate function.