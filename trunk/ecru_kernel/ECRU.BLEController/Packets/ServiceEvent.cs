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
               else if(Type == DiscoverType.Descriptors)
               {
                   return handleUUIDList();
               }
               else
               {
                   return CharacteristicPairList();
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
                pair.handle = (ushort)((Payload[i+1]<<8) + Payload[i]);
                pair.Endhandle = (ushort)((Payload[i + 3] << 8) + Payload[i + 2]);
                pair.UUID = (ushort)((Payload[i + 5] << 8) + Payload[i + 4]);
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
                DescriptorPair pair = new DescriptorPair();
                pair.handle = (ushort)((Payload[i] << 8) + Payload[i+1]);
                pair.UUID = (ushort)((Payload[i + 3] << 8) + Payload[i + 2]);
                reslist[index++] = pair;
            }

            return reslist;
        }

        private ServiceDirRes[] CharacteristicPairList()
        {
            ServiceDirRes[] reslist = new ServiceDirRes[(Payload.Length - 6) / 5];
            int index = 0;
            for (int i = 6; i < Payload.Length; i = i + 5)
            {
                CharacteristicPair pair = new CharacteristicPair();
                pair.ReadWriteProp = Payload[i];
                pair.handle = (ushort)((Payload[i+2] << 8) + Payload[i + 1]);
                pair.UUID = (ushort)((Payload[i + 4] << 8) + Payload[i + 3]);
                reslist[index++] = pair;
            }

            return reslist;
        }
        

    }



    public class DescriptorPair : ServiceDirRes
    {
        public UInt16 UUID { get; set; }
        public UInt16 handle { get; set; }
    }

    public class CharacteristicPair: DescriptorPair
    {
        public byte ReadWriteProp { get; set;  }
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
        UInt16 handle { get; set; }


    }

}
