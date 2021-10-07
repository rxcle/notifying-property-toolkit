// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using System;
using System.Windows.Input;

namespace NotifyingPropertyToolkit
{
    /// <summary>
    /// Represents a Command with a parameter which can be notified for updates on the CanExecute state.
    /// </summary>    
    public class NotifiableCommand<T> : ICommand, INotifiableItem
    {
        private readonly Action<T> m_ExecuteAction;
        private readonly Func<T, bool> m_CanExecuteAction;
        private readonly Action<bool, EventHandler> m_RequeryHook;

        private EventHandler m_CanExecuteChanged;

        /// <summary>
        /// Creates a new instance of the NotifyingCommand.
        /// </summary>
        /// <param name="executeAction">Action to invoke when the command is executed. Required.</param>
        /// <param name="canExecuteAction">Function that returns a boolean indicating whether the Command can be invoked.</param>
        /// <param name="requeryHook">Hook to attach the command to an external update trigger like RequerySuggested of the CommandManager.</param> 
        public NotifiableCommand(
            Action<T> executeAction,
            Func<T, bool> canExecuteAction = null,
            Action<bool, EventHandler> requeryHook = null)
        {
            m_ExecuteAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            m_CanExecuteAction = canExecuteAction;
            m_RequeryHook = requeryHook;
            Reevaluate();
        }

        /// <summary>
        /// Event that is triggered when the CanExecute state of the command
        /// changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (m_CanExecuteAction is null) return;
                if (m_CanExecuteChanged is null) m_RequeryHook?.Invoke(true, RequerySuggested);
                m_CanExecuteChanged += value;
            }
            remove
            {
                if (m_CanExecuteAction is null) return;
                m_CanExecuteChanged -= value;
                if (m_CanExecuteChanged is null) m_RequeryHook?.Invoke(false, RequerySuggested);
            }
        }

        /// <summary>
        /// Returns a value indicating whether or not the Command can be executed.
        /// </summary>
        /// <param name="parameter">Parameter</param>
        public bool CanExecute(T parameter) =>
            m_CanExecuteAction?.Invoke(parameter) ?? true;

        /// <summary>
        /// Request to execute the Command.
        /// In case CanExecute returns <c>false</c> for the given parameter, the request is ignored.
        /// </summary>
        /// <param name="parameter">Parameter</param>
        public void Execute(T parameter)
        {
            if (CanExecute(parameter)) m_ExecuteAction.Invoke(parameter);
        }

        /// <summary>
        /// Request the Command to reevaluate its CanExecute state.
        /// </summary>
        public void Reevaluate() =>
            m_CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        private void RequerySuggested(object sender, EventArgs e) =>
            Reevaluate();

        bool ICommand.CanExecute(object parameter) =>
            CanExecute((T)parameter);

        void ICommand.Execute(object parameter) =>
            Execute((T)parameter);
    }
}
