using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBus.Test
{
    class MockMessage : TMessage
    {
        public MockMessage(string type)
        {
            Type = type;
        }

        public string Type { get; private set; }
    }
}
