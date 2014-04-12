using System;
using Microsoft.SPOT;
using ECRU.Utilities;
using ECRU.Utilities.Factories.ModuleFactory;
using ECRU.Utilities.EventBus;

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

            Utilities.EventBus.EventBus.Subscribe(typeof(int), test);
            Utilities.EventBus.EventBus.Subscribe(typeof(object), test);

        
            Utilities.EventBus.EventBus.Publish(2);
            Utilities.EventBus.EventBus.Publish(new EventBus.Messages.Reset());
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



    }
}
