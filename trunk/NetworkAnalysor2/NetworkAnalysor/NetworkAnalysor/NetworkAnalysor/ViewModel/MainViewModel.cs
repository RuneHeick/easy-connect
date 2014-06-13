using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Net;
using System.Windows;

namespace NetworkAnalysor.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<ECRU> UnitsDiscovered { get; set; }

        private UdpClient NetworkSocket;
        public MainViewModel()
        {
            UnitsDiscovered = new ObservableCollection<ECRU>();

            NetworkSocket = new UdpClient(new IPEndPoint(IPAddress.Any, 4543));
            NetworkSocket.BeginReceive(PacketRecived, null);

            ECRU Test = new ECRU() { Mac = new byte[] { 0x05, 0x06, 0x07, 0x0A, 0x0B, 0x0C }, NetState = new byte[] { 0x12, 0x31, 0x43, 0x43, 0x43, 0x0C } };
            Test.ECMS.Add(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 });
            Test.ECMS.Add(new byte[] { 0x01, 0x05, 0x03, 0x04, 0x05, 0x06 });

            UnitsDiscovered.Add(Test);
            UnitsDiscovered.Add(new ECRU());
        }

        private void PacketRecived(IAsyncResult ar)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 4543);
            Byte[] receiveBytes = NetworkSocket.EndReceive(ar, ref endPoint);
            if(receiveBytes.Length>0)
            {
                switch (receiveBytes[0])
                {
                    case 1: //ECRU Her er jeg 
                        {
                            if(receiveBytes.Length == 39)
                            {
                                UpdateECRU(receiveBytes, endPoint.Address); 
                            }
                        }
                        break;
                    case 3: //Got Device
                    case 4: //Lost Device 
                        {
                            if (receiveBytes.Length == 13)
                            {
                                try
                                {
                                    byte[] ecru = new byte[6];
                                    Array.Copy(receiveBytes, 1, ecru, 0, 6);

                                    ECRU element = UnitsDiscovered.First((o) => ArraysEqual(o.Mac,ecru));
                                    if(element != null)
                                    {
                                        byte[] ECMmac = new byte[6];
                                        Array.Copy(receiveBytes, 7, ECMmac, 0, 6);
                                        if(receiveBytes[0] == 3)
                                            element.ECMS.Add(ECMmac); 
                                        else if(receiveBytes[0] == 4)
                                            element.ECMS.Remove(ECMmac); 
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }
                        break; 
                }
            }
            NetworkSocket.BeginReceive(PacketRecived, null);
        }

        private void UpdateECRU(byte[] receiveBytes, IPAddress iPAddress)
        {
            byte[] mac = new byte[6];
            byte[] netState = new byte[32];
            Array.Copy(receiveBytes, 1, mac, 0, 6);
            Array.Copy(receiveBytes, 1+6, netState, 0, 32);
            
            ECRU unit = new ECRU {
                LastSeen = DateTime.Now,
                NetState = netState,
                Mac = mac, 
                IPAddres = iPAddress.ToString()
            };

            if(!UnitsDiscovered.Contains(unit,new Compare()))
            {
                Application.Current.Dispatcher.Invoke(()=>UnitsDiscovered.Add(unit)); 
            }
            else
            {
                ECRU oldunit = UnitsDiscovered.First((a) => ArraysEqual(a.Mac, mac));
                oldunit.LastSeen = DateTime.Now;
                oldunit.NetState = netState; 
            }
        }

        class Compare : IEqualityComparer<ECRU>
        {
            public bool Equals(ECRU x, ECRU y)
            {
                return ArraysEqual(x.Mac, y.Mac); 
            }
            public int GetHashCode(ECRU codeh)
            {
                return codeh.GetHashCode();
            }
        }

        static bool ArraysEqual(byte[] a1, byte[] a2)
        {
            if (a1 == a2)
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                    return false; 
            }
            return true;
        }

        private RelayCommand clearCommand;
        public ICommand ClearCommand
        {
            get
            {
                if (clearCommand == null)
                    clearCommand = new RelayCommand((p) => clearCommandExecute());
                return clearCommand;
            }
        }

        private void clearCommandExecute()
        {
            UnitsDiscovered.Clear();
        }

    }
}
