using System;
using NUnit.Framework;
using NUnit;

namespace ECRU.EventBus.Test
{
    [TestFixture]
    public class EventBusUnitTest
    {
        [Test]
        public void Senario_Input_ExResult()
        {
            //arrange

            //act

            //assert
        }

        [Test]
        public void AddSubscriber_NoMessageToMessageBus_ThrowArgumentNullException()
        {
            var sub = new MockSubscriber();

            try
            {
                EventBus.Subscribe(sub);
            }
            catch (ArgumentNullException)
            {
            }
        }

        [Test]
        public void AddSubscriber_NoMessageBusFunctionPointerToMessageBus_ThrowArgumentNullException()
        {
            var sub = new MockSubscriber(new MockMessage("test"), null);

            try
            {
                EventBus.Subscribe(sub);
            }
            catch (ArgumentNullException)
            {
            }
        }

        [Test]
        public void AddSubscriberToMessageBus_ReturnTrue()
        {
            var sub = new MockSubscriber(new MockMessage("test"), new MockMessageBusFunctionPointer());

            Assert.IsTrue(EventBus.Subscribe(sub));
        }

        [Test]
        public void AddSubscribers_SameMessageToMessageBus_ReturnTrue()
        {
            var message = new MockMessage("test");
            var sub1 = new MockSubscriber(message, new MockMessageBusFunctionPointer());
            var sub2 = new MockSubscriber(message, new MockMessageBusFunctionPointer());

            EventBus.Subscribe(sub1);
            Assert.IsTrue(EventBus.Subscribe(sub2));
        }

        [Test]
        public void RemoveNotSubscribedSubscriber_ThrowArgumentException()
        {
            var sub = new MockSubscriber(new MockMessage("test"), new MockMessageBusFunctionPointer());

            try
            {
                EventBus.Unsubscribe(sub);
            }
            catch (ArgumentException)
            {
            }
        }

        [Test]
        public void RemoveSubscriber_NoMessageToMessageBus_ThrowArgumentNullException()
        {
            var sub = new MockSubscriber();

            try
            {
                EventBus.Unsubscribe(sub);
            }
            catch (ArgumentNullException)
            {
            }
        }

        [Test]
        public void RemoveSubscriber_NoMessageBusFunctionPointerToMessageBus_ThrowArgumentNullException()
        {
            var sub = new MockSubscriber(new MockMessage("test"), null);

            try
            {
                EventBus.Unsubscribe(sub);
            }
            catch (ArgumentNullException)
            {
            }
        }

        [Test]
        public void RemoveSubscriber_AddedThenRemovedFromMessageBus_ReturnTrue()
        {
            var sub = new MockSubscriber(new MockMessage("test"), new MockMessageBusFunctionPointer());

            EventBus.Subscribe(sub);

            Assert.IsTrue(EventBus.Unsubscribe(sub));
        }
    }
}