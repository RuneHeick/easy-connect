namespace ECRU.BLEController
{
    public interface IPacket
    {
        byte[] Payload { get; set; }

        CommandType Command { get; }
    }
}