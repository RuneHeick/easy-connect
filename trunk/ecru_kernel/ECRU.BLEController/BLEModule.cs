using System;
using Microsoft.SPOT;
using ECRU.Utilities;
using ECRU.Utilities.Factories.ModuleFactory;
using ECRU.Utilities.EventBus;
using ECRU.BLEController.Packets;
using System.Threading;
using ECRU.BLEController.Util;

namespace ECRU.BLEController
{
    public class BLEModule: IModule
    {
        SerialController serial = new SerialController();
        PacketManager packetmanager;
        DataManager data;
        DeviceInfoFactory fac; 

        string ConfigPath = "";

        bool IsInitMode { get; set; }

        public BLEModule()
        {
            packetmanager = new PacketManager(serial);
            data = new DataManager(packetmanager);
            fac = new DeviceInfoFactory(packetmanager);
        }

        public void LoadConfig(string configFilePath)
        {
            ConfigPath = configFilePath;
        }

        public void Start()
        {
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
            packetmanager.Subscrib(CommandType.ServiceEvent, RecivedServicDirEvnet);

            IsInitMode = false; 

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
            packetmanager.Unsubscrib(CommandType.ServiceEvent, RecivedServicDirEvnet);
            data.Reset(); 
        }

        public void Reset()
        {
            Stop();
            Start();
        }


        //******************   Resived **************************


        private void FoundDevices(IPacket pack)
        {
            DeviceEvent newDevice = pack as DeviceEvent;
            if(newDevice != null)
            {
                if (data.IsSystemDevice(newDevice.Address))
                {
                    fac.GetDeviceInfo(newDevice.Address, DiscoverComplete);
                }
                else
                {
                    data.AddToSeenDevices(newDevice);
                }
            }
        }

        private void DiscoverComplete(DeviceInfoFactory.Status_t status, DeviceInfo item)
        {
            Debug.Print("Done:");
            if (status == DeviceInfoFactory.Status_t.Done)
            {
                Debug.Print("ok");
                AddDeviceEvent add = new AddDeviceEvent();
                add.Address = item.Address;
                add.ConnectionTimeHandle = item.TimeHandel;
                add.PassCodeHandle = item.PassCodeHandel;

                packetmanager.Send(add);

                fac.DoFullRead(item, Done);
            }
            else
                Debug.Print("Fail");
        }

        private void Done(DeviceInfoFactory.Status_t status, DeviceInfo item)
        {
            Debug.Print("Read Done:");
            if (status == DeviceInfoFactory.Status_t.Done)
            {
                Debug.Print("ok");
            }
            else
            {
                Debug.Print("fail");
            }
        }

        // Called if the CC2540 reset. 
        public void BLEControllerReset(IPacket packet)
        {
            data.Reset();
            SendSystemInfo();
            fac.Dispose();
            fac = new DeviceInfoFactory(packetmanager);
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
                data.DisconnectDevice(DisPacket.Address);
            }
        }

        public void RecivedServicDirEvnet(IPacket packet)
        {
            ServiceEvent serviceDir = packet as ServiceEvent;
            if (serviceDir != null)
            {
                data.RecivedServiceDir(serviceDir);
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
