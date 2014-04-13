using System;
using Microsoft.SPOT;
using ECRU.Utilities.HelpFunction;

namespace ECRU.BLEController.Packets
{
    class ResetEvent:IPacket
    {
        private byte[] payload;

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
                return CommandType.Reset;
            }
        }
    }
}
