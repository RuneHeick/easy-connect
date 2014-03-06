using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UartTester.Commands
{
    class DeviceNameCommand:Command
    {
        const byte MainCommand = 1;

        private string deviceName;
        public string DeviceName
        {
            get
            {
                return deviceName; 
            }
            set
            {
                if (value.Length < 24)
                    deviceName = value;
                else
                    throw new FormatException("to long");
                OnPropertyChanged("DeviceName");
            }
        }

        override public byte[] Payload
        {
            get
            {
                if (DeviceName == null) return null;
                return Encoding.UTF8.GetBytes(DeviceName);
            }
            set
            {
                DeviceName = Encoding.UTF8.GetString(value);
                OnPropertyChanged("Payload");
            }
        }

        public DeviceNameCommand():base(1,"Device Name")
        {
            
        }

    }
}
