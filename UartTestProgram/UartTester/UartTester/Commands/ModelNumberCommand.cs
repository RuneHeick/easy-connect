using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UartTester.Commands
{
    class ModelNumberCommand:Command
    {
        const byte MainCommand = 1;

        private string modelNumber;
        public string ModelNumber
        {
            get
            {
                return modelNumber;
            }
            set
            {
                modelNumber = value;
                OnPropertyChanged("ModelNumber");
            }
        }

        override public byte[] Payload
        {
            get
            {
                if (ModelNumber == null) return null; 
                return Encoding.UTF8.GetBytes(ModelNumber);
            }
            set
            {
                ModelNumber = Encoding.UTF8.GetString(value);
                OnPropertyChanged("Payload");
            }
        }

        public ModelNumberCommand()
            : base(3, "Model Number")
        {
        }

    }
}
