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

        private static String _PublishedNetstate = "";
        private static String _netstate;
        private static bool _isInSync;

        private static object Lock = new object();
        private static object _lockNetstate = new object();

        private static event NetworkTableChange AddUnit;
        private static event NetworkTableChange RemovedUnit;
        private delegate void NetworkTableChange(Neighbour neighbour);

        public static int MaxLastSeenTimeSeconds
        {
            get
            {
                return NetworkDiscovery.BroadcastIntrevalSeconds * 5;
            }
        }
        static NetworkTable()
        {
            _isInSync = false;
            AddUnit += (NeighbourAdded);
            RemovedUnit += (NeighbourRemoved);
        }

        public static string SetLocalIP { get; set; }

        public static void ClearNetworkTable()
        {
            foreach (Neighbour neighbour in Neighbours)
            {
                SystemInfo.ConnectionOverview.Remove(neighbour.Mac.FromHex());
            }

            Neighbours.Clear();
        }

        private static void NeighbourAdded(Neighbour neighbour)
        {
            Debug.Print("Added neighbour: " + neighbour.IP.ToString());


            SystemInfo.ConnectionOverview.Add(neighbour.Mac.FromHex());

            networkStatus();

            // update request devices from roomunit
            EventBus.Publish(new NewConnectionMessage { ConnectionCallback = RequestedDevices, ConnectionType = "RequestDevices", Receiver = neighbour.Mac.FromHex() });

        }

        private static void NeighbourRemoved(Neighbour neighbour)
        {
            Debug.Print("Removed neighbour: " + neighbour.IP.ToString()+ "                                         Waring");

            // update MacHierachy / MacList
            SystemInfo.ConnectionOverview.Remove(neighbour.Mac.FromHex());
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
                if (AddUnit != null)
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
            UpdateNetstate();

            lock (Lock)
            {
                var tmpNetworkStatus = _isInSync;


                _isInSync = true;
                foreach (Neighbour neighbour in Neighbours.Values)
                {
                    if (neighbour.Netstate == _netstate) continue;
                    _isInSync = false;
                }
                if (tmpNetworkStatus != _isInSync || _PublishedNetstate != _netstate)
                {
                    _PublishedNetstate = _netstate;
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
            if (RemovedUnit != null)
                RemovedUnit(neighbour);
        }

        public static void UpdateNetworkTableEntry(IPAddress ipAddress, string mac, string netstate)
        {
            if (netstateIPList == null)
            {
                netstateIPList = new[] { SetLocalIP };
            }

            var neighbour = new Neighbour(mac);

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

            var n = Neighbours[mac] as Neighbour;
            if (n != null)
            {
                var neighbourIP = n.IP;

                //check if address is valid
                if (ValidAddress(mac))
                {
                    return neighbourIP;
                }
                else
                {
                    Remove(n);
                }
            }
            return null;
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
            var lastSeen = ((Neighbour)Neighbours[mac]).Lastseen;

            var timeDifference = DateTime.Now - lastSeen;

            return timeDifference.Ticks <= (TimeSpan.TicksPerSecond * MaxLastSeenTimeSeconds);
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
            if (NetstateChanged != null)
                NetstateChanged(_netstate);
        }


        private static void RequestedDevices(Socket s, byte[] receiver)
        {
            if (s != null)
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

                            waitingForData = false;

                            var result = JsonSerializer.DeserializeString(buffer.GetString()) as Hashtable;

                            if (result == null) continue;

                            var nMac = result["mac"] as string;

                            if (nMac == null) continue;


                            var nDevices = result["Devices"] as ArrayList;

                            if (nDevices == null && SystemInfo.ConnectionOverview != null) continue;
                            foreach (string nDevice in nDevices)
                            {
                                SystemInfo.ConnectionOverview.Add(nMac.FromHex(), nDevice.FromHex());
                            }

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