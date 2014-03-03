namespace ECRU.EventBus.Test
{
    public class MockMessage : TMessage
    {
        public MockMessage(string type)
        {
            Type = type;
        }

        public string Type { get; private set; }
    }
}