using System;
using ECRU.EventBus;
using ECRU.EventBus.Messages;
using Microsoft.SPOT;

namespace ECRU.netd.Handlers
{
    class NetworkProfileConfigMessageHandler : TMessageHandler
    {
        public void Handle(TMessage message)
        {
            var _message = message as NetworkProfileConfigMessage;
            
            Debug.Print(_message.ECRUName);
            Debug.Print(_message.ECRUNetworkName);
            Debug.Print(_message.ECRUNetworkPassword);
            Debug.Print(_message.WiFiPassword);
            Debug.Print(_message.WiFiSSID);
            Debug.Print(_message.Type);
        }
    }
}
