using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation; 

namespace Brodcaster_Test
{
    public class ECRUPetent
    {
        const int ECM_MAX = 10; 
        private UdpClient client;
        private Timer timer;
        private Timer ECMmtimer;

        private List<byte[]> ECMS = new List<byte[]>();
        private List<byte[]> AddedECMS = new List<byte[]>();
        IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, 4543);

        public ECRUPetent(UdpClient c)
        {

            client = c;
            
            foreach (NetworkInterface nics in NetworkInterface.GetAllNetworkInterfaces())
            {
                if(nics.Name.ToLower().Contains("wi"))
                {
                    var prope = nics.GetIPProperties();
                    var hep = prope.UnicastAddresses;
                    var o = hep[1];
                    var mask = o.IPv4Mask.ToString();
                    var ipstring = o.Address.ToString();

                    ip = new IPEndPoint(IPAddress.Parse(GetBroadcastAddress(ipstring, mask)),4543);
                    break;
                }
            }

            for (int i = 0; i < ECM_MAX; i++)
            {
                ECMS.Add(GetRandomArray(6));
            }
            timer = new Timer(SendCallback, null, 10000, 30000);
            ECMmtimer = new Timer(ECMCallback, null, 30000, 10000);
        }

        public static string GetBroadcastAddress(string ipAddress, string subnetMask)
        {
            //determines a broadcast address from an ip and subnet
            IPAddress ip = IPAddress.Parse(ipAddress);
            IPAddress mask = IPAddress.Parse(subnetMask);

            byte[] ipAdressBytes = ip.GetAddressBytes();
            byte[] subnetMaskBytes = mask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            var broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress).ToString();
        }

        private void ECMCallback(object state)
        {
                int i = ran.Next(0, ECM_MAX);
                byte[] ecm = ECMS[i];
                byte[] packet = new byte[13];
                Array.Copy(Mac, 0, packet, 1, 6);
                Array.Copy(ecm, 0, packet, 7, 6);
                if(AddedECMS.Contains(ecm,new ByteComparer()))
                {
                    ecm = AddedECMS.FirstOrDefault((o) => ArraysEqual(o, ecm));
                    AddedECMS.Remove(ecm);
                    packet[0] = 4; 
                }
                else
                {
                    AddedECMS.Add(ecm);
                    packet[0] = 3;
                }
                client.Send(packet, packet.Length, ip); 
        }

        private void SendCallback(object state)
        {
            byte[] packet = new byte[1+Mac.Length+NetState.Length];
            packet[0] = 1; 
            Array.Copy(Mac,0,packet,1,6); 
            Array.Copy(NetState,0,packet,7,NetState.Length);

            client.Send(packet, packet.Length, ip); 
        }

        private byte[] mac;
        public byte[] Mac
        {
            get
            {
                if (mac == null)
                    mac = GetRandomArray(6); 
                return mac;
            }
            set
            {
                mac = value;
            }
        }

        public byte[] NetState
        {
            get
            {
                return GetRandomArray(32);
            }
        }

        static Random ran = new Random((int)DateTime.Now.Ticks);
        private static byte[] GetRandomArray(int size)
        {
            byte[] array = new byte[size];  
            ran.NextBytes(array); 
            return array; 
        }

        class ByteComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] x, byte[] y)
            {
                return ArraysEqual(x, y);
            }
            public int GetHashCode(byte[] codeh)
            {
                return codeh.GetHashCode();
            }
        }

        public static bool ArraysEqual(byte[] a1, byte[] a2)
        {
            if (a1 == a2)
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                    return false;
            }
            return true;
        }
    }
}
