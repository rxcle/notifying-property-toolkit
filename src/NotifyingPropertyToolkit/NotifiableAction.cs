// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using System;

namespace NotifyingPropertyToolkit
{
    /// <summary>
    /// Represents a delegate action that is invoked when notified.
    /// </summary>
    public class NotifiableAction : INotifiableItem
    {
        private readonly Action m_ExecuteAction;

        /// <summary>
        /// Creates a new instance of the NotifyingCommand.
        /// </summary>
        /// <param name="executeAction">Delegate to invoke when the action is notified. Required.</param>
        public NotifiableAction(
            Action executeAction)
        {
            m_ExecuteAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
        }

        /// <summary>
        /// Request that the delegate action should be executed.
        /// </summary>
        public void Reevaluate() =>
            m_ExecuteAction.Invoke();
    }
}
