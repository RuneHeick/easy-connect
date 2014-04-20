using System;
using System.Collections;
using System.Net;
using ECRU.Utilities;
using ECRU.Utilities.HelpFunction;
using Microsoft.SPOT;

namespace ECRU.netd
{

    public delegate void NetworkStateChange(UInt64 netstate);

    public static class NetworkTable
    {
        public static event NetworkStateChange NetstateChanged;
   
        private static readonly Hashtable Neighbours = new Hashtable();
        private static UInt64 _netstate = 0;
        static readonly object Lock = new object(); 

        private static string localMac;

        private static event NetworkTableChange UpdatedUnit;
        private static event NetworkTableChange RemovedUnit;
        private delegate void NetworkTableChange(Neighbour neighbour);


        static NetworkTable()
        {
            localMac = "B3E795DE1C11";
            UpdatedUnit += (o => NetworkChanged(o, UpdatedUnit) );
            RemovedUnit += (o=> NetworkChanged(o, RemovedUnit) );
        }

        private static void NetworkChanged(Neighbour neighbour, NetworkTableChange call)
        {
            UpdateNetstate();

            // update MacHierachy / MacList
            Debug.Print("NetworkChanged: " + call.GetType());
        }

        private static void Add(Neighbour neighbour)
        {
            //Add unit
            Neighbours[neighbour.Mac] = neighbour;

            //Call event
            UpdatedUnit(neighbour);
        }

        private static void Remove(Neighbour neighbour)
        {
            //Remove unit
            Neighbours.Remove(neighbour.Mac);

            //Call Event
            RemovedUnit(neighbour);
        }

        public static void UpdateNetworkTableEntry(IPAddress ipAddress, string mac, string netstate)
        {
            var neighbour = Neighbours[mac] as Neighbour ?? new Neighbour(mac);

            neighbour.IP = ipAddress;
            neighbour.Netstate = netstate;
            neighbour.Lastseen = DateTime.Now;

            Add(neighbour);
        }

        public static IPAddress GetAddress(string mac)
        {
            var neighbourIP = ((Neighbour) Neighbours[mac]).IP;

            //check if address is valid
            if (ValidAddress(mac))
            {
                return neighbourIP;
            }
            else
            {
                throw new IPAddressNotValidException();
            }
        }

        public static bool ValidAddress(string mac)
        {
            var lastSeen = ((Neighbour) Neighbours[mac]).Lastseen;

            TimeSpan timeDifference = DateTime.Now - lastSeen;

            return timeDifference.Minutes <= 5;
        }

        public static void UpdateNetstate()
        {
            var data = localMac;
            foreach (var key in Neighbours.Keys)
            {
                data += (string) key;
            }

            _netstate = Knuthhash.doHash(data);
            Debug.Print("Netstate: " + _netstate);

            NetstateChanged(_netstate);
        }

        public static UInt64 GetNetstate()
        {
            if(_netstate == 0)
            {
                UpdateNetstate();
            }

            return _netstate;
        }

    }

    public class IPAddressNotValidException : Exception
    {
    }


    internal class Neighbour
    {

        public Neighbour(string Mac)
        {
            _mac = Mac;
        }

        public String Mac { get { return _mac; } }
        public DateTime Lastseen;
        public String Netstate;
        private string _mac;
        public IPAddress IP { get; set; }
    }


}