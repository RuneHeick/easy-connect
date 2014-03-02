using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Subscriber;

namespace MessageBus
{
    public interface IMessageBus
    {

        bool Subscribe(ISubscriber subscriber);
        bool Unsubscribe(ISubscriber subscriber);
        bool Publish(IMessage message);
    }
}
