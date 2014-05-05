using System;
using ECRU.Utilities.HelpFunction;

namespace ECRU.BLEController.Packets
{
    internal class ReadEvent : IPacket
    {
        private byte[] payload = new byte[8];

        public UInt16 handel
        {
            set
            {
                payload[6] = (byte) (value >> 8);
                payload[7] = (byte) (value);
            }
        }

        public byte[] Address
        {
            set { Payload.Set(value, 0); }
        }

        public byte[] Payload
        {
            get { return payload; }
            set { payload = value; }
        }

        public CommandType Command
        {
            get { return CommandType.ReadEvent; }
        }
    }
}