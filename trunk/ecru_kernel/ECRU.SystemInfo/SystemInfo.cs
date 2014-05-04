using System;
using System.IO;
using ECRU.File;
using ECRU.File.Files;
using ECRU.Utilities;
using ECRU.Utilities.Factories.ModuleFactory;
using ECRU.Utilities.HelpFunction;

namespace ECRU.SystemInfo
{
    public class SystemInfo : IModule
    {
        public const int SYSID_LENGTH = 8;
        private static byte[] sysId = new byte[SYSID_LENGTH];
        private static byte[] sysMac;
        private static string _systemConfigFilePath;

        private static object writeLock = new object();

        public static MacHierarchy ConnectionOverview { get; private set; }

        public static MacList ConnectedDevices { get; private set; }


        public static byte[] SystemID
        {
            get { return PassCode.FromHex(); }
        }

        public static string PassCode
        {
            get { return ReadConfigValueFromFile("PassCode"); }
            set
            {
                WriteConfigValueToFile("PassCode", doHash(value).ToHex());
            }
        }

        public static string Name { get { return ReadConfigValueFromFile("Name"); } set{WriteConfigValueToFile("Name", value);} }

        public static byte[] SystemMAC
        {
            get { return sysMac; }
            set
            {
                sysMac = value;
                WriteConfigValueToFile("SystemMAC", value.ToHex());
            }
        }

        private static byte[] doHash(string input)
        {
            UInt64 hashedValue = Knuthhash.doHash(input);

            byte[] hash = new byte[SYSID_LENGTH];
            for (int i = 0; i < 64 / 8; i++)
            {
                hash[i] = (byte)(hashedValue >> (8 * i));
            }
            return hash;
        }

        public void LoadConfig(string configFilePath)
        {

            _systemConfigFilePath = configFilePath;
            ConfigFile systemConfigFile = null;
            lock (writeLock)
            {
                try
                {
                    systemConfigFile = new ConfigFile(FileSystem.GetFile(configFilePath, FileAccess.Read, FileType.Local));

                    if (systemConfigFile.File != null)
                    {

                        SystemMAC = systemConfigFile.Contains("SystemMAC") ? systemConfigFile["SystemMAC"].FromHex() : null;

                    }
                    else
                    {
                        systemConfigFile = new ConfigFile(FileSystem.CreateFile(configFilePath, FileType.Local));
                        systemConfigFile["SystemMAC"] = null;
                    }
                }
                finally
                {
                    if (systemConfigFile != null) systemConfigFile.Close();
                }
            }
            
        }

        public void Start()
        {
            if (SystemMAC != null)
            {
                ConnectionOverview = new MacHierarchy();
                ConnectionOverview.Add(SystemMAC);
                ConnectedDevices = ConnectionOverview.GetDecices(SystemMAC);
            }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
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

                    if (systemConfigFile.File == null) return;

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
                systemConfigFile = new ConfigFile(FileSystem.GetFile(_systemConfigFilePath, FileAccess.Read, FileType.Local));

                if (systemConfigFile.File != null)
                {
                    if (systemConfigFile.Contains(property))
                    {
                        returnValue = systemConfigFile[property];
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