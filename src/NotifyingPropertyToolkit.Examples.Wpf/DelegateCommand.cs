// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using System;
using System.Windows.Input;

namespace NotifyingPropertyToolkit.Examples.Wpf
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> m_ExecuteAction;
        private readonly Func<object, bool> m_CanExecuteAction;
        private event EventHandler m_CanExecuteChanged;

        public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecuteAction = null)
        {
            m_ExecuteAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            m_CanExecuteAction = canExecuteAction;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (m_CanExecuteAction is null) return;
                if (m_CanExecuteChanged is null)
                {
                    CommandManager.RequerySuggested += CommandManager_RequerySuggested;
                }
                m_CanExecuteChanged += value;
            }
            remove
            {
                if (m_CanExecuteAction is null) return;
                m_CanExecuteChanged -= value;
                if (m_CanExecuteChanged is null)
                {
                    CommandManager.RequerySuggested -= CommandManager_RequerySuggested;
                }
            }
        }

        private void CommandManager_RequerySuggested(object sender, EventArgs e) =>
            RaiseCanExecuteChanged();

        public void RaiseCanExecuteChanged() =>
            m_CanExecuteChanged.Invoke(this, EventArgs.Empty);

        public bool CanExecute(object parameter) =>
            m_CanExecuteAction?.Invoke(parameter) ?? true;

        public void Execute(object parameter) =>
            m_ExecuteAction.Invoke(parameter);
    }
}
