using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace UartTester.Settings
{
    public class SettingManger
    {
        public Dictionary<string, SettingBase> SettingsDir = new Dictionary<string, SettingBase>();

        public List<SettingBase> Settings
        {
            get
            {
                return SettingsDir.Values.ToList();
            }
        }

        private void Compile(List<SettingBase> Items)
        {
            foreach (SettingBase sb in Items)
            {
                if (Settings.Contains(sb))
                    sb.Complie();
            }
        }

        public void Load(List<SettingBase> Items)
        {
            foreach (SettingBase sb in Items)
            {
                sb.Load();
            }
        }

        public void add(SettingBase setting)
        {
            this[setting.SettingName] = setting;
        }

        public void addIfNone(SettingBase setting)
        {
            if (!SettingsDir.ContainsKey(setting.SettingName))
            {
                this[setting.SettingName] = setting;
            }
        }

        public SettingBase this[string Name]
        {
            set
            {
                if (Name == null)
                    Name = value.SettingName;

                if (SettingsDir.ContainsKey(Name.ToLower()))
                {
                    value.SettingName = Name.ToLower();
                    SettingsDir[Name.ToLower()] = value;
                }
                else
                {
                    value.SettingName = Name.ToLower();
                    SettingsDir.Add(Name.ToLower(), value);
                }
            }
            get
            {
                if (SettingsDir.ContainsKey(Name.ToLower()))
                {
                    return SettingsDir[Name.ToLower()];
                }
                throw new IndexOutOfRangeException("Setting " + Name + "not in SettingManger");
            }
        }

        public void Serilize(string Path, List<SettingBase> Items)
        {
            FileStream s = new FileStream(Path, FileMode.Create);
            try
            {
                Compile(Items);
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, Items);
            }
            catch
            {
                MessageBox.Show("Error Serilizing File");
            }
            finally
            {
                s.Close();
            }
        }

        public void Deserialize(string Path, List<SettingBase> Items)
        {
            FileStream s = new FileStream(Path, FileMode.Open);
            try
            {
                IFormatter formatter = new BinaryFormatter();
                object a = formatter.Deserialize(s);
                List<SettingBase> TempList = a as List<SettingBase>;
                if (TempList != null)
                {
                    Items.Clear();
                    foreach (SettingBase setting in TempList)
                    {
                        Items.Add(setting);
                    }
                }
            }
            catch
            {
                MessageBox.Show("File not supportet");
            }
            finally
            {
                s.Close();
            }
        }

        public void Remove(string Name)
        {
            if (SettingsDir.ContainsKey(Name))
            {
                SettingsDir.Remove(Name);
            }
        }


    }
}
