using System;
using System.Collections;
using System.Net;
using ECRU.Utilities;
using ECRU.Utilities.HelpFunction;
using Microsoft.SPOT;
using ECRU.Utilities.EventBus;

namespace ECRU.netd
{

    public delegate void NetworkStateChange(String netstate);

    public static class NetworkTable
    {
        public static event NetworkStateChange NetstateChanged;
   
        private static readonly Hashtable Neighbours = new Hashtable();
        private static string[] netstateIPList;

        private static String _netstate;
        private static bool _isInSync;

        static readonly object Lock = new object(); 

        private static event NetworkTableChange UpdatedUnit;
        private static event NetworkTableChange RemovedUnit;
        private delegate void NetworkTableChange(Neighbour neighbour);

        
        static NetworkTable()
        {
            UpdatedUnit += (NeighbourAdded);
            RemovedUnit += (NeighbourRemoved);
        }

        public static string SetLocalIP { get; set; }

        private static void NeighbourAdded(Neighbour neighbour)
        {
            UpdateNetstate();

            // update MacHierachy / MacList
            SystemInfo.ConnectionOverview.Add(neighbour.Mac.FromHex());

            // network status -> eventbus
            EventBus.Publish(new NetworkStatusMessage{isinsync = _isInSync, NetState = _netstate});

        }

        private static void NeighbourRemoved(Neighbour neighbour)
        {
            UpdateNetstate();

            // update MacHierachy / MacList
            SystemInfo.ConnectionOverview.Remove(neighbour.Mac.FromHex());

            // network status -> eventbus
            EventBus.Publish(new NetworkStatusMessage{isinsync = _isInSync, NetState = _netstate});
        }

        private static void Add(Neighbour neighbour)
        {
            if (!Neighbours.Contains(neighbour.Mac))
            {
                //Add unit
                Neighbours[neighbour.Mac] = neighbour;

                //Update netstate ip list
                netstateIPList = netstateIPList.Add(neighbour.IP.ToString());

                //Call event
                UpdatedUnit(neighbour);
            }
            else
            {
                var neb = Neighbours[neighbour.Mac] as Neighbour;

                //if ip not identical - device has received new IP
                if (neb == null || neb.IP.ToString() == neighbour.IP.ToString()) return;
                
                //overwrite unit
                Neighbours[neighbour.Mac] = neighbour;

                //Call event
                UpdatedUnit(neighbour);
            }
        }

        private static void Remove(Neighbour neighbour)
        {
            //Remove unit
            Neighbours.Remove(neighbour.Mac);

            //Update netstate ip list
            netstateIPList = netstateIPList.Remove(neighbour.IP.ToString());

            //Call Event
            RemovedUnit(neighbour);
        }

        public static void UpdateNetworkTableEntry(IPAddress ipAddress, string mac, string netstate)
        {
            if (netstateIPList == null)
            {
                netstateIPList = new []{SetLocalIP};
            }

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

        public static void CheckTable()
        {
            if (Neighbours.Count < 1) return;

            foreach (Neighbour neighbour in Neighbours.Values)
            {
                if (!ValidAddress(neighbour.Mac))
                {
                    Remove(neighbour);
                }
            }
        }

        public static bool ValidAddress(string mac)
        {
            var lastSeen = ((Neighbour) Neighbours[mac]).Lastseen;

            var timeDifference = DateTime.Now - lastSeen;

            return timeDifference.Ticks <= (TimeSpan.TicksPerMinute*3);
        }


        private static void UpdateNetstate()
        {
            string data = null;

            netstateIPList = netstateIPList.Quicksort(0, (netstateIPList.Length - 1) );

            foreach (var s in netstateIPList)
            {
                data += s;
            }
            
            var buffer = data.StringToBytes();

            var md5State = new MD5();

            md5State.HashCore(buffer, 0, buffer.Length);

            _netstate = md5State.HexStr();

            Debug.Print("Netstate: " + _netstate);

            _isInSync = true;

            foreach (Neighbour neighbour in Neighbours.Values)
            {
                if (neighbour.Netstate == _netstate) continue;
                _isInSync = false;
            }

            NetstateChanged(_netstate);
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

        public string Mac { get { return _mac; } }

        public DateTime Lastseen;
        public String Netstate;

        private string _mac;

        public IPAddress IP { get; set; }

    }


}