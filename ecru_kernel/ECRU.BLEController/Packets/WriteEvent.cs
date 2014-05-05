using System;
using ECRU.Utilities.HelpFunction;

namespace ECRU.BLEController.Packets
{
    internal class WriteEvent : IPacket
    {
        private byte[] payload = new byte[8];

        public byte[] Address
        {
            set { Payload.Set(value, 0); }
        }

        public UInt16 Handle
        {
            set
            {
                payload[6] = (byte) (value >> 8);
                payload[7] = (byte) (value);
            }
        }

        public byte[] Value
        {
            set
            {
                int len = value.Length;
                var ret = new byte[8 + len];
                ret.Set(value, 8);
                for (int i = 0; i < 8; i++)
                {
                    ret[i] = Payload[i];
                }
                Payload = ret;
            }
        }

        public byte[] Payload
        {
            get { return payload; }
            set { payload = value; }
        }

        public CommandType Command
        {
            get { return CommandType.WriteEvent; }
        }
    }
}