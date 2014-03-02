using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MessageBus
{
    public interface ISubscriber
    {
        TMessage Message { get; }
        TMessageHandler FunctionPointer { get; }
    }
}
