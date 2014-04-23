using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ECRU.Utilities.HelpFunction;
using Microsoft.SPOT;
using ECRU.Utilities.EventBus;

namespace ECRU.netd
{
    static class EasyConnect
    {

        public delegate Socket GetSocket();

        private static Socket _receiveSocket;
        private static Thread _listenerThread;

        public static int Port { get; set; }

        static EasyConnect()
        {
            Port = 4543;
        }

        public static void Start()
        {
            
            //Start listening for EasyConnect
            Debug.Print("Starting EasyConnect listener");

            _receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _receiveSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.AcceptConnection, true);

            _listenerThread = new Thread(Listen);
            _listenerThread.Start();
        }

        public static void Stop()
        {
            if (_listenerThread.IsAlive)
            {
                 _listenerThread.Abort();
            }
           
        }

        private static void OnDataReceived(byte[] data, int length, EndPoint sender)
        {
            var ep = sender as IPEndPoint;
            if (ep == null) return;
            if (Equals(ep.Address, IPAddress.GetDefaultLocalAddress())) return;

            Debug.Print(data.ToHex() + " received from: " + ep.Address + " with length: " + length);

            var connectionType = data.GetString();

            //new eventbus event with
            var new Thread(EventBus.Publish());
        }

        private static void Listen()
        {

            EndPoint endpoint = new IPEndPoint(IPAddress.Any, Port);

            try
            {
                _receiveSocket.Bind(endpoint);
                _receiveSocket.Listen(1);

                var connection = _receiveSocket.Accept();

                var buffer = new byte[connection.Available];

                var length = connection.ReceiveFrom(buffer, ref endpoint);

                OnDataReceived(buffer, length, endpoint);

            }
            catch (Exception exception)
            {
                Debug.Print("Listen failed: " + exception);
                throw;
            }
        }

        public static void ConnectionRequest(object message)
        {
            string connectionType;
            Delegate GetSocket;
        }
    }

    internal class 

}
