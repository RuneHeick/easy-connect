namespace ECRU.EventBus.Test
{
    public class MockSubscriber : ISubscriber
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