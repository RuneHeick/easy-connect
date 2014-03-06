using ECRU.EventBus;

namespace ECRU.EventBus.Messages
{
    class SendMessage : TMessage
    {
        private const string _type = "ECRU.Netd.Messages.SendMessage";

        public string Type
        {
            get { return _type; }
        }
    }
}
