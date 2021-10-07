// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NotifyingPropertyToolkit
{
    /// <summary>
    /// Compare an old value and new value and return <c>true</c> if the values are to
    /// be regarded as equal or <c>false</c> if they are to be regarded as different.
    /// </summary>
    /// <typeparam name="T">Type of the arguments</typeparam>
    /// <param name="oldValue">Old value</param>
    /// <param name="newValue">New value</param>
    /// <returns>Return <c>true</c> if the value is equal, return <c>false</c> if the value is different.</returns>
    public delegate bool ValueEqualityComparer<T>(T oldValue, T newValue);

    /// <summary>
    /// A named property with a read-only cached value which reevaluates its value on-demand
    /// and which can call a delegate to notify when its internal value changed.
    /// </summary>
    /// <typeparam name="T">Type of the property value</typeparam>
    public abstract class NotifyingProperty<T> : INotifyingProperty
    {
        private readonly Action<INotifyingProperty> m_ChangedAction;
        private readonly ValueEqualityComparer<T> m_ValueEqualityComparer;

        /// <summary>
        /// Creates a new instance of a NotifyingProperty. The only required aregument is the <c>name</c>.
        /// NotifyingProperty implicitly converts to the wrapped type.
        /// </summary>
        /// <param name="name">Property name, cannot be null, empty or all whitespace</param>
        /// <param name="changedAction">Optional action to perform when the item state changes</param>
        /// <param name="isPrivate">Specifies whether the property should be private and not sent out change notifications</param>
        /// <param name="valueEqualityComparer">Optional custom value equality comparer which gets called when the value is about to be changed.</param>
        protected NotifyingProperty(
            string name,
            Action<INotifyingProperty> changedAction = null,
            bool isPrivate = false,
            ValueEqualityComparer<T> valueEqualityComparer = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be null, empty or all whitespace", nameof(name));
            Name = name;
            IsPrivate = isPrivate;
            m_ChangedAction = changedAction;
            m_ValueEqualityComparer = valueEqualityComparer ?? ((oldValue, newValue) => Equals(oldValue, newValue));
        }

        /// <summary>
        /// Trigger a call to the <c>changedAction</c> delegate passed to the constructor.
        /// </summary>
        protected void NotifyChanged() =>
            m_ChangedAction?.Invoke(this);

        /// <summary>
        /// Property name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// Gets a value specifying whether the property is private.
        /// Private properties are internal for class-internal used. The behave like normal properties but they do not 
        /// generate change notifications on the Context.
        /// </summary>
        public bool IsPrivate { get; }

        /// <summary>
        /// Implicitly converts a NotifyingProperty to its wrapped type.
        /// </summary>
        /// <param name="notifyingProperty">NotifyingProperty to convert</param>
        public static implicit operator T(NotifyingProperty<T> notifyingProperty) =>
            notifyingProperty is null ? default : notifyingProperty.Value;

        /// <summary>
        /// Returns a string representation of the value for the wrapped type.
        /// </summary>
        /// <returns>Value as string</returns>
        public override string ToString() => Value?.ToString() ?? string.Empty;

        /// <summary>
        /// Update the property value.
        /// </summary>
        /// <param name="newValue">New value to set</param>
        protected void UpdateValue(T newValue, bool notify = true)
        {
            if (m_ValueEqualityComparer(newValue, Value)) return;
            AttachEvents(Value, newValue);
            Value = newValue;
            if (notify) NotifyChanged();
        }

        private void AttachEvents(T oldValue, T newValue)
        {
            // A (typical) NotifyingCollection does not also need to subscribe to individual property changes
            if (newValue is INotifyCollectionChanged newNotifyCollection)
                newNotifyCollection.CollectionChanged += OnValueCollectionChanged;
            else if (newValue is INotifyPropertyChanged newNotifyPropertyChanged)
                newNotifyPropertyChanged.PropertyChanged += OnValuePropertyChanged;

            if (oldValue is INotifyCollectionChanged oldNotifyCollection)
                oldNotifyCollection.CollectionChanged -= OnValueCollectionChanged;
            else if (oldValue is INotifyPropertyChanged oldNotifyPropertyChanged)
                oldNotifyPropertyChanged.PropertyChanged -= OnValuePropertyChanged;
        }

        private void OnValueCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) =>
            NotifyChanged();

        private void OnValuePropertyChanged(object sender, PropertyChangedEventArgs e) =>
            NotifyChanged();
    }
}
