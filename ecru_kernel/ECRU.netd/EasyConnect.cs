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
        private static Hashtable _connectionRequests = new Hashtable();
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

                    var t = new Thread(() => OnDataReceived(connection));
                    _listenerThreadsArrayList.Add(t);
                    t.Start();
                }

            }
            catch (Exception exception)
            {
                Debug.Print("Start network discovery listener failed: " + exception.Message + " Stacktrace: " + exception.StackTrace);
                if (_receiveSocket != null)
                {
                    try
                    {
                        _receiveSocket.Poll(-1, SelectMode.SelectRead);
                        _receiveSocket.Close();
                    }
                    catch (Exception)
                    {
                        
                    }
                    
                }

                foreach (Thread t in _listenerThreadsArrayList)
                {
                    if (t.IsAlive)
                    {
                        t.Abort();
                    }
                }
            }
        }

        private static void OnDataReceived(Socket connection)
        {

            var waitingForData = true;

            while (waitingForData)
            {
                waitingForData = !connection.Poll(10, SelectMode.SelectRead) && !connection.Poll(10, SelectMode.SelectError);

                if (connection.Available > 0)
                {
                    var availableBytes = connection.Available;

                    var buffer = new byte[availableBytes];

                    var bytesReceived = connection.Receive(buffer);

                    if (bytesReceived == availableBytes)
                    {
                        waitingForData = false;
                        var timer = new ECTimer(ConnectionTimeout, connection, 5000, Timeout.Infinite);
                        timer.Start();

                        var msg = new ConnectionRequestMessage() { connectionType = buffer.GetString(), GetSocket = () => GetSocket(connection) };

                        lock (_lock)
                        {
                            _connectionRequests[connection] = timer;
                        }

                        //new eventbus event with
                        EventBus.Publish(msg);
                    }
                }
            }

        }

        private static void SendRequest(object message)
        {
            NewConnectionMessage msg = null;
            try
            {
                msg = message as NewConnectionMessage;
            }
            catch (Exception exception)
            {
                Debug.Print("SendRequest failed: " + exception.Message + " Stacktrace: " + exception.StackTrace);
            }

            //Start Sender for EasyConnect
            Debug.Print("Starting EasyConnect SendRequest");
            try
            {
                if (msg == null) return;
                var send = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //var ip = NetworkTable.GetAddress(msg.Receiver.ToHex());

                var ip = IPAddress.GetDefaultLocalAddress();

                var destination = new IPEndPoint(ip, Port);

                send.Connect(destination);

                Debug.Print("Socket information: remote-" + send.RemoteEndPoint + " local-" + send.LocalEndPoint + " timeout-" + send.ReceiveTimeout);

                var length = msg.ConnectionType.StringToBytes().Length;

                var bytesSent = send.Send(msg.ConnectionType.StringToBytes());

                if (bytesSent == length)
                {
                    var waitingForData = true;

                    while (waitingForData)
                    {
                        waitingForData = !send.Poll(10, SelectMode.SelectRead) && !send.Poll(10, SelectMode.SelectError);

                        if (send.Available > 0)
                        {
                            var buffer = new byte[send.Available];

                            send.Receive(buffer);
                            Debug.Print("Data from sendRequest: " + buffer.GetString());
                            switch (buffer.GetString())
                            {
                                case "Accepted":
                                    new Thread(() => msg.ConnectionCallback.Invoke(send)).Start();
                                    waitingForData = false;
                                    break;

                                default:
                                    new Thread(() => msg.ConnectionCallback.Invoke(null)).Start();
                                    waitingForData = false;
                                    break;
                            }
                        }
                    }

                }
            }
            catch (Exception exception)
            {
                Debug.Print("SendRequest failed: " + exception.Message + " Stacktrace: " + exception.StackTrace);
                new Thread(() => { if (msg != null) msg.ConnectionCallback.Invoke(null); }).Start();
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
                    var timer = _connectionRequests[con] as ECTimer;
                    _connectionRequests.Remove(con);
                    timer.Stop();
                }
            }
        }

        
        public static Socket GetSocket(Socket connection)
        {

            //end timer
            lock (_lock)
            {
                var timer = _connectionRequests[connection] as ECTimer;
                if (timer != null) timer.Stop();
            }

            //send message that socket is accepted
            connection.Send("Accepted".StringToBytes());

            //Give socket
            return connection;
        }

    }

}
