using System;
using Microsoft.SPOT;
using ECRU.Utilities.HelpFunction;

namespace ECRU.BLEController.Packets
{
    class DiscoverEvent:IPacket
    {
        public enum DiscoverType
        {
            Service = 0x00,
            Characteristic = 0x01,
            Descriptors = 0x02
        }

        byte[] payload = new byte[11];

        public DiscoverType Type
        {
            set
            {
                Payload[6] =  (byte)value;
            }
        }

        public byte[] Address
        {
            set
            {
                payload.Set(value, 0);
            }
        }

        public UInt16 StartHandle
        {
            set
            {
                payload[7] = (byte)(value >> 8);
                payload[8] = (byte)(value);
            }
        }
        public UInt16 EndHandle
        {
            set
            {
                payload[9] = (byte)(value >> 8);
                payload[10] = (byte)(value);
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
