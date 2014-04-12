using System;
using Microsoft.SPOT;

namespace ECRU.BLEController.Packets
{
    class ServiceEvent:IPacket
    {

        public ServicePair Services
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public byte[] Payload
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public CommandType Command
        {
            get
            {
                return CommandType.ServiceEvent;
            }
        }
        
        public class ServicePair 
        {
            public UInt16 UUID {get; set;}
            public UInt16 handle {get; set;}
        }

    }
}
