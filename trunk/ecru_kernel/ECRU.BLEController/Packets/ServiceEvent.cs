using System;
using Microsoft.SPOT;
using ECRU.Utilities.HelpFunction;

namespace ECRU.BLEController.Packets
{
    class ServiceEvent:IPacket
    {

        public byte[] Address
        {
            get
            {
                return Payload.GetPart(0, 6);
            }
        }

        public DiscoverType Type { get; set; }

        public ServiceDirRes[] Services
        {
            get
            {
               if(Payload == null) return null; 
               if(Type == DiscoverType.Service )
               {
                   return ServiceList();
               }
               else
               {
                   return handleUUIDList();
               }
            
            }
        }

        public byte[] Payload { get; set;  }

        public CommandType Command
        {
            get
            {
                return CommandType.ServiceEvent;
            }
        }

        private ServiceDirRes[] ServiceList()
        {
            ServiceDirRes[] reslist = new ServiceDirRes[(Payload.Length-6)/6];
            int index = 0; 
            for(int i = 6;i<Payload.Length;i=i+6)
            {
                PrimaryServicePair pair = new PrimaryServicePair(); 
                pair.handle = (ushort)((Payload[i]<<8) + Payload[i+1]);
                pair.Endhandle = (ushort)((Payload[i + 2] << 8) + Payload[i + 3]);
                pair.UUID = (ushort)((Payload[i + 4] << 8) + Payload[i + 5]);
                reslist[index++] = pair;
            }

            return reslist;
        }

        private ServiceDirRes[] handleUUIDList()
        {
            ServiceDirRes[] reslist = new ServiceDirRes[(Payload.Length - 6) / 4];
            int index = 0;
            for (int i = 6; i < Payload.Length; i = i + 4)
            {
                ServicePair pair = new ServicePair();
                pair.handle = (ushort)((Payload[i] << 8) + Payload[i + 1]);
                pair.UUID = (ushort)((Payload[i + 2] << 8) + Payload[i + 3]);
                reslist[index++] = pair;
            }

            return reslist;
        }
        

    }



    public class ServicePair : ServiceDirRes
    {
        public UInt16 UUID { get; set; }
        public UInt16 handle { get; set; }
    }


    public class PrimaryServicePair : ServiceDirRes
    {
        public UInt16 UUID { get; set; }
        public UInt16 handle { get; set; }
        public UInt16 Endhandle { get; set; }
    }

    public interface ServiceDirRes
    {
        UInt16 UUID { get; set; }

    }

}
