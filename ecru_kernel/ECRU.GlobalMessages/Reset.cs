using System;
using ECRU.EventBus;
using Microsoft.SPOT;

namespace ECRU.GlobalMessages
{
    public class Reset : TMessage
    {
        private const string _type = "ECRU.GlobalMessages.Reset";

        public string Type
        {
            get { return _type; }
        }
    }
}
