using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using ECRU.Utilities;
using ECRU.Utilities.EventBus.Events;
using ECRU.Utilities.HelpFunction;
using Json.NETMF;
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

        private static object Lock = new object();
        private static object _lockNetstate = new object();

        private static event NetworkTableChange AddUnit;
        private static event NetworkTableChange RemovedUnit;
        private delegate void NetworkTableChange(Neighbour neighbour);

        
        static NetworkTable()
        {
            _isInSync = true;
            AddUnit += (NeighbourAdded);
            RemovedUnit += (NeighbourRemoved);
        }

        public static string SetLocalIP { get; set; }

        private static void NeighbourAdded(Neighbour neighbour)
        {
            UpdateNetstate();

            // update request devices from roomunit
            EventBus.Publish(new NewConnectionMessage { ConnectionCallback = RequestedDevices, ConnectionType = "RequestDevices", Receiver = neighbour.Mac.FromHex() });
        }

        private static void NeighbourRemoved(Neighbour neighbour)
        {
            UpdateNetstate();

            // update MacHierachy / MacList
            SystemInfo.ConnectionOverview.Remove(neighbour.Mac.FromHex());

            // network status -> eventbus
            networkStatus();
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
                AddUnit(neighbour);
            }
            else
            {
                var neb = Neighbours[neighbour.Mac] as Neighbour;

                if (neb == null) return;
                
                //overwrite unit
                Neighbours.Remove(neb.Mac);
                Neighbours[neighbour.Mac] = neighbour;

                if (!Equals(neb.IP, neighbour.IP))
                {
                    //if ip not identical - device ip has to be added to the table
                    netstateIPList = netstateIPList.Remove(neb.IP.ToString());
                    netstateIPList = netstateIPList.Add(neighbour.IP.ToString());

                    //Update netstate to reflect changes
                    UpdateNetstate();
                    networkStatus();
                }
                else if (neb.Netstate != neighbour.Netstate)
                {
                    networkStatus();
                }
                
            }
        }

        private static void networkStatus()
        {
            lock (Lock)
            {
                var tmpNetworkStatus = _isInSync;

                foreach (Neighbour neighbour in Neighbours.Values)
                {
                    if (neighbour.Netstate == _netstate) continue;
                    _isInSync = false;
                }
                if (tmpNetworkStatus != _isInSync)
                {
                    EventBus.Publish(new NetworkStatusMessage { isinsync = _isInSync, NetState = _netstate });
                }
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
            if (mac == SystemInfo.SystemMAC.ToHex())
            {
                return IPAddress.GetDefaultLocalAddress();
            }
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

        public static byte[] GetMac(IPAddress ip)
        {
            var returnval = new byte[6]; 
            foreach (Neighbour neighbour in Neighbours)
            {
                if (neighbour.IP.Equals(ip))
                {
                    returnval = neighbour.Mac.FromHex();
                }
            }

            return returnval;
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
            lock (_lockNetstate)
            {
                string data = null;
                netstateIPList = netstateIPList.Quicksort(0, (netstateIPList.Length - 1));

                foreach (var s in netstateIPList)
                {
                    data += s;
                }

                var buffer = data.StringToBytes();

                var md5State = new MD5();

                md5State.HashCore(buffer, 0, buffer.Length);

                _netstate = md5State.HexStr();
            }

            Debug.Print("Netstate: " + _netstate);

            NetstateChanged(_netstate);
        }


        private static void RequestedDevices(Socket s, byte[] receiver)
        {
            if (s == null)
            {
                var n = new Neighbour(receiver.ToHex());
                n.IP = GetAddress(receiver.ToHex());
                Remove(n);
            }
            else
            {
                using (s)
                {
                    try
                    {
                        var connectioninfo = s.RemoteEndPoint as IPEndPoint;
                        if (connectioninfo != null)
                            Debug.Print("Connected to: " + connectioninfo.Address + ":" + connectioninfo.Port);

                        var waitingForData = true;

                        while (waitingForData)
                        {
                            waitingForData = !s.Poll(10, SelectMode.SelectRead) && !s.Poll(10, SelectMode.SelectError);

                            if (s.Available <= 0) continue;

                            var availableBytes = s.Available;

                            var buffer = new byte[availableBytes];

                            var bytesReceived = s.Receive(buffer);

                            if (bytesReceived != availableBytes) continue;

                            var result = JsonSerializer.DeserializeString(buffer.GetString()) as Hashtable;

                            if (result == null) continue;

                            var nMac = result["mac"] as string;

                            if (nMac == null) continue;

                            SystemInfo.ConnectionOverview.Add(nMac.FromHex());

                            var nDevices = result["Devices"] as ArrayList;

                            if (nDevices == null) continue;
                            foreach (string nDevice in nDevices)
                            {
                                SystemInfo.ConnectionOverview.Add(nMac.FromHex(), nDevice.FromHex());
                            }

                            // network status -> eventbus
                            networkStatus();
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.Print("Network error: " + exception.Message + " stacktrace: " + exception.StackTrace);
                    }
                    finally
                    {
                        if (s != null)
                        {
                            s.Close();
                        }
                    }
                }
            }
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