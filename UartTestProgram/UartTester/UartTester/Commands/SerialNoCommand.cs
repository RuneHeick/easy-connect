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

        public string Serial { get; set; }

        public byte[] SerialByte
        {
            get
            {
                return Encoding.UTF8.GetBytes(Serial);
            }
            set
            {
                Serial = Encoding.UTF8.GetString(value);
            }
        }

        public SerialNoCommand()
            : base(4, "Serial No.")
        {
        }

    }
}
