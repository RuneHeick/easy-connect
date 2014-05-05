using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ECRU.Utilities;
using ECRU.Utilities.HelpFunction;
using Microsoft.SPOT;

namespace ECRU.netd
{
    internal class EasyConnectUDP
    {
        private static Socket _receiveSocket;

        private static Thread _ecThread;

        private static string _broadcastAdd;

        static EasyConnectUDP()
        {
            Port = 4543;
            Subnetmask = "255.255.255.0";
        }

        public static int Port { get; set; }
        public static string Subnetmask { get; set; }

        public static void Start()
        {
            _broadcastAdd = Utilities.GetBroadcastAddress(IPAddress.GetDefaultLocalAddress().ToString(), Subnetmask);
            EventBus.Subscribe(typeof (SendBroadcastMessage), Broadcast);

            _ecThread = new Thread(Listen);
            _ecThread.Start();
        }

        public static void Stop()
        {
            if (_receiveSocket != null && _receiveSocket.Poll(-1, SelectMode.SelectRead))
            {
                _receiveSocket.Close();
            }

            if (_ecThread != null && _ecThread.IsAlive)
            {
                _ecThread.Abort();
            }

            EventBus.Unsubscribe(typeof (SendBroadcastMessage), Broadcast);
        }

        private static void Listen()
        {
            //Start listening for EasyConnectUDP
            Debug.Print("Starting EasyConnectUDP listener");
            try
            {
                _receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _receiveSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 5);

                EndPoint endpoint = new IPEndPoint(IPAddress.Any, Port);

                _receiveSocket.Bind(endpoint);

                while (true)
                {
                    if (_receiveSocket.Available > 0)
                    {
                        var buffer = new byte[_receiveSocket.Available];

                        int length = _receiveSocket.ReceiveFrom(buffer, ref endpoint);

                        var endpoint1 = endpoint as IPEndPoint;

                        if (endpoint1 == null || Equals(endpoint1.Address, IPAddress.GetDefaultLocalAddress()) ||
                            length < 1) continue; // packet not correct size - discard it.

                        OnDataReceived(buffer, length, endpoint1);
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.Print("Start EasyConnectUDP listener failed: " + exception.Message + " Stacktrace: " +
                            exception.StackTrace);
                if (_receiveSocket != null && _receiveSocket.Poll(-1, SelectMode.SelectRead))
                {
                    _receiveSocket.Close();
                }
            }
        }

        private static void OnDataReceived(byte[] data, int length, EndPoint sender)
        {
            var ep = sender as IPEndPoint;
            Debug.Print(data.ToHex().Substring(0, length*2) + " received from: " + ep.Address + " with length: " +
                        length);

            try
            {
                byte[] _messageType = data.GetPart(0, 1);
                byte[] _message = data.GetPart(1, length - 1);

                if (_message.Length > 0)
                {
                    EventBus.Publish(new RecivedBroadcastMessage
                    {
                        Message = _message,
                        MessageType = _messageType,
                        SenderIPAddress = ep.Address
                    });
                }
            }
            catch (Exception exception)
            {
                Debug.Print("EasyConnect packet incorrect: " + exception.Message + " Stacktrace: " +
                            exception.StackTrace);
                // packet not correct - discard it.
            }
        }

        private static void Broadcast(object message)
        {
            SendBroadcastMessage msg = null;
            try
            {
                msg = message as SendBroadcastMessage;
            }
            catch (Exception exception)
            {
                Debug.Print("Broadcast failed: " + exception.Message + " Stacktrace: " + exception.StackTrace);
            }

            //Start Sender for EasyConnectUDP
            Debug.Print("Starting EasyConnectUDP Broadcast");
            var _broadcastEndPoint = new IPEndPoint(IPAddress.Parse(_broadcastAdd), Port);

            var _sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _sendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 5);

            try
            {
                if (msg == null) return;

                byte[] _broadcastMessage = msg.BroadcastType;
                _broadcastMessage = _broadcastMessage.Add(msg.Message);


                int result = _sendSocket.SendTo(_broadcastMessage, _broadcastEndPoint);
                _sendSocket.Close();
                Debug.Print("Broadcasting length: " + result);
            }
            catch (Exception exception)
            {
                if (_sendSocket != null && _sendSocket.Poll(-1, SelectMode.SelectRead))
                {
                    _sendSocket.Close();
                }
                Debug.Print("Broadcast failed: " + exception.Message + " Stacktrace: " + exception.StackTrace);
            }
        }
    }
}