using System.Threading;
using ECRU.BLEController.Packets;
using ECRU.BLEController.Util;
using ECRU.Utilities;
using ECRU.Utilities.Factories.ModuleFactory;
using ECRU.Utilities.HelpFunction;
using Microsoft.SPOT;
using ResetEvent = ECRU.BLEController.Packets.ResetEvent;
using ECRU.Utilities.Timers; 

namespace ECRU.BLEController
{
    public class BLEModule : IModule
    {
        private readonly PacketManager packetmanager;
        private readonly SerialController serial = new SerialController();
        private DeviceInfoFactory Infofactory;
        private DataManager data;
        bool started = false;
        bool isreset = false;
        ECTimer ResetTimer;

        public BLEModule()
        {
            packetmanager = new PacketManager(serial);
            Infofactory = new DeviceInfoFactory(packetmanager);
            ResetTimer = new ECTimer(SendResetCallBack, null, 0, 2000);
        }

        private bool IsInitMode { get; set; }

        public void LoadConfig(string configFilePath)
        {
            if (SystemInfo.Name != "" && SystemInfo.Name != null
                && SystemInfo.PassCode != "" && SystemInfo.PassCode != null)
            {
                IsInitMode = false;
            }
            else
            {
                IsInitMode = true;
            }
        }

        public void Start()
        {
            if (started == false)
            {
                //Datamanager
                data = new DataManager(packetmanager);

                //rest
                LoadConfig(null);
                serial.Start();

                //Discover Devices 
                packetmanager.Subscrib(CommandType.DeviceEvent, FoundDevices);
                packetmanager.Subscrib(CommandType.DisconnectEvent, RecivedDisconnectEvnet);

                //Setup Information 
                packetmanager.Subscrib(CommandType.Info, DeviceInfoRecived);
                packetmanager.Subscrib(CommandType.AddrEvent, AddrsEventRecived);
                packetmanager.Subscrib(CommandType.Reset, BLEControllerReset);

                //Data Update 
                packetmanager.Subscrib(CommandType.DataEvent, RecivedDataEvnet);

                SendReset();
            }
            started = true; 
        }


        public void Stop()
        {
            if (started)
            {
                //Discover Devices 
                packetmanager.Unsubscrib(CommandType.DeviceEvent, FoundDevices);
                packetmanager.Unsubscrib(CommandType.DisconnectEvent, RecivedDisconnectEvnet);

                //Setup Information 
                packetmanager.Unsubscrib(CommandType.Info, DeviceInfoRecived);
                packetmanager.Unsubscrib(CommandType.AddrEvent, AddrsEventRecived);
                packetmanager.Unsubscrib(CommandType.Reset, BLEControllerReset);

                //Data Update 
                packetmanager.Unsubscrib(CommandType.DataEvent, RecivedDataEvnet);

                data.Dispose();
                SendReset();
            }
            started = false; 
        }

        public void Reset()
        {
            Stop();
            LoadConfig("");
            Start();
        }


        //******************   Resived **************************


        private void FoundDevices(IPacket pack)
        {
            lock (data)
            {
                var newDevice = pack as DeviceEvent;
                Debug.Print("Found Device: " + newDevice.Address.ToHex());
                
                if (newDevice != null)
                {
                    if (data.IsSystemDevice(newDevice.Address))
                    {
                        DataManager.InfoFileStatus  fileStatus = data.hasInfoFile(newDevice.Address);
                        switch(fileStatus)
                        {
                            case DataManager.InfoFileStatus.HaveNone:
                                Infofactory.GetDeviceInfo(newDevice.Address, DiscoverComplete);
                                break;
                            case DataManager.InfoFileStatus.NeedRead:
                                {
                                    DeviceInfo item = data.GetDeviceInfo(newDevice.Address);
                                    if(item != null)
                                    {
                                        DoFullRead(item);
                                    }
                                }
                                break;
                            case DataManager.InfoFileStatus.Done:
                                {
                                    DeviceInfo item = data.GetDeviceInfo(newDevice.Address);
                                    if (item != null)
                                    {
                                        AddDevice(item);
                                        /*{
                                            WriteEvent Revent = new WriteEvent();
                                            Revent.Address = newDevice.Address;


                                            Revent.Handle = 0x0023;
                                            Revent.Value = new byte[] { 0x08 };



                                            packetmanager.Send(Revent);

                                            Revent.Handle = 0x0020;
                                            Revent.Value = new byte[] { 0x01 };
                                            Debug.Print("Start Kaffe");
                                            
                                            
                                            
                                            packetmanager.Send(Revent);
                                        }*/
                                    }
                                }
                                break; 


                        }
                    }
                    else
                    {
                        data.AddToSeenDevices(newDevice);
                    }
                }
            }
        }

        private void AddDevice(DeviceInfo item)
        {
            data.addConnectedDevice(item.Address);
            var add = new AddDeviceEvent();
            add.Address = item.Address;
            add.ConnectionTimeHandle = item.TimeHandel;
            add.PassCodeHandle = item.PassCodeHandel;
            packetmanager.Send(add);
        }

        private void DiscoverComplete(DeviceInfoFactory.Status_t status, DeviceInfo item)
        {
            Debug.Print("Done:");
            if (status == DeviceInfoFactory.Status_t.Done)
            {
                Debug.Print("ok");
                data.UpdateOrCreateInfoFile(item);
                DoFullRead(item);
            }
        }

        private void DoFullRead(DeviceInfo item)
        {
            AddDevice(item);
            Infofactory.DoFullRead(item, DoneRead);
        }

        private void DoneRead(DeviceInfoFactory.Status_t status, DeviceInfo item)
        {
            Debug.Print("Read Done:");
            if (status == DeviceInfoFactory.Status_t.Done)
            {
                Debug.Print("ok");
                data.UpdateOrCreateInfoFile(item);
            }
            else if (status == DeviceInfoFactory.Status_t.TimeOut)
            {
                Infofactory.DoFullRead(item, DoneRead);
            }
            else
            {
                data.DisconnectDevice(item.Address);
            }
        }

        // Called if the CC2540 reset. 
        public void BLEControllerReset(IPacket packet)
        {
            isreset = true; 
            Debug.Print("BLE RESET");
            data.Reset();
            SendSystemInfo();
            Infofactory.Dispose();
            Infofactory = new DeviceInfoFactory(packetmanager);
        }

        public void RecivedDataEvnet(IPacket packet)
        {
            var DataPacket = packet as DataEvent;
            if (DataPacket != null)
            {
                if (!Infofactory.IsHandledByFactory(DataPacket.Address))
                    data.GotData(DataPacket);
            }
        }

        public void RecivedDisconnectEvnet(IPacket packet)
        {
            var DisPacket = packet as DisconnectEvent;
            if (DisPacket != null)
            {
                Debug.Print("Recived Dis :" + DisPacket.Address.ToHex());
                data.DisconnectDevice(DisPacket.Address);
            }
        }


        //******************   Init Mode *********************
        private void DeviceInfoRecived(IPacket packet)
        {
            var namePacket = packet as NameEvent;
            var codePacket = packet as PassCodeEvent;

            if (namePacket != null)
            {
                SystemInfo.Name = namePacket.Name;
                return;
            }

            if (codePacket != null)
            {
                SystemInfo.PassCode = codePacket.Code;
            }
        }

        private void AddrsEventRecived(IPacket packet)
        {
            var addr = packet as AddrEvent;
            if (addr != null)
            {
                SystemInfo.SystemMAC = addr.Address;
            }
        }

        //******************   Send **************************


        public void SendSystemInfo()
        {
            var info = new SystemInfoEvent();
            try
            {
                if(IsInitMode == true)
                    info.SystemID = new byte[8]; 
                else
                    info.SystemID = SystemInfo.SystemID;
            }
            catch
            {
                info.SystemID = new byte[8]; 
            }
            info.InitMode = IsInitMode;
            packetmanager.Send(info);
        }

        public void SendReset()
        {
            isreset = false;
            ResetTimer.Start(); 
        }

        private void SendResetCallBack(object o)
        {
            if (isreset == false)
            {
                var reset = new ResetEvent();
                packetmanager.Send(reset);
            }
            else
            {
                ResetTimer.Stop();
            }
        }

    }
}