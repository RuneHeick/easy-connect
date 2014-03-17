using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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

        private RelayCommand sendAllCommand;
        public ICommand SendAllCommand
        {
            get
            {
                if (sendAllCommand == null)
                    sendAllCommand = new RelayCommand((p) => sendAllCommandExecute(),(p) => sendAllCommandCanExecute());
                return sendAllCommand;
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

    }
}
