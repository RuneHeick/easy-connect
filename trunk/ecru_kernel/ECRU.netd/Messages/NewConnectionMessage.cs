using System;
using System.Net.Sockets;
using Microsoft.SPOT;

namespace ECRU.netd.Messages
{
    public delegate void NewNetworkConnectionCallback(Socket s);

    public class NewConnectionMessage
    {
        public NewNetworkConnectionCallback ConnectionCallback { get; set; }
        public string ConnectionType { get; set; }
        public byte[] Sender { get; set; }
        public byte[] Receiver { get; set; }
    }
}
