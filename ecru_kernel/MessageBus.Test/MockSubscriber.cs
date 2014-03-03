using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MessageBus.Test
{
    public class MockSubscriber : ISubscriber
    {
        private MockMessage mockMessage;
        private MockMessageBusFunctionPointer mockMessageBusFunctionPointer;

        public MockSubscriber(TMessage message, TMessageHandler functionPointer)
        {
            FunctionPointer = functionPointer;
            Message = message;
        }

        public MockSubscriber()
        {
            
        }

        public MockSubscriber(MockMessage mockMessage, MockMessageBusFunctionPointer mockMessageBusFunctionPointer)
        {
            // TODO: Complete member initialization
            this.mockMessage = mockMessage;
            this.mockMessageBusFunctionPointer = mockMessageBusFunctionPointer;
        }

        public TMessage Message { get; private set; }
        public TMessageHandler FunctionPointer { get; private set; }
    }
}
