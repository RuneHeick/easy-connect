using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ECRU.Utilities;
using ECRU.Utilities.EventBus;
using ECRU.Utilities.EventBus.Events;
using ECRU.Utilities.HelpFunction;
using Microsoft.SPOT;

namespace ECRU.netd
{
    class NetworkDiscovery
    {

        private static ArrayList _listenerThreadsArrayList = new ArrayList();

        private static Thread _broadcastThread;

        public static int UDPPort { get; set; }
        public static string LocalIP { get; set; }
        public static string SubnetMask { get; set; }
        public static int BroadcastIntrevalSeconds { get; set; }
        public static bool EnableBroadcast { get; set; }
        public static bool EnableListener { get; set; }

        private static byte[] _broadcastMessage = new byte[38];

        private static bool _broadcastrunning = true; 

        public static void Stop()
        {
            if (_broadcastThread != null && _broadcastThread.IsAlive)
            {
                _broadcastrunning = false; 
            }

            NetworkTable.NetstateChanged -= UpdateBroadcastMessage;

        }

        public static void Start()
        {
            BroadcastIntrevalSeconds = 5;

            //Subscribe to network state changes
            NetworkTable.NetstateChanged += UpdateBroadcastMessage;

            //first time broadcastMessage
            Array.Copy(SystemInfo.SystemMAC, _broadcastMessage, SystemInfo.SystemMAC.Length);
            Array.Copy("00000000000000000000000000000000".StringToBytes(), 0, _broadcastMessage, 6, "00000000000000000000000000000000".StringToBytes().Length);

            _broadcastrunning = true; 
            _broadcastThread = new Thread(Broadcast);
            _broadcastThread.Start();
            Thread.Sleep(200); // allow first Broadcast
            EventBus.Subscribe(typeof(RecivedBroadcastMessage), ReceivedBroadcast);

        }

        private static void ReceivedBroadcast(object message)
        {
            var msg = message as RecivedBroadcastMessage;

            if (msg != null)
            {
                if (msg.MessageType.ByteArrayCompare(new byte[] {1}))
                {
                    try
                    {
                        var mac = msg.Message.GetPart(0, 6);
                        var netstate = msg.Message.GetPart(6, 32);
                        var senderip = msg.SenderIPAddress;

                        //routing table update here!
                        NetworkTable.UpdateNetworkTableEntry(senderip, mac.ToHex(), netstate.GetString());
                    }
                    catch (Exception exception)
                    {
                        Debug.Print("NetworkDiscovery packet incorrect: " + exception);
                    }
                }
            }
        }

        private static void Broadcast()
        {
            while (_broadcastrunning)
            {
                NetworkTable.CheckTable();

                try
                {
                    EventBus.Publish(new SendBroadcastMessage
                    {
                        BroadcastType = new byte[] {1},
                        Message = _broadcastMessage
                    });

                }
                catch (Exception exception)
                {
                    Debug.Print("Broadcast failed: " + exception);
                }

                Thread.Sleep(BroadcastIntrevalSeconds * 1000);
            }
        }


        private static void UpdateBroadcastMessage(string netstate)
        {
            Array.Copy(SystemInfo.SystemMAC, _broadcastMessage, SystemInfo.SystemMAC.Length);
            Array.Copy(netstate.StringToBytes(), 0, _broadcastMessage, 6, netstate.StringToBytes().Length);
            Debug.Print("Broadcast Message Updated: " + _broadcastMessage);
        }
    }
}
