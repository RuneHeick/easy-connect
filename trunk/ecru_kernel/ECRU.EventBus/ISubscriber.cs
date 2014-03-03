namespace ECRU.EventBus
{
    public interface ISubscriber
    {
        TMessage Message { get; }
        TMessageHandler FunctionPointer { get; }
    }
}