using System;
using System.Net;
using System.Net.Sockets;
using ECRU.Utilities.HelpFunction;
using Microsoft.SPOT;

namespace ECRU.netd.FileSync
{
    public static class Broadcast
    {
        private static IPEndPoint _broadcastEndPoint;
        private static string _broadcastMessage;
        private static readonly object _sendlock = new object();
        private static Socket _sendSocket;
        public static string SubnetMask { private get; set; }
        public static string LocalIP { private get; set; }

        public static void Send(object message)
        {
            lock (_sendlock)
            {
                try
                {
                    var msg = message as BroadcastMessage;
                    if (msg != null)
                    {
                        _broadcastMessage = msg.Content;

                        _broadcastEndPoint =
                            new IPEndPoint(IPAddress.Parse(Utilities.GetBroadcastAddress(LocalIP, SubnetMask)), msg.Port);

                        _sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        _sendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 5);

                        int result = _sendSocket.SendTo(_broadcastMessage.StringToBytes(), _broadcastEndPoint);

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