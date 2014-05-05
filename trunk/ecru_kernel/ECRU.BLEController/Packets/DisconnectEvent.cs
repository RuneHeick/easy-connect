namespace ECRU.BLEController.Packets
{
    internal class DisconnectEvent : IPacket
    {
        private byte[] address = new byte[6];

        public byte[] Address
        {
            set { address = value; }
            get { return address; }
        }

        public byte[] Payload
        {
            get { return address; }
            set { address = value; }
        }

        public CommandType Command
        {
            get { return CommandType.DisconnectEvent; }
        }
    }
}