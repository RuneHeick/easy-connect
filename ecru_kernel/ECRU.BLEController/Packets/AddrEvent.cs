﻿using System;
using Microsoft.SPOT;
using System.Text;
using ECRU.Utilities.HelpFunction;

namespace ECRU.BLEController.Packets
{
    class AddrEvent: IPacket
    {
        private byte[] payload = new byte[6]; 

        public byte[] Address
        {
            get
            {
                return Payload.GetPart(0, 6);
            }
        }

        public byte[] Payload
        {
            get
            {
                return payload; 
            }
            set
            {
                payload = value; 
            }
        }

        public CommandType Command
        {
            get
            {
                return CommandType.AddrEvent;
            }
        }
    }
}