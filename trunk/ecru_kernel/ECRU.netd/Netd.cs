using System;
using ECRU.EventBus.Messages;
using ECRU.netd.Handlers;
using Microsoft.SPOT;

namespace ECRU.netd
{
    public class Netd
    {
        public Netd()
        {
            EventBus.EventBus.Subscribe(new NetworkProfileConfigMessage(), new NetworkProfileConfigMessageHandler());
        }

        //setup subscriptions
        //setup ethernet after config
        //Network Discovery
        //Send network packets
        //get direct packets
        //get broadcast packets
        //
    }
}
