using System;
using System.Collections;
using System.IO;
using ECRU.EventBus;
using ECRU.EventBus.Messages;
using Microsoft.SPOT;

namespace ECRU.SD.Handlers
{
    class GetNetworkProfilMessageHandler : TMessageHandler
    {
        private FileStream _configFileStream;
        private readonly NetworkProfileConfigMessage _configMessage = new NetworkProfileConfigMessage();
        private StreamReader _configStreamReader;
        private GetNetworkProfilMessage _message;
        private readonly Hashtable _configHashtable = new Hashtable();

        public void Handle(TMessage message)
        {
            
            _message = message as GetNetworkProfilMessage;
            if (_message == null) throw new ArgumentNullException("_message");

            var configFilePath = _message.ConfigFilePath;
            try
            {
                if (File.Exists(configFilePath))
                {
                    _configFileStream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
                    _configStreamReader = new StreamReader(_configFileStream);

                    var line = _configStreamReader.ReadLine();
                    if (line != null)
                    {
                        while (line != null)
                        {
                            if (line[0] != '#')
                            {
                                var splitVals = line.Split('=');
                                if (splitVals.Length == 2)
                                {
                                    _configHashtable.Add(splitVals[0], splitVals[1]);
                                }
                            }
                            line = _configStreamReader.ReadLine();
                        }
                    }
                    _configStreamReader.Close();
                    _configFileStream.Close();
                }

                
            }
            catch (Exception exception)
            {
                Debug.Print(exception.StackTrace);
                throw;
            }
            foreach (var key in _configHashtable.Keys)
            {
                switch (key.ToString())
                {
                    case "ECRUNetworkName":
                        _configMessage.ECRUNetworkName = _configHashtable[key] as string;
                        break;
                    case "ECRUName":
                        _configMessage.ECRUName = _configHashtable[key] as string;
                        break;
                    case "ECRUNetworkPassword":
                        _configMessage.ECRUNetworkPassword = _configHashtable[key] as string;
                        break;
                    case "WiFiSSID":
                        _configMessage.WiFiSSID = _configHashtable[key] as string;
                        break;
                    case "WiFiPassword":
                        _configMessage.WiFiPassword = _configHashtable[key] as string;
                        break;
                }
            }
            EventBus.EventBus.Publish(_configMessage);
        }
    }
}
