using System;
using System.Collections;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ECRU.Utilities.EventBus.Events;
using ECRU.Utilities.HelpFunction;
using ECRU.Utilities.Timers;
using Microsoft.SPOT;
using ECRU.Utilities.EventBus;
using Microsoft.SPOT.Hardware;

namespace ECRU.netd
{
    

    static class EasyConnect
    {

        private static Socket _receiveSocket;
        private static Hashtable _connectionRequests;
        private static object _lock = new object();

        private static ArrayList _listenerThreadsArrayList = new ArrayList();
        private static Thread _ecThread;

        public static int Port { get; set; }

        static EasyConnect()
        {
            Port = 4543;
        }

        public static void Start()
        {

            EventBus.Subscribe(typeof(NewConnectionMessage), SendRequest);

            _ecThread = new Thread(Listen);
            _ecThread.Start();

        }

        public static void Stop()
        {
            if (_receiveSocket != null && _receiveSocket.Poll(-1, SelectMode.SelectRead))
            {
                _receiveSocket.Close();
            }

            foreach (Thread thread in _listenerThreadsArrayList)
            {
                if (thread.IsAlive)
                {
                    thread.Abort();
                }
            }

            if (_ecThread != null && _ecThread.IsAlive)
            {
                _ecThread.Abort();
            }

            EventBus.Unsubscribe(typeof (NewConnectionMessage), SendRequest);

        }

        private static void Listen()
        {
            //Start listening for EasyConnect
            Debug.Print("Starting EasyConnect listener");
            try
            {
                _receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _receiveSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
                _receiveSocket.Listen(10);

                while (true)
                {
                    var connection = _receiveSocket.Accept();
                    var buffer = new byte[4];

                    var length = _receiveSocket.Receive(buffer);

                    var t = new Thread(() => OnDataReceived(buffer, length, connection));
                    _listenerThreadsArrayList.Add(t);
                    t.Start();
                }

            }
            catch (Exception exception)
            {
                if (_receiveSocket != null && _receiveSocket.Poll(-1, SelectMode.SelectRead))
                {
                    _receiveSocket.Close();
                }

                foreach (Thread t in _listenerThreadsArrayList)
                {
                    if (t.IsAlive)
                    {
                        t.Abort();
                    }
                }
                Debug.Print("Start network discovery listener failed: " + exception.Message + " Stacktrace: " + exception.StackTrace);
            }
        }

        private static void SendRequest(object message)
        {
            try
            {
                var msg = message as NewConnectionMessage;
                if (msg == null) return;

                //Start Sender for EasyConnect
                Debug.Print("Starting EasyConnect SendRequest");

                var send = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                
                using (send)
                {
                    //var ip = NetworkTable.GetAddress(msg.Receiver.ToHex());

                    var ip = IPAddress.GetDefaultLocalAddress();

                    var destination = new IPEndPoint(ip, Port);

                    send.Connect(destination);

                    var length = msg.ConnectionType.StringToBytes().Length.ToBytes();

                    send.Send(length);

                    send.Send(msg.ConnectionType.StringToBytes());

                    var receivedLength = new byte[4];

                    send.Receive(receivedLength);

                    var buffer = new byte[receivedLength.ToInt()];

                    send.Receive(buffer);

                    Debug.Print("SendRequest buffer: " + buffer.GetString());

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
                
            }
            catch (Exception exception)
            {
                Debug.Print("SendRequest failed: " + exception.Message + " Stacktrace: " + exception.StackTrace);
            }
            
        }

        private static void OnDataReceived(byte[] data, int length, Socket connection)
        {
            var ep = connection.RemoteEndPoint as IPEndPoint;

            Debug.Print(data.ToHex() + " received from: " + ep.Address + " with length: " + length);

            var transmissionLength = data.ToInt();

            var connectionTypeBuffer = new Byte[transmissionLength];

            var compare = connection.Receive(connectionTypeBuffer);

            if (compare == transmissionLength)
            {
                var connectionType = data.GetString();

                var timer = new Timer(ConnectionTimeout, connection, 0, 5000);

                var msg = new ConnectionRequestMessage() { connectionType = connectionType, GetSocket = () => GetSocket(connection) };

                lock (_lock)
                {
                    _connectionRequests[connection] = timer;
                }

                //new eventbus event with
                EventBus.Publish(msg);
            }

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
