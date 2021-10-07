// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Windows;

namespace NotifyingPropertyToolkit.Examples.Wpf
{
    public class UserFormViewModel : ViewModelDebugBase
    {
        public UserFormViewModel()
        {
            FirstName = NotifyingContext.CreateWritableProperty<string>(nameof(FirstName));

            LastName = NotifyingContext.CreateWritableProperty<string>(nameof(LastName));

            FullName = NotifyingContext.CreateReadonlyProperty(nameof(FullName),
                valueProvider: () => $"{FirstName} {LastName}".ToUpper().Trim(),
                 dependencies: new[] { FirstName, LastName });

            WelcomeMessage = NotifyingContext.CreateReadonlyProperty(nameof(WelcomeMessage),
                valueProvider: () => string.IsNullOrWhiteSpace(FullName) ? string.Empty : $"Hello {FullName}!",
                 dependencies: new[] { FullName });

            ResetCommand = NotifyingContext.CreateCommand(
                executeAction: () =>
                    {
                        NotifyingContext.BulkUpdate(() =>
                        {
                            FirstName.Value = string.Empty;
                            LastName.Value = string.Empty;
                        });
                    },
                canExecuteAction: () => !string.IsNullOrWhiteSpace(WelcomeMessage),
                    dependencies: new[] { FullName });

            DefaultsCommand = NotifyingContext.CreateCommand(
                executeAction: () =>
                    {
                        NotifyingContext.BulkUpdate(() =>
                        {
                            FirstName.Value = "John";
                            LastName.Value = "Doe";
                        });
                    },
                canExecuteAction: () => string.IsNullOrWhiteSpace(WelcomeMessage),
                    dependencies: new[] { FullName });

            TestCommand = NotifyingContext.CreateCommand<NotifyingProperty<string>>(
                s => MessageBox.Show(s.Value),
                s => !string.IsNullOrWhiteSpace(s?.Value),
                new[] { FirstName, LastName });

            NotifyingContext.CreateAction(
                () => Debug.WriteLine("LastName Changed!"),
                new[] { LastName });

            NotifyingContext.BulkUpdate(() =>
            {
                FirstName.Value = "John";
                LastName.Value = "Doe";
            });

            TestCollection = NotifyingContext.CreateCollectionProperty(nameof(TestCollection),
                initialValues: new[] { "This", "Is", "A", "Test" });

            TestCollectionCount = NotifyingContext.CreateReadonlyProperty(nameof(TestCollectionCount),
                () => $"Items * 2 + 1 = {TestCollection.Value.Count * 2}",
                new[] { TestCollection });

            AddToCollectionCommand = NotifyingContext.CreateCommand(
                () => TestCollection.Value.Add("Added Item"),
                () => TestCollection.Value.Count < 10,
                new[] { TestCollection });
        }

        public NotifyingWritableProperty<string> FirstName { get; }
        public NotifyingWritableProperty<string> LastName { get; }

        public NotifyingProperty<string> FullName { get; }
        public NotifyingProperty<string> WelcomeMessage { get; }

        public NotifiableCommand ResetCommand { get; }
        public NotifiableCommand DefaultsCommand { get; }

        public NotifiableCommand<NotifyingProperty<string>> TestCommand { get; }

        public NotifyingCollectionProperty<string> TestCollection { get; }
        public NotifyingProperty<string> TestCollectionCount { get; }
        public NotifiableCommand AddToCollectionCommand { get; }
    }
}
