using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using MessageBus;


namespace MessageBus.Test
{
    [TestFixture]
    public class MessageBusUnitTest
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
                MessageBus.Subscribe(sub);
            }
            catch (ArgumentNullException)
            {

            }
        }

        [Test]
        public void AddSubscriber_NoMessageBusFunctionPointerToMessageBus_ThrowArgumentNullException()
        {
            var sub = new MockSubscriber(new MockMessage(), null);

            try
            {
                MessageBus.Subscribe(sub);
            }
            catch (ArgumentNullException)
            {

            }
        }

        [Test]
        public void AddSubscriberToMessageBus_ReturnTrue()
        {
            var sub = new MockSubscriber(new MockMessage(), new MockMessageBusFunctionPointer());

            Assert.IsTrue(MessageBus.Subscribe(sub));

        }

        [Test]
        public void AddSubscribers_SameMessageToMessageBus_ReturnTrue()
        {
            var message = new MockMessage();
            var sub1 = new MockSubscriber(message, new MockMessageBusFunctionPointer());
            var sub2 = new MockSubscriber(message, new MockMessageBusFunctionPointer());

            MessageBus.Subscribe(sub1);
            Assert.IsTrue(MessageBus.Subscribe(sub2));

        }

        [Test]
        public void RemoveNotSubscribedSubscriber_ThrowArgumentException()
        {
            var sub = new MockSubscriber(new MockMessage(), new MockMessageBusFunctionPointer());

            try
            {
                MessageBus.Unsubscribe(sub);
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
                MessageBus.Unsubscribe(sub);
            }
            catch (ArgumentNullException)
            {

            }
        }

        [Test]
        public void RemoveSubscriber_NoMessageBusFunctionPointerToMessageBus_ThrowArgumentNullException()
        {
            var sub = new MockSubscriber(new MockMessage(), null);

            try
            {
                MessageBus.Unsubscribe(sub);
            }
            catch (ArgumentNullException)
            {

            }
        }

        [Test]
        public void RemoveSubscriber_AddedThenRemovedFromMessageBus_ReturnTrue()
        {
            var sub = new MockSubscriber(new MockMessage(), new MockMessageBusFunctionPointer());

            MessageBus.Subscribe(sub);

            Assert.IsTrue(MessageBus.Unsubscribe(sub));
        }
    }
}
