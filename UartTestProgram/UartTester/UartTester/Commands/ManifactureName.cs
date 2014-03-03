using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UartTester.Commands
{
    class ManifactureNameCommand:Command
    {
        const byte MainCommand = 1;

        public string ManifactureName { get; set; }

        public byte[] ManifactureNameByte
        {
            get
            {
                return Encoding.UTF8.GetBytes(ManifactureName);
            }
            set
            {
                ManifactureName = Encoding.UTF8.GetString(value);
            }
        }

        public ManifactureNameCommand()
            : base(2, "Manifacture Name")
        {
        }

    }
}
