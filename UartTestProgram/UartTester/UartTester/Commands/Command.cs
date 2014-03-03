using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UartTester
{
    public class Command
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
    }
}
