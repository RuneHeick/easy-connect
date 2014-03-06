using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UartTester.Commands
{
    class SmartFunftionCommand:Command
    {
        const byte MainCommand = 1;

        private string functionDescription; 
        public string FunctionDescription
        {
            get
            {
                return functionDescription;
            }
            set
            {
                functionDescription = value;
                OnPropertyChanged("FunctionDescription");
            }
        }

        override public byte[] Payload
        {
            get
            {
                if (FunctionDescription == null) return null; 
                return Encoding.UTF8.GetBytes(FunctionDescription);
            }
            set
            {
                FunctionDescription = Encoding.UTF8.GetString(value);
                OnPropertyChanged("Payload");
            }
        }

        public SmartFunftionCommand()
            : base(5, "Smart Funcrion (service)")
        {
        }

    }
}
