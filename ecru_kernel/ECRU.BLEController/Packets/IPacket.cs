using System;
using Microsoft.SPOT;

namespace ECRU.BLEController
{
    public interface IPacket
    {
        byte[] Payload {get; set;}

        CommandType Command { get;}

    }





}
