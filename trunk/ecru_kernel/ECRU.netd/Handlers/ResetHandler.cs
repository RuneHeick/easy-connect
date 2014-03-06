using System;
using ECRU.EventBus;
using ECRU.GlobalMessages;
using Microsoft.SPOT;

namespace ECRU.netd.Handlers
{
    class ResetHandler : TMessageHandler
    {
        public void Handle(TMessage message)
        {
            Reset _message = message as Reset;
            if (_message == null) throw new ArgumentNullException("_message");
        }
    }
}
