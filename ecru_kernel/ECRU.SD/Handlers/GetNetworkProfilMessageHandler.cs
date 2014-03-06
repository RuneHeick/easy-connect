using System;
using System.IO;
using ECRU.EventBus;
using ECRU.SD.Messages;
using ECRU.netd.Messages;
using Microsoft.SPOT;

namespace ECRU.SD.Handlers
{
    class GetNetworkProfilMessageHandler : TMessageHandler
    {
        private FileStream configFileStream;

        private StreamReader configStreamReader;

        public void Handle(TMessage message)
        {
            var _message = message as GetNetworkProfilMessage;

            var configFilePath = _message.ConfigFilePath;

            if(File.Exists(configFilePath))
            {
                configFileStream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
                configStreamReader = new StreamReader(configFilePath);

                var line = configStreamReader.ReadLine();
                if (line != null)
                {
                    while (line != null)
                    {
                        if (line[0] != '#') 
                        {
                            string[] SplitVals = val.Split('=');
                            if (SplitVals.Length == 2)
                            {
                                _Configuration.Add(SplitVals[0], SplitVals[1]);
                            }
                        }
                    }
                }
            }

        }
    }
}
