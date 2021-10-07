// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using System;

namespace NotifyingPropertyToolkit
{
    /// <summary>
    /// A named property with a read-only cached value which reevaluates its value on-demand
    /// and which can call a delegate to notify when its internal value changed.
    /// </summary>
    /// <typeparam name="T">Type of the property value</typeparam>
    public class NotifyingReadonlyProperty<T> : NotifyingProperty<T>, INotifiableItem
    {
        private readonly Func<T> m_ValueProvider;

        /// <summary>
        /// Creates a new instance of a NotifyingReadonlyProperty. 
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
        public NotifyingReadonlyProperty(
            string name,
            Func<T> valueProvider = null,
            Action<INotifyingProperty> changedAction = null,
            bool isPrivate = false,
            ValueEqualityComparer<T> valueEqualityComparer = null)
            : base(name, changedAction, isPrivate, valueEqualityComparer)
        {
            m_ValueProvider = valueProvider ?? (() => Value == null ? default : Value);
            UpdateValue(m_ValueProvider.Invoke(), false);
        }

        /// <summary>
        /// Request that the item should reevaluate its state.
        /// </summary>
        public void Reevaluate()
        {
            UpdateValue(m_ValueProvider.Invoke());
        }
    }
}
