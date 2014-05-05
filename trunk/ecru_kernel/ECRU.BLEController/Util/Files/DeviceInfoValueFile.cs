using System;
using ECRU.Utilities;
using ECRU.Utilities.HelpFunction;

namespace ECRU.BLEController.Util
{
    public class DeviceInfoValueFile : ConfigFile
    {
        public DeviceInfoValueFile(FileBase file) :
            base(file)
        {
        }

        public void Add(UInt16 handle, byte[] data, byte WRProp)
        {
            base[handle.ToString()] = WRProp.ToHex() + (data != null ? data.ToHex() : "");
        }

        public void Update(UInt16 handle, byte[] data)
        {
            if (base.Contains(handle.ToString()))
            {
                var wrprop = base[handle.ToString()].Substring(0, 2);
                byte[] wr = wrprop.FromHex();
                Add(handle, data, wr[0]);
            }
        }

        public byte[] GetData(UInt16 handle)
        {
            string data = base[handle.ToString()];
            return data != null && data.Length > 2 ? data.Substring(2, data.Length - 2).FromHex() : null;
        }

        public byte GetWrProp(UInt16 handle)
        {
            var wrprop = base[handle.ToString()].Substring(0, 2);
            if (wrprop != null)
            {
                byte[] wr = wrprop.FromHex();
                return wr[0];
            }
            return 0;
        }

        public void Create(DeviceInfo info)
        {
            foreach (Service s in info.Services)
            {
                foreach (Characteristic c in s.Characteristics)
                {
                    Add(c.Value.handle, c.Value.Value, c.Value.ReadWriteProps);
                }
            }
        }
    }
}