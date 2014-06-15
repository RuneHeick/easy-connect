using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO; 

namespace UartTester.ViewModel
{
    public class ItemsViewModel: ViewModelBase
    {
        public ObservableCollection<SerialCommand> Queue { get; set; }
        Task Sender = new Task(() => { });

        public ItemsViewModel()
        {
            Queue = new ObservableCollection<SerialCommand>();
        }

        public void Add(SerialCommand cmd)
        {
            Queue.Add(new SerialCommand(cmd.Packet.ToArray().ToList<byte>()));
        }

        private RelayCommand saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                    saveCommand = new RelayCommand((p) => saveCommandExecute(), (o) => Queue.Count > 0);
                return saveCommand;
            }
        }

        private void saveCommandExecute()
        {
            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".PACK"; // Default file extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                StreamWriter file = new StreamWriter(filename);
                foreach (SerialCommand c in Queue)
                {
                    file.WriteLine(BitConverter.ToString(c.Packet.ToArray()).Replace("-"," "));
                }
                file.Close(); 
            }
        }

        private RelayCommand loadCommand;
        public ICommand LoadCommand
        {
            get
            {
                if (loadCommand == null)
                    loadCommand = new RelayCommand((p) => loadCommandExecute());
                return loadCommand;
            }
        }

        private RelayCommand sendAllCommand;
        public ICommand SendAllCommand
        {
            get
            {
                if (sendAllCommand == null)
                    sendAllCommand = new RelayCommand((p) => sendAllCommandExecute(), (o) => Queue.Count>0);
                return sendAllCommand;
            }
        }

        private void loadCommandExecute()
        {
            // Configure save file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".PACK"; // Default file extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                string[] lines = System.IO.File.ReadAllLines(filename);

                Queue.Clear();

                foreach (string line in lines)
                {
                    string l = line.Trim();
                    l = l.Replace(" ", "");
                    Queue.Add(new SerialCommand(StringToByteArray(l).ToList()));
                }

            }
        }

        private void sendAllCommandExecute()
        {
            if (Sender.Status == TaskStatus.Running)
                Sender.Wait();

            Sender = new Task(() => SendPacketes(Queue.ToArray()));
            Sender.Start();
        }

        void SendPacketes(SerialCommand[] Commands)
        {
            foreach(SerialCommand c in Commands)
            {
                SerialViewModel.Serial.SendCommandGetResponse(c, 3);
            }
        }

        private bool sendAllCommandCanExecute()
        {
            return Sender.Status != TaskStatus.Running;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

    }
}
