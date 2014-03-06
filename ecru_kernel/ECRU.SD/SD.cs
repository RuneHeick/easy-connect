using System;
using System.IO;
using ECRU.EventBus.Messages;
using ECRU.SD.Handlers;
using Microsoft.SPOT;

namespace ECRU.SD
{
    public class SD
    {
        //Subscribe handlers

        private GetNetworkProfilMessageHandler handler = new GetNetworkProfilMessageHandler();

        private GetNetworkProfilMessage message = new GetNetworkProfilMessage(Path.DirectorySeparatorChar + "SD" + Path.DirectorySeparatorChar + "Config" + Path.DirectorySeparatorChar + "netd.txt");

        public SD()
        {
            EventBus.EventBus.Subscribe(message, handler);
        }

    }
}
