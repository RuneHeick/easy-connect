using System;
using Microsoft.SPOT;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using System.Threading;
using System.IO.Ports;
using System.Collections;

namespace ECRU.BLEController
{
    class SerialController
    {

        public Status SerialStatus { get; set; }
        public event Recived PacketRecived;
        private readonly object uartLock = new object(); 

        SerialPort serial = new SerialPort(SerialPorts.COM1, 115200, Parity.None, 8, StopBits.One);
       

        public void Start()
        {
            lock (uartLock)
            {
                try
                {
                    serial.Open();
                    serial.DataReceived += serialport_DataReceived;
                    serial.ErrorReceived += serial_ErrorReceived;
                    SerialStatus = Status.Running;
                }
                catch
                {
                    Stop();
                    SerialStatus = Status.Error;
                }
            }
        }

        public void Stop()
        {
            lock (uartLock)
            {
                if (serial.IsOpen)
                    serial.Close();

                serial.DataReceived -= serialport_DataReceived;
                serial.ErrorReceived -= serial_ErrorReceived;

                SerialStatus = Status.Stoped;
            }
        }

        public void Reset()
        {
            Stop(); 
            Start();
        }


        public bool SendByte(Byte[] data)
        {
            lock (uartLock)
            {
                if (serial.IsOpen)
                {
                    try
                    {
                        serial.Write(data, 0, data.Length);
                        return true;
                    }
                    catch
                    {
                        return false; 
                    }
                   
                }
                return false;
            }
        }

        ArrayList packet = new ArrayList(); 
        void serialport_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int len = serial.BytesToRead;
                byte[] data = new byte[len];

                serial.Read(data, 0, len);
                foreach (byte b in data)
                {
                    if (b == 0xEC && packet.Count == 0)
                        packet.Add(b);
                    else if (packet.Count > 0)
                    {
                        packet.Add(b);

                        if (packet.Count - 1 == (((byte)packet[1]) & 0x7F))
                        {
                            if (PacketRecived != null)
                                PacketRecived((byte[])packet.ToArray(typeof(byte)));
                            packet.Clear();
                        }
                        if (packet.Count > 128)
                        {
                            packet.Clear();
                        }

                    }
                }
            }
            catch
            {
                Stop();
                SerialStatus = Status.Error;
            }

        }

        void serial_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            SerialStatus = Status.Error;
            Reset(); 
        }


    }

    public delegate void Recived(byte[] packet);

    enum Status
    {
        Running,
        Stoped, 
        Error
    }
}
