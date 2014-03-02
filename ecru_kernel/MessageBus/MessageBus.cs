using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Subscriber;

namespace MessageBus
{
    public class MessageBus : IMessageBus
    {
        

        public bool Subscribe(ISubscriber subscriber)
        {
            throw new NotImplementedException();
        }

        public bool Unsubscribe(ISubscriber subscriber)
        {
            throw new NotImplementedException();
        }

        public bool Publish(IMessage message)
        {
            throw new NotImplementedException();
        }

        
    }
}
