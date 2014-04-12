using System;
using Microsoft.SPOT;
using ECRU.Utilities.HelpFunction;

namespace ECRU.BLEController.Packets
{
    class ReadEvent:IPacket
    {
        private byte[] payload = new byte[8]; 

        public UInt16 handel
        {
            get
            {
                return (UInt16)((payload[6]) + (payload[7] << 8));
            }
        }

        public byte[] Address
        {
            get
            {
                return Payload.GetPart(0, 6);
            }
        }

        public byte[] Payload
        {
            get
            {
                return payload;
            }
            set
            {
                payload = value;
            }
        }

        public CommandType Command
        {
            get
            {
                return CommandType.ReadEvent;
            }
        }
    }
}
