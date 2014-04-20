using System;
using Microsoft.SPOT;

namespace ECRU.BLEController.Util
{
    public class DeviceInfo
    {
        public byte[] Address { get; set; }

        public UInt16 TimeHandel { get; set; }
        public UInt16 PassCodeHandel { get; set; }


        public HandleValuePair Name { get; set; }
        public HandleValuePair Model { get; set; }
        public HandleValuePair Serial { get; set; }
        public HandleValuePair Manufacture { get; set; }

        public Service[] Services { get; set; }

        public DeviceInfo()
        {
            Name = new HandleValuePair();
            Name.handle = 0; 
            Model = new HandleValuePair();
            Model.handle = 0; 
            Serial = new HandleValuePair();
            Serial.handle = 0; 
            Manufacture = new HandleValuePair();
            Manufacture.handle = 0; 
        }

        public bool isCompleted()
        {
            if(TimeHandel != 0 && PassCodeHandel != 0 && Name.handle != 0 && Model.handle != 0 && Serial.handle != 0 && Manufacture.handle != 0)
            {
                if(Services != null)
                {
                    for(int i = 0; i<Services.Length; i++)
                    {
                        if(Services[i] == null || !Services[i].isCompleted())
                        {
                            return false; 
                        }

                    }
                    return true; 
                }
            }
            return false; 
        }
    }

    public class Service
    {
        public UInt16 StartHandel { get; set;  }
        public UInt16 EndHandel { get; set; }

        public HandleValuePair Description { get; set; }

        public UInt16 UpdateHandel { get; set; }

        public Characteristic[] Characteristics { get; set; }

        public Service()
        {
            Description = new HandleValuePair();
            StartHandel = 0;
            EndHandel = 0;
            Description.handle = 0;
            UpdateHandel = 0; 
        }

        public bool isCompleted()
        {
            if( StartHandel != 0 && EndHandel != 0 && Description.handle != 0 && UpdateHandel != 0)
            {
                if(Characteristics != null)
                {
                    for(int i = 0; i<Characteristics.Length; i++)
                    {
                        if(Characteristics[i] == null)
                        {
                            return false; 
                        }
                    }
                    return true;
                }
            }
            return false;

        }

    }

    public class Characteristic
    {
        public CharacteristicValueHandle Value { get; set; }
        public HandleValuePair Description { get; set; }
        public HandleValuePair Format { get; set; }
        public HandleValuePair GUIFormat { get; set; }
        public HandleValuePair Range { get; set; }
        public HandleValuePair Subscription { get; set; }

        public Characteristic()
        {
            Value = new CharacteristicValueHandle();
            Description = new HandleValuePair();
            Format = new HandleValuePair();
            GUIFormat = new HandleValuePair();
            Range = new HandleValuePair();
            Subscription = new HandleValuePair(); 
        }
    }

    public class HandleValuePair : IHandleValue
    {
        public UInt16 handle { get; set; }
        public byte[] Value { get; set; }
    }

    public class CharacteristicValueHandle : IHandleValue
    {
        public UInt16 handle { get; set; }

        public byte ReadWriteProps { get; set;  }

        public byte[] Value
        {
            get
            {
                return null; 
            }
            set
            {

            }
        }
    }

    public interface IHandleValue
    {
        UInt16 handle { get; set; }
        byte[] Value { get; set; }
    }

}
