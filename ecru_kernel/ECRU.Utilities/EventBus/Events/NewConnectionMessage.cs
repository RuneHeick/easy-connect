using System.Net.Sockets;

namespace ECRU.Utilities.EventBus.Events
{
    public delegate void NewNetworkConnectionCallback(Socket s, byte[] Receiver);

    public class NewConnectionMessage
    {
        public NewNetworkConnectionCallback ConnectionCallback { get; set; }
        public string ConnectionType { get; set; }
        public byte[] Receiver { get; set; }
    }
}
