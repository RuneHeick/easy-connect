using System;
using System.Net.Sockets;
using ECRU.Utilities.EventBus;
using ECRU.Utilities.EventBus.Events;
using ECRU.Utilities.Timers;
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
    class DataManager : IDisposable
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
            EventBus.Subscribe(typeof (ConnectionRequestMessage), RequestDeviceInformation);
            EventBus.Subscribe(typeof (ConnectionRequestMessage), RequestECMData);

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

        //****************************************************

        public static void RequestDeviceInformation(object message)
        {
            var msg = message as ConnectionRequestMessage;

            if (msg == null) return;

            if (msg.connectionType != "RequestDeviceInformation") return;

            FileBase file = null;

            using (var socket = msg.GetSocket())
            {
                try
                {
                    var waitingForData = true;

                    while (waitingForData)
                    {
                        waitingForData = !socket.Poll(10, SelectMode.SelectRead) &&
                                         !socket.Poll(10, SelectMode.SelectError);

                        if (socket.Available <= 0) continue;

                        var availableBytes = socket.Available;

                        var buffer = new byte[availableBytes];

                        var bytesReceived = socket.Receive(buffer);

                        if (bytesReceived != availableBytes) continue;
                        waitingForData = false;

                            
                        if (FileSystem.Exists(buffer.ToHex() + ".BLE", FileType.Local))
                        {
                            file = FileSystem.GetFile(buffer.ToHex() + ".BLE", FileAccess.Read, FileType.Local);
                        }

                        if (file == null) continue;

                        socket.Send(file.Data);
                        file.Close();
                    }
                }
                catch (Exception exception)
                {
                    Debug.Print("Network error: " + exception.Message + " stacktrace: " + exception.StackTrace);
                }
                finally
                {
                    if (socket != null && socket.Poll(-1, SelectMode.SelectRead))
                    {
                        socket.Close();
                    }
                    if (file != null)
                    {
                        file.Close();
                    }
                }
            }
        }

        //*********************************************************
        public static void RequestECMData(object message)
        {
            var msg = message as ConnectionRequestMessage;

            if (msg == null) return;

            if (msg.connectionType != "RequestECMData") return;

            FileBase file = null;

            using (var socket = msg.GetSocket())
            {
                try
                {
                    var waitingForData = true;

                    while (waitingForData)
                    {
                        waitingForData = !socket.Poll(10, SelectMode.SelectRead) &&
                                         !socket.Poll(10, SelectMode.SelectError);

                        if (socket.Available <= 0) continue;

                        var availableBytes = socket.Available;

                        var buffer = new byte[availableBytes];

                        var bytesReceived = socket.Receive(buffer);

                        if (bytesReceived != availableBytes) continue;

                        waitingForData = false;

                        var mac = buffer.GetPart(0, 6);
                        var handle = buffer.GetPart(6, 2);

                        if (FileSystem.Exists(mac.ToHex() + ".val", FileType.Local))
                        {
                            file = FileSystem.GetFile(mac.ToHex() + ".val", FileAccess.Read, FileType.Local);
                        }

                        var valfile = new DeviceInfoValueFile(file);

                        var newHandle = (ushort)((handle[0] << 8) + handle[1]);

                        var data = valfile.GetData(newHandle);
                        if (((valfile.GetWrProp(newHandle) & 0x02) > 0) && data != null )
                        {
                            socket.Send(data);
                        }

                        valfile.Close(); 
                    }
                }
                catch (Exception exception)
                {
                    Debug.Print("Network error: " + exception.Message + " stacktrace: " + exception.StackTrace);
                }
                finally
                {
                    if (socket != null && socket.Poll(-1, SelectMode.SelectRead))
                    {
                        socket.Close();
                    }
                    if (file != null)
                    {
                        file.Close();
                    }
                }
            }
        }

        public void Dispose()
        {
            EventBus.Unsubscribe(typeof (ConnectionRequestMessage), RequestDeviceInformation);
            EventBus.Unsubscribe(typeof(ConnectionRequestMessage), RequestECMData);
            Reset();
        }
    }
}
