// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

namespace NotifyingPropertyToolkit
{
    /// <summary>
    /// Defines a notifiable item.
    /// </summary>
    public interface INotifiableItem
    {
        /// <summary>
        /// Request that the item should reevaluate its state.
        /// </summary>
        void Reevaluate();
    }
}
