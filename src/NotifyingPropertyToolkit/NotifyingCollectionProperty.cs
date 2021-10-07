// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NotifyingPropertyToolkit
{
    /// <summary>
    /// A standard read-only property with a type value of <c>ObservableCollection</c>.
    /// Note that while the property Value is read-only, the contained collection is mutable.
    /// Changes on the collection will trigger a change notification on the property.
    /// </summary>
    /// <typeparam name="T">Type of elements in the collection</typeparam>
    public class NotifyingCollectionProperty<T> : NotifyingProperty<ObservableCollection<T>>
    {
        /// <summary>
        /// Creates a new instance of a NotifyingCollectionProperty. The only required aregument is the <c>name</c>.
        /// </summary>
        /// <param name="name">Property name, cannot be null, empty or all whitespace</param>
        /// <param name="initialValues">Initial values for the collection</param>
        /// <param name="changedAction">Optional action to perform when the item state changes</param>
        /// <param name="isPrivate">Specifies whether the property should be private and not sent out change notifications</param>
        public NotifyingCollectionProperty(
            string name,
            IEnumerable<T> initialValues = null,
            Action<INotifyingProperty> changedAction = null,
            bool isPrivate = false)
            : base(name, changedAction, isPrivate)
        {
            UpdateValue(new ObservableCollection<T>(initialValues ?? Enumerable.Empty<T>()), false);
        }
    }
}
