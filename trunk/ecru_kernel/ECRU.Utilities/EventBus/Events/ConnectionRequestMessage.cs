using System.Net.Sockets;

namespace ECRU.Utilities.EventBus.Events
{
    public delegate Socket GetRequestSocket(); 

    public class ConnectionRequestMessage
    {
        public string connectionType { get; set; }
        public GetRequestSocket  GetSocket { get; set; }
    }
}
