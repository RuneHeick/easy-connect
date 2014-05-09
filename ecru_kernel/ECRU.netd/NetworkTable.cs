using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using ECRU.Utilities;
using ECRU.Utilities.HelpFunction;
using Json.NETMF;
using Microsoft.SPOT;

namespace ECRU.netd
{
    public delegate void NetworkStateChange(String netstate);

    public static class NetworkTable
    {
        private static readonly Hashtable Neighbours = new Hashtable();
        private static string[] netstateIPList;

        private static String _PublishedNetstate = "";
        public static String _netstate { get; private set; }
        private static bool _isInSync;

        private static readonly object Lock = new object();
        private static readonly object _lockNetstate = new object();
        private static readonly object _netstateIPListLock = new object();

        static NetworkTable()
        {
            _isInSync = false;
        }

        public static int MaxLastSeenTimeSeconds
        {
            get { return NetworkDiscovery.BroadcastIntrevalSeconds*3; }
        }

        public static string SetLocalIP { get; set; }
        public static event NetworkStateChange NetstateChanged;

        public static void ClearNetworkTable()
        {
            foreach (Neighbour neighbour in Neighbours)
            {
                SystemInfo.ConnectionOverview.Remove(neighbour.Mac.FromHex());
            }

            Neighbours.Clear();
        }

        private static void Add(Neighbour neighbour)
        {
            lock (_netstateIPListLock)
            {

                if (!Neighbours.Contains(neighbour.Mac))
                {
                    //Add unit
                    Neighbours[neighbour.Mac] = neighbour;

                    //Update netstate ip list
                    netstateIPList = netstateIPList.Add(neighbour.IP.ToString());

                    //Call event
                    Debug.Print("Added neighbour: " + neighbour.IP);

                    SystemInfo.ConnectionOverview.Add(neighbour.Mac.FromHex());

                    networkStatus();

                    // update request devices from roomunit
                    EventBus.Publish(new NewConnectionMessage
                    {
                        ConnectionCallback = RequestedDevices,
                        ConnectionType = "RequestDevices",
                        Receiver = neighbour.Mac.FromHex()
                    });
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
        }

        private static void networkStatus()
        {
            UpdateNetstate();

            lock (Lock)
            {
                bool tmpNetworkStatus = _isInSync;


                _isInSync = true;
                foreach (Neighbour neighbour in Neighbours.Values)
                {
                    if (neighbour.Netstate == _netstate) continue;
                    _isInSync = false;
                }
                if (tmpNetworkStatus != _isInSync || _PublishedNetstate != _netstate)
                {
                    _PublishedNetstate = _netstate;
                    EventBus.Publish(new NetworkStatusMessage {isinsync = _isInSync, NetState = _netstate});
                }
            }
        }

        private static void Remove(Neighbour neighbour)
        {
            lock (_netstateIPListLock)
            {
                //Remove unit
                Neighbours.Remove(neighbour.Mac);

                //Update netstate ip list
                netstateIPList = netstateIPList.Remove(neighbour.IP.ToString());

                //Call Event
                Debug.Print("Removed neighbour: " + neighbour.IP + "                                         Waring");

                // update MacHierachy / MacList
                SystemInfo.ConnectionOverview.Remove(neighbour.Mac.FromHex());
                networkStatus();
            }
        }

        public static void UpdateNetworkTableEntry(IPAddress ipAddress, string mac, string netstate)
        {
            if (netstateIPList == null)
            {
                netstateIPList = new[] {SetLocalIP};
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
                IPAddress neighbourIP = n.IP;

                //check if address is valid
                if (ValidAddress(mac))
                {
                    return neighbourIP;
                }
                Remove(n);
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
            DateTime lastSeen = ((Neighbour) Neighbours[mac]).Lastseen;

            TimeSpan timeDifference = DateTime.Now - lastSeen;

            return timeDifference.Ticks <= (TimeSpan.TicksPerSecond*MaxLastSeenTimeSeconds);
        }


        private static void UpdateNetstate()
        {
            lock (_lockNetstate)
            {
                string data = null;
                netstateIPList = netstateIPList.Quicksort(0, (netstateIPList.Length - 1));

                foreach (string s in netstateIPList)
                {
                    data += s;
                }

                byte[] buffer = data.StringToBytes();

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

                        bool waitingForData = true;

                        while (waitingForData)
                        {
                            waitingForData = !s.Poll(10, SelectMode.SelectRead) && !s.Poll(10, SelectMode.SelectError);

                            if (s.Available <= 0) continue;

                            int availableBytes = s.Available;

                            var buffer = new byte[availableBytes];

                            int bytesReceived = s.Receive(buffer);

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

        private delegate void NetworkTableChange(Neighbour neighbour);
    }


    public class IPAddressNotValidException : Exception
    {
    }


    internal class Neighbour
    {
        private readonly string _mac;
        public DateTime Lastseen;
        public String Netstate;

        public Neighbour(string Mac)
        {
            _mac = Mac;
        }

        public string Mac
        {
            get { return _mac; }
        }

        public IPAddress IP { get; set; }
    }
}