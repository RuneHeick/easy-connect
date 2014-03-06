using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UartTester
{
    public class SerialCommand
    {
        private List<byte> packet = new List<byte>();
        public List<byte> Packet
        {
            get
            {
                return packet; 
            }
            set
            {
                packet = value;
            }
        }

        // start byte 
        public bool IsResponse 
        {
            get
            {
                return ((int)Packet[0] & 0x80) == 0x80; 
            }
        }

        public int Length 
        {
            get
            {
                return (int)Packet[0] & 0x7F;
            }
            set
            {
                Packet[0] = (byte)((Packet[0] & 0x80) | value);
            }
        }

        //comand 
        public int MainCommand
        {
            get
            {
                return ((int)Packet[1] & 0xF0)>>4;
            }
            set
            {
                Packet[1] = (byte)((Packet[1] & 0x0F) | (value << 4 & 0xF0));
            }
        }


        public int SubCommand
        {
            get
            {
                return ((int)Packet[1] & 0x0F);
            }
            set
            {
                Packet[1] = (byte)((Packet[1] & 0xF0) | (value & 0x0F));
            }
        }

        public bool IsCrcOK
        {
            get
            {
                byte[] pack = new byte[packet.Count - 2];
                ushort crc = CalcCrc(pack);
                return (ushort)((Packet[Packet.Count - 2] << 8) + Packet[Packet.Count - 1]) == crc;
            }
        }

        public SerialCommand(List<byte> packet)
        {
            Packet = packet;
            if (Packet.Count < 4)
                throw new InvalidOperationException("to short"); 
        }

        public byte[] Payload
        {
            get
            {
                byte[] retpacket = new byte[Packet.Count-4];
                Array.Copy(Packet.ToArray(), retpacket, Packet.Count - 4);
                return retpacket;
            }
            set
            {
                byte[] startAndCmd = new byte[] { Packet[0], Packet[1] };
                Packet.Clear();
                Packet.AddRange(startAndCmd);
                if(value!=null)
                    Packet.AddRange(value);
                Packet.Add(00);
                Packet.Add(00);
                Update();
            }
        }

        public SerialCommand()
        {
            Packet = new List<byte>();
            Packet.Add(00); //start
            Packet.Add(00); //command
            Packet.Add(00); // crc
            Packet.Add(00); //crc

            Length = Packet.Count - 1;
        }

        public void Update()
        {
            Length = Packet.Count - 1;
            byte[] pack = new byte[packet.Count-2];
            Array.Copy(pack.ToArray(), pack, pack.Length);
            ushort Crc = CalcCrc(pack);
            Packet[Packet.Count - 2] = (byte)(Crc >> 8);
            Packet[Packet.Count - 1] = (byte)(Crc);
        }

        static ushort UpdateCrc(ushort crc, byte b) 
        { 
            crc ^= (ushort)(b << 8); 
            for (int i = 0; i < 8; i++) 
            { 
                if ((crc & 0x8000) > 0)
                    crc = (ushort)((crc << 1) ^ 0x1021);
                else            
                    crc <<= 1;
            } 
            return crc;
        }

        static ushort CalcCrc(byte[] data)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < data.Length; i++)
                crc = UpdateCrc(crc, data[i]);
            return crc; 
        }

    }
}
