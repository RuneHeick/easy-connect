using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UartTester.Commands
{
    class RangesViewModel:Command
    {
        const byte MainCommand = 1;

        private byte[] minimum = new byte[1];
        public byte[] Minimum
        {
            get
            {
                return minimum;
            }
            set
            {
                minimum = value;
                OnPropertyChanged("Minimum");
            }
        }

        private byte[] maximum = new byte[1];
        public byte[] Maximum
        {
            get
            {
                return maximum;
            }
            set
            {
                maximum = value;
                OnPropertyChanged("Maximum");
            }
        }

        override public byte[] Payload
        {
            get
            {
                if (minimum != null && Maximum != null)
                {
                    List<byte> packet = new List<byte>();
                    packet.AddRange(Minimum);
                    packet.AddRange(Maximum);
                    return packet.ToArray();
                }
                return null;
            }
            set
            {
                int len = value.Length;

                byte[] min = new byte[len / 2];
                byte[] max = new byte[len / 2];
                Array.Copy(value, 0, min, 0, len / 2);
                Array.Copy(value, len / 2, max, 0, len / 2);
                OnPropertyChanged("Payload");
            }
        }

        public RangesViewModel()
            : base(7, "Ranges")
        {
        }

    }
}
