using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace UartTester.Settings
{
    [Serializable]
    public abstract class SettingBase
    {
        private string settingName = "Default";
        public string SettingName
        {
            get
            {
                return settingName;
            }
            set
            {
                settingName = value;
            }
        }

        public abstract void Complie();

        public abstract void Load();
    }
}
