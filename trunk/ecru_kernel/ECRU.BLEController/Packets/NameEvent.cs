using ECRU.Utilities.HelpFunction;

namespace ECRU.BLEController.Packets
{
    internal class NameEvent : IPacket
    {
        private byte[] payload = new byte[9];

        public string Name
        {
            get
            {
                try
                {
                    return Payload.GetString();
                }
                catch
                {
                    return "";
                }
            }
        }

        public byte[] Payload
        {
            get { return payload; }
            set { payload = value; }
        }

        public CommandType Command
        {
            get { return CommandType.NameEvent; }
        }
    }
}