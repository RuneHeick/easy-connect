using System;
using ECRU.netd.Configuration;
using ECRU.Utilities;
using ECRU.Utilities.Factories.ModuleFactory;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;

namespace ECRU.netd
{
    public class Netd : IModule
    {
        private string _ip;
        private int _port;

        private int resets;

        //setup subscriptions
        //setup ethernet after config
        //Network Discovery
        //Send network packets
        //get direct packets
        //get broadcast packets

        public Netd()
        {
            NetworkChange.NetworkAvailabilityChanged += NetworkAvailabilityChangedHandler;
            _port = 4543;
        }

        public void LoadConfig(string configFilePath)
        {
            //Network interface configuration
            try
            {

                NetworkInterface networkAdapter = NetworkInterface.GetAllNetworkInterfaces()[0];

                //Setup network configuration (Dynamic DNS/DCHP on ethernet interface)
                if (networkAdapter == null || networkAdapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                {
                    Stop();
                }

            
                var networkConfig = new EthernetConfig {EthernetInterface = networkAdapter, DynamicIP = true};
                networkConfig.InitNetworkInterface();
                _ip = networkConfig.EthernetInterface.IPAddress;

                if (_ip.Equals("0.0.0.0"))
                {
                    throw new IPAddressNotValidException();
                }

                //Network Table Configuration
                NetworkTable.SetLocalIP = networkConfig.EthernetInterface.IPAddress;
            }
            catch (Exception exception)
            {
                Debug.Print("Network Config error: " + exception.Message + " stacktrace: " + exception.StackTrace);
                Reset();
            }

            //Network Configuration done

            Debug.Print("Network config done. IP: " + _ip);
        }

        public void Start()
        {
            try
            {
                EventBus.Publish(new NetworkStatusMessage {isinsync = true, NetState = "000000000000"});

                EasyConnectTCP.Start();
                EasyConnectUDP.Start();
                NetworkDiscovery.Start();
                EventBus.Subscribe(typeof (ConnectionRequestMessage), MacSync.MacSync.RequestDevices);
                EventBus.Subscribe(typeof (RecivedBroadcastMessage), MacSync.MacSync.GotDeviceNetworkEvent);
                EventBus.Subscribe(typeof (RecivedBroadcastMessage), MacSync.MacSync.LostDeviceNetworkEvent);

                if (SystemInfo.ConnectedDevices != null)
                {
                    SystemInfo.ConnectedDevices.MacAdded += MacSync.MacSync.GotDevice;
                    SystemInfo.ConnectedDevices.MacRemoved += MacSync.MacSync.LostDevice;
                }
                else
                {
                    //The system was not configured correctly
                    Stop();
                }

                //start sucess reset counter
                resets = 0;
            }
            catch (Exception exception)
            {
                Debug.Print("Network start error: " + exception.Message + " stacktrace: " + exception.StackTrace);
                Reset();
            }
        }

        public void Stop()
        {
            try
            {
                EasyConnectTCP.Stop();
                EasyConnectUDP.Stop();
                NetworkDiscovery.Stop();
                EventBus.Unsubscribe(typeof (ConnectionRequestMessage), MacSync.MacSync.RequestDevices);
                EventBus.Unsubscribe(typeof (RecivedBroadcastMessage), MacSync.MacSync.GotDeviceNetworkEvent);
                EventBus.Unsubscribe(typeof (RecivedBroadcastMessage), MacSync.MacSync.LostDeviceNetworkEvent);

                if (SystemInfo.ConnectedDevices == null) return;
                SystemInfo.ConnectedDevices.MacAdded -= MacSync.MacSync.GotDevice;
                SystemInfo.ConnectedDevices.MacRemoved -= MacSync.MacSync.LostDevice;

                EventBus.Publish(new NetworkStatusMessage { isinsync = true, NetState = "000000000000" });
            }
            catch (Exception exception)
            {
                Debug.Print("Network stop error: " + exception.Message + " stacktrace: " + exception.StackTrace);
            }
        }

        public void Reset()
        {
            Stop();

            if (resets > 3) return;

            LoadConfig("");
            Start();
        }

        private void NetworkAvailabilityChangedHandler(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                LoadConfig("");
                Start();
            }
            else
            {
                Stop();
            }
        }
    }
}