using System;
using Microsoft.SPOT;

namespace ECRU.Utilities.EventBus.Events
{
    public class SendBroadcastMessage
    {
        public byte[] BroadcastType { get; set; }
        public byte[] Message { get; set; }
    }
}
