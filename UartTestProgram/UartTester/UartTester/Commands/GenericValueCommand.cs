using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UartTester.Commands
{
    public class GenericValueCommand:Command
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
                OnPropertyChanged("MaxSize");
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

        public byte MaxSize
        {
            get
            {
                if (Format == null)
                    return 0; 
                return Format.ByteSize == null ? (byte)0 : Format.ByteSize.Value;
            }
            set
            {
                if (Format == null) return;
                Format.ByteSize = value;
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
                    packet.Add(Format.Value);
                    
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

                    Format.Value = value[1];

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

    public class PresentationFormat
    {
        public string Name {get; private set;}
        public byte Value {get; set;}
        public byte? ByteSize {get; set;}

        public PresentationFormat()
        {
        }

        public PresentationFormat(string Name, byte Value, byte? ByteSize)
        {
            this.Name = Name;
            this.ByteSize = ByteSize;
            this.Value = Value; 
        }

        private static PresentationFormat[] items = new PresentationFormat[]
                {
                    new PresentationFormat("Boolean",1,1),
                    new PresentationFormat("unsigned2_bit",2,1),
                    new PresentationFormat("unsigned4_bit",3,1),
                    new PresentationFormat("unsigned8_bit",4,1),
                    new PresentationFormat("unsigned12_bit",5,2),
                    new PresentationFormat("unsigned16_bit",6,2),
                    new PresentationFormat("unsigned24_bit",7,3),
                    new PresentationFormat("unsigned32_bit",8,4),
                    new PresentationFormat("unsigned48_bit",9,6),
                    new PresentationFormat("unsigned64_bit",10,4),
                    new PresentationFormat("unsigned128_bit",11,16),
                    new PresentationFormat("signed8_bit",12,1),
                    new PresentationFormat("signed12_bit",13,2),
                    new PresentationFormat("signed16_bit",14,2),
                    new PresentationFormat("signed24_bit",15,3),
                    new PresentationFormat("signed32_bit",16,4),
                    new PresentationFormat("signed48_bit",17,6),
                    new PresentationFormat("signed64_bit",18,8),
                    new PresentationFormat("signed128_bit",19,16),
                    new PresentationFormat("bitfloating_32",20,4),
                    new PresentationFormat("bitfloating_64",21,8),
                    new PresentationFormat("bitSFLOAT_16",22,2),
                    new PresentationFormat("bitFLOAT_32",23,4),
                    new PresentationFormat("IEEE_20601format",24,4),
                    new PresentationFormat("UTF_8string",25,null),
                    new PresentationFormat("UTF_16string",26,null)
                };
        public static PresentationFormat[] Items
        {
            get
            {
                return items; 
            }
        }

    }


    public enum GUIFormat
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

    public enum GuiColor
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
