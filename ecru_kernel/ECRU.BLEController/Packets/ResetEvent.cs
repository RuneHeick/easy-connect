namespace ECRU.BLEController.Packets
{
    internal class ResetEvent : IPacket
    {
        private byte[] payload = new byte[1];

        public byte[] Payload
        {
            get { return payload; }
            set { payload = value; }
        }

        public CommandType Command
        {
            get { return CommandType.Reset; }
        }
    }
}