﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBus.Test
{
    class MockMessageBusFunctionPointer : TMessageHandler
    {
        public void Handle(TMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
