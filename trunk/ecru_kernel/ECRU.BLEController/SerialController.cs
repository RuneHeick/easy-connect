using System;
using System.Collections;
using System.IO.Ports;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using ECRU.Utilities.HelpFunction;
using Microsoft.SPOT;

namespace ECRU.BLEController
{
    internal class SerialController
    {
        private readonly ArrayList packet = new ArrayList();
        private readonly SerialPort serial = new SerialPort(SerialPorts.COM1, 115200, Parity.None, 8, StopBits.One);
        private readonly object uartLock = new object();
        public Status SerialStatus { get; set; }
        public event Recived PacketRecived;
        private byte[] lastPacket { get; set; }

        public void Start()
        {
            lock (uartLock)
            {
                try
                {
                    lastPacket = new byte[1]; 
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
                        lastPacket = new byte[1];
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

        private void serialport_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int len = serial.BytesToRead;
                var data = new byte[len];

                int itemsRead = serial.Read(data, 0, len);
                if (itemsRead > 0)
                {
                    foreach (byte b in data)
                    {
                        Debug.Print(b.ToHex());
                        if (b == 0xEC && packet.Count == 0)
                            packet.Add(b);
                        else if (packet.Count > 0)
                        {
                            packet.Add(b);

                            if (packet.Count - 1 == (((byte)packet[1]) & 0x7F))
                            {
                                byte[] newPacket = (byte[])packet.ToArray(typeof(byte));
                                if (!lastPacket.ByteArrayCompare(newPacket))
                                {
                                    if (PacketRecived != null)
                                        PacketRecived(newPacket);
                                }
                                lastPacket = newPacket;
                                packet.Clear();
                            }
                            if (packet.Count > 128)
                            {
                                packet.Clear();
                            }
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
        

        private void serial_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            SerialStatus = Status.Error;
            Reset();
        }
    }

    public delegate void Recived(byte[] packet);

    internal enum Status
    {
        Running,
        Stoped,
        Error
    }
}