using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UartTester.Commands
{
    class SerialNoCommand:Command
    {
        const byte MainCommand = 1;

        private string serial; 
        public string Serial
        {
            get
            {
                return serial;
            }
            set
            {
                serial = value;
                OnPropertyChanged("Serial");
            }
        }

        override public byte[] Payload
        {
            get
            {
                if (Serial == null) return null; 
                return Encoding.UTF8.GetBytes(Serial);
            }
            set
            {
                Serial = Encoding.UTF8.GetString(value);
                OnPropertyChanged("Payload");
            }
        }

        public SerialNoCommand()
            : base(4, "Serial No.")
        {
        }

    }
}
