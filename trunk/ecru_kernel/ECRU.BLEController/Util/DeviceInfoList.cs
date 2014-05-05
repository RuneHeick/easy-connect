using System.Collections;
using ECRU.Utilities.HelpFunction;

namespace ECRU.BLEController.Util
{
    public class DeviceInfoList
    {
        private readonly ArrayList data = new ArrayList();

        public DeviceInfo this[int i]
        {
            get { return data[i] as DeviceInfo; }
        }

        public void Add(string Name, byte[] Addr)
        {
            Add(new DeviceInfo(Name, Addr));
        }

        public void Add(DeviceInfo item)
        {
            for (int i = 0; i < data.Count; i++)
            {
                var testitem = data[i] as DeviceInfo;
                if (testitem.Addr.ByteArrayCompare(item.Addr))
                    return;
            }
            data.Add(item);
        }

        public void Remove(byte[] addr)
        {
            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i] as DeviceInfo;
                if (item != null && item.Addr.ByteArrayCompare(addr))
                {
                    data.RemoveAt(i);
                    break;
                }
            }
        }

        public void Clear()
        {
            data.Clear();
        }

        public class DeviceInfo
        {
            public DeviceInfo(string Name, byte[] Addr)
            {
                this.Name = Name;
                this.Addr = Addr;
            }

            public string Name { get; private set; }
            public byte[] Addr { get; private set; }
        }
    }
}