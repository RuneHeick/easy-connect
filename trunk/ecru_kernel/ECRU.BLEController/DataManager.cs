using System;
using Microsoft.SPOT;
using ECRU.BLEController.Packets;

namespace ECRU.BLEController
{
    class DataManager
    {
        private const string SeenDevicesPath = ""; 

        public bool IsSystemDevice(byte[] address)
        {
            return false;
        }

        public void addConnectedDevice(byte[] address)
        {

        }
        
        public void AddToSeenDevices(DeviceEvent device)
        {

        }

        public void GotData(DataEvent data)
        {

        }

    }
}
