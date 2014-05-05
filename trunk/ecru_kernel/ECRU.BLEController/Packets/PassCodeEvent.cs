using ECRU.Utilities.HelpFunction;

namespace ECRU.BLEController.Packets
{
    internal class PassCodeEvent : IPacket
    {
        private byte[] payload = new byte[9];

        public string Code
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
            get { return CommandType.PassCode; }
        }
    }
}