// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using NUnit.Framework;

using System;
using System.Windows.Input;

namespace NotifyingPropertyToolkit.Test
{
    [TestFixture]
    public class NotifiableCommandOfTTest
    {
        [TestCase("one", true)]
        [TestCase("one", false)]
        [TestCase("two", true)]
        [TestCase("two", false)]
        public void CreatingNotifiableCommand_HasExpectedInitialState(string param, bool canExecute)
        {
            var isActionExecuted = false;
            var notifiableCommand = new NotifiableCommand<string>(
                p => { if (p == param) isActionExecuted = true; },
                p => p == param && canExecute);

            Assert.IsFalse(isActionExecuted);
            Assert.AreEqual(canExecute, notifiableCommand.CanExecute(param));
        }

        [TestCase("one", true)]
        [TestCase("one", false)]
        [TestCase("two", true)]
        [TestCase("two", false)]
        public void ExecuteNotifiableCommand_CallsExecuteAction(string param, bool canExecute)
        {
            var isActionExecuted = false;
            var notifiableCommand = new NotifiableCommand<string>(
                p => { if (p == param) isActionExecuted = true; },
                p => p == param && canExecute);

            notifiableCommand.Execute(param);
            Assert.AreEqual(canExecute, isActionExecuted);
        }

        [Test]
        public void NotifyNotifiableCommand_TriggersCanExecuteChangedEvent()
        {
            var canExecuteChangedCallCount = 0;
            var canExecute = true;
            var notifiableCommand = new NotifiableCommand<string>(_ => { }, _ => canExecute = !canExecute);

            void NotifiableCommandCanExecuteChanged(object sender, EventArgs e) => canExecuteChangedCallCount++;
            notifiableCommand.CanExecuteChanged += NotifiableCommandCanExecuteChanged;
            notifiableCommand.Reevaluate();
            notifiableCommand.Reevaluate();
            notifiableCommand.Reevaluate();
            notifiableCommand.CanExecuteChanged -= NotifiableCommandCanExecuteChanged;
            notifiableCommand.Reevaluate();

            Assert.AreEqual(3, canExecuteChangedCallCount);
        }

        [Test]
        public void NotifyNotifiableCommandWithNoCanExecuteAction_DoesNotTriggersCanExecuteChangedEvent()
        {
            var canExecuteChangedCallCount = 0;
            var notifiableCommand = new NotifiableCommand<string>(_ => { });

            void NotifiableCommandCanExecuteChanged(object sender, EventArgs e) => canExecuteChangedCallCount++;
            notifiableCommand.CanExecuteChanged += NotifiableCommandCanExecuteChanged;
            notifiableCommand.Reevaluate();
            notifiableCommand.CanExecuteChanged -= NotifiableCommandCanExecuteChanged;
            notifiableCommand.Reevaluate();

            Assert.AreEqual(0, canExecuteChangedCallCount);
        }

        [TestCase("one", true)]
        [TestCase("one", false)]
        [TestCase("two", true)]
        [TestCase("two", false)]
        public void NotifiableCommandAsICommand_ReturnsExpectedCanExecuteState(string param, bool canExecute)
        {
            ICommand notifiableCommand = new NotifiableCommand<string>(
                _ => { },
                p => p == param && canExecute);

            var actualCanExecute = notifiableCommand.CanExecute(param);

            Assert.AreEqual(canExecute, actualCanExecute);
        }

        [TestCase("one", true)]
        [TestCase("one", false)]
        [TestCase("two", true)]
        [TestCase("two", false)]
        public void NotifiableCommandAsICommand_ExecutePerformsExecuteAction(string param, bool canExecute)
        {
            var isActionExecuted = false;
            ICommand notifiableCommand = new NotifiableCommand<string>(
                p => { if (p == param) isActionExecuted = true; },
                p => p == param && canExecute);

            notifiableCommand.Execute(param);

            Assert.AreEqual(canExecute, isActionExecuted);
        }

        [Test]
        public void NotifiableCommandWithRequeryHook_CallsNotifyOnRequeryRequest()
        {
            EventHandler requiryHandler = null;
            var canExecuteChangedCallCount = 0;
            var canExecute = true;
            var notifiableCommand = new NotifiableCommand<string>(
                p => { },
                p => canExecute = !canExecute,
                (attach, handler) => requiryHandler = attach ? handler : null);

            void NotifiableCommandCanExecuteChanged(object sender, EventArgs e) => canExecuteChangedCallCount++;
            notifiableCommand.CanExecuteChanged += NotifiableCommandCanExecuteChanged;
            requiryHandler.Invoke(null, EventArgs.Empty);
            notifiableCommand.CanExecuteChanged -= NotifiableCommandCanExecuteChanged;

            Assert.IsNull(requiryHandler);
            Assert.AreEqual(1, canExecuteChangedCallCount);
        }

        [Test]
        public void CreatingNotifiableCommand_ArgumentsAreChecked()
        {
            Assert.Throws<ArgumentNullException>(() => new NotifiableCommand<string>(null, null, null));
            Assert.Throws<ArgumentNullException>(() => new NotifiableCommand<string>(null, _ => true, null));
            Assert.Throws<ArgumentNullException>(() => new NotifiableCommand<string>(null, null, (a, e) => { }));
            Assert.DoesNotThrow(() => new NotifiableCommand<string>(_ => { }, null, null));
        }
    }
}

