// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using NUnit.Framework;

namespace NotifyingPropertyToolkit.Test
{
    [TestFixture]
    public class CascadingUpdateTests
    {
        [Test]
        public void PropertiesWithPropertyDependencies_UpdatesPropagate()
        {
            var propDUpdates = 0;
            var propEUpdates = 0;

            var notifyingContext = new NotifyingContext();
            var numberAProperty = notifyingContext.CreateWritableProperty("NumberA", initialValue: 1);
            var numberBProperty = notifyingContext.CreateWritableProperty("NumberB", initialValue: 5);
            var numberCProperty = notifyingContext.CreateWritableProperty("NumberC", initialValue: 5);

            var numberDProperty = notifyingContext.CreateReadonlyProperty("NumberD",
                () =>
                {
                    propDUpdates++;
                    return numberAProperty.Value + numberBProperty.Value;
                }, dependencies: new INotifyingProperty[] { numberAProperty, numberBProperty });

            var numberEProperty = notifyingContext.CreateReadonlyProperty("NumberE",
                () =>
                {
                    propEUpdates++;
                    return numberAProperty.Value + numberCProperty.Value + numberDProperty.Value;
                }, dependencies: new INotifyingProperty[] { numberAProperty, numberCProperty });

            numberAProperty.Value = 1;
            numberBProperty.Value = 6;
            numberAProperty.Value = 2;
            numberCProperty.Value = 6;
            numberCProperty.Value = 7;

            Assert.AreEqual(3, propDUpdates);
            Assert.AreEqual(4, propEUpdates);
            Assert.AreEqual(2, numberAProperty.Value);
            Assert.AreEqual(6, numberBProperty.Value);
            Assert.AreEqual(7, numberCProperty.Value);
            Assert.AreEqual(8, numberDProperty.Value);
            Assert.AreEqual(17, numberEProperty.Value);
        }

        [Test]
        public void ActionWithPropertyDependency_UpdatesPropagate()
        {
            var actionCallCount = 0;

            var notifyingContext = new NotifyingContext();
            var numberAProperty = notifyingContext.CreateWritableProperty("NumberA", initialValue: 1);
            var numberBProperty = notifyingContext.CreateWritableProperty("NumberB", initialValue: 5);

            var action = notifyingContext.CreateAction(() => actionCallCount++,
                dependencies: new[] { numberAProperty, numberBProperty });

            numberAProperty.Value = 1;
            numberBProperty.Value = 6;
            numberAProperty.Value = 2;
            numberBProperty.Value = 7;

            Assert.AreEqual(3, actionCallCount);
        }

        [Test]
        public void CommandsWithActionDependency_UpdatesPropagate()
        {
            var commandAUpdateCount = 0;
            var commandBUpdateCount = 0;

            var notifyingContext = new NotifyingContext();
            var numberAProperty = notifyingContext.CreateWritableProperty("NumberA", initialValue: 1);
            var numberBProperty = notifyingContext.CreateWritableProperty("NumberB", initialValue: 5);

            var actionA = notifyingContext.CreateCommand(() => { },
                canExecuteAction: () =>
                {
                    commandAUpdateCount++;
                    return true;
                },
                dependencies: new[] { numberAProperty, numberBProperty });
            var actionB = notifyingContext.CreateCommand(() => { },
                canExecuteAction: () =>
                {
                    commandBUpdateCount++;
                    return true;
                },
                dependencies: new[] { numberAProperty });

            numberAProperty.Value = 1;
            numberBProperty.Value = 6;
            numberAProperty.Value = 2;
            numberBProperty.Value = 7;

            Assert.AreEqual(4, commandAUpdateCount);
            Assert.AreEqual(2, commandBUpdateCount);
        }
    }
}
