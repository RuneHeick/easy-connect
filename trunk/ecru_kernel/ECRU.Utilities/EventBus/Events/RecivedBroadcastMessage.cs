using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Messaging;

namespace ECRU.Utilities.EventBus.Events
{
    public class RecivedBroadcastMessage
    {
        public byte[] Sender { get; set; }
        public byte[] Message { get; set; }
        public byte[] MessageType { get; set; }
    }
}
