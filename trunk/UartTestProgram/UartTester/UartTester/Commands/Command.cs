using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UartTester
{
    public class Command : INotifyPropertyChanged
    {
        public byte Number { get; set; }
        public string Name { get; set; }

        public List<Command> SubCommands { get; set; }

        public Command(byte nr, string Name)
        {
            Number = nr;
            this.Name = Name; 
        }

        public Command(byte nr, string Name, List<Command> sub)
        {
            Number = nr;
            this.Name = Name;
            SubCommands = sub; 
        }

        virtual public byte[] Payload { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

    }
}
