using System;
using Microsoft.SPOT;
using ECRU.File.Files;
using ECRU.Utilities.HelpFunction;
using System.Collections;

namespace ECRU.BLEController.Util
{
    class DeviceInfoDefFile: FileBase
    {

        private FileBase _fileBase;
        private CloseFunction _closeFunction;


        public DeviceInfoDefFile(FileBase file)
        {

            _fileBase = file;
            _closeFunction = file.Closefunc;
            file.Closefunc = (f) => Close();
            Init();
        }

        public DeviceInfo Object { get; set; }

        private void Init()
        {
            if (_fileBase.Data == null) return;
            Object = ByteToInfo(_fileBase.Data);
        }

        public new void Close()
        {
            if (Closefunc == null || Object == null) return;
            _fileBase.Data = InfoToByte(Object);
            _closeFunction(_fileBase);
            _closeFunction = null;
        }

        private static byte[] InfoToByte(DeviceInfo deviceInfo)
        {
            byte[] ByteArray = new byte[GetSize(deviceInfo)];
            int index = 0;

            ByteArray[index++] = 0xA0;
            ByteArray[index++] = (byte)deviceInfo.Address.Length;
            ByteArray.Set(deviceInfo.Address, index);
            index += deviceInfo.Address.Length;

            ByteArray[index++] = 0xA1;
            ByteArray[index++] = 4;
            ByteArray[index++] = (byte)(deviceInfo.TimeHandel >> 8);
            ByteArray[index++] = (byte)(deviceInfo.TimeHandel);
            ByteArray[index++] = (byte)(deviceInfo.PassCodeHandel >> 8);
            ByteArray[index++] = (byte)(deviceInfo.PassCodeHandel);

            ByteArray[index++] = 0x10;
            ByteArray[index++] = (byte)(2 + (deviceInfo.Manufacture.Value != null ? deviceInfo.Manufacture.Value.Length : 0));
            ByteArray[index++] = (byte)(deviceInfo.Manufacture.handle >> 8);
            ByteArray[index++] = (byte)(deviceInfo.Manufacture.handle);
            if (deviceInfo.Manufacture.Value != null)
            {
                ByteArray.Set(deviceInfo.Manufacture.Value, index);
                index += deviceInfo.Manufacture.Value.Length;
            }

            ByteArray[index++] = 0x11;
            ByteArray[index++] = (byte)(2 + (deviceInfo.Model.Value != null ? deviceInfo.Model.Value.Length : 0));
            ByteArray[index++] = (byte)(deviceInfo.Model.handle >> 8);
            ByteArray[index++] = (byte)(deviceInfo.Model.handle);
            if (deviceInfo.Model.Value != null)
            {
                ByteArray.Set(deviceInfo.Model.Value, index);
                index += deviceInfo.Model.Value.Length;
            }

            ByteArray[index++] = 0x12;
            ByteArray[index++] = (byte)(2 + (deviceInfo.Name.Value != null ? deviceInfo.Name.Value.Length : 0));
            ByteArray[index++] = (byte)(deviceInfo.Name.handle >> 8);
            ByteArray[index++] = (byte)(deviceInfo.Name.handle);
            if (deviceInfo.Name.Value != null)
            {
                ByteArray.Set(deviceInfo.Name.Value, index);
                index += deviceInfo.Name.Value.Length;
            }

            ByteArray[index++] = 0x13;
            ByteArray[index++] = (byte)(2 + (deviceInfo.Serial.Value != null ? deviceInfo.Serial.Value.Length : 0));
            ByteArray[index++] = (byte)(deviceInfo.Serial.handle >> 8);
            ByteArray[index++] = (byte)(deviceInfo.Serial.handle);
            if (deviceInfo.Serial.Value != null)
            {
                ByteArray.Set(deviceInfo.Serial.Value, index);
                index += deviceInfo.Serial.Value.Length;
            }



            foreach (Service s in deviceInfo.Services)
            {
                ByteArray[index++] = 0x20;      //-command
                ByteArray[index++] = (byte)(6); //-len
                ByteArray[index++] = (byte)(s.StartHandel>>8) ;
                ByteArray[index++] = (byte)(s.StartHandel);
                ByteArray[index++] = (byte)(s.EndHandel >> 8);
                ByteArray[index++] = (byte)(s.EndHandel);
                ByteArray[index++] = (byte)(s.UpdateHandel >> 8);
                ByteArray[index++] = (byte)(s.UpdateHandel);


                ByteArray[index++] = 0x14;
                ByteArray[index++] = (byte)(2 + (s.Description.Value != null ? s.Description.Value.Length : 0));
                ByteArray[index++] = (byte)(s.Description.handle >> 8);
                ByteArray[index++] = (byte)(s.Description.handle);
                if (s.Description.Value != null)
                {
                    ByteArray.Set(s.Description.Value, index);
                    index += s.Description.Value.Length;
                }

                foreach (Characteristic c in s.Characteristics)
                {
                    ByteArray[index++] = 0xC0;
                    ByteArray[index++] = (byte)(3);
                    ByteArray[index++] = (byte)(c.Value.handle >> 8);
                    ByteArray[index++] = (byte)(c.Value.handle);
                    ByteArray[index++] = (byte)(c.Value.ReadWriteProps);

                    ByteArray[index++] = 0xC1;
                    ByteArray[index++] = (byte)(2 + (c.Description.Value != null ? c.Description.Value.Length : 0));
                    ByteArray[index++] = (byte)(c.Description.handle >> 8);
                    ByteArray[index++] = (byte)(c.Description.handle);
                    if (c.Description.Value != null)
                    {
                        ByteArray.Set(c.Description.Value, index);
                        index += c.Description.Value.Length;
                    }

                    ByteArray[index++] = 0xC2;
                    ByteArray[index++] = (byte)(2 + (c.Format.Value != null ? c.Format.Value.Length : 0));
                    ByteArray[index++] = (byte)(c.Format.handle >> 8);
                    ByteArray[index++] = (byte)(c.Format.handle);
                    if (c.Format.Value != null)
                    {
                        ByteArray.Set(c.Format.Value, index);
                        index += c.Format.Value.Length;
                    }

                    ByteArray[index++] = 0xC3;
                    ByteArray[index++] = (byte)(2 + (c.GUIFormat.Value != null ? c.GUIFormat.Value.Length : 0));
                    ByteArray[index++] = (byte)(c.GUIFormat.handle >> 8);
                    ByteArray[index++] = (byte)(c.GUIFormat.handle);
                    if (c.GUIFormat.Value != null)
                    {
                        ByteArray.Set(c.GUIFormat.Value, index);
                        index += c.GUIFormat.Value.Length;
                    }

                    ByteArray[index++] = 0xC4;
                    ByteArray[index++] = (byte)(2 + (c.Range.Value != null ? c.Range.Value.Length : 0));
                    ByteArray[index++] = (byte)(c.Range.handle >> 8);
                    ByteArray[index++] = (byte)(c.Range.handle);
                    if (c.Range.Value != null)
                    {
                        ByteArray.Set(c.Range.Value, index);
                        index += c.Range.Value.Length;
                    }
                    ByteArray[index++] = 0xC5;
                    ByteArray[index++] = (byte)(2 + (c.Subscription.Value != null ? c.Subscription.Value.Length : 0));
                    ByteArray[index++] = (byte)(c.Subscription.handle >> 8);
                    ByteArray[index++] = (byte)(c.Subscription.handle);
                    if (c.Subscription.Value != null)
                    {
                        ByteArray.Set(c.Subscription.Value, index);
                        index += c.Subscription.Value.Length;
                    }
                    
                }
            }

            return ByteArray;
        }

        private static int GetSize(DeviceInfo deviceInfo)

        {
            int size = 16; // addrs + device info 
            IHandleValue[] pool = new IHandleValue[] { deviceInfo.Manufacture, deviceInfo.Model, deviceInfo.Name, deviceInfo.Serial };
            foreach (IHandleValue p in pool)
            {
                size += 2 + 2 + p.Value.Length;
            }

            foreach (Service s in deviceInfo.Services)
            {
                size += 2 + 2 + s.Description.Value.Length;
                size += 12;
                
                foreach (Characteristic c in s.Characteristics)
                {
                    size += 2 + 3; // value 
                    pool = new IHandleValue[] { c.Description, c.Format, c.GUIFormat, c.Range, c.Subscription };
                    foreach (IHandleValue p in pool)
                    {
                        size += 2 + 2;
                        if (p.Value != null)
                            size += p.Value.Length;
                    }
                }
            }

            return size;

        }

        private static DeviceInfo ByteToInfo(byte[] data)
        {
            try
            {
                DeviceInfo file = new DeviceInfo();
                int i = 0;

                int serviceIndex = -1;
                int charIndex = -1;

                MakeDeviceInfoReady(data, file);

                while (i < data.Length)
                {
                    byte Command = data[i];
                    int len = data[i + 1];

                    switch (Command)
                    {
                        case 0xA0:
                            file.Address = data.GetPart(i + 2, len);
                            break;
                        case 0xA1:
                            file.TimeHandel = (UInt16)((data[i + 2] << 8) + data[i + 3]);
                            file.PassCodeHandel = (UInt16)((data[i + 4] << 8) + data[i + 5]);
                            break;
                        case 0x10:
                            file.Manufacture.handle = (UInt16)((data[i + 2] << 8) + data[i + 3]);
                            file.Manufacture.Value = data.GetPart(i + 4, len - 2);
                            break;
                        case 0x11:
                            file.Model.handle = (UInt16)((data[i + 2] << 8) + data[i + 3]);
                            file.Model.Value = data.GetPart(i + 4, len - 2);
                            break;
                        case 0x12:
                            file.Name.handle = (UInt16)((data[i + 2] << 8) + data[i + 3]);
                            file.Name.Value = data.GetPart(i + 4, len - 2);
                            break;
                        case 0x13:
                            file.Serial.handle = (UInt16)((data[i + 2] << 8) + data[i + 3]);
                            file.Serial.Value = data.GetPart(i + 4, len - 2);
                            break;
                        case 0x20:
                            serviceIndex++;
                            charIndex = -1;
                            file.Services[serviceIndex].StartHandel = (UInt16)((data[i + 2] << 8) + data[i + 3]);
                            file.Services[serviceIndex].EndHandel = (UInt16)((data[i + 4] << 8) + data[i + 5]);
                            file.Services[serviceIndex].UpdateHandel = (UInt16)((data[i + 6] << 8) + data[i + 7]);
                            break;
                        case 0x14:
                            file.Services[serviceIndex].Description.handle = (UInt16)((data[i + 2] << 8) + data[i + 3]);
                            file.Services[serviceIndex].Description.Value = data.GetPart(i + 4, len - 2);
                            break;
                        case 0xC0:
                            charIndex++;
                            file.Services[serviceIndex].Characteristics[charIndex].Value.handle = (UInt16)((data[i + 2] << 8) + data[i + 3]);
                            file.Services[serviceIndex].Characteristics[charIndex].Value.ReadWriteProps = data[i + 4];
                            break;
                        case 0xC1:
                            file.Services[serviceIndex].Characteristics[charIndex].Description.handle = (UInt16)((data[i + 2] << 8) + data[i + 3]);
                            file.Services[serviceIndex].Characteristics[charIndex].Description.Value = data.GetPart(i + 4, len - 2);
                            break;
                        case 0xC2:
                            file.Services[serviceIndex].Characteristics[charIndex].Format.handle = (UInt16)((data[i + 2] << 8) + data[i + 3]);
                            file.Services[serviceIndex].Characteristics[charIndex].Format.Value = data.GetPart(i + 4, len - 2);
                            break;
                        case 0xC3:
                            file.Services[serviceIndex].Characteristics[charIndex].GUIFormat.handle = (UInt16)((data[i + 2] << 8) + data[i + 3]);
                            file.Services[serviceIndex].Characteristics[charIndex].GUIFormat.Value = data.GetPart(i + 4, len - 2);
                            break;
                        case 0xC4:
                            file.Services[serviceIndex].Characteristics[charIndex].Range.handle = (UInt16)((data[i + 2] << 8) + data[i + 3]);
                            file.Services[serviceIndex].Characteristics[charIndex].Range.Value = data.GetPart(i + 4, len - 2);
                            break;
                        case 0xC5:
                            file.Services[serviceIndex].Characteristics[charIndex].Subscription.handle = (UInt16)((data[i + 2] << 8) + data[i + 3]);
                            file.Services[serviceIndex].Characteristics[charIndex].Subscription.Value = data.GetPart(i + 4, len - 2);
                            break;
                    }

                    i += len + 2;
                }

                return file;
            }
            catch
            {
                return null; 
            }
        }

        private static void MakeDeviceInfoReady(byte[] data, DeviceInfo file)
        {
            int i = 0;
            ArrayList items = new ArrayList(); 

            while (i < data.Length)
            {
                byte Command = data[i];
                int len = data[i + 1];

                if (Command == 0x20)
                    items.Add(new ServiceStarter());
                if (Command == 0xC0)
                {
                    (items[items.Count - 1] as ServiceStarter).Characteristic++; 
                }

                i += 2 + len; 
            }

            file.Services = new Service[items.Count];
            for(int z = 0; z<items.Count; z++)
            {
                ServiceStarter starterInfo = items[z] as ServiceStarter;
                file.Services[z] = new Service();
                file.Services[z].Characteristics = new Characteristic[starterInfo.Characteristic];
                for (int k = 0; k < starterInfo.Characteristic; k++)
                {
                    file.Services[z].Characteristics[k] = new Characteristic(); 
                }

            }
        }


        private class ServiceStarter
        {
            public ServiceStarter()
            {
                Characteristic = 0; 
            }
            public int Characteristic { get; set; }
        }

    }
}
