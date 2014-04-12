using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace ECRU.netd
{
    public static class BroadcastNetworkDiscovery
    {
        private const string BroadcastMessage = "ECT";

        public static int UDPPort { get; set; }
        public static string BroadcastIP { get; set; }

        private static Socket _socket = null;
        private static OutputPort ledPort = new OutputPort(Pins.ONBOARD_LED, false);

        static BroadcastNetworkDiscovery()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
        }

        public static void Broadcast(object state)
        {

            //Debug.Print("Broadcasting...");
            // This the address to local broadcast for me, may be different for others.
            var brodcastEndPoint = new IPEndPoint(IPAddress.Parse(BroadcastIP), UDPPort);

            // Enable broadcast
            //Debug.Print(brodcastEndPoint.Address.ToString() + brodcastEndPoint.Port);
            try
            {
                ledPort.Write(!ledPort.Read());
                var result = _socket.SendTo(Utilities.StringToBytes(BroadcastMessage), brodcastEndPoint);
            }
            catch (SocketException e)
            {
                
                
            }
            catch (ObjectDisposedException)
            {

            }
            // This actually generates a Malformed Netbios response from the router, but it does demonstrate UDP Broadcast.
            //Debug.Print("result is: " + result);
           
        }

    

    }
}
