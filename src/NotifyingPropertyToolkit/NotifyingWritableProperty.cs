// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using System;

namespace NotifyingPropertyToolkit
{
    /// <summary>
    /// A named property with a read-write cached value which can call a delegate to notify 
    /// when its internal value changed. 
    /// </summary>
    /// <remarks>
    /// Although a writable property implements both Notifying and Notifiable traits, Notifying has 
    /// no practical effect on the property; The value will only be changed by explicity setting a different
    /// value on the <c>Value</c> property.
    /// <typeparam name="T">Type of the property value</typeparam>
    public class NotifyingWritableProperty<T> : NotifyingProperty<T>, INotifyingWritableProperty
    {
        /// <summary>
        /// Creates a new instance of a NotifyingWritableProperty. 
        /// NotifyingWritableProperty inherit from <c>NotifyingProperty</c> but have a writable <c>Value</c>
        /// property. Changes to the <c>Value</c> property will send out a notification.
        /// Their value is never automatically reevaluated/updated based and thus the <c>Reevaluate</c> method
        /// will return <c>false</c> always.
        /// </summary>
        /// <param name="name">Property name, cannot be null, empty or all whitespace</param>
        /// <param name="initialValue">Optional initial value. No notification for this value is sent.</param>
        /// <param name="changedAction">Optional action to perform when the item state changes</param>
        /// <param name="isPrivate">Specifies whether the property should be private and not sent out change notifications</param>
        /// <param name="valueEqualityComparer">Optional custom value equality comparer which gets called when the value is about to be changed.</param>
        public NotifyingWritableProperty(
            string name,
            T initialValue = default,
            Action<INotifyingProperty> changedAction = null,
            bool isPrivate = false,
            ValueEqualityComparer<T> valueEqualityComparer = null)
            : base(name, changedAction, isPrivate, valueEqualityComparer)
        {
            UpdateValue(initialValue, false);
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public new T Value
        {
            get => base.Value;
            set => UpdateValue(value);
        }
    }
}
