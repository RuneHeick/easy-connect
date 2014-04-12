using System;
using Microsoft.SPOT;
using ECRU.Utilities.HelpFunction;

namespace ECRU.BLEController.Packets
{
    class AddDeviceEvent:IPacket
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
                return CommandType.AddDeviceEvent;
            }
        }
    }
}
