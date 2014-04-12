using System;
using Microsoft.SPOT;

namespace ECRU.BLEController.Packets
{
    class DisconnectEvent: IPacket 
    {
        
        private byte[] address = new byte[6];

        public byte[] Address
        {
            set
            {
                address = value;
            }
        }

        public byte[] Payload
        {
            get
            {
                return address;
            }
            set
            {
                address = value;
            }
        }

        public CommandType Command
        {
            get
            {
                return CommandType.DisconnectEvent;
            }
        }
    }
}
