using System;
using ECRU.EventBus;
using Microsoft.SPOT;

namespace ECRU.netd.Messages
{
    class NetworkProfileConfigMessage : TMessage
    {
        private const string _type = "ECRU.Netd.Messages.NetworkProfileConfigMessage";

        public string Type
        {
            get { return _type; }
        }

        public string ECRUNetworkName { get; set; }

        public string ECRUName { get; set; }

        public string ECRUNetworkPassword { get; set; }

        public string WiFiSSID { get; set; }

        public string WiFiPassword { get; set; }

    }
}
