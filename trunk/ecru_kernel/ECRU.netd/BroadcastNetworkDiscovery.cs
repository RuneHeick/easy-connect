using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ECRU.Utilities;
using ECRU.Utilities.HelpFunction;
using Microsoft.SPOT;

namespace ECRU.netd
{
    public static class BroadcastNetworkDiscovery
    {
        private static ArrayList _listenerThreadsArrayList = new ArrayList();

        private static Thread _broadcastThread;
        private static Thread _listenerThread;

        private static Socket _sendSocket;
        private static IPEndPoint _broadcastEndPoint;

        private static Socket _receiveSocket;
        public static int UDPPort { get; set; }
        public static string LocalIP { get; set; }
        public static string SubnetMask { get; set; }
        public static int BroadcastIntrevalSeconds { get; set; }
        public static bool EnableBroadcast { get; set; }
        public static bool EnableListener { get; set; }

        private static string _broadcastMessage;

        public static void Stop()
        {
            if (_receiveSocket != null && _receiveSocket.Poll(-1,SelectMode.SelectRead) )
            {
                _receiveSocket.Close();
            }

            if ( _sendSocket != null && _sendSocket.Poll(-1, SelectMode.SelectWrite))
            {
                _sendSocket.Close();
            }
            
            if (_broadcastThread != null && _broadcastThread.IsAlive)
            {
                _broadcastThread.Abort();
            }

            if (_listenerThread != null && _listenerThread.IsAlive)
            {
                _listenerThread.Abort();
            }

            foreach (Thread thread in _listenerThreadsArrayList)
            {
                if (thread.IsAlive)
                {
                    thread.Abort();
                }
            }

            NetworkTable.NetstateChanged -= UpdateBroadcastMessage;

        }

        public static void Start()
        {
            //Subscribe to network state changes
            NetworkTable.NetstateChanged += UpdateBroadcastMessage;
            
            //first time fetch broadcastMessage
            _broadcastMessage = SystemInfo.SystemMAC.ToHex() + "00000000000000000000000000000000";

            if (EnableBroadcast)
            {
                //Start broadcast
                Debug.Print("Starting network discovery sender");
                _broadcastEndPoint = new IPEndPoint(IPAddress.Parse(GetBroadcastAddress(LocalIP, SubnetMask)), UDPPort);

                _sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _sendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 5);

                _broadcastThread = new Thread(Broadcast);
                _broadcastThread.Start();
            }

            if (EnableListener)
            {
                _listenerThread = new Thread(StartListenThread);
                _listenerThread.Start();
            }
            
        }

        private static void Broadcast()
        {
            while (_broadcastThread.IsAlive)
            {
                Thread.Sleep(BroadcastIntrevalSeconds*1000);

                try
                {
                    var result = _sendSocket.SendTo(_broadcastMessage.StringToBytes(), _broadcastEndPoint);

                    Debug.Print("Broadcasting: " + _broadcastMessage + " length: " + result);
                }
                catch (Exception exception)
                {
                    Debug.Print("Broadcast failed: " + exception);
                }
                
            }
        }

        private static void OnDataReceived(byte[] data, int length, EndPoint sender)
        {

            var ep = sender as IPEndPoint;


            Debug.Print(data.ToHex() + " received from: " + ep.Address + " with length: " + length);

            try
            {
                var mac = data.GetPart(0, 6);
                var netstate = data.GetPart(6, 38);

                //routing table update here!
                NetworkTable.UpdateNetworkTableEntry(ep.Address, mac.ToHex(), netstate.GetString());

                _listenerThreadsArrayList.Remove(Thread.CurrentThread);
            }
            catch (Exception exception)
            {
                // packet not correct - discard it.
                _listenerThreadsArrayList.Remove(Thread.CurrentThread);
                Thread.CurrentThread.Abort();
                Debug.Print("NetworkDiscovery packet incorrect: " + exception);
            }

        }

        private static string GetBroadcastAddress(string ipAddress, string subnetMask)
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
                broadcastAddress[i] = (byte) (ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress).ToString();
        }

        private static void UpdateBroadcastMessage(string netstate)
        {
            _broadcastMessage = SystemInfo.SystemMAC.ToHex() + netstate;
            Debug.Print("Broadcast Message Updated: " + _broadcastMessage);
        }


        private static void StartListenThread()
        {
            //Start listening for broadcast
            Debug.Print("Starting network discovery receiver");
            try
            {
                _receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _receiveSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 5);

                EndPoint endpoint = new IPEndPoint(IPAddress.Any, UDPPort);

                _receiveSocket.Bind(endpoint);

                while (true)
                {
                    var buffer = new byte[_receiveSocket.Available];

                    var length = _receiveSocket.ReceiveFrom(buffer, ref endpoint);

                    var endpoint1 = endpoint as IPEndPoint;

                    if (endpoint1 == null || Equals(endpoint1.Address, IPAddress.GetDefaultLocalAddress()) || length != 44) continue; // packet not correct size - discard it.

                    var t = new Thread(() => OnDataReceived(buffer, length, endpoint1));
                    _listenerThreadsArrayList.Add(t);

                    t.Start();
                }

            }
            catch (Exception exception)
            {
                if(_receiveSocket != null && _receiveSocket.Poll(-1, SelectMode.SelectRead))
                {
                    _receiveSocket.Close();
                }

                foreach (Thread t in _listenerThreadsArrayList)
                {
                    if (t.IsAlive)
                    {
                        t.Abort();
                    }
                }
                Debug.Print("Start network discovery listener failed: " + exception.Message + " Stacktrace: " + exception.StackTrace);
            }

        }
    }
}