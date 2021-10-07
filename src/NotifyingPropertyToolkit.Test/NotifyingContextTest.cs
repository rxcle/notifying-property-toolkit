// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using NUnit.Framework;

using System;
using System.Collections.Generic;

namespace NotifyingPropertyToolkit.Test
{
    [TestFixture]
    public class NotifyingContextTest
    {
        [Test]
        public void CreatingNotifyingContext_ArgumentsAreChecked()
        {
            Assert.DoesNotThrow(() => new NotifyingContext());
            Assert.DoesNotThrow(() => new NotifyingContext(null, null));
            Assert.DoesNotThrow(() => new NotifyingContext(_ => { }, null));
            Assert.DoesNotThrow(() => new NotifyingContext(null, (a, e) => { }));
        }

        [Test]
        public void CreateNotifyingReadonlyProperty_PropertyCreated()
        {
            const string PropName = "Prop";

            var context = new NotifyingContext();
            var prop = context.CreateReadonlyProperty(PropName, () => true);

            Assert.IsNotNull(prop);
            Assert.IsInstanceOf<NotifyingReadonlyProperty<bool>>(prop);
            Assert.IsTrue(prop.Value);
            Assert.AreEqual(PropName, prop.Name);
        }

        [Test]
        public void CreateNotifyingWritableProperty_PropertyCreated()
        {
            const string PropName = "Prop";

            var context = new NotifyingContext();
            var prop = context.CreateWritableProperty(PropName, true);

            Assert.IsNotNull(prop);
            Assert.IsInstanceOf<NotifyingWritableProperty<bool>>(prop);
            Assert.IsTrue(prop.Value);
            Assert.AreEqual(PropName, prop.Name);
        }

        [Test]
        public void CreateNotifyingCollectionProperty_PropertyCreated()
        {
            const string PropName = "Prop";

            var context = new NotifyingContext();
            var prop = context.CreateCollectionProperty<int>(PropName);

            Assert.IsNotNull(prop);
            Assert.IsInstanceOf<NotifyingCollectionProperty<int>>(prop);
            CollectionAssert.IsEmpty(prop.Value);
            Assert.AreEqual(PropName, prop.Name);
        }

        [Test]
        public void CreateNotifiableAction_ActionCreated()
        {
            var context = new NotifyingContext();
            var action = context.CreateAction(() => { });

            Assert.IsNotNull(action);
            Assert.IsInstanceOf<NotifiableAction>(action);
        }

        [Test]
        public void CreateNotifiableCommand_ActionCreated()
        {
            var context = new NotifyingContext();
            var command = context.CreateCommand(() => { });

            Assert.IsNotNull(command);
            Assert.IsInstanceOf<NotifiableCommand>(command);
            Assert.IsTrue(command.CanExecute());
        }

        [Test]
        public void CreateNotifiableCommandOfT_ActionCreated()
        {
            var context = new NotifyingContext();
            var command = context.CreateCommand<int>(_ => { });

            Assert.IsNotNull(command);
            Assert.IsInstanceOf<NotifiableCommand<int>>(command);
            Assert.IsTrue(command.CanExecute(0));
        }

        [Test]
        public void CreateNotifyingPropertyWithNullDependency_ThrowsArgumentException()
        {
            const string PropName = "Prop";
            var context = new NotifyingContext();
            Assert.Throws<ArgumentException>(() => context.CreateReadonlyProperty(PropName, () => true,
                dependencies: new INotifyingProperty[] { null }));
        }

        [Test]
        public void CreateNotifyingPropertyWithExistingName_ThrowsArgumentException()
        {
            const string PropName = "Prop";
            var context = new NotifyingContext();
            Assert.DoesNotThrow(() => context.CreateReadonlyProperty(PropName, () => true));
            Assert.Throws<ArgumentException>(() => context.CreateReadonlyProperty(PropName, () => true));
        }

        [Test]
        public void CreateNotifyingPropertyWithDependencyOfOtherContext_ThrowsArgumentException()
        {
            const string PropName = "Prop";
            var firstContext = new NotifyingContext();
            var firstProp = firstContext.CreateReadonlyProperty(PropName, () => true);
            var secondContext = new NotifyingContext();
            Assert.Throws<ArgumentException>(() => secondContext.CreateReadonlyProperty(PropName, () => true, dependencies: new[] { firstProp }));
        }

        [Test]
        public void CreateNotifyingPropertyWithDependency_RegistersCorrectly()
        {
            var context = new NotifyingContext();
            var firstProp = context.CreateWritableProperty("Prop1", 10);
            var secondProp = context.CreateReadonlyProperty("Prop2", () => firstProp.Value, dependencies: new[] { firstProp });

            firstProp.Value = 9;

            Assert.AreEqual(9, secondProp.Value);
        }

        [Test]
        public void CallingNotifyAll_ReevaluatesPropertyValues()
        {
            var context = new NotifyingContext();
            var propValue = 10;
            var prop = context.CreateReadonlyProperty("Prop", () => propValue);

            propValue = 9;
            context.ReevaluateAll();

            Assert.AreEqual(propValue, prop.Value);
        }


        [Test]
        public void CreateNotifyingPropertyWithName_NameArgumentIsChecked()
        {
            var context = new NotifyingContext();
            Assert.Throws<ArgumentException>(() => context.CreateReadonlyProperty(null, () => true));
            Assert.Throws<ArgumentException>(() => context.CreateReadonlyProperty(string.Empty, () => true));
            Assert.Throws<ArgumentException>(() => context.CreateReadonlyProperty("    ", () => true));
        }

        [Test]
        public void BulkUpdate_NotifiesPropertiesOnlyOnce()
        {
            var notifiedProperties = new List<INotifyingProperty>();
            var dependentPropUpdateCalled = 0;

            var context = new NotifyingContext(p => notifiedProperties.Add(p));
            var propA = context.CreateWritableProperty("PropA", 1);
            var propB = context.CreateWritableProperty("PropB", 100);
            var propC = context.CreateReadonlyProperty("PropC", () =>
            {
                dependentPropUpdateCalled++;
                return propA.Value + propB.Value;
            }, dependencies: new[] { propA, propB });

            context.BulkUpdate(() =>
            {
                propA.Value += 1;
                propB.Value += 1;
                propA.Value += 1;
                propB.Value += 1;
            });

            CollectionAssert.AreEqual(new INotifyingProperty[] { propA, propB, propC }, notifiedProperties);
            Assert.AreEqual(105, propC.Value);

            // Dependent properties are notified once for every updated notifying property in the bulk update, so propC is updated twice
            Assert.AreEqual(2, dependentPropUpdateCalled);
        }

        [Test]
        public void BulkUpdate_ArgumentsAreChecked()
        {
            var context = new NotifyingContext();
            Assert.Throws<ArgumentNullException>(() => context.BulkUpdate(null));
        }

        [Test]
        public void BulkUpdate_NoNestedBulkUpdateAllowed()
        {
            var context = new NotifyingContext();
            Assert.Throws<InvalidOperationException>(() => context.BulkUpdate(() => context.BulkUpdate(() => { })));
        }

    }
}

