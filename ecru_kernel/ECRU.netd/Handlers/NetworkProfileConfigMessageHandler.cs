using System;
using ECRU.EventBus;
using ECRU.netd.Messages;
using Microsoft.SPOT;

namespace ECRU.netd.Handlers
{
    class NetworkProfileConfigMessageHandler : TMessageHandler
    {
        public void Handle(TMessage message)
        {
            var _message = message as NetworkProfileConfigMessage;

            //HANDLE IT!
        }
    }
}
