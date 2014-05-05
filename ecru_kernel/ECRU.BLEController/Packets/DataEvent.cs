using System;
using ECRU.Utilities.HelpFunction;

namespace ECRU.BLEController.Packets
{
    internal class DataEvent : IPacket
    {
        private byte[] payload = new byte[9];

        public byte[] Address
        {
            get { return Payload.GetPart(0, 6); }
        }

        public byte[] Value
        {
            get { return Payload.GetPart(8, Payload.Length - 8); }
        }

        public UInt16 Handel
        {
            get
            {
                byte[] a = Payload.GetPart(6, 2);
                return (UInt16) ((a[0] << 8) + (a[1]));
            }
        }

        public byte[] Payload
        {
            get { return payload; }
            set { payload = value; }
        }

        public CommandType Command
        {
            get { return CommandType.DataEvent; }
        }
    }
}