﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBus
{
    public interface TMessageHandler
    {
        void Handle(TMessage message);
    }
}