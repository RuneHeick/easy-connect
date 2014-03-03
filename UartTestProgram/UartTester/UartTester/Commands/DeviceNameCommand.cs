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
            }
        }

        public byte[] DeviceNameByte
        {
            get
            {
                return Encoding.UTF8.GetBytes(DeviceName);
            }
            set
            {
                DeviceName = Encoding.UTF8.GetString(value);
            }
        }

        public DeviceNameCommand():base(1,"Device Name")
        {
            
        }

    }
}
