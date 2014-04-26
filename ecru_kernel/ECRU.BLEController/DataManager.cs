using System;
using Microsoft.SPOT;
using ECRU.BLEController.Packets;
using ECRU.Utilities;
using ECRU.BLEController.Util;
using System.Threading;
using ECRU.File;
using ECRU.Utilities.HelpFunction;
using ECRU.File.Files;

namespace ECRU.BLEController
{
    class DataManager
    {
        MacList ConnectedDevices;
        const string FileFolder = "BLE Devices";

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

        public bool IsSystemDevice(byte[] addr)
        {
            if (FileSystem.Exists(addr.ToHex() + ".BLE", FileType.Local))
                return true;

            return true; // Accepted Devices. 
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
            if (FileSystem.Exists(data.Address.ToHex() + ".val", FileType.Local))
            {
                Debug.Print("Got Data From: " + data.Address.ToHex());
                FileBase file = FileSystem.GetFile(data.Address.ToHex() + ".val", FileAccess.ReadWrite, FileType.Local);
                if (file != null)
                {
                    DeviceInfoValueFile valfile = new DeviceInfoValueFile(file);
                    valfile.Update(data.Handel, data.Value);
                    valfile.Close();
                }
            }
        }


        internal bool hasInfoFile(byte[] addr)
        {
            return FileSystem.Exists(addr.ToHex() + ".BLE", FileType.Local); 
        }

        internal DeviceInfo GetDeviceInfo(byte[] addr)
        {
            if (FileSystem.Exists(addr.ToHex() + ".BLE", FileType.Local))
            {
                FileBase file = FileSystem.GetFile(addr.ToHex() + ".BLE", FileAccess.Read, FileType.Local);
                DeviceInfoDefFile defFile = new DeviceInfoDefFile(file);
                defFile.Close();
                return defFile.Object;
            }

            return null; 
        }

        internal void DeleteSystemInfo(byte[] addr)
        {
            if (FileSystem.Exists(addr.ToHex() + ".BLE", FileType.Local))
            {
                FileSystem.DeleteFile(addr.ToHex() + ".BLE", FileType.Local);
            }
        }

        internal void UpdateOrCreateInfoFile(DeviceInfo item)
        {

            // info File 
            FileBase file = null;
            if(FileSystem.Exists(item.Address.ToHex() + ".BLE",FileType.Local))
            {
                file = FileSystem.GetFile(item.Address.ToHex() + ".BLE",FileAccess.ReadWrite ,FileType.Local);
            }
            else
            {
                file = FileSystem.CreateFile(item.Address.ToHex() + ".BLE", FileType.Local);
            }
            
            DeviceInfoDefFile defFile = new DeviceInfoDefFile(file);
            defFile.Object = item;
            defFile.Close(); 

            // val file 

            if (FileSystem.Exists(item.Address.ToHex() + ".val", FileType.Local))
            {
                file = FileSystem.GetFile(item.Address.ToHex() + ".val", FileAccess.ReadWrite, FileType.Local);
            }
            else
            {
                file = FileSystem.CreateFile(item.Address.ToHex() + ".val", FileType.Local);
            }

            DeviceInfoValueFile valfile = new DeviceInfoValueFile(file);
            valfile.Create(item);
            valfile.Close(); 
        }
    }
}
