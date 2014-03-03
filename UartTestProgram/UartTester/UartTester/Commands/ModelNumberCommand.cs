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

        public string ModelNumber { get; set; }

        public byte[] ModelNumberByte
        {
            get
            {
                return Encoding.UTF8.GetBytes(ModelNumber);
            }
            set
            {
                ModelNumber = Encoding.UTF8.GetString(value);
            }
        }

        public ModelNumberCommand()
            : base(3, "Model Number")
        {
        }

    }
}
