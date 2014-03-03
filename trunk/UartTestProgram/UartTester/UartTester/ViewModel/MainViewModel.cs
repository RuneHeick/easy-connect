using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UartTester.Commands;

namespace UartTester.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<SerialCommand> SerialCommands { get; set; }

        public ObservableCollection<Command> MainCommands { get; set; }
        public ObservableCollection<string> SubCommands { get; set; }

        private Command selectedMainCommand;
        public Command SelectedMainCommand
        {
            set
            {
                selectedMainCommand = value;
                OnPropertyChanged("SelectedMainCommand");
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
                selectedSubCommand = value;
                OnPropertyChanged("SelectedSubCommand");
            }
            get
            {
                return selectedSubCommand;
            }
        }

        public enum MainCommandsValue
        {
            Setup = 0x01,
            Clear = 0x02,
            Read = 0x03,
            Write = 0x04
        }

        public MainViewModel()
        {
            MainCommands = new ObservableCollection<Command>();

            Command[] setupSubCommands = new Command[] { new DeviceNameCommand(), new ManifactureNameCommand(), new ModelNumberCommand(), new SerialNoCommand(), new Command(5, "Smart Function"), new Command(6, "Value"), new Command(7, "Ranges") };
            MainCommands.Add(new Command(1, "Setup", setupSubCommands.ToList())); 
            MainCommands.Add(new Command(2,"Clear"));
            MainCommands.Add(new Command(3, "Read"));
            MainCommands.Add(new Command(4, "Write")); 
        }
        



    }
}
