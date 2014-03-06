using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UartTester.ViewModel
{
    public class SerialViewModel
    {
        static public SerialInterface Serial { get; private set; }

        public ObservableCollection<SerialCommand> Log
        {
            get
            {
                return SerialInterface.Log;
            }
        }

        public string SelectedPort
        {
            get
            {
                return Properties.Settings.Default.ComPort;
            }
            set
            {
                Properties.Settings.Default.ComPort = value;
                Properties.Settings.Default.Save(); 
            }
        }

        public SerialViewModel()
        {
            try
            {
                if(SelectedPort!=null)
                {
                    startCommandExecute(); 
                }
            }
            catch
            {

            }
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

        private void startCommandExecute()
        {
            if (Serial != null)
                Serial.Close();
            Serial = new SerialInterface(SelectedPort);
        }
        
    }
}
