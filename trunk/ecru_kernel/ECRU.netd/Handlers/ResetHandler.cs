using System;
using ECRU.EventBus;
using ECRU.EventBus.Messages;
using Microsoft.SPOT;

namespace ECRU.netd.Handlers
{
    class ResetHandler : TMessageHandler
    {
        Reset _message;
        public void Handle(TMessage message)
        {
            _message = message as Reset;
            if (_message == null) throw new ArgumentNullException("_message");
        }
    }
}
