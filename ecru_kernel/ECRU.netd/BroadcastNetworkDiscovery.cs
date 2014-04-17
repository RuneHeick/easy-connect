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


        public static void Start()
        {
            //Start broadcast
            Debug.Print("Starting boradcaster");
            _broadcastEndPoint = new IPEndPoint(IPAddress.Parse(GetBroadcastAddress(LocalIP, SubnetMask)), UDPPort);

            _sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _sendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 5);

            _broadcastThread = new Thread(Broadcast);
            _broadcastThread.Start();

            //Start listening for broadcast
            Debug.Print("Starting listener");

            _receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _receiveSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 5);

            _listenerThread = new Thread(Listen);
            _listenerThread.Start();
        }

        private static void Broadcast()
        {
            while (_broadcastThread.IsAlive)
            {
                Thread.Sleep(BroadcastIntrevalSeconds*1000);
                string broadcastMessage = SystemInfo.SystemMAC.ToString() + NetworkTable.GetNetstate();
                int result = _sendSocket.SendTo(broadcastMessage.StringToBytes(), _broadcastEndPoint);
            }
        }

        private static void OnDataReceived(byte[] data, int length, EndPoint sender)
        {
            var ep = sender as IPEndPoint;
            if (ep == null) return;
            if (Equals(ep.Address, IPAddress.GetDefaultLocalAddress())) return;

            byte[] mac = data.GetPart(0, 6);

            byte[] netstate = data.GetPart(6, length - 6);

            //routing table update here!
            NetworkTable.Update(ep.Address, mac, netstate);
        }

        private static void Listen()
        {
            EndPoint endpoint = new IPEndPoint(IPAddress.Any, UDPPort);
            _receiveSocket.Bind(endpoint);

            while (_receiveSocket.Poll(-1, SelectMode.SelectRead))
            {
                Debug.Print("Listening");

                var buffer = new byte[_receiveSocket.Available];

                int length = _receiveSocket.ReceiveFrom(buffer, ref endpoint);

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
    }
}