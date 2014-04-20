using System;
using System.Threading;
using ECRU.netd.Configuration;
using ECRU.Utilities.Factories.ModuleFactory;
using Microsoft.SPOT.Net.NetworkInformation;

namespace ECRU.netd
{
    public class Netd : IModule
    {
        //setup subscriptions
        //setup ethernet after config
        //Network Discovery
        //Send network packets
        //get direct packets
        //get broadcast packets
        //


        public void LoadConfig(string configFilePath)
        {
            //wait for interfaces (Magic number dont change)
            //Thread.Sleep(3000);

            //var config = SD.ReadConfugurationFromFile(configFilePath);
            //Debug.Print("Loading NetDaemon Configuration from " + configFilePath);

            //Debug.Print("NetDaemon Configured");
            //throw new NotImplementedException();

            //Network interface configuration
            
            NetworkInterface networkAdapter = NetworkInterface.GetAllNetworkInterfaces()[0];

            //Setup network configuration (Dynamic DNS/DCHP on ethernet interface)
            if (networkAdapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
            {
                throw new NotImplementedException();
            }

            var networkConfig = new EthernetConfig {EthernetInterface = networkAdapter, DynamicIP = true};
            networkConfig.InitNetworkInterface();


            //Network Discovery Configuration
            netd.BroadcastNetworkDiscovery.UDPPort = 4544;
            netd.BroadcastNetworkDiscovery.LocalIP = networkAdapter.IPAddress;
            netd.BroadcastNetworkDiscovery.SubnetMask = networkAdapter.SubnetMask;
            netd.BroadcastNetworkDiscovery.BroadcastIntrevalSeconds = 10;
            netd.BroadcastNetworkDiscovery.EnableBroadcast = true;
            netd.BroadcastNetworkDiscovery.EnableListener = true;

            //Network Communication Configuration
        }

        public void Start()
        {
            netd.BroadcastNetworkDiscovery.Start();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        private void BroadcastNetworkDiscovery()
        {
        }
    }
}