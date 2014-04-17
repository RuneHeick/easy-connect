using System;
using Microsoft.SPOT;
using ECRU.Utilities;
using ECRU.Utilities.Factories.ModuleFactory;
using ECRU.Utilities.EventBus;
using ECRU.BLEController.Packets;
using System.Threading;

namespace ECRU.BLEController
{
    public class BLEModule: IModule
    {
        SerialController serial = new SerialController();
        PacketManager packetmanager;
        DataManager data = new DataManager();

        public BLEModule()
        {
            packetmanager = new PacketManager(serial); 
        }

        public void LoadConfig(string configFilePath)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            serial.Start();
            packetmanager.Subscrib(CommandType.DeviceEvent, FoundDevices);
            packetmanager.Subscrib(CommandType.Reset, BLEControllerReset);
            packetmanager.Subscrib(CommandType.DataEvent, RecivedDataEvnet);
            packetmanager.Subscrib(CommandType.DisconnectEvent, RecivedDisconnectEvnet);
            packetmanager.Subscrib(CommandType.ServiceEvent, DiscoverCompleteEvent);

            SendReset();
        }

        private void DiscoverCompleteEvent(IPacket packet)
        {
            ServiceEvent Event = packet as ServiceEvent; 
            if(Event != null)
            {



            }
        }

        void test(object msg)
        {

        }

        public void Stop()
        {
            
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }


        //******************   Resived **************************


        private void FoundDevices(IPacket pack)
        {
            DeviceEvent newDevice = pack as DeviceEvent;
            if(newDevice != null)
            {
                if (data.IsSystemDevice(newDevice.Address))
                {
                    AddDeviceEvent packet = new AddDeviceEvent();
                    packet.Address = newDevice.Address;
                    packetmanager.Send(packet);
                    data.addConnectedDevice(newDevice.Address);
                }
                else
                {
                    data.AddToSeenDevices(newDevice);
                }

                DiscoverEvent Event = new DiscoverEvent();
                Event.Address = newDevice.Address;
                Event.EndHandle = 0xFFFF;
                Event.StartHandle = 0x0001;
                Event.Type = DiscoverType.Descriptors;
                packetmanager.Send(Event);
            }
        }


        public void BLEControllerReset(IPacket packet)
        {
            SendSystemInfo();
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

        }

        public void RecivedDicoverEvnet(IPacket packet)
        {
            DisconnectEvent DisPacket = packet as DisconnectEvent;

        }
        

        //******************   Send **************************


        public void SendSystemInfo()
        {

            SystemInfo.Name = "Test";
            SystemInfoEvent info = new SystemInfoEvent();
            info.Name = SystemInfo.Name;
            info.SystemID = SystemInfo.SystemID;
            info.InitMode = false;
            packetmanager.Send(info);
        }

        public void SendReset()
        {
            ResetEvent reset = new ResetEvent();
            packetmanager.Send(reset);
        }


    }
}
