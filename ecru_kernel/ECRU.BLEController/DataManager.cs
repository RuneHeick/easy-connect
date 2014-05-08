using System;
using System.Net.Sockets;
using ECRU.BLEController.Packets;
using ECRU.BLEController.Util;
using ECRU.Utilities;
using ECRU.Utilities.HelpFunction;
using Microsoft.SPOT;
using System.Threading;

namespace ECRU.BLEController
{
    internal class DataManager : IDisposable
    {
        private const string FileFolder = "BLE Devices";
        private readonly MacList ConnectedDevices;
        private readonly PacketManager packetmanager;

        //Seen
        private readonly DeviceInfoList seenDevices = new DeviceInfoList();
        private DateTime LastUpdated;

        public DataManager(PacketManager packetmanager)
        {
            this.packetmanager = packetmanager;
            ConnectedDevices = SystemInfo.ConnectedDevices;
            LastUpdated = DateTime.Now;
            EventBus.Subscribe(typeof (ConnectionRequestMessage), RequestDeviceInformation);
            EventBus.Subscribe(typeof (ConnectionRequestMessage), RequestECMData);
            EventBus.Subscribe(typeof (ConnectionRequestMessage), SetECMData);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe(typeof (ConnectionRequestMessage), RequestDeviceInformation);
            EventBus.Unsubscribe(typeof (ConnectionRequestMessage), RequestECMData);
            EventBus.Unsubscribe(typeof (ConnectionRequestMessage), SetECMData);
            Reset();
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
                    var valfile = new DeviceInfoValueFile(file);
                    valfile.Update(data.Handel, data.Value);
                    valfile.Close();
                }
            }
        }


        internal bool hasInfoFile(byte[] addr)
        {
            return FileSystem.Exists(addr.ToHex() + ".BLE", FileType.Shared);
        }

        internal DeviceInfo GetDeviceInfo(byte[] addr)
        {
            if (FileSystem.Exists(addr.ToHex() + ".BLE", FileType.Shared))
            {
                FileBase file = FileSystem.GetFile(addr.ToHex() + ".BLE", FileAccess.Read, FileType.Shared);
                var defFile = new DeviceInfoDefFile(file);
                defFile.Close();
                return defFile.Object;
            }

            return null;
        }

        internal void DeleteSystemInfo(byte[] addr)
        {
            if (FileSystem.Exists(addr.ToHex() + ".BLE", FileType.Shared))
            {
                FileSystem.DeleteFile(addr.ToHex() + ".BLE", FileType.Shared);
            }
        }

        internal void UpdateOrCreateInfoFile(DeviceInfo item)
        {
            // info File 
            int trycount = 0;

            FileBase file = null;

            while (trycount < 3)
            {
                
                if (FileSystem.Exists(item.Address.ToHex() + ".BLE", FileType.Shared))
                {
                    file = FileSystem.GetFile(item.Address.ToHex() + ".BLE", FileAccess.ReadWrite, FileType.Shared);
                }
                else
                {
                    file = FileSystem.CreateFile(item.Address.ToHex() + ".BLE", FileType.Shared);
                }

                if (file != null)
                {
                    var defFile = new DeviceInfoDefFile(file);
                    defFile.Object = item;
                    defFile.Close();
                    break;
                }
                else
                {
                    trycount++;
                    Thread.Sleep(30000);
                }

            }

            // val file 

            if (FileSystem.Exists(item.Address.ToHex() + ".val", FileType.Local))
            {
                file = FileSystem.GetFile(item.Address.ToHex() + ".val", FileAccess.ReadWrite, FileType.Local);
            }
            else
            {
                file = FileSystem.CreateFile(item.Address.ToHex() + ".val", FileType.Local);
            }

            var valfile = new DeviceInfoValueFile(file);
            valfile.Create(item);
            valfile.Close();
        }

        //****************************************************

        public void RequestDeviceInformation(object message)
        {
            var msg = message as ConnectionRequestMessage;

            if (msg == null) return;

            if (msg.connectionType != "RequestDeviceInformation") return;

            FileBase file = null;

            using (Socket socket = msg.GetSocket())
            {
                try
                {
                    bool waitingForData = true;

                    while (waitingForData)
                    {
                        waitingForData = !socket.Poll(10, SelectMode.SelectRead) &&
                                         !socket.Poll(10, SelectMode.SelectError);

                        if (socket.Available <= 0) continue;

                        int availableBytes = socket.Available;

                        var buffer = new byte[availableBytes];

                        int bytesReceived = socket.Receive(buffer);

                        if (bytesReceived != availableBytes) continue;
                        waitingForData = false;


                        if (FileSystem.Exists(buffer.ToHex() + ".BLE", FileType.Shared))
                        {
                            file = FileSystem.GetFile(buffer.ToHex() + ".BLE", FileAccess.Read, FileType.Shared);
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
                    if (socket != null)
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
        public void RequestECMData(object message)
        {
            var msg = message as ConnectionRequestMessage;

            if (msg == null) return;

            if (msg.connectionType != "RequestECMData") return;

            FileBase file = null;

            using (Socket socket = msg.GetSocket())
            {
                try
                {
                    bool waitingForData = true;

                    while (waitingForData)
                    {
                        waitingForData = !socket.Poll(10, SelectMode.SelectRead) &&
                                         !socket.Poll(10, SelectMode.SelectError);

                        if (socket.Available <= 0) continue;

                        int availableBytes = socket.Available;

                        var buffer = new byte[availableBytes];

                        int bytesReceived = socket.Receive(buffer);

                        if (bytesReceived != availableBytes) continue;

                        waitingForData = false;

                        byte[] mac = buffer.GetPart(0, 6);
                        byte[] handle = buffer.GetPart(6, 2);

                        if (FileSystem.Exists(mac.ToHex() + ".val", FileType.Local))
                        {
                            file = FileSystem.GetFile(mac.ToHex() + ".val", FileAccess.Read, FileType.Local);
                        }

                        var valfile = new DeviceInfoValueFile(file);

                        var newHandle = (ushort) ((handle[0] << 8) + handle[1]);

                        byte[] data = valfile.GetData(newHandle);
                        if (((valfile.GetWrProp(newHandle) & 0x02) > 0) && data != null)
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
                    if (socket != null)
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
        public void SetECMData(object message)
        {
            var msg = message as ConnectionRequestMessage;

            if (msg == null) return;

            if (msg.connectionType != "SetECMData") return;

            FileBase file = null;

            using (Socket socket = msg.GetSocket())
            {
                try
                {
                    bool waitingForData = true;

                    while (waitingForData)
                    {
                        waitingForData = !socket.Poll(10, SelectMode.SelectRead) &&
                                         !socket.Poll(10, SelectMode.SelectError);

                        if (socket.Available <= 0) continue;

                        int availableBytes = socket.Available;

                        var buffer = new byte[availableBytes];

                        int bytesReceived = socket.Receive(buffer);

                        if (bytesReceived != availableBytes) continue;

                        waitingForData = false;

                        byte[] mac = buffer.GetPart(0, 6);
                        byte[] handle = buffer.GetPart(6, 2);
                        byte[] data = buffer.GetPart(8, availableBytes - 8);


                        if (!FileSystem.Exists(mac.ToHex() + ".val", FileType.Local)) continue;

                        var newHandle = (ushort) ((handle[0] << 8) + handle[1]);

                        file = FileSystem.GetFile(mac.ToHex() + ".val", FileAccess.ReadWrite, FileType.Local);

                        var valfile = new DeviceInfoValueFile(file);


                        //write data to val file
                        valfile.Update(newHandle, data);

                        //send data event
                        var packet = new WriteEvent();

                        packet.Address = mac;
                        packet.Handle = newHandle;
                        packet.Value = data;

                        packetmanager.Send(packet);
                    }
                }
                catch (Exception exception)
                {
                    Debug.Print("Network error: " + exception.Message + " stacktrace: " + exception.StackTrace);
                }
                finally
                {
                    if (socket != null)
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
    }
}