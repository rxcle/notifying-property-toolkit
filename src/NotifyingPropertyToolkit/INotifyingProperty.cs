// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

namespace NotifyingPropertyToolkit
{
    /// <summary>
    /// Defines a notifying property.
    /// </summary>
    public interface INotifyingProperty
    {
        /// <summary>
        /// Property name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value specifying whether the property is private.
        /// Private properties are internal for class-internal used. The behave like normal properties but they do not 
        /// generate change notifications on the Context.
        /// </summary>
        bool IsPrivate { get; }
    }
}
