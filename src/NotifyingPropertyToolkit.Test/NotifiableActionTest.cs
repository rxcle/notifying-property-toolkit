// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using NUnit.Framework;

namespace NotifyingPropertyToolkit.Test
{
    [TestFixture]
    public class NotifiableActionTest
    {
        [Test]
        public void CreatingNotifiableActionAndNotify_ActionIsPerformed()
        {
            var isActionPerformed = false;
            var notifyingProperty = new NotifiableAction(() => isActionPerformed = true);

            notifyingProperty.Reevaluate();

            Assert.IsTrue(isActionPerformed);
        }
    }
}
