using System;
using Microsoft.SPOT;
using System.Collections;
using ECRU.Utilities.LeadFollow;

namespace ECRU.BLEController
{
    class PacketManager
    {
        SerialController seriel;
        ArrayList subscribers = new ArrayList();
        WorkPool workPool = new WorkPool(3); 

        public PacketManager(SerialController seriel)
        {
            seriel.PacketRecived += seriel_PacketRecived;
            this.seriel = seriel; 
        }

        public bool Subscrib(byte command,PacketHandler handler)
        {
            try
            {
                for (int i = 0; i < subscribers.Count; i++)
                {
                    SupscriptionPair item = (SupscriptionPair)subscribers[i];
                    if (item.handler == handler && command == item.Command)
                        return true;
                }
                subscribers.Add(new SupscriptionPair(command, handler));
                return true;
            }
            catch
            {
                return false; 
            }
        }

        public bool Unsubscrib(byte command, PacketHandler handler)
        {
            try
            {
                for (int i = 0; i < subscribers.Count;i++)
                {
                    SupscriptionPair item = (SupscriptionPair)subscribers[i];
                    if (item.handler == handler && command == item.Command)
                    {
                        subscribers.RemoveAt(i);
                        return true; 
                    }
                        
                }
                return false; 
                    
            }
            catch
            {
                return false; 
            }
        }


        void seriel_PacketRecived(byte[] packet)
        {
            ushort crc = CRC.CalcCrc(packet, packet.Length - 2);
            ushort packetcrc = (ushort)((packet[packet.Length - 2] << 8) + packet[packet.Length - 1]);
            if(packetcrc == crc)
            {
                HandelPacket(packet);
                SendAck(packet[Def.COMMAND_INDEX]);
            }
        }


        void HandelPacket(byte[] packet)
        {
            int command = packet[Def.COMMAND_INDEX];
            for (int i = 0; i < subscribers.Count; i++)
            {
                SupscriptionPair item = (SupscriptionPair)subscribers[i];
                if(item.Command == command && item.handler != null)
                {
                    workPool.EnqueueAction(()=> item.handler(parsePacket(packet)));
                }

            }
        }

        IPacket parsePacket(byte[] packet)
        {



            return null; 
        }
        
        public void SendAck(byte Cmd)
        {
            byte[] response = new byte[6];
            response[0] = 0xEC;
            response[1] = 0x85;
            response[2] = Cmd;

            response[3] = 0x01; //Ack

            ushort crc = CRC.CalcCrc(response, response.Length - 2);

            response[4] = (byte)(crc >> 8);
            response[5] = (byte)(crc);

            seriel.SendByte(response);
        }


        private class SupscriptionPair
        {
            public SupscriptionPair(byte cmd ,PacketHandler handle )
            {
                Command = cmd;
                handler = handle; 
            }

            public byte Command {get; set;}
            public PacketHandler handler { get; set; }
        }

    }

    delegate void PacketHandler(IPacket packet);


}
