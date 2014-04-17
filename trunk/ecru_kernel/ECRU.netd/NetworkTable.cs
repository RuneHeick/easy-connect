using System;
using System.Collections;
using System.Net;
using ECRU.Utilities;

namespace ECRU.netd
{
    public static class NetworkTable
    {
        private static readonly Hashtable Neighbours = new Hashtable();
        private static int Netstate = 0;


        public static void Update(IPAddress ipAddress, byte[] mac, byte[] netstate)
        {
            Neighbour neighbour = Neighbours[mac] as Neighbour ?? new Neighbour();

            neighbour.IP = ipAddress;
            neighbour.Netstate = netstate;
            neighbour.Lastseen = DateTime.Now;

            Neighbours[mac] = neighbour;
            UpdateNetstate();
        }

        public static void Remove(byte[] mac)
        {
            Neighbours.Remove(mac);
            UpdateNetstate();
        }


        public static bool ValidAddress(IPAddress ipAddress)
        {
            object lastSeen = Neighbours[ipAddress.ToString()];

            TimeSpan timeDifference = DateTime.Now - (DateTime) lastSeen;

            return timeDifference.Minutes <= 5;
        }

        public static int UpdateNetstate()
        {
            string data = SystemInfo.SystemMAC.GetString();
            foreach (object key in Neighbours.Keys)
            {
                data += ((byte[]) key).GetString();
            }

            return data.GetHashCode();
        }

        public static int GetNetstate()
        {
            return Netstate;
        }
    }

    internal class Neighbour
    {
        public DateTime Lastseen;
        public byte[] Netstate;
        public IPAddress IP { get; set; }
    }
}