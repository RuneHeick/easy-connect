using System;
using System.Net;
using System.Threading;
using ECRU.netd.Configuration;
using ECRU.netd.FileSync;
using ECRU.Utilities.Factories.ModuleFactory;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;
using ECRU.Utilities;
using ECRU.Utilities.EventBus;

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
            
            var networkAdapter = NetworkInterface.GetAllNetworkInterfaces()[0];

            //Setup network configuration (Dynamic DNS/DCHP on ethernet interface)
            if (networkAdapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
            {
                throw new NotImplementedException();
            }

            var networkConfig = new EthernetConfig {EthernetInterface = networkAdapter, DynamicIP = true};


            try
            {
                networkConfig.InitNetworkInterface();
            }
            catch (Exception exception)
            {
                Debug.Print("Network Config error: " + exception.Message + " stacktrace: " + exception.StackTrace);
                throw;
            }

            //Network Discovery Configuration
            netd.BroadcastNetworkDiscovery.UDPPort = 4543;
            netd.BroadcastNetworkDiscovery.LocalIP = networkConfig.EthernetInterface.IPAddress;
            netd.BroadcastNetworkDiscovery.SubnetMask = networkAdapter.SubnetMask;
            netd.BroadcastNetworkDiscovery.BroadcastIntrevalSeconds = 30;
            netd.BroadcastNetworkDiscovery.EnableBroadcast = true;
            netd.BroadcastNetworkDiscovery.EnableListener = true;

            //Network Table Configuration
            NetworkTable.SetLocalIP = networkConfig.EthernetInterface.IPAddress;

            //Network Communication Configuration

            //Filesync sentbroadcast config
            Broadcast.LocalIP = networkConfig.EthernetInterface.IPAddress;
            Broadcast.SubnetMask = networkConfig.EthernetInterface.SubnetMask;


        }

        public void Start()
        {
            netd.BroadcastNetworkDiscovery.Start();
            EasyConnect.Start();
        }

        public void Stop()
        {
            netd.BroadcastNetworkDiscovery.Stop();
            EasyConnect.Stop();
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