using System;
using System.Net;
using System.Threading;
using Microsoft.SPOT.Net.NetworkInformation;

namespace ECRU.netd.Configuration
{
    public interface IEthernetConfig
    {
        NetworkInterface EthernetInterface { get; set; }
        bool DynamicIP { get; set; }
        bool InitNetworkInterface();
    }


    public class EthernetConfig : IEthernetConfig
    {
        public NetworkInterface EthernetInterface { get; set; }
        public bool DynamicIP { get; set; }

        public bool InitNetworkInterface()
        {
            if (DynamicIP)
            {
                try
                {
                    if (!EthernetInterface.IsDynamicDnsEnabled)
                    {
                        EthernetInterface.EnableDynamicDns();
                    }
                    if (!EthernetInterface.IsDhcpEnabled)
                    {
                        EthernetInterface.EnableDhcp();
                        Thread.Sleep(1000);
                        EthernetInterface.RenewDhcpLease();
                    }

                    if (EthernetInterface.IPAddress == IPAddress.Any.ToString())
                    {
                        EthernetInterface.RenewDhcpLease();
                    }
                    Thread.Sleep(2000);
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}