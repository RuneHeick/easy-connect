namespace ECRU.BLEController
{
    internal static class CRC
    {
        private static ushort UpdateCrc(ushort crc, byte b)
        {
            crc ^= (ushort) (b << 8);
            for (int i = 0; i < 8; i++)
            {
                if ((crc & 0x8000) > 0)
                    crc = (ushort) ((crc << 1) ^ 0x1021);
                else
                    crc <<= 1;
            }
            return crc;
        }

        public static ushort CalcCrc(byte[] data, int length)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < length; i++)
                crc = UpdateCrc(crc, data[i]);
            return crc;
        }
    }
}