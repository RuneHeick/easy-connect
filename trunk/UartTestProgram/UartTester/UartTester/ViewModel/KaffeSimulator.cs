using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UartTester.ViewModel
{
    public class KaffeSimulator:ViewModelBase
    {
        private int stride;
        private byte [] ImageInfo;
        private int pWidth;
        private int pHeight;

        private bool simulationRunning = false; 

        byte targetRed;
        byte targetGreen;
        byte targetBlue;
        byte targetAlpha;

        
        private byte[] UpdateHandle
        {
            get
            {
                return new byte[] { 0x01, 0x00 }; 
            }
        }
        public WriteableBitmap SourceImage { get; set; }

        private byte[] antalkopperHandle { get; set; }
        private int antalKopper = 5; 
        public int AntalKopper 
        { 
            get
            {
                return antalKopper; 
            }
            set
            {
                if (value < 1)
                    antalKopper = 1;
                else if (value > 7)
                    antalKopper = 8;
                else
                    antalKopper = value;

                OnPropertyChanged("AntalKopper");
            }
        
        }

        private byte[] statusHandle { get; set; }
        private string status = "Klar";
        public string Status
        {
            get
            {
                return status; 
            }
            set
            {
                status = value;
                UpdateStatusOnBLEUnit(value); 
                OnPropertyChanged("Status"); 
            }
        }

        public KaffeSimulator()
        {
            SourceImage = new WriteableBitmap(new BitmapImage(new Uri("pack://application:,,,/UartTester;component/Kaffekande.bmp")));

            pWidth = SourceImage.PixelWidth;
            pHeight = SourceImage.PixelHeight; 

            stride = SourceImage.PixelWidth * 4;
        }


        private RelayCommand startCommand;
        public ICommand StartCommand
        {
            get
            {
                if (startCommand == null)
                    startCommand = new RelayCommand((p) => startCommandExecute());
                return startCommand;
            }
        }

        private RelayCommand setupCommand;
        public ICommand SetupCommand
        {
            get
            {
                if (setupCommand == null)
                    setupCommand = new RelayCommand((p) => setupCommandExecute());
                return setupCommand;
            }
        }

        private void setupCommandExecute()
        {
            Status = "Sætter op";
            string filename = "Kaffe.PACK";
            string[] lines = System.IO.File.ReadAllLines(filename);

            foreach (string line in lines)
            {
                string l = line.Trim();
                l = l.Replace(" ", "");
                SerialCommand cmd = new SerialCommand(ItemsViewModel.StringToByteArray(l).ToList());
                SerialCommand reply = SerialViewModel.Serial.SendCommandGetResponse(cmd, 3);
                if(reply == null)
                {
                    Status = "Fejl: ingen svar ved Opsætning";
                    return; 
                }

                if(cmd.MainCommand == 1 && cmd.SubCommand == 6) // Generic Value command
                {
                    if(reply.Payload != null && reply.Payload.Length<2)
                    {
                        Status = "Fejl: Modtog Nack i Opsætning";
                        return; 
                    }
                    else
                    {
                        if(antalkopperHandle == null)
                        {
                            antalkopperHandle = new byte[2];
                            Array.Copy(reply.Payload, 1, antalkopperHandle, 0, 2); 
                        }
                        else
                        {
                            statusHandle = new byte[2];
                            Array.Copy(reply.Payload, 1, statusHandle, 0, 2); 
                        }
                    }
                }
            }
            SerialViewModel.Serial.PacketRecived += Serial_PacketRecived;
            Status = "Klar"; 
        }

        void Serial_PacketRecived(SerialCommand obj)
        {
            if(obj.IsResponse == false && obj.IsCrcOK == true)
            {
                if(obj.SubCommand == 1 && obj.MainCommand == 3) //Data Update Notification; 
                {
                    if(obj.Payload.Length > 2 && obj.Payload[0] == antalkopperHandle[0] && obj.Payload[1] == antalkopperHandle[1]) // Antal kopper update
                    {
                        AntalKopper = obj.Payload[2];
                    }

                    if (obj.Payload[0] == UpdateHandle[0] && obj.Payload[1] == UpdateHandle[1]) // Antal kopper update
                    {
                        StartSimulation(); 
                    }
                }
            }
        }

        private void StartSimulation()
        {
            if(!simulationRunning)
            {
                simulationRunning = true;
                Status = "Brygger "+AntalKopper.ToString()+ (AntalKopper == 1 ? " Kop" : " Kopper"); 
                startCommandExecute();
            }
        }

        private void UpdateStatusOnBLEUnit(string value)
        {
            if(statusHandle != null)
            {
                byte[] status = System.Text.Encoding.UTF8.GetBytes(value);
                byte[] data = new byte[status.Length + statusHandle.Length];

                Array.Copy(statusHandle, 0, data, 0, statusHandle.Length);
                Array.Copy(status, 0, data,statusHandle.Length, status.Length); 

                SerialCommand writeCommand = new SerialCommand();
                writeCommand.MainCommand = 4;
                writeCommand.SubCommand = 0;
                writeCommand.Payload = data;

                SerialViewModel.Serial.SendCommand(writeCommand);
            }
        }

        private void startCommandExecute()
        {
            SourceImage = new WriteableBitmap(new BitmapImage(new Uri("pack://application:,,,/UartTester;component/Kaffekande.bmp")));
            int size = SourceImage.PixelHeight * stride;
            ImageInfo = new byte[size];

            SourceImage.CopyPixels(ImageInfo, stride, 0);
            int Centerindex = ((int)(SourceImage.PixelHeight / 2) * stride) + (4 * (int)(SourceImage.PixelWidth / 2));
            targetRed = ImageInfo[Centerindex];
            targetGreen = ImageInfo[Centerindex + 1];
            targetBlue = ImageInfo[Centerindex + 2];
            targetAlpha = ImageInfo[Centerindex + 2];
            Thread t = new Thread(() => DoAnimation(AntalKopper));
            t.IsBackground = true; 
            t.Start();
        }



        private void DoAnimation(int AntalKopper)
        {

            int del = 680 - (((680 - 230) / 8) * AntalKopper);

            for (int y = 680; y > del; y--)
            {
                for (int x = 0; x < pWidth; x++)
                {
                    int index = y * stride + 4 * x;

                    byte red = ImageInfo[index];
                    byte green = ImageInfo[index + 1];
                    byte blue = ImageInfo[index + 2];
                    byte alpha = ImageInfo[index + 3];

                    if (red == targetRed && green == targetGreen && blue == targetBlue && alpha == targetAlpha)
                    {
                        ImageInfo[index] = 0;
                        ImageInfo[index + 1] = 0;
                        ImageInfo[index + 2] = 0;
                    }

                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    SourceImage.WritePixels(new Int32Rect(0, 0, SourceImage.PixelWidth, SourceImage.PixelHeight), ImageInfo, SourceImage.PixelWidth * SourceImage.Format.BitsPerPixel / 8, 0);
                    OnPropertyChanged("SourceImage");
                });

                Thread.Sleep(10000 / 230);
            }

            Status = "Done";
            simulationRunning = false;
        }






    }
}
