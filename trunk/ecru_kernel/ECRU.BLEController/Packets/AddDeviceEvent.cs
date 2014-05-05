using System;
using ECRU.Utilities.HelpFunction;

namespace ECRU.BLEController.Packets
{
    internal class AddDeviceEvent : IPacket
    {
        private byte[] address = new byte[10];

        public byte[] Address
        {
            set { address.Set(value, 0); }
        }

        public UInt16 PassCodeHandle
        {
            set
            {
                Payload[6] = (byte) (value >> 8);
                Payload[7] = (byte) (value);
            }
        }

        public UInt16 ConnectionTimeHandle
        {
            set
            {
                Payload[8] = (byte) (value >> 8);
                Payload[9] = (byte) (value);
            }
        }


        public byte[] Payload
        {
            get { return address; }
            set { address = value; }
        }

        public CommandType Command
        {
            get { return CommandType.AddDeviceEvent; }
        }
    }
}