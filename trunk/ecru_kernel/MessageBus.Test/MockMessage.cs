﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MessageBus.Test
{
    public class MockMessage : TMessage
    {
        public MockMessage(string type)
        {
            Type = type;
        }

        public string Type { get; private set; }
    }
}
