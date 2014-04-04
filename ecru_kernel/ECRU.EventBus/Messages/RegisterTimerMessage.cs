using System;
using ECRU.EventBus;
using Microsoft.SPOT;

namespace EventBus.Messages
{
    class RegisterTimerMessage : TMessage
    {
        public RegisterTimerMessage(uint milliseconds, string eventType)
        {
            EventType = eventType;
            Milliseconds = milliseconds;
        }

        private const string _type = "ECRU.Timer.RegisterTimer";

        public string Type
        {
            get { return _type; }
        }

        public uint Milliseconds { get; private set; }

        public string EventType { get; private set; }

    }
}
