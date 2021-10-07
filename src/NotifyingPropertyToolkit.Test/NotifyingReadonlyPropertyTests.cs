// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using NUnit.Framework;

using System;
using System.Linq;

namespace NotifyingPropertyToolkit.Test
{
    [TestFixture]
    public class NotifyingReadonlyPropertyTests
    {
        [Test]
        public void CreatingStringReadonlyPropertyWithName_HasExpectedInitialState()
        {
            const string PropName = "Prop";
            var notifyingProperty = new NotifyingReadonlyProperty<string>(PropName);

            Assert.AreEqual(PropName, notifyingProperty.Name);
            Assert.IsNull(notifyingProperty.Value);
            Assert.IsEmpty(notifyingProperty.ToString());
            Assert.IsNull((string)notifyingProperty);
            Assert.IsFalse(notifyingProperty.IsPrivate);
        }

        [Test]
        public void CreatingIntegerReadonlyPropertyWithName_HasExpectedInitialState()
        {
            const string PropName = "Prop";
            var notifyingProperty = new NotifyingReadonlyProperty<int>(PropName);

            Assert.AreEqual(PropName, notifyingProperty.Name);
            Assert.AreEqual(default(int), notifyingProperty.Value);
            Assert.AreEqual(default(int).ToString(), notifyingProperty.ToString());
            Assert.AreEqual(default(int), (int)notifyingProperty);
            Assert.IsFalse(notifyingProperty.IsPrivate);
        }

        [Test]
        public void CreatingObjectReadonlyPropertyWithName_HasExpectedInitialState()
        {
            const string PropName = "Prop";
            var notifyingProperty = new NotifyingReadonlyProperty<Version>(PropName);

            Assert.AreEqual(PropName, notifyingProperty.Name);
            Assert.AreEqual(null, notifyingProperty.Value);
            Assert.AreEqual(string.Empty, notifyingProperty.ToString());
            Assert.AreEqual(null, (Version)notifyingProperty);
            Assert.IsFalse(notifyingProperty.IsPrivate);
        }

        [Test]
        public void CreatingObjectReadonlyPropertyWithValueProvider_HasExpectedInitialState()
        {
            const string PropName = "Prop";
            var version = new Version(1, 2, 3);
            var notifyingProperty = new NotifyingReadonlyProperty<Version>(PropName, () => version);

            Assert.AreEqual(PropName, notifyingProperty.Name);
            Assert.AreEqual(version, notifyingProperty.Value);
            Assert.AreEqual(version.ToString(), notifyingProperty.ToString());
            Assert.AreEqual(version, (Version)notifyingProperty);
        }

        [Test]
        public void CreatingReadonlyPropertyWithName_ArgumentsAreChecked()
        {
            Assert.Throws<ArgumentException>(() => new NotifyingReadonlyProperty<object>(null, null, null, false, null));
            Assert.Throws<ArgumentException>(() => new NotifyingReadonlyProperty<object>(string.Empty, null, null, false, null));
            Assert.Throws<ArgumentException>(() => new NotifyingReadonlyProperty<object>(" ", null, null, false, null));
            Assert.DoesNotThrow(() => new NotifyingReadonlyProperty<object>("Name", null, null, false, null));
        }

        [Test]
        public void ReadonlyPropertyNotified_UpdatesValueWithValueProviderAndNotifiesWhenDifferent()
        {
            const string PropName = "Prop";
            var values = new[] { "Hello", "World", "World" };
            var valuesEnumerator = values.AsEnumerable().GetEnumerator();

            var numberNotificationSent = 0;
            var notifyingProperty = new NotifyingReadonlyProperty<string>(PropName,
                () => { valuesEnumerator.MoveNext(); return valuesEnumerator.Current; },
                _ => numberNotificationSent++);

            notifyingProperty.Reevaluate();
            notifyingProperty.Reevaluate();

            Assert.AreEqual(1, numberNotificationSent);
            Assert.AreEqual(values[1], notifyingProperty.Value);
        }

        [Test]
        public void ReadonlyPropertyWithCustomValueComparer_UpdatesValueOnlyWhenValuesNotEqual()
        {
            const string PropName = "Prop";
            var values = new[] { 1.234, 1.230, 1.238 };
            var valuesEnumerator = values.AsEnumerable().GetEnumerator();

            var numberNotificationSent = 0;
            var notifyingProperty = new NotifyingReadonlyProperty<double>(PropName,
                () => { valuesEnumerator.MoveNext(); return valuesEnumerator.Current; },
                _ => numberNotificationSent++,
                valueEqualityComparer: (oldValue, newValue) => Math.Round(oldValue, 2) == Math.Round(newValue, 2));

            notifyingProperty.Reevaluate();
            notifyingProperty.Reevaluate();

            Assert.AreEqual(1, numberNotificationSent);
            Assert.AreEqual(values[2], notifyingProperty.Value);
        }

        [Test]
        public void PrivateReadonlyPropertyNotified_HasPrivateFlagAndNotifiesNormally()
        {
            const string PropName = "Prop";
            var values = new[] { "Hello", "World" };
            var valuesEnumerator = values.AsEnumerable().GetEnumerator();

            var numberNotificationSent = 0;
            var notifyingProperty = new NotifyingReadonlyProperty<string>(PropName,
                () => { valuesEnumerator.MoveNext(); return valuesEnumerator.Current; },
                _ => numberNotificationSent++,
                isPrivate: true);

            notifyingProperty.Reevaluate();

            Assert.AreEqual(1, numberNotificationSent);
            Assert.AreEqual(values[1], notifyingProperty.Value);
            Assert.IsTrue(notifyingProperty.IsPrivate);
        }
    }
}