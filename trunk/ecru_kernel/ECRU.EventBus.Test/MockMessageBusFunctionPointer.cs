using System;

namespace ECRU.EventBus.Test
{
    public class MockMessageBusFunctionPointer : TMessageHandler
    {
        public void Handle(TMessage message)
        {
            throw new NotImplementedException();
        }
    }
}