using ECRU.EventBus;

namespace ECRU.EventBus.Messages
{
    public class Reset : TMessage
    {
        private const string _type = "ECRU.GlobalMessages.Reset";

        public string Type
        {
            get { return _type; }
        }
    }
}
