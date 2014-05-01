using System;
using System.Net;
using Microsoft.SPOT;
using Microsoft.SPOT.Messaging;

namespace ECRU.Utilities.EventBus.Events
{
    public class RecivedBroadcastMessage
    {
        public IPAddress SenderIPAddress { get; set; }
        public byte[] Message { get; set; }
        public byte[] MessageType { get; set; }
    }
}
