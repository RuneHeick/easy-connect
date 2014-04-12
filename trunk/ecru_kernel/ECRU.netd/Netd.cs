using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ECRU.EventBus.Messages;
using ECRU.netd.Configuration;
using ECRU.netd.Handlers;
using ECRU.Utilities.Factories.ModuleFactory;
using ECRU.Utilities.SD;
using Microsoft.SPOT;
using Microsoft.SPOT.Net;
using Microsoft.SPOT.Net.NetworkInformation;

namespace ECRU.netd
{
    public class Netd : IModule
    {
        private Thread netDaemonThread = null;
        //setup subscriptions
        //setup ethernet after config
        //Network Discovery
        //Send network packets
        //get direct packets
        //get broadcast packets
        //

        

        public void LoadConfig(string configFilePath)
        {
            //var config = SD.ReadConfugurationFromFile(configFilePath);
            //Debug.Print("Loading NetDaemon Configuration from " + configFilePath);

            //Debug.Print("NetDaemon Configured");
            //throw new NotImplementedException();
        }

        public void Start()
        {
            Thread.Sleep(2000);
            var networkAdapter = NetworkInterface.GetAllNetworkInterfaces()[0];

            //Debug.Print(networkAdapter.NetworkInterfaceType.ToString());

            //Setup network configuration (Dynamic DNS/DCHP on ethernet interface)
            if (networkAdapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
            {
                throw new NotImplementedException();
            }

            var networkConfig = new EthernetConfig {EthernetInterface = networkAdapter, DynamicIP = true};
            networkConfig.InitNetworkInterface();

            Debug.Print(networkAdapter.IPAddress);
            Debug.Print(networkAdapter.GatewayAddress);
            Debug.Print(networkAdapter.SubnetMask);

            
            
            //Subscribe eventhandlers
            //  - Send Network Packets
            
            //Timer - Network Discovery
            netd.BroadcastNetworkDiscovery.UDPPort = 11000;
            netd.BroadcastNetworkDiscovery.BroadcastIP = "192.168.1.255";
          

            var callback = new TimerCallback(netd.BroadcastNetworkDiscovery.Broadcast);

            //var networkDiscoveryTimer = new Timer(callback, null, 0, 5000);


            //Setup thread for getting packets


            var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var endPoint = new IPEndPoint(IPAddress.Any, 11000);

            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            
            // Binding is required with ReceiveFrom calls.
            s.Bind(endPoint);

            // Creates an IPEndPoint to capture the identity of the sending host.
            EndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            var msg = new Byte[3];

            // This call blocks.
            while (true)
            {
                try
                {
                    s.ReceiveFrom(msg, ref sender);
                    Debug.Print(Utilities.GetString(msg));

                }
                catch(Exception e)
                {

                }

            }
            
            
            s.Close();

            Debug.Print(Utilities.GetString(msg));

        }

        private void BroadcastNetworkDiscovery()
        {
            
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
