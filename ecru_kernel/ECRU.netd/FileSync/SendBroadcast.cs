using System;
using Microsoft.SPOT;
using System.Net;
using System.Net.Sockets;

namespace ECRU.netd.FileSync
{
    public static class SendBroadcast
    {

        private static IPEndPoint _broadcastEndPoint;
        private static int UDPPort;
        private static string _broadcastMessage;
        public static string SubnetMask { private get; set; }
        public static string LocalIP { private get; set; }
        private static object _lock = new object();
        private static Socket _sendSocket;

        public static void Send(object message)
        {
            lock (_lock)
            {
                try
                {
                    var msg = message as BroadcastMessage;
                    if (msg != null)
                    {
                        UDPPort = msg.Port;
                        _broadcastMessage = msg.Content;

                        _broadcastEndPoint = new IPEndPoint(IPAddress.Parse(Utilities.GetBroadcastAddress(LocalIP, SubnetMask)), UDPPort);

                        _sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        _sendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 5);

                        var result = _sendSocket.SendTo(_broadcastMessage.StringToBytes(), _broadcastEndPoint);

                        Debug.Print("Broadcasting: " + _broadcastMessage);
                    }
                }
                catch (Exception exception)
                {
                    Debug.Print("FileSync broadcast message failed: " + exception);
                }

            }
        }
    }
}
