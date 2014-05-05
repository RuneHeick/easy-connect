using System.Net;

namespace ECRU.Utilities
{
    public class RecivedBroadcastMessage
    {
        public IPAddress SenderIPAddress { get; set; }
        public byte[] Message { get; set; }
        public byte[] MessageType { get; set; }
    }
}