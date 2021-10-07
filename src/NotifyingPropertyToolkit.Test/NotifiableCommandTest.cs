// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using NUnit.Framework;

using System;
using System.Windows.Input;

namespace NotifyingPropertyToolkit.Test
{
    [TestFixture]
    public class NotifiableCommandTest
    {
        [TestCase(true)]
        [TestCase(false)]
        public void CreatingNotifiableCommand_HasExpectedInitialState(bool canExecute)
        {
            var isActionExecuted = false;
            var notifiableCommand = new NotifiableCommand(() => isActionExecuted = true, () => canExecute);

            Assert.IsFalse(isActionExecuted);
            Assert.AreEqual(canExecute, notifiableCommand.CanExecute());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ExecuteNotifiableCommand_CallsExecuteAction(bool canExecute)
        {
            var isActionExecuted = false;
            var notifiableCommand = new NotifiableCommand(() => isActionExecuted = true, () => canExecute);

            notifiableCommand.Execute();
            Assert.AreEqual(canExecute, isActionExecuted);
        }

        [Test]
        public void NotifyNotifiableCommand_TriggersCanExecuteChangedEvent()
        {
            var canExecuteChangedCallCount = 0;
            var canExecute = true;
            var notifiableCommand = new NotifiableCommand(() => { }, () => canExecute = !canExecute);

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
            var notifiableCommand = new NotifiableCommand(() => { });

            void NotifiableCommandCanExecuteChanged(object sender, EventArgs e) => canExecuteChangedCallCount++;
            notifiableCommand.CanExecuteChanged += NotifiableCommandCanExecuteChanged;
            notifiableCommand.Reevaluate();
            notifiableCommand.CanExecuteChanged -= NotifiableCommandCanExecuteChanged;
            notifiableCommand.Reevaluate();

            Assert.AreEqual(0, canExecuteChangedCallCount);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NotifiableCommandAsICommand_ReturnsExpectedCanExecuteState(bool canExecute)
        {
            ICommand notifiableCommand = new NotifiableCommand(() => { }, () => canExecute);

            var actualCanExecute = notifiableCommand.CanExecute(null);

            Assert.AreEqual(canExecute, actualCanExecute);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NotifiableCommandAsICommand_ExecutePerformsExecuteAction(bool canExecute)
        {
            var isActionExecuted = false;
            ICommand notifiableCommand = new NotifiableCommand(() => isActionExecuted = true, () => canExecute);

            notifiableCommand.Execute(null);

            Assert.AreEqual(canExecute, isActionExecuted);
        }

        [Test]
        public void NotifiableCommandWithRequeryHook_CallsNotifyOnRequeryRequest()
        {
            EventHandler requiryHandler = null;
            var canExecuteChangedCallCount = 0;
            var canExecute = true;
            var notifiableCommand = new NotifiableCommand(() => { }, () => canExecute = !canExecute,
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
            Assert.Throws<ArgumentNullException>(() => new NotifiableCommand(null, null, null));
            Assert.Throws<ArgumentNullException>(() => new NotifiableCommand(null, () => true, null));
            Assert.Throws<ArgumentNullException>(() => new NotifiableCommand(null, null, (a, e) => { }));
            Assert.DoesNotThrow(() => new NotifiableCommand(() => { }, null, null));
        }
    }
}

