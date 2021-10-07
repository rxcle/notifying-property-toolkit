// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using NUnit.Framework;

namespace NotifyingPropertyToolkit.Test
{
    [TestFixture]
    public class NotifyingCollectionPropertyTest
    {
        [Test]
        public void CreatingStringNotifyingCollectionPropertyWithInitialValues_ValueIsCorrectAndNoNotificationSent()
        {
            const string PropName = "Prop";
            string[] initialValue = new[] { "One", "Two", "Three" };

            var notificationSent = false;
            var notifyingProperty = new NotifyingCollectionProperty<string>(PropName,
                initialValues: initialValue,
                changedAction: _ => notificationSent = true);

            CollectionAssert.AreEqual(initialValue, notifyingProperty.Value);
            Assert.IsFalse(notifyingProperty.IsPrivate);
            Assert.IsFalse(notificationSent);
        }
    }
}
