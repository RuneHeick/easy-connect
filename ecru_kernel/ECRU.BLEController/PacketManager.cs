using System;
using Microsoft.SPOT;
using System.Collections;
using ECRU.Utilities.LeadFollow;
using ECRU.BLEController.Packets;
using ECRU.Utilities.HelpFunction;

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

        public bool Subscrib(CommandType command,PacketHandler handler)
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

        public bool Unsubscrib(CommandType command, PacketHandler handler)
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
            CommandType command = (CommandType)packet[Def.COMMAND_INDEX];
            for (int i = 0; i < subscribers.Count; i++)
            {
                SupscriptionPair item = (SupscriptionPair)subscribers[i];
                if(item.Command == command && item.handler != null)
                {

                    IPacket evt = parsePacket(packet);
                    if (evt != null)
                        workPool.EnqueueAction(() => item.handler(evt));
                }
            }
        }

        IPacket parsePacket(byte[] packet)
        {
            IPacket ret = null; 
            switch((CommandType)packet[Def.COMMAND_INDEX])
            {
                case CommandType.AddDeviceEvent:
                    {
                        ret = new AddDeviceEvent();
                        ret.Payload = packet.GetPart(3, packet.Length - 3 - 2); 
                    }
                    break;
                case CommandType.DataEvent:
                    {
                        ret = new DataEvent();
                        ret.Payload = packet.GetPart(3, packet.Length - 3 - 2);
                    }
                    break;
                case CommandType.DeviceEvent:
                    {
                        ret = new DeviceEvent();
                        ret.Payload = packet.GetPart(3, packet.Length - 3 - 2);
                    }
                    break;
                case CommandType.DisconnectEvent:
                    {
                        ret = new DisconnectEvent();
                        ret.Payload = packet.GetPart(3, packet.Length - 3 - 2);
                    }
                    break;
                case CommandType.ReadEvent:
                    {
                        ret = new ReadEvent();
                        ret.Payload = packet.GetPart(3, packet.Length - 3 - 2);
                    }
                    break;
                case CommandType.Reset:
                    {
                        ret = new ResetEvent();
                        ret.Payload = packet.GetPart(3, packet.Length - 3 - 2);
                    }
                    break;
                case CommandType.ServiceEvent:
                    {
                        ret = new ServiceEvent();
                        ret.Payload = packet.GetPart(3, packet.Length - 3 - 2);
                    }
                    break;
                case CommandType.SystemInfo:
                    {
                        //ret = new AddDeviceEvent();
                        //ret.Payload = packet;
                    }
                    break;
                case CommandType.WriteEvent:
                    {
                        ret = new WriteEvent();
                        ret.Payload = packet.GetPart(3, packet.Length - 3 - 2);
                    }
                    break;
                default:
                    ret = null;
                    break;

            }
            return ret; 
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

        public void Send(IPacket packet)
        {
            byte[] data = new byte[packet.Payload.Length+5];
            if (data.Length <= Def.MAX_PACKETSIZE)
            {
                data.Set(data, 3);
                data[0] = Def.SYNC_WORD;
                data[1] = (byte)(data.Length - 1);
                data[2] = (byte)packet.Command;
                ushort crc = CRC.CalcCrc(data, data.Length - 2);

                data[data.Length - 2] = (byte)(crc >> 8);
                data[data.Length - 1] = (byte)(crc);
                seriel.SendByte(data);
            }
        }

        private class SupscriptionPair
        {
            public SupscriptionPair(CommandType cmd ,PacketHandler handle )
            {
                Command = cmd;
                handler = handle; 
            }

            public CommandType Command {get; set;}
            public PacketHandler handler { get; set; }
        }

    }

    delegate void PacketHandler(IPacket packet);


}
