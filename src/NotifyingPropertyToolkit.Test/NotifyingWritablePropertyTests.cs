// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using NUnit.Framework;

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NotifyingPropertyToolkit.Test
{
    public class NotifyingWritablePropertyTests
    {
        [Test]
        public void CreatingStringWritablePropertyWithInitialValue_ValueIsCorrectAndNoNotificationSent()
        {
            const string PropName = "Prop";
            const string InitialValue = "InitialValue";

            var notificationSent = false;
            var notifyingProperty = new NotifyingWritableProperty<string>(PropName,
                initialValue: InitialValue,
                changedAction: _ => notificationSent = true);

            Assert.AreEqual(InitialValue, notifyingProperty.Value);
            Assert.AreEqual(InitialValue, notifyingProperty.ToString());
            Assert.AreEqual(InitialValue, (string)notifyingProperty);
            Assert.IsFalse(notifyingProperty.IsPrivate);
            Assert.IsFalse(notificationSent);
        }

        [Test]
        public void CreatingStringWritablePropertyWithNameAndValue_HasExpectedInitialState()
        {
            const string PropName = "Prop";
            const string NewValue = "NewValue";

            var changedPropertyName = string.Empty;
            var notifyingProperty = new NotifyingWritableProperty<string>(PropName,
                changedAction: prop => changedPropertyName = prop.Name)
            {
                Value = NewValue
            };

            Assert.AreEqual(PropName, notifyingProperty.Name);
            Assert.AreEqual(PropName, changedPropertyName);
            Assert.AreEqual(NewValue, notifyingProperty.Value);
            Assert.AreEqual(NewValue, notifyingProperty.ToString());
            Assert.AreEqual(NewValue, (string)notifyingProperty);
        }

        [Test]
        public void CreatingIntegerWritablePropertyWithInitialValue_ValueIsCorrectAndNoNotificationSent()
        {
            const string PropName = "Prop";
            const int InitialValue = 102;

            var notificationSent = false;
            var notifyingProperty = new NotifyingWritableProperty<int>(PropName,
                initialValue: InitialValue,
                changedAction: _ => notificationSent = true);

            Assert.AreEqual(PropName, notifyingProperty.Name);
            Assert.AreEqual(InitialValue, notifyingProperty.Value);
            Assert.AreEqual(InitialValue.ToString(), notifyingProperty.ToString());
            Assert.AreEqual(InitialValue, (int)notifyingProperty);
            Assert.IsFalse(notificationSent);
        }

        [Test]
        public void CreatingIntegerWritablePropertyWithNameAndValue_HasExpectedInitialState()
        {
            const string PropName = "Prop";
            const int NewValue = 513;

            var changedPropertyName = string.Empty;
            var notifyingProperty = new NotifyingWritableProperty<int>(PropName,
                changedAction: prop => changedPropertyName = prop.Name)
            {
                Value = NewValue
            };

            Assert.AreEqual(PropName, notifyingProperty.Name);
            Assert.AreEqual(PropName, changedPropertyName);
            Assert.AreEqual(NewValue, notifyingProperty.Value);
            Assert.AreEqual(NewValue.ToString(), notifyingProperty.ToString());
            Assert.AreEqual(NewValue, (int)notifyingProperty);
        }

        [Test]
        public void CreateNotifyPropertyChangedProperty_NotifiesPropertyOnInnerPropertyNotify()
        {
            const string PropName = "Prop";

            var dummyProp = new DummyProp();
            var changedPropertyName = string.Empty;
            var changedPropertyCallCount = 0;
            var notifyingProperty = new NotifyingWritableProperty<DummyProp>(PropName,
                changedAction: prop =>
                {
                    changedPropertyName = prop.Name;
                    changedPropertyCallCount++;
                },
                initialValue: dummyProp);

            dummyProp.Notify("test");
            notifyingProperty.Value = null;
            dummyProp.Notify("test");

            Assert.AreEqual(PropName, changedPropertyName);
            Assert.AreEqual(2, changedPropertyCallCount);
        }

        [Test]
        public void CreateObservableCollectionProperty_NotifiesPropertyOnObservableCollectionNotify()
        {
            const string PropName = "Prop";

            var dummyCollection = new ObservableCollection<int>();
            var changedPropertyName = string.Empty;
            var changedPropertyCallCount = 0;
            var notifyingProperty = new NotifyingWritableProperty<ObservableCollection<int>>(PropName,
                changedAction: prop =>
                {
                    changedPropertyName = prop.Name;
                    changedPropertyCallCount++;
                },
                initialValue: dummyCollection);

            dummyCollection.Add(1);
            notifyingProperty.Value = null;
            dummyCollection.Add(2);

            Assert.AreEqual(PropName, changedPropertyName);
            Assert.AreEqual(2, changedPropertyCallCount);
        }

        class DummyProp : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public void Notify(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}