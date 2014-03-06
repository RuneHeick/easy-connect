using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UartTester.Commands
{
    class GenericValueCommand:Command
    {
        const byte MainCommand = 1;

        private bool read;
        public bool Read
        {
            get
            {
                return read;
            }
            set
            {
                read = value;
                OnPropertyChanged("Read");
            }
        }

        private bool write;
        public bool Write
        {
            get
            {
                return write;
            }
            set
            {
                write = value;
                OnPropertyChanged("Write");
            }
        }

        private bool subscription;
        public bool Subscription
        {
            get
            {
                return subscription;
            }
            set
            {
                subscription = value;
                OnPropertyChanged("Subscription");
            }
        }

        private PresentationFormat format;
        public PresentationFormat Format
        {
            get
            {
                return format;
            }
            set
            {
                format = value;
                OnPropertyChanged("Format");
            }
        }

        private GUIFormat GUIformat;
        public GUIFormat GUI
        {
            get
            {
                return GUIformat;
            }
            set
            {
                GUIformat = value;
                OnPropertyChanged("GUI");
            }
        }

        private GuiColor gUIcolor;
        public GuiColor GUIcolor
        {
            get
            {
                return gUIcolor;
            }
            set
            {
                gUIcolor = value;
                OnPropertyChanged("GUIcolor");
            }
        }

        private byte gpio = 0;
        public byte GPIO
        {
            get
            {
                return gpio;
            }
            set
            {
                gpio = value; 
                OnPropertyChanged("GPIO");
            }
        }

        private byte maxSize;
        public byte MaxSize
        {
            get
            {
                return maxSize;
            }
            set
            {
                maxSize = value;
                OnPropertyChanged("MaxSize");
            }
        }

        private string description;
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }


        override public byte[] Payload
        {
            get
            {
                try
                {
                    byte reWrSub = (byte)((Convert.ToInt16(Subscription) << 2) + (Convert.ToInt16(Read) << 1) + (Convert.ToInt16(Write)));

                    byte[] descript = new byte[0];

                    if (Description!=null && Description.Length > 0)
                        descript = Encoding.UTF8.GetBytes(Description);

                    List<byte> packet = new List<byte>();
                    packet.Add(reWrSub);
                    packet.Add((byte)Format);
                    packet.Add((byte)GUI);
                    packet.Add((byte)GUIcolor);
                    packet.Add(GPIO);
                    packet.Add(MaxSize);
                    packet.AddRange(descript);

                    return packet.ToArray();
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                if (value.Length > 5)
                {
                    Subscription = Convert.ToBoolean((value[0] & 0x4) >> 2);
                    Read = Convert.ToBoolean((value[0] & 0x2) >> 1);
                    Write = Convert.ToBoolean((value[0] & 0x1));

                    Format = (PresentationFormat)value[1];

                    GUI = (GUIFormat) value[2];
                    GUIcolor= (GuiColor) value[3];
                    GPIO = value[4];
                    MaxSize = value[5];
                    byte[] descrip = new byte[value.Length - 6];
                    Array.Copy(value, 6, descrip, 0, value.Length - 6);
                    Description = Encoding.UTF8.GetString(descrip);
                    OnPropertyChanged("Payload");
                }
            }
        }

        public GenericValueCommand()
            : base(6, "Generic Value")
        {
        }

    }

    public enum PresentationFormat
    {
        Reserved = 0,
        Boolean = 1,
        unsigned2_bit = 2,
        unsigned4_bit = 3,
        unsigned8_bit = 4,
        unsigned12_bit = 5,
        unsigned16_bit = 6,
        unsigned24_bit = 7,
        unsigned32_bit = 8,
        unsigned48_bit = 9,
        unsigned64_bit = 10,
        unsigned128_bit = 12,
        signed8_bit = 13,
        signed12_bit = 14,
        signed16_bit = 15,
        signed24_bit = 16,
        signed32_bit = 17,
        signed48_bit = 18,
        signed64_bit = 19,
        signed128_bit = 20,
        bitfloating_32 = 21,
        bitfloating_64 = 22,
        bitSFLOAT_16 = 23,
        bitFLOAT_32 = 24,
        IEEE_20601format = 25,
        UTF_8string = 26,
        UTF_16string = 27
    }

    enum GUIFormat
    {
        Lable = 1,
        Field = 2,
        Slider = 3,
        List = 4,
        checkBox = 5,
        Time = 6,
        Date = 7,
        Time_Date = 8
    }

    enum GuiColor
    {
        White = 1,
        Silver = 2,

        Gray = 3,

        Black = 4,

        Red = 5,

        Maroon = 6,

        Yellow = 7,

        Olive = 8,

        Lime = 9,

        Green = 10,

        Aqua = 11,

        Teal = 12,

        Blue = 13,

        Navy = 14,

        Fuchsia = 15,

        Purple = 16
    }


}
