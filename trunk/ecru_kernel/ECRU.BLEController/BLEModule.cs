using System;
using Microsoft.SPOT;
using ECRU.Utilities;
using ECRU.Utilities.Factories.ModuleFactory;
using ECRU.Utilities.EventBus;
using ECRU.BLEController.Packets;
using System.Threading;
using ECRU.BLEController.Util;
using ECRU.Utilities.HelpFunction;

namespace ECRU.BLEController
{
    public class BLEModule: IModule
    {
        SerialController serial = new SerialController();
        PacketManager packetmanager;
        DataManager data;
        DeviceInfoFactory Infofactory; 

        bool IsInitMode { get; set; }

        public BLEModule()
        {
            packetmanager = new PacketManager(serial);
            Infofactory = new DeviceInfoFactory(packetmanager);
        }

        public void LoadConfig(string configFilePath)
        {
            if(SystemInfo.Name != "" && SystemInfo.Name != null
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
            //Datamanager
            data = new DataManager(packetmanager);

            //rest
            SystemInfo.PassCode = "Rune Heick";
            SystemInfo.Name = "Test2";
            LoadConfig(null);
            serial.Start();

            //Discover Devices 
            packetmanager.Subscrib(CommandType.DeviceEvent, FoundDevices);
            packetmanager.Subscrib(CommandType.DisconnectEvent, RecivedDisconnectEvnet);

            //Setup Information 
            packetmanager.Subscrib(CommandType.Info, DeviceInfoRecived );
            packetmanager.Subscrib(CommandType.AddrEvent, AddrsEventRecived);
            packetmanager.Subscrib(CommandType.Reset, BLEControllerReset);

            //Data Update 
            packetmanager.Subscrib(CommandType.DataEvent, RecivedDataEvnet);

            SendReset();

        }


        public void Stop()
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

        public void Reset()
        {
            Stop();
            LoadConfig("");
            Start();
        }


        //******************   Resived **************************


        private void FoundDevices(IPacket pack)
        {
            DeviceEvent newDevice = pack as DeviceEvent;
            Debug.Print("Found Device: " + newDevice.Address.ToHex());

            if(newDevice != null)
            {
                if (data.IsSystemDevice(newDevice.Address))
                {
                    if (!data.hasInfoFile(newDevice.Address))
                        Infofactory.GetDeviceInfo(newDevice.Address, DiscoverComplete);
                    else
                        Infofactory.GetDeviceInfo(newDevice.Address, DiscoverCompleteForCheck);
                }
                else
                {
                    data.AddToSeenDevices(newDevice);
                }
            }
        }

        private void DiscoverCompleteForCheck(DeviceInfoFactory.Status_t status, DeviceInfo item)
        {
            if (status == DeviceInfoFactory.Status_t.Done)
            {
                DeviceInfo info = data.GetDeviceInfo(item.Address);
                if (info != null && item.IsEqual(info))
                    AddDevice(info);
                else
                {
                    Infofactory.DoFullRead(item, DoneRead);
                }
            }
        }

        private void AddDevice(DeviceInfo item)
        {
            data.addConnectedDevice(item.Address);
            AddDeviceEvent add = new AddDeviceEvent();
            add.Address = item.Address;
            add.ConnectionTimeHandle = item.TimeHandel;
            add.PassCodeHandle = item.PassCodeHandel;
            packetmanager.Send(add);
            Thread.Sleep(1000);
        }

        private void DiscoverComplete(DeviceInfoFactory.Status_t status, DeviceInfo item)
        {
            Debug.Print("Done:");
            if (status == DeviceInfoFactory.Status_t.Done)
            {
                Debug.Print("ok");
                AddDevice(item);
                Infofactory.DoFullRead(item, DoneRead);
            }
        }

        private void DoneRead(DeviceInfoFactory.Status_t status, DeviceInfo item)
        {
            Debug.Print("Read Done:");
            if (status == DeviceInfoFactory.Status_t.Done)
            {
                data.UpdateOrCreateInfoFile(item);
            }
            else
            {
                data.DeleteSystemInfo(item.Address);
                data.DisconnectDevice(item.Address);
            }
        }

        // Called if the CC2540 reset. 
        public void BLEControllerReset(IPacket packet)
        {
            Debug.Print("BLE RESET");
            data.Reset();
            SendSystemInfo();
            Infofactory.Dispose();
            Infofactory = new DeviceInfoFactory(packetmanager);
        }

        public void RecivedDataEvnet(IPacket packet)
        {
            DataEvent DataPacket = packet as DataEvent;
            if (DataPacket != null)
            {
                data.GotData(DataPacket);
            }
        }

        public void RecivedDisconnectEvnet(IPacket packet)
        {
            DisconnectEvent DisPacket = packet as DisconnectEvent;
            if(DisPacket != null)
            {
                Debug.Print("Recived Dis :" + DisPacket.Address.ToHex());
                data.DisconnectDevice(DisPacket.Address);
            }
        }


        //******************   Init Mode *********************
        private void DeviceInfoRecived(IPacket packet)
        {
            NameEvent namePacket = packet as NameEvent;
            PassCodeEvent codePacket = packet as PassCodeEvent;

            if(namePacket != null)
            {
                SystemInfo.Name = namePacket.Name;
                return;
            }

            if(codePacket != null)
            {
                SystemInfo.PassCode = codePacket.Code;
                return;
            }

        }

        private void AddrsEventRecived(IPacket packet)
        {
            AddrEvent addr = packet as AddrEvent; 
            if(addr != null)
            {
                SystemInfo.SystemMAC = addr.Address;
            }
        }

        //******************   Send **************************


        public void SendSystemInfo()
        {
            SystemInfoEvent info = new SystemInfoEvent();
            info.SystemID = SystemInfo.SystemID;
            info.InitMode = IsInitMode;
            packetmanager.Send(info);
        }

        public void SendReset()
        {
            ResetEvent reset = new ResetEvent();
            packetmanager.Send(reset);
        }


    }
}
