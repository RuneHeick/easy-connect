﻿using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using ECRU.Utilities;
using ECRU.Utilities.HelpFunction;
using ECRU.Utilities.Timers;
using Microsoft.SPOT;

namespace ECRU.netd
{
    internal static class EasyConnectTCP
    {
        private static Socket _receiveSocket;
        private static readonly Hashtable _connectionRequests = new Hashtable();
        private static readonly object _lock = new object();

        private static readonly ArrayList _listenerThreadsArrayList = new ArrayList();
        private static Thread _ecThread;

        static EasyConnectTCP()
        {
            Port = 4543;
        }

        public static int Port { get; set; }

        public static void Start()
        {
            EventBus.Subscribe(typeof (NewConnectionMessage), SendRequest);

            _ecThread = new Thread(Listen);
            _ecThread.Start();
        }

        public static void Stop()
        {
            if (_receiveSocket != null)
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

            _listenerThreadsArrayList.Clear();

            if (_ecThread != null && _ecThread.IsAlive)
            {
                _ecThread.Abort();
            }

            EventBus.Unsubscribe(typeof (NewConnectionMessage), SendRequest);
        }

        private static void Listen()
        {
            //Start listening for EasyConnectTCP
            Debug.Print("Starting EasyConnectTCP listener");
            try
            {
                _receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _receiveSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
                _receiveSocket.Listen(10);

                while (true)
                {
                    Socket connection = _receiveSocket.Accept();

                    var t = new Thread(() => OnDataReceived(connection));
                    _listenerThreadsArrayList.Add(t);
                    t.Start();
                }
            }
            catch (Exception exception)
            {
                Debug.Print("Start network discovery listener failed: " + exception.Message + " Stacktrace: " +
                            exception.StackTrace);
                if (_receiveSocket != null)
                {
                    _receiveSocket.Close();
                }
            }
        }

        private static void OnDataReceived(Socket connection)
        {
            Debug.Print("Connection from: " + connection.RemoteEndPoint);
            bool waitingForData = true;
            try
            {
                while (waitingForData)
                {
                    waitingForData = !connection.Poll(10, SelectMode.SelectRead) &&
                                     !connection.Poll(10, SelectMode.SelectError);

                    if (connection.Available > 0)
                    {
                        int availableBytes = connection.Available;

                        var buffer = new byte[availableBytes];

                        int bytesReceived = connection.Receive(buffer);

                        if (bytesReceived == availableBytes)
                        {
                            waitingForData = false;
                            var timer = new ECTimer(ConnectionTimeout, connection, 30000, Timeout.Infinite);
                            timer.Start();


                            var msg = new ConnectionRequestMessage
                            {
                                connectionType = buffer.GetString(),
                                GetSocket = () => GetSocket(connection)
                            };

                            Debug.Print("Connection Type RQ: " + msg.connectionType);

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
            catch (Exception exception)
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
            finally
            {
                if (_listenerThreadsArrayList.Contains(Thread.CurrentThread))
                {
                    _listenerThreadsArrayList.Remove(Thread.CurrentThread);
                }
            }
        }

        private static void SendRequest(object message)
        {
            var msg = message as NewConnectionMessage;
            if (msg == null) return;
            //Start Sender for EasyConnectTCP
            Debug.Print("Starting EasyConnectTCP SendRequest");

            Socket send = null;
            try
            {
                IPAddress ip = NetworkTable.GetAddress(msg.Receiver.ToHex());

                if (ip == null)
                {
                    new Thread(() => msg.ConnectionCallback(null, msg.Receiver)).Start();
                    return;
                }

                send = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                var destination = new IPEndPoint(ip, Port);
                Type sType = Type.GetType("System.Net.Sockets.Socket");
                FieldInfo blockingInfo = sType.GetField("m_fBlocking", BindingFlags.NonPublic | BindingFlags.Instance);
                blockingInfo.SetValue(send, false);

                try
                {
                    send.Connect(destination);
                }
                catch (SocketException se)
                {
                    Thread.Sleep(500);
                }
                


                Debug.Print("Socket information: remote-" + send.RemoteEndPoint + " local-" + send.LocalEndPoint +
                            " timeout-" + send.ReceiveTimeout);
                send.SendTimeout = 2000;

                int length = msg.ConnectionType.StringToBytes().Length;

                int bytesSent = 0;
                
                try
                {
                    bytesSent = send.Send(msg.ConnectionType.StringToBytes());
                }
                catch (Exception se)
                {
                    new Thread(() => msg.ConnectionCallback(null, msg.Receiver)).Start();
                    return;
                }
                

                if (bytesSent == length)
                {
                    bool waitingForData = true;
                    bool TimeOut = false;
                    DateTime start = DateTime.Now;

                    while (waitingForData)
                    {
                        waitingForData = !send.Poll(10, SelectMode.SelectRead) && !send.Poll(10, SelectMode.SelectError);

                        if (((DateTime.Now - start).Ticks/TimeSpan.TicksPerMillisecond) > 60000)
                            throw new TimeOutException();

                        if (send.Available > 0)
                        {
                            var buffer = new byte[send.Available];
                            string s;
                            if (0 != send.Receive(buffer) && buffer.GetString() != null &&
                                buffer.GetString() == "Accepted")
                            {
                                Debug.Print("Accepted Request: " + buffer.GetString());
                                // to finaly
                            }
                            else
                            {
                                send.Close(); // only close if not used; 
                                send = null;
                            }

                            waitingForData = false;
                        }
                    }
                }
                else
                {
                    new Thread(() => msg.ConnectionCallback(null, msg.Receiver)).Start();
                }
            }
            catch (TimeOutException exception)
            {
                (new Thread(() => { if (msg != null) msg.ConnectionCallback(null, msg.Receiver); })).Start();
                if (send != null)
                {
                    send.Close();
                    send = null;
                }
            }
            catch (Exception exception)
            {
                Debug.Print("SendRequest failed: " + exception.Message + " Stacktrace: " + exception.StackTrace);
                if (send != null)
                {
                    send.Close();
                    send = null;
                }
            }
            finally
            {
                new Thread(() => { if (msg != null) msg.ConnectionCallback(send, msg.Receiver); }).Start();
            }
        }

        private static void ConnectionTimeout(object connection)
        {
            //Noone wanted connection so we terminate it
            var con = connection as Socket;
            if (con != null)
            {
                try
                {
                    con.Send("Not Accepted".StringToBytes());
                }
                catch
                {
                    
                }
                finally
                {
                    con.Close();
                }
                lock (_lock)
                {
                    if (_connectionRequests.Contains(con))
                    {
                        var timer = _connectionRequests[con] as ECTimer;
                        _connectionRequests.Remove(con);
                        if (timer != null) timer.Stop();
                    }
                }
            }
        }

        public static Socket GetSocket(Socket connection)
        {
            Socket con = null;
            //end timer
            lock (_lock)
            {
                if (_connectionRequests.Contains(connection))
                {
                    var timer = _connectionRequests[connection] as ECTimer;
                    if (timer != null) timer.Stop();

                    //send message that socket is accepted
                    try
                    {
                        connection.Send("Accepted".StringToBytes());

                        //remove socket and timer from connection queue
                        _connectionRequests.Remove(connection);

                        //handover reference to requester
                        con = connection;
                    }
                    catch (Exception)
                    {
                        if (connection != null)
                            connection.Close();
                        con = null;
                    }
                }
            }

            //Give socket
            return con;
        }
    }
}