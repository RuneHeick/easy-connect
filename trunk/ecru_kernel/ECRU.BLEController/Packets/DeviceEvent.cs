using System.Text;
using ECRU.Utilities.HelpFunction;

namespace ECRU.BLEController.Packets
{
    internal class DeviceEvent : IPacket
    {
        private byte[] payload = new byte[7];

        public byte[] Address
        {
            get { return Payload.GetPart(0, 6); }
        }

        public string Name
        {
            get { return new string(Encoding.UTF8.GetChars(Payload, 6, Payload.Length - 6)); }
        }

        public byte[] Payload
        {
            get { return payload; }
            set { payload = value; }
        }

        public CommandType Command
        {
            get { return CommandType.DeviceEvent; }
        }
    }
}