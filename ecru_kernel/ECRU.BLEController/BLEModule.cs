using System;
using Microsoft.SPOT;
using ECRU.Utilities;
using ECRU.Utilities.Factories.ModuleFactory;

namespace ECRU.BLEController
{
    public class BLEModule: IModule
    {
        SerialController serial = new SerialController();
        PacketManager packetmanager; 

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
            packetmanager.Subscrib(0x11, handel);
        }

        public void handel(IPacket b)
        {

        }

        public void Stop()
        {
            
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }



    }
}
