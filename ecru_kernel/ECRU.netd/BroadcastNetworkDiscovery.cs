using System;
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
        private static Thread _listenerThread;
        private static Thread _broadcastThread;

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


        public static void Start()
        {
            //Subscribe to network state changes
            NetworkTable.NetstateChanged += UpdateBroadcastMessage;
            
            //first time fetch broadcastMessage
            _broadcastMessage = SystemInfo.SystemMAC.ToHex() + "cc9d4028d80b7d9c2255cf5fc8cb25f2";

            if (EnableBroadcast)
            {
                //Start broadcast
                Debug.Print("Starting broadcaster");
                _broadcastEndPoint = new IPEndPoint(IPAddress.Parse(GetBroadcastAddress(LocalIP, SubnetMask)), UDPPort);

                _sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _sendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 5);

                _broadcastThread = new Thread(Broadcast);
                _broadcastThread.Start();
            }

            if (EnableListener)
            {
                //Start listening for broadcast
                Debug.Print("Starting listener");

                _receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _receiveSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 5);

                _listenerThread = new Thread(Listen);
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

                    Debug.Print("Broadcasting: " + _broadcastMessage);
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
            if (ep == null) return;
            if (Equals(ep.Address, IPAddress.GetDefaultLocalAddress())) return;

            Debug.Print(data.ToHex() + " received from: " + ep.Address + " with length: " + length);

            if (length != 44) return; // packet not correct size - discard it.
            var mac = data.GetPart(0, 6);
            var netstate = data.GetPart(6, 38);

            //routing table update here!
            NetworkTable.UpdateNetworkTableEntry(ep.Address, mac.ToHex(), netstate.ToHex());
        }

        private static void Listen()
        {
            EndPoint endpoint = new IPEndPoint(IPAddress.Any, UDPPort);
            _receiveSocket.Bind(endpoint);

            while (_receiveSocket.Poll(-1, SelectMode.SelectRead))
            {

                var buffer = new byte[_receiveSocket.Available];

                var length = _receiveSocket.ReceiveFrom(buffer, ref endpoint);

                OnDataReceived(buffer, length, endpoint);
            }

            Debug.Print("Somewthing broke :(");
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
    }
}