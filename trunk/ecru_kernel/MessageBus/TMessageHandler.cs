﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MessageBus
{
    public interface TMessageHandler
    {
        void Handle(TMessage message);
    }
}
