using System;
using System.Collections;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ECRU.netd.Messages;
using ECRU.Utilities.HelpFunction;
using Microsoft.SPOT;
using ECRU.Utilities.EventBus;

namespace ECRU.netd
{
    

    static class EasyConnect
    {

        private static Socket _receiveSocket;
        private static Thread _listenerThread;
        private static Hashtable _connectionRequests;
        private static object _lock = new object();

        public static int Port { get; set; }

        static EasyConnect()
        {
            Port = 4543;
        }

        public static void Start()
        {

            EventBus.Subscribe(typeof(NewConnectionMessage), SendRequest);

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

        private static void SendRequest(object message)
        {
            try
            {
                var msg = message as NewConnectionMessage;
                if (msg == null) return;

                //Start Sender for EasyConnect
                Debug.Print("Starting EasyConnect sender");

                var send = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                var ip = NetworkTable.GetAddress(msg.Receiver.ToHex());

                var destination = new IPEndPoint(ip, Port);

                send.Connect(destination);

                var length = msg.ConnectionType.StringToBytes().Length.ToBytes();

                send.Send(length);

                send.Send(msg.ConnectionType.StringToBytes());

                var receivedLength = new byte[4];

                send.Receive(receivedLength);

                var buffer = new byte[receivedLength.ToInt()];

                send.Receive(buffer);

                switch (buffer.GetString())
                {
                    case "Accepted":
                        new Thread(() => msg.ConnectionCallback.Invoke(send)).Start();
                        break;

                    default:
                        new Thread(() => msg.ConnectionCallback.Invoke(null)).Start();
                        break;
                }
            }
            catch (Exception exception)
            {
                Debug.Print("SendRequest failed: " + exception.Message + " Stacktrace: " + exception.StackTrace);
                throw;
            }
            
        }

        private static void OnDataReceived(byte[] data, int length, EndPoint sender, Socket connection)
        {
            var ep = sender as IPEndPoint;
            if (ep == null) return;
            if (Equals(ep.Address, IPAddress.GetDefaultLocalAddress())) return;

            Debug.Print(data.ToHex() + " received from: " + ep.Address + " with length: " + length);

            var connectionType = data.GetString();
            
            var timer = new Timer(ConnectionTimeout, connection, 0, 5000);

            var msg = new ConnectionRequestMessage() {connectionType = connectionType, GetSocket = () => GetSocket(connection)};

            lock (_lock)
            {
                _connectionRequests[connection] = timer;
            }
            
            //new eventbus event with
            EventBus.Publish(msg);
        }

        private static void ConnectionTimeout(object connection)
        {
            //Noone wanted connection so we terminate it
            var con = connection as Socket;
            if (con != null)
            {
                con.Send("Not Accepted".StringToBytes());
                lock (_lock)
                {
                    _connectionRequests.Remove(con);
                }
            }
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

                var ECthread = new Thread(() => OnDataReceived(buffer, length, endpoint, connection));
                ECthread.Start();

            }
            catch (Exception exception)
            {
                Debug.Print("Listen failed: " + exception.Message + " Stacktrace: " + exception.StackTrace);
                throw;
            }
        }

        
        public static Socket GetSocket(Socket connection)
        {

            //end timer
            lock (_lock)
            {
                var timer = _connectionRequests[connection] as Timer;
                if (timer != null) timer.Dispose();
            }

            //send message that socket is accepted
            connection.Send("Accepted".StringToBytes());

            //Give socket
            return connection;
        }


    }

}
