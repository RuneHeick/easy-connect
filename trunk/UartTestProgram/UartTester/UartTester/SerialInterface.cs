using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows;
using System.Threading;
using System.Windows.Threading;

namespace UartTester
{
    public class SerialInterface
    {
        private SerialPort serialport;
        private SerialCommand LastPacket;
        public static ObservableCollection<SerialCommand> Log { get; set; }

        public static string[] Ports
        {
            get
            {
                return SerialPort.GetPortNames(); 
            }
        }

        public SerialInterface(string ComPort)
        {
            try
            {
                serialport = new SerialPort(ComPort, 115200, Parity.None, 8, StopBits.One);
                serialport.Open();
                serialport.DataReceived += serialport_DataReceived;
                Log.Clear();
            }
            catch
            {
                MessageBox.Show("Serial port could not Open"); 
            }
        }

        static SerialInterface()
        {
            Log = new ObservableCollection<SerialCommand>();
        }

        ~SerialInterface()
        {
            Close();
            serialport.DataReceived -= serialport_DataReceived;
        }

        public void Close()
        {
            if (serialport.IsOpen)
                serialport.Close(); 
        }

        public void SendCommand(SerialCommand cmd)
        {
            if (cmd != null)
            {
                if (serialport.IsOpen && cmd.Packet.Count < 128)
                {
                    serialport.Write(cmd.Packet.ToArray(), 0, cmd.Packet.Count);
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                    Log.Add(new SerialCommand(cmd.Packet.ToArray().ToList()));
                    }));
                }
                if (!(cmd.Packet.Count < 128))
                {
                    MessageBox.Show("To long Packet");
                }
            }

        }

        public SerialCommand SendCommandGetResponse(SerialCommand cmd, int retrys)
        {
            LastPacket = null; 
            for (int i = 0; i < retrys; i++)
            {
                SendCommand(cmd);
                Thread.Sleep(200);
                if (LastPacket != null)
                    return LastPacket;
            }
            return null; 
        }



        private List<byte> buffer = new List<byte>(128);  
        void serialport_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int bytetoread = serialport.BytesToRead;
                byte[] resived = new byte[bytetoread];
                serialport.Read(resived, 0, bytetoread);

                if (buffer.Count == 0)
                {
                    buffer.Add(resived[0]);
                    buffer.Add(resived[1]);
                    --bytetoread;
                    byte[] temp = new byte[--bytetoread];
                    if (temp != null)
                    {
                        Array.Copy(resived, 2, temp, 0, bytetoread);
                        resived = temp;
                    }
                }

                if (buffer.Count > 0)
                {
                    int len = (buffer[1] & 0x7F) + 1;

                    if (len >= buffer.Count + bytetoread)
                    {
                        buffer.AddRange(resived);
                    }
                    else
                    {
                        for (int i = 0; buffer.Count < len; i++)
                        {
                            buffer.Add(resived[i]);
                        }
                    }

                    if (buffer.Count == len)
                    {
                        LastPacket = new SerialCommand(buffer);
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {

                            Log.Add(new SerialCommand(buffer.ToArray().ToList()));

                        }));


                        buffer = new List<byte>();
                    }
                    if (buffer.Count > len)
                    {
                        buffer.Clear(); // skulle aldrig ske;
                    }
                }
            }
            catch
            {
                buffer.Clear();
            }

        }


    }
}
