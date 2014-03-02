using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBus.Test
{
    class MockSubscriber : ISubscriber
    {
        public MockSubscriber(TMessage message, TMessageHandler functionPointer)
        {
            FunctionPointer = functionPointer;
            Message = message;
        }

        public MockSubscriber()
        {
            
        }

        public TMessage Message { get; private set; }
        public TMessageHandler FunctionPointer { get; private set; }
    }
}
