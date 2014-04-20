using System;
using Microsoft.SPOT;
using ECRU.BLEController.Packets;
using ECRU.Utilities;
using ECRU.BLEController.Util;
using System.Threading;

namespace ECRU.BLEController
{
    class DataManager
    {
        MacList ConnectedDevices;

        //Seen
        DeviceInfoList seenDevices = new DeviceInfoList();
        DateTime LastUpdated;

        PacketManager packetmanager;

        public DataManager(PacketManager packetmanager)
        {
            this.packetmanager = packetmanager;
            ConnectedDevices = SystemInfo.ConnectedDevices;
            LastUpdated = DateTime.Now;
        }

        public void Reset()
        {
            seenDevices.Clear();
            if (ConnectedDevices != null)
                ConnectedDevices.Clear();
        }

        public bool IsSystemDevice(byte[] address)
        {
            return true;
        }

        public AddDeviceEvent addConnectedDevice(byte[] address)
        {
            if (ConnectedDevices != null)
                ConnectedDevices.Add(address);
            return null; 
        }
        
        public void DisconnectDevice(byte[] address)
        {
            if (ConnectedDevices != null)
                ConnectedDevices.Remove(address);
        }

        public void AddToSeenDevices(DeviceEvent device)
        {
            if ((DateTime.Now - LastUpdated).Milliseconds > Def.SCAN_TIMEOUT)
                seenDevices.Clear();

            seenDevices.Add(device.Name, device.Address);
            LastUpdated = DateTime.Now; 
        }

        public void GotData(DataEvent data)
        {

        }

        public void RecivedServiceDir(ServiceEvent item)
        {

        }

    }
}
