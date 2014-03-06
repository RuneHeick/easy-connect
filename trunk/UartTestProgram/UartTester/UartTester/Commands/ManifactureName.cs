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

        public string manifactureName;
        public string ManifactureName
        {
            get
            {
                return manifactureName;
            }
            set
            {
                manifactureName = value;
                OnPropertyChanged("ManifactureName");
            }
        }

        override public byte[] Payload
        {
            get
            {
                if (ManifactureName == null) return null; 
                return Encoding.UTF8.GetBytes(ManifactureName);
            }
            set
            {
                ManifactureName = Encoding.UTF8.GetString(value);
                OnPropertyChanged("Payload");
            }
        }

        public ManifactureNameCommand()
            : base(2, "Manifacture Name")
        {
        }

    }
}
