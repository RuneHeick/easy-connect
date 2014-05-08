using System;
using ECRU.Utilities.HelpFunction;

namespace ECRU.Utilities
{
    public delegate void SystemInfoChanged(byte[] SystemMAC, String Name, String PassCode);

    public static class SystemInfo
    {
        public const int SYSID_LENGTH = 8;
        private static byte[] sysId = new byte[SYSID_LENGTH];
        private static byte[] _sysMac;
        private static string _systemConfigFilePath;

        private static readonly object writeLock = new object();

        public static MacHierarchy ConnectionOverview { get; private set; }

        public static MacList ConnectedDevices { get; private set; }


        public static byte[] SystemID
        {
            get { return doHash(PassCode); }
        }

        public static string PassCode
        {
            get { return ReadConfigValueFromFile("PassCode"); }
            set
            {
                WriteConfigValueToFile("PassCode", value);
                if (SysInfoChange != null)
                {
                    SysInfoChange(SystemMAC, Name, value);
                }
            }
        }

        public static string Name
        {
            get { return ReadConfigValueFromFile("Name"); }
            set
            {
                WriteConfigValueToFile("Name", value);
                if (SysInfoChange != null)
                {
                    SysInfoChange(SystemMAC, value, PassCode);
                }
            }
        }

        public static byte[] SystemMAC
        {
            get { return _sysMac ?? (_sysMac = ReadConfigValueFromFile("SystemMAC").FromHex()); }
            set
            {
                _sysMac = value;
                WriteConfigValueToFile("SystemMAC", value.ToHex());
                Start();
                if (SysInfoChange != null)
                {
                    SysInfoChange(value, Name, PassCode);
                }
            }
        }

        public static event SystemInfoChanged SysInfoChange;

        private static byte[] doHash(string input)
        {
            UInt64 hashedValue = Knuthhash.doHash(input);

            var hash = new byte[SYSID_LENGTH];
            for (int i = 0; i < 64/8; i++)
            {
                hash[i] = (byte) (hashedValue >> (8*i));
            }
            return hash;
        }

        public static void LoadConfig(string configFilePath)
        {
            _systemConfigFilePath = configFilePath;
            ConfigFile systemConfigFile = null;
            lock (writeLock)
            {
                try
                {
                    FileBase file = FileSystem.GetFile(configFilePath, FileAccess.Read, FileType.Local); 
                    if(file != null)
                        systemConfigFile = new ConfigFile(file);

                    if (systemConfigFile != null)
                    {
                        _sysMac = systemConfigFile.Contains("SystemMAC")
                            ? systemConfigFile["SystemMAC"].FromHex()
                            : null;
                    }
                    else
                    {
                        systemConfigFile = new ConfigFile(FileSystem.CreateFile(configFilePath, FileType.Local));
                        systemConfigFile["SystemMAC"] = null;
                        systemConfigFile["Name"] = null;
                        systemConfigFile["PassCode"] = null;
                    }
                }
                finally
                {
                    if (systemConfigFile != null) systemConfigFile.Close();
                }
            }
        }

        public static void Start()
        {
            if (SystemMAC != null)
            {
                ConnectionOverview = new MacHierarchy();
                ConnectionOverview.Add(SystemMAC);
                ConnectedDevices = ConnectionOverview.GetDecices(SystemMAC);
                
            }
            if (SysInfoChange != null)
            {
                SysInfoChange(SystemMAC, Name, PassCode);
            }
        }

        private static void WriteConfigValueToFile(String property, String value)
        {
            ConfigFile systemConfigFile = null;
            lock (writeLock)
            {
                try
                {
                    systemConfigFile =
                        new ConfigFile(FileSystem.GetFile(_systemConfigFilePath, FileAccess.ReadWrite, FileType.Local));

                    if (systemConfigFile == null) return;

                    if (systemConfigFile.Contains(property))
                    {
                        systemConfigFile[property] = value;
                    }
                }
                finally
                {
                    if (systemConfigFile != null)
                    {
                        systemConfigFile.Close();
                    }
                }
            }
        }

        private static String ReadConfigValueFromFile(String property)
        {
            ConfigFile systemConfigFile = null;
            String returnValue = null;

            try
            {
                systemConfigFile =
                    new ConfigFile(FileSystem.GetFile(_systemConfigFilePath, FileAccess.Read, FileType.Local));

                if (systemConfigFile != null)
                {
                    if (systemConfigFile.Contains(property))
                    {
                        returnValue = systemConfigFile[property];
                        if (returnValue == "")
                            returnValue = null; 
                    }
                }
            }
            finally
            {
                if (systemConfigFile != null)
                {
                    systemConfigFile.Close();
                }
            }

            return returnValue;
        }
    }
}