using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UartTester.Commands;

namespace UartTester.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<SerialCommand> SerialCommands { get; set; }

        public ObservableCollection<Command> MainCommands { get; set; }
        public ObservableCollection<string> SubCommands { get; set; }

        public ItemsViewModel Items { get; set;  }

        private Command selectedMainCommand;
        public Command SelectedMainCommand
        {
            set
            {
                if(selectedMainCommand!=null)
                    selectedMainCommand.PropertyChanged -= selectedMainCommand_PropertyChanged;
                selectedMainCommand = value;
                selectedMainCommand.PropertyChanged += selectedMainCommand_PropertyChanged;
                OnPropertyChanged("SelectedMainCommand");
                OnPropertyChanged("serialCommandFromSelected");
            }
            get
            {
                return selectedMainCommand;
            }
        }

        private Command selectedSubCommand;
        public Command SelectedSubCommand
        {
            set
            {
                if (selectedSubCommand != null)
                    selectedSubCommand.PropertyChanged -= selectedMainCommand_PropertyChanged;
                selectedSubCommand = value;
                if (selectedSubCommand != null)
                    selectedSubCommand.PropertyChanged += selectedMainCommand_PropertyChanged;
                OnPropertyChanged("SelectedSubCommand");
                OnPropertyChanged("serialCommandFromSelected");
            }
            get
            {
                return selectedSubCommand;
            }
        }

        private SerialCommand serialCommandFromSelected = new SerialCommand();
        public SerialCommand SerialCommandFromSelected
        {
            get
            {
                serialCommandFromSelected.MainCommand = SelectedMainCommand.Number;
                serialCommandFromSelected.SubCommand = SelectedSubCommand == null ? 0 : SelectedSubCommand.Number;
                serialCommandFromSelected.Payload = SelectedSubCommand == null ? SelectedMainCommand.Payload : SelectedSubCommand.Payload;
                return serialCommandFromSelected;
            }
        }

        public SerialViewModel SerialSetup { get; set; }


        public enum MainCommandsValue
        {
            Setup = 0x01,
            Clear = 0x02,
            Read = 0x03,
            Write = 0x04
        }

        public MainViewModel()
        {
            Items = new ItemsViewModel(); 
            SerialSetup = new SerialViewModel(); 
            MainCommands = new ObservableCollection<Command>();
            serialCommandFromSelected = new SerialCommand(); 
            Command[] setupSubCommands = new Command[] { new DeviceNameCommand(), new ManifactureNameCommand(), new ModelNumberCommand(), new SerialNoCommand(), new SmartFunftionCommand(), new GenericValueCommand(), new RangesViewModel() };
            MainCommands.Add(new Command(1, "Setup", setupSubCommands.ToList()));

            Command[] clearSubCommands = new Command[] { new Command(2, "Restart"), new Command(1, "PassReset"), new Command(2, "Factory") };
            MainCommands.Add(new Command(2, "Clear", clearSubCommands.ToList()));
            MainCommands.Add(new Command(3, "Read"));
            MainCommands.Add(new Command(4, "Write")); 
        }

        void selectedMainCommand_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("SerialCommandFromSelected");
        }


        private RelayCommand serialSendCommand;
        public ICommand SerialSendCommand
        {
            get
            {
                if (serialSendCommand == null)
                    serialSendCommand = new RelayCommand((p) => serialSendCommandExecute(p));
                return serialSendCommand;
            }
        }


        private RelayCommand addToItemsCommand;
        public ICommand AddToItemsCommand
        {
            get
            {
                if (addToItemsCommand == null)
                    addToItemsCommand = new RelayCommand((p) => addToItemsCommandExecute(p));
                return addToItemsCommand;
            }
        }


        private RelayCommand serialCustomSendCommand;
        public ICommand SerialCustomSendCommand
        {
            get
            {
                if (serialCustomSendCommand == null)
                    serialCustomSendCommand = new RelayCommand((p) => serialCustomSendCommandExecute(p));
                return serialCustomSendCommand;
            }
        }

        private void serialCustomSendCommandExecute(object p)
        {
            List<byte> list = ((new HexStringConverter()).ConvertBack(p,typeof(List<byte>),null,null)) as List<byte>;
            if (list != null && list.Count > 4)
            {
                SerialCommand cmd = new SerialCommand(list);
                cmd.Update();
                serialSendCommandExecute(cmd);
            }
            else
                MessageBox.Show("Packet not Send"); 
        }

        private void serialSendCommandExecute(object p)
        {
            SerialViewModel.Serial.SendCommand((p as SerialCommand));
        }

        private void addToItemsCommandExecute(object p)
        {
            Items.Add((p as SerialCommand));
        }



    }
}
