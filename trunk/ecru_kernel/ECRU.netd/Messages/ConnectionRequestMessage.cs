using System;
using System.Net.Sockets;
using Microsoft.SPOT;

namespace ECRU.netd.Messages
{
    public delegate Socket GetRequestSocket(); 

    public class ConnectionRequestMessage
    {
        public string connectionType { get; set; }
        public GetRequestSocket  GetSocket { get; set; }
    }
}
