using System;
using Microsoft.SPOT;
using System.Net;
using System.Net.Sockets;

namespace ECRU.netd.FileSync
{
    public static class SendBroadcast
    {

        private static IPEndPoint _broadcastEndPoint;
        private static string UDPPort;
        private static string _broadcastMessage;
        private static string SubnetMask;
        private static string LocalIP;
        private static object _lock = new object();
        private static Socket _sendSocket;

        public static void Send(object message)
        {
            lock (_lock)
            {
                var msg = message as BroadcastMessage;
                UDPPort = msg.Port;
                _broadcastMessage = msg.Content;

                Debug.Print("Starting broadcast");
                _broadcastEndPoint = new IPEndPoint(IPAddress.Parse(Utilities.GetBroadcastAddress(LocalIP, SubnetMask)), UDPPort);

                _sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _sendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 5);

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
    }
}
