using System;
using Microsoft.SPOT;
using ECRU.Utilities.HelpFunction;
using ECRU.Utilities;

namespace ECRU.BLEController.Packets
{
    class SystemInfoEvent:IPacket
    {

        private byte[] payload = new byte[22]; 

        public byte[] SystemID
        {
            set
            {
                if(value.Length == SystemInfo.SYSID_LENGTH)
                    payload.Set(value, 0); 
            }
        }

        public string Name
        {
            set
            {
                byte[] name = value.StringToBytes();
                byte[] newpayload = new byte[name.Length + SystemInfo.SYSID_LENGTH+1];
                newpayload.Set(Payload.GetPart(0, SystemInfo.SYSID_LENGTH + 1), 0);
                newpayload.Set(name, SystemInfo.SYSID_LENGTH + 1);
                Payload = newpayload;
            }
        }

        public bool InitMode
        {
            set
            {
                if(value == true)
                {
                    payload[20] = 0x00;
                }
                else
                {
                    payload[20] = 0x01;
                }
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
                return CommandType.SystemInfo;
            }
        }
    }
}
