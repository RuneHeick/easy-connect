using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Subscriber;


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
        public void AddSubscriberToMessageBus_ReturnTrue()
        {
            var sub = new Subscriber();
            MessageBus.Subscribe(sub);
        }
    }
}
