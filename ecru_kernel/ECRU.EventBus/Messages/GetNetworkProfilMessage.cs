using System;
using ECRU.EventBus;
using Microsoft.SPOT;

namespace ECRU.EventBus.Messages
{
    public class GetNetworkProfilMessage : TMessage
    {
        public GetNetworkProfilMessage(string configFilePath)
        {
            ConfigFilePath = configFilePath;
        }

        private const string _type = "ECRU.SD.Messages.GetNetworkProfilMessage";

        public string Type
        {
            get { return _type; }
        }

        public string ConfigFilePath { get; private set; }
    }
}
