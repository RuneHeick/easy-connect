namespace ECRU.EventBus
{
    public interface TMessageHandler
    {
        void Handle(TMessage message);
    }
}