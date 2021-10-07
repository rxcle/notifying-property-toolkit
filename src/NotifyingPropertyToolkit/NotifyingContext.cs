// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace NotifyingPropertyToolkit
{
    /// <summary>
    /// Context for NotifyingItems. This context maintains uniquely named items, tracks dependencies and will automatically
    /// reavaluate item values/state based on change notifications and the configured dependencies.
    /// </summary>
    public sealed class NotifyingContext
    {
        private readonly Action<INotifyingProperty> m_NotifyingPropertyChangedAction;
        private readonly Dictionary<INotifyingProperty, ISet<INotifiableItem>> m_RegisteredProperties = new Dictionary<INotifyingProperty, ISet<INotifiableItem>>();
        private readonly HashSet<INotifiableItem> m_RegisteredNotifiableItems = new HashSet<INotifiableItem>();
        private readonly HashSet<INotifyingWritableProperty> m_BulkChangedNotificationProperties = new HashSet<INotifyingWritableProperty>();
        private readonly Action<bool, EventHandler> m_CommandRequeryHook;
        private bool m_InBulkUpdate = false;

        /// <summary>
        /// Creates an instance of the NotifyingContext class.
        /// </summary>
        /// <param name="notifyingPropertyChangedAction">Optional delegate that gets called on any value/state change of a NotifyingItem.</param>
        /// <param name="commandRequeryHook">Optional delegate that gets called when the first handler is attached and when the last handler is
        /// detached from a the CanExecuteChanged event on a Command. This can be used to connect command requery requests to events from an 
        /// external CommandManager.</param>
        public NotifyingContext(
            Action<INotifyingProperty> notifyingPropertyChangedAction = null,
            Action<bool, EventHandler> commandRequeryHook = null)
        {
            m_NotifyingPropertyChangedAction = notifyingPropertyChangedAction;
            m_CommandRequeryHook = commandRequeryHook;
        }

        /// <summary>
        /// Creates a writable Property and adds it to the context.
        /// Writable properties only update their value by explicitly changing the Value property.
        /// This means that <c>Reevaluate()</c> does not do anything and will always return <c>false</c> and
        /// they have no dependencies on other <c>NotifyingItems</c>.
        /// </summary>
        /// <typeparam name="T">Type for the property</typeparam>
        /// <param name="name">Property name. Must be non-empty and unique.</param>
        /// <param name="initialValue">Optional initial value for the property</param>
        /// <param name="isPrivate">Specifies whether the property should be private and not sent out change notifications</param>
        /// <param name="valueEqualityComparer">Optional custom value equality comparer which gets called when the value is about to be changed.</param>
        /// <returns>Created NotifyingWritableProperty object</returns>
        public NotifyingWritableProperty<T> CreateWritableProperty<T>(
            string name,
            T initialValue = default,
            bool isPrivate = false,
            ValueEqualityComparer<T> valueEqualityComparer = null)
        {
            var property = new NotifyingWritableProperty<T>(
                name,
                initialValue: initialValue,
                changedAction: OnNotifyingPropertyChanged,
                isPrivate: isPrivate,
                valueEqualityComparer: valueEqualityComparer);

            AddNotifyingProperty(property);

            return property;
        }

        /// <summary>
        /// Creates a read-only Property and adds it to the context.
        /// </summary>
        /// <typeparam name="T">Type for the property</typeparam>
        /// <param name="name">Property name. Must be non-empty and unique</param>
        /// <param name="valueProvider">Delegate that provides the value for the property</param>
        /// <param name="dependencies">Optional enumerable of NotifyingItems on which the property depends for automatic reevaluation.</param>
        /// <param name="isPrivate">Specifies whether the property should be private and not sent out change notifications</param>
        /// <param name="valueEqualityComparer">Optional custom value equality comparer which gets called when the value is about to be changed.</param>
        /// <returns>Created NotifyingProperty object</returns>
        public NotifyingReadonlyProperty<T> CreateReadonlyProperty<T>(
            string name,
            Func<T> valueProvider,
            IEnumerable<INotifyingProperty> dependencies = null,
            ValueEqualityComparer<T> valueEqualityComparer = null)
        {
            var property = new NotifyingReadonlyProperty<T>(
                name,
                valueProvider: valueProvider,
                changedAction: OnNotifyingPropertyChanged,
                valueEqualityComparer: valueEqualityComparer);

            AddNotifyingProperty(property);
            AddNotifiableItem(property, dependencies);

            return property;
        }

        /// <summary>
        /// Creates a Collection Property and adds it to the context.
        /// </summary>
        /// <typeparam name="T">Type for items in the collection for the property</typeparam>
        /// <param name="name">Property name. Must be non-empty and unique</param>
        /// <param name="initialValues">Optional initial items for the collection</param>
        /// <returns>Created NotifyingCollectionProperty object with collection</returns>
        public NotifyingCollectionProperty<T> CreateCollectionProperty<T>(
            string name,
            IEnumerable<T> initialValues = null)
        {
            var collectionProperty = new NotifyingCollectionProperty<T>(
                name,
                initialValues: initialValues,
                changedAction: OnNotifyingPropertyChanged);

            AddNotifyingProperty(collectionProperty);

            return collectionProperty;
        }

        /// <summary>
        /// Creates a simple Action. 
        /// </summary>
        /// <param name="executeAction">The delegate to execute when the action is notified.</param>
        /// <returns>Created NotifiableAction</returns>
        public NotifiableAction CreateAction(
            Action executeAction,
            IEnumerable<INotifyingProperty> dependencies = null)
        {
            var action = new NotifiableAction(
                executeAction);

            AddNotifiableItem(action, dependencies);

            return action;
        }

        /// <summary>
        /// Creates a simple (non-parameterized) Command. 
        /// The created command implements <c>ICommand</c> (though parameters are ignored)
        /// </summary>
        /// <param name="executeAction">The delegate to execute when the command is executed.</param>
        /// <param name="canExecuteAction">Optional delegate that returns a boolean indicating whether the command can be executed.</param>
        /// <param name="dependencies">Optional enumerable of NotifyingItems on which the command depends for automatic reevaluation.</param>
        /// <returns>Created NotifiableCommand</returns>
        public NotifiableCommand CreateCommand(
            Action executeAction,
            Func<bool> canExecuteAction = null,
            IEnumerable<INotifyingProperty> dependencies = null)
        {
            var command = new NotifiableCommand(
                executeAction,
                canExecuteAction,
                requeryHook: m_CommandRequeryHook);

            AddNotifiableItem(command, dependencies);

            return command;
        }

        /// <summary>
        /// Creates a Command that expects a parameter on Execution, providing all support of the <c>ICommand</c> interface.
        /// </summary>
        /// <param name="executeAction">The delegate to execute when the command is executed.</param>
        /// <param name="canExecuteAction">Optional delegate that returns a boolean indicating whether the command can be executed.</param>
        /// <param name="dependencies">Optional enumerable of NotifyingItems on which the command depends for automatic reevaluation.</param>
        /// <typeparam name="T">Type of the parameter. For a basic ICommand implementation this can be set to <c>object</c> though a more specific type is recommended.</typeparam>
        /// <returns>Created NotifiableCommand</returns>
        public NotifiableCommand<T> CreateCommand<T>(
            Action<T> executeAction,
            Func<T, bool> canExecuteAction = null,
            IEnumerable<INotifyingProperty> dependencies = null)
        {
            var command = new NotifiableCommand<T>(
                executeAction,
                canExecuteAction,
                requeryHook: m_CommandRequeryHook);

            AddNotifiableItem(command, dependencies);

            return command;
        }

        /// <summary>
        /// Request that all notifiable items in the context reevaluate its state.
        /// </summary>
        public void ReevaluateAll()
        {
            foreach (var cmd in m_RegisteredNotifiableItems) cmd.Reevaluate();
        }

        /// <summary>
        /// Update multiple writable properties without updating dependend items or sending notifications during the update. 
        /// After the update action is complete depedencies are processed and notifications are sent out.
        /// </summary>
        /// <remarks>
        /// Dependendenent properties and commands are not updated during the execution of the update action and thus might
        /// contain stale values.
        /// </remarks>
        /// <param name="updateAction">Delegate that updates the properties while supressing notifications temporarily</param>
        public void BulkUpdate(Action updateAction)
        {
            if (updateAction is null) throw new ArgumentNullException(nameof(updateAction));
            if (m_InBulkUpdate) throw new InvalidOperationException("There is already a Bulk Update in progress.");

            m_InBulkUpdate = true;
            m_BulkChangedNotificationProperties.Clear();
            try
            {
                updateAction?.Invoke();
            }
            finally
            {
                m_InBulkUpdate = false;
                var allDependentItems = new HashSet<INotifiableItem>();
                foreach (var notifyingProperty in m_BulkChangedNotificationProperties)
                {
                    if (m_RegisteredProperties.TryGetValue(notifyingProperty, out var dependentItems))
                    {
                        foreach (var depItem in dependentItems) _ = allDependentItems.Add(depItem);
                    }
                    NotifyPropertyChanged(notifyingProperty);
                }
                ReevaluateNotifiableItems(allDependentItems);
            }
        }

        private void AddNotifyingProperty(INotifyingProperty notifyingProperty)
        {
            if (!string.IsNullOrEmpty(notifyingProperty.Name) && m_RegisteredProperties.Keys.Any(rp => Equals(rp.Name, notifyingProperty.Name)))
                throw new ArgumentException($"There is already a registered notifying property with the name '{notifyingProperty.Name}'", nameof(notifyingProperty));

            m_RegisteredProperties.Add(notifyingProperty, new HashSet<INotifiableItem>());
        }

        private void AddNotifiableItem(
            INotifiableItem notifiableItem,
            IEnumerable<INotifyingProperty> dependencies = null)
        {
            m_RegisteredNotifiableItems.Add(notifiableItem);
            RegisterDependencies(notifiableItem, dependencies);
        }

        private void RegisterDependencies(INotifiableItem notifiableItem, IEnumerable<INotifyingProperty> dependencies = null)
        {
            if (dependencies is null) return;

            foreach (var dependency in dependencies)
            {
                if (dependency is null)
                    throw new ArgumentException($"Oen of the items for argument '{nameof(dependencies)}' is null", nameof(dependencies));
                if (!m_RegisteredProperties.TryGetValue(dependency, out var dependentItems))
                    throw new ArgumentException($"Argument '{nameof(dependencies)}' may only reference properties that are registered in the same Scope.", nameof(dependencies));
                dependentItems.Add(notifiableItem);
            }
        }

        private void OnNotifyingPropertyChanged(INotifyingProperty notifyingProperty)
        {
            if (m_InBulkUpdate)
            {
                if (notifyingProperty is INotifyingWritableProperty nwp)
                {
                    m_BulkChangedNotificationProperties.Add(nwp);
                }
            }
            else
            {
                NotifyPropertyChanged(notifyingProperty);
                ReevaluateNotifiableItems(m_RegisteredProperties[notifyingProperty]);
            }
        }

        private void NotifyPropertyChanged(INotifyingProperty notifyingProperty)
        {
            if (!notifyingProperty.IsPrivate) m_NotifyingPropertyChangedAction?.Invoke(notifyingProperty);
        }

        private void ReevaluateNotifiableItems(ISet<INotifiableItem> notifiableItems)
        {
            foreach (var di in notifiableItems) di.Reevaluate();
        }
    }
}
